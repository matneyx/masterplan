using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Extensibility;
using Masterplan.Tools;

namespace Masterplan.UI
{
    /// <summary>
    ///     Enumeration containing the various methods for rolling initiative.
    /// </summary>
    public enum InitiativeMode
    {
        /// <summary>
        ///     Creatures of the same type share the same initiative roll.
        /// </summary>
        AutoGroup,

        /// <summary>
        ///     Roll initiative for each creature individually.
        /// </summary>
        AutoIndividual,

        /// <summary>
        ///     Roll initiative for creatures manually.
        /// </summary>
        ManualIndividual,

        /// <summary>
        ///     Roll initiative for creatures manually in groups.
        /// </summary>
        ManualGroup
    }

    internal partial class CombatForm : Form
    {
        private readonly List<OngoingCondition> _fEffects = new List<OngoingCondition>();

        private readonly Encounter _fEncounter;

        private readonly StringFormat _fLeft = new StringFormat();
        private readonly EncounterLog _fLog = new EncounterLog();
        private readonly int _fPartyLevel = Session.Project.Party.Level;
        private readonly StringFormat _fRight = new StringFormat();

        private readonly Dictionary<Guid, CombatData> _fTrapData;

        private bool _fCombatStarted;

        private CombatData _fCurrentActor;
        private int _fCurrentRound = 1;
        private bool _fPromptOnClose = true;

        private int _fRemovedCreatureXp;

        private bool _fUpdatingList;

        public List<IToken> SelectedTokens
        {
            get
            {
                var tokens = new List<IToken>();

                foreach (ListViewItem lvi in CombatList.SelectedItems)
                {
                    var token = lvi.Tag as IToken;
                    if (token != null)
                        tokens.Add(token);
                }

                return tokens;
            }
        }

        public Trap SelectedTrap
        {
            get
            {
                if (CombatList.SelectedItems.Count != 0)
                    return CombatList.SelectedItems[0].Tag as Trap;

                return null;
            }
        }

        public SkillChallenge SelectedChallenge
        {
            get
            {
                if (CombatList.SelectedItems.Count != 0)
                    return CombatList.SelectedItems[0].Tag as SkillChallenge;

                return null;
            }
        }

        public MapView PlayerMap
        {
            get
            {
                if (Session.PlayerView == null)
                    return null;

                if (Session.PlayerView.Controls.Count == 0)
                    return null;

                var splitter = Session.PlayerView.Controls[0] as SplitContainer;
                if (splitter == null)
                    return null;

                if (splitter.Panel1Collapsed)
                    return null;

                if (splitter.Panel1.Controls.Count == 0)
                    return null;

                return splitter.Panel1.Controls[0] as MapView;
            }
        }

        public WebBrowser PlayerInitiative
        {
            get
            {
                if (Session.PlayerView == null)
                    return null;

                if (Session.PlayerView.Controls.Count == 0)
                    return null;

                var splitter = Session.PlayerView.Controls[0] as SplitContainer;
                if (splitter == null)
                    return null;

                if (splitter.Panel2Collapsed)
                    return null;

                if (splitter.Panel2.Controls.Count == 0)
                    return null;

                foreach (Control ctrl in splitter.Panel2.Controls)
                {
                    var init = ctrl as WebBrowser;
                    if (init != null)
                        return init;
                }

                return null;
            }
        }

        public bool TwoColumnPreview => _fCurrentActor != null && Preview.Width > 630;

        public CombatForm(CombatState cs)
        {
            InitializeComponent();
            Preview.DocumentText = "";
            LogBrowser.DocumentText = "";

            Application.Idle += Application_Idle;

            _fLeft.Alignment = StringAlignment.Near;
            _fLeft.LineAlignment = StringAlignment.Center;
            _fRight.Alignment = StringAlignment.Far;
            _fRight.LineAlignment = StringAlignment.Center;

            _fEncounter = cs.Encounter.Copy() as Encounter;
            _fPartyLevel = cs.PartyLevel;
            _fRemovedCreatureXp = cs.RemovedCreatureXp;

            _fCurrentRound = cs.CurrentRound;
            RoundLbl.Text = "Round " + _fCurrentRound;

            if (cs.QuickEffects != null)
                foreach (var effect in cs.QuickEffects)
                    add_quick_effect(effect);

            if (cs.HeroData != null)
            {
                // Update the hero combat information
                foreach (var h in Session.Project.Heroes)
                    if (cs.HeroData.ContainsKey(h.Id))
                        h.CombatData = cs.HeroData[h.Id];
            }
            else
            {
                // We're starting an encounter; clear PC locations
                foreach (var h in Session.Project.Heroes)
                    h.CombatData.Location = CombatData.NoPoint;
            }

            foreach (var h in Session.Project.Heroes)
            {
                h.CombatData.Id = h.Id;
                h.CombatData.DisplayName = h.Name;
            }

            if (cs.TrapData != null)
                _fTrapData = cs.TrapData;
            else
                _fTrapData = new Dictionary<Guid, CombatData>();
            foreach (var t in _fEncounter.Traps)
            {
                if (_fTrapData.ContainsKey(t.Id))
                    continue;

                var cd = new CombatData();
                cd.DisplayName = t.Name;
                cd.Id = t.Id;

                _fTrapData[t.Id] = cd;
            }

            if (_fEncounter.MapId != Guid.Empty)
            {
                foreach (var hero in Session.Project.Heroes)
                foreach (var ct in hero.Tokens)
                {
                    var name = hero.Name + ": " + ct.Name;

                    var lvi = TemplateList.Items.Add(name);
                    lvi.Tag = ct;
                    lvi.Group = TemplateList.Groups[0];
                }

                var sizes = Enum.GetValues(typeof(CreatureSize));
                foreach (CreatureSize size in sizes)
                {
                    var ct = new CustomToken();
                    ct.Type = CustomTokenType.Token;
                    ct.TokenSize = size;
                    ct.Colour = Color.Black;
                    ct.Name = size + " Token";

                    var lvi = TemplateList.Items.Add(ct.Name);
                    lvi.Tag = ct;
                    lvi.Group = TemplateList.Groups[1];
                }

                for (var n = 2; n <= 10; ++n)
                {
                    var ct = new CustomToken();
                    ct.Type = CustomTokenType.Overlay;
                    ct.OverlaySize = new Size(n, n);
                    ct.Name = n + " x " + n + " Zone";
                    ct.Colour = Color.Transparent;

                    var lvi = TemplateList.Items.Add(ct.Name);
                    lvi.Tag = ct;
                    lvi.Group = TemplateList.Groups[2];
                }
            }
            else
            {
                Pages.TabPages.Remove(TemplatesPage);
            }

            _fLog = cs.Log;
            _fLog.Active = false;
            if (_fLog.Entries.Count != 0)
            {
                _fLog.Active = true;
                _fLog.AddResumeEntry();
            }

            update_log();

            // Set current actor
            if (cs.CurrentActor != Guid.Empty)
            {
                _fCombatStarted = true;

                var hero = Session.Project.FindHero(cs.CurrentActor);
                if (hero != null)
                {
                    _fCurrentActor = hero.CombatData;
                }
                else if (_fTrapData.ContainsKey(cs.CurrentActor))
                {
                    _fCurrentActor = _fTrapData[cs.CurrentActor];
                }
                else
                {
                    var cd = _fEncounter.FindCombatData(cs.CurrentActor);
                    if (cd != null)
                        _fCurrentActor = cd;
                }
            }

            CombatList.ListViewItemSorter = new InitiativeSorter(_fTrapData, _fEncounter);

            set_map(cs.TokenLinks, cs.Viewpoint, cs.Sketches);
            MapMenu.Visible = _fEncounter.MapId != Guid.Empty;

            InitiativePanel.InitiativeScores = get_initiatives();
            InitiativePanel.CurrentInitiative = InitiativePanel.Maximum;

            PlayerViewMapMenu.Visible = _fEncounter.MapId != Guid.Empty;
            PlayerViewNoMapMenu.Visible = _fEncounter.MapId == Guid.Empty;

            if (!Session.Preferences.Combat.CombatColumnInitiative)
                InitHdr.Width = 0;
            if (!Session.Preferences.Combat.CombatColumnHp)
                HPHdr.Width = 0;
            if (!Session.Preferences.Combat.CombatColumnDefences)
                DefHdr.Width = 0;
            if (!Session.Preferences.Combat.CombatColumnEffects)
                EffectsHdr.Width = 0;

            var screen = Screen.FromControl(this);
            if (screen.Bounds.Height > screen.Bounds.Width)
                OptionsPortrait_Click(null, null);

            Session.CurrentEncounter = _fEncounter;

            update_list();
            update_log();
            update_preview_panel();
            update_maps();
            update_statusbar();
        }

        ~CombatForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void DetailsBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedTokens.Count == 1)
                    edit_token(SelectedTokens[0]);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void DamageBtn_Click(object sender, EventArgs e)
        {
            try
            {
                do_damage(SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void HealBtn_Click(object sender, EventArgs e)
        {
            try
            {
                do_heal(SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void DelayBtn_Click(object sender, EventArgs e)
        {
            try
            {
                set_delay(SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NextInitBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (!_fCombatStarted)
                {
                    start_combat();
                    return;
                }

                var scores = get_initiatives();
                if (scores.Count == 0)
                    return;

                handle_ended_effects(false);
                handle_saves();

                // Select the next combatant
                _fCurrentActor = get_next_actor(_fCurrentActor);
                _fLog.AddStartTurnEntry(_fCurrentActor.Id);

                if (_fCurrentActor.Initiative > InitiativePanel.CurrentInitiative)
                {
                    _fCurrentRound += 1;
                    RoundLbl.Text = "Round: " + _fCurrentRound;

                    _fLog.AddStartRoundEntry(_fCurrentRound);
                }

                InitiativePanel.CurrentInitiative = _fCurrentActor.Initiative;

                handle_regen();
                handle_ended_effects(true);
                handle_ongoing_damage();
                handle_recharge();

                if (_fCurrentActor != null && !TwoColumnPreview)
                    select_current_actor();

                update_list();
                update_log();
                update_preview_panel();

                highlight_current_actor();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatantsAdd_Click(object sender, EventArgs e)
        {
            try
            {
                var enc = new Encounter();
                var dlg = new EncounterBuilderForm(enc, _fPartyLevel, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var slot in dlg.Encounter.Slots)
                    {
                        _fEncounter.Slots.Add(slot);

                        if (_fCombatStarted)
                            roll_initiative();
                    }

                    foreach (var trap in dlg.Encounter.Traps)
                    {
                        if (trap.Initiative != int.MinValue)
                        {
                            _fTrapData[trap.Id] = new CombatData();

                            if (_fCombatStarted)
                                roll_initiative();
                        }

                        _fEncounter.Traps.Add(trap);
                    }

                    foreach (var sc in dlg.Encounter.SkillChallenges) _fEncounter.SkillChallenges.Add(sc);

                    update_list();
                    update_preview_panel();
                    update_statusbar();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatantsAddCustom_Click(object sender, EventArgs e)
        {
            try
            {
                var ct = new CustomToken();
                ct.Name = "Custom Token";
                ct.Type = CustomTokenType.Token;

                var dlg = new CustomTokenForm(ct);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fEncounter.CustomTokens.Add(dlg.Token);

                    update_list();
                    update_maps();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatantsAddOverlay_Click(object sender, EventArgs e)
        {
            try
            {
                var ct = new CustomToken();
                ct.Name = "Custom Overlay";
                ct.Type = CustomTokenType.Overlay;

                var dlg = new CustomOverlayForm(ct);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fEncounter.CustomTokens.Add(dlg.Token);

                    update_list();
                    update_maps();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatantsRemove_Click(object sender, EventArgs e)
        {
            if (SelectedTokens.Count != 0)
                remove_from_combat(SelectedTokens);
        }

        private void CombatantsHideAll_Click(object sender, EventArgs e)
        {
            show_or_hide_all(false);
        }

        private void CombatantsShowAll_Click(object sender, EventArgs e)
        {
            show_or_hide_all(true);
        }

        private void ShowMap_Click(object sender, EventArgs e)
        {
            try
            {
                MapSplitter.Panel2Collapsed = !MapSplitter.Panel2Collapsed;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapLOS_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.LineOfSight = !MapView.LineOfSight;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapGrid_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowGrid = MapView.ShowGrid == MapGridMode.None ? MapGridMode.Overlay : MapGridMode.None;
                Session.Preferences.Combat.CombatGrid = MapView.ShowGrid == MapGridMode.Overlay;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapGridLabels_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowGridLabels = !MapView.ShowGridLabels;
                Session.Preferences.Combat.CombatGridLabels = MapView.ShowGridLabels;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapHealth_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowHealthBars = !MapView.ShowHealthBars;
                Session.Preferences.Combat.CombatHealthBars = MapView.ShowHealthBars;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapConditions_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowConditions = !MapView.ShowConditions;
                Session.Preferences.Combat.CombatConditionBadges = MapView.ShowConditions;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapPictureTokens_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowPictureTokens = !MapView.ShowPictureTokens;
                Session.Preferences.Combat.CombatPictureTokens = MapView.ShowPictureTokens;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapFogAllCreatures_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowCreatures = CreatureViewMode.All;
                Session.Preferences.Combat.CombatFog = CreatureViewMode.All;

                update_list();
                update_preview_panel();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapFogVisibleCreatures_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowCreatures = CreatureViewMode.Visible;
                Session.Preferences.Combat.CombatFog = CreatureViewMode.Visible;

                update_list();
                update_preview_panel();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapFogHideCreatures_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowCreatures = CreatureViewMode.None;
                Session.Preferences.Combat.CombatFog = CreatureViewMode.None;

                update_list();
                update_preview_panel();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapNavigate_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.AllowScrolling = !MapView.AllowScrolling;
                ZoomGauge.Visible = MapView.AllowScrolling;

                if (Session.PlayerView != null)
                {
                    if (!MapView.AllowScrolling)
                    {
                        cancelled_scrolling();
                    }
                    else
                    {
                        Session.Preferences.Combat.PlayerViewMap = PlayerMap != null;
                        Session.Preferences.Combat.PlayerViewInitiative = PlayerInitiative != null;

                        Session.PlayerView.ShowMessage("DM is editing the map; please wait");
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapReset_Click(object sender, EventArgs e)
        {
            try
            {
                ZoomGauge.Value = 50;
                MapView.ScalingFactor = 1.0;

                if (_fEncounter.MapAreaId != Guid.Empty)
                {
                    var area = MapView.Map.FindArea(_fEncounter.MapAreaId);
                    MapView.Viewpoint = area.Region;
                }
                else
                {
                    MapView.Viewpoint = Rectangle.Empty;
                }

                if (PlayerMap != null) PlayerMap.Viewpoint = MapView.Viewpoint;

                MapView.MapChanged();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new MapPrintingForm(MapView);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = MapView.Map.Name;
                if (_fEncounter.MapAreaId != Guid.Empty)
                {
                    var area = MapView.Map.FindArea(_fEncounter.MapAreaId);
                    dlg.FileName += " - " + area.Name;
                }

                dlg.Filter = "Bitmap Image |*.bmp|JPEG Image|*.jpg|GIF Image|*.gif|PNG Image|*.png";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var format = ImageFormat.Bmp;
                    switch (dlg.FilterIndex)
                    {
                        case 1:
                            format = ImageFormat.Bmp;
                            break;
                        case 2:
                            format = ImageFormat.Jpeg;
                            break;
                        case 3:
                            format = ImageFormat.Gif;
                            break;
                        case 4:
                            format = ImageFormat.Png;
                            break;
                    }

                    var bmp = Screenshot.Map(MapView);
                    bmp.Save(dlg.FileName, format);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewMap_Click(object sender, EventArgs e)
        {
            try
            {
                show_player_view(PlayerMap == null, PlayerInitiative != null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewInitList_Click(object sender, EventArgs e)
        {
            try
            {
                show_player_view(PlayerMap != null, PlayerInitiative == null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerLabels_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.Combat.PlayerViewCreatureLabels =
                    !Session.Preferences.Combat.PlayerViewCreatureLabels;

                if (PlayerMap != null) PlayerMap.ShowCreatureLabels = !PlayerMap.ShowCreatureLabels;

                if (PlayerInitiative != null) PlayerInitiative.DocumentText = InitiativeView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerHealth_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowHealthBars = !PlayerMap.ShowHealthBars;
                    Session.Preferences.Combat.PlayerViewHealthBars = PlayerMap.ShowHealthBars;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerConditions_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowConditions = !PlayerMap.ShowConditions;
                    Session.Preferences.Combat.PlayerViewConditionBadges = PlayerMap.ShowConditions;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerPictureTokens_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowPictureTokens = !PlayerMap.ShowPictureTokens;
                    Session.Preferences.Combat.PlayerViewPictureTokens = PlayerMap.ShowPictureTokens;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewLOS_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null) PlayerMap.LineOfSight = !PlayerMap.LineOfSight;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewGrid_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowGrid =
                        PlayerMap.ShowGrid == MapGridMode.None ? MapGridMode.Overlay : MapGridMode.None;
                    Session.Preferences.Combat.PlayerViewGrid = PlayerMap.ShowGrid == MapGridMode.Overlay;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewGridLabels_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowGridLabels = !PlayerMap.ShowGridLabels;
                    Session.Preferences.Combat.PlayerViewGridLabels = PlayerMap.ShowGridLabels;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerFogAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowCreatures = CreatureViewMode.All;
                    Session.Preferences.Combat.PlayerViewFog = CreatureViewMode.All;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerFogVisible_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowCreatures = CreatureViewMode.Visible;
                    Session.Preferences.Combat.PlayerViewFog = CreatureViewMode.Visible;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerFogNone_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlayerMap != null)
                {
                    PlayerMap.ShowCreatures = CreatureViewMode.None;
                    Session.Preferences.Combat.PlayerViewFog = CreatureViewMode.None;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void OneColumn_Click(object sender, EventArgs e)
        {
            try
            {
                if (ListSplitter.Orientation == Orientation.Horizontal)
                    return;

                if (_fEncounter.MapId != Guid.Empty)
                    Session.Preferences.Combat.CombatTwoColumns = false;
                else
                    Session.Preferences.Combat.CombatTwoColumnsNoMap = false;

                ListSplitter.Orientation = Orientation.Horizontal;
                ListSplitter.SplitterDistance = ListSplitter.Height / 2;

                MapSplitter.SplitterDistance = 350;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void TwoColumns_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fEncounter.MapId != Guid.Empty)
                    Session.Preferences.Combat.CombatTwoColumns = true;
                else
                    Session.Preferences.Combat.CombatTwoColumnsNoMap = true;

                ListSplitter.Orientation = Orientation.Vertical;
                if (!MapSplitter.Panel2Collapsed && MapSplitter.Orientation == Orientation.Vertical)
                {
                    MapSplitter.SplitterDistance = 700;
                    ListSplitter.SplitterDistance = 350;
                }
                else
                {
                    ListSplitter.SplitterDistance = ListSplitter.Width / 2;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsAutoRemove_Click(object sender, EventArgs e)
        {
            Session.Preferences.Combat.CreatureAutoRemove = !Session.Preferences.Combat.CreatureAutoRemove;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                var mob = false;
                var delayed = false;

                if (SelectedTokens.Count != 0)
                {
                    mob = true;
                    delayed = true;

                    foreach (var token in SelectedTokens)
                    {
                        var tokenIsMob = token is CreatureToken || token is Hero;
                        if (!tokenIsMob)
                        {
                            mob = false;
                            delayed = false;
                        }

                        if (token is CreatureToken)
                        {
                            var ct = token as CreatureToken;
                            if (!ct.Data.Delaying)
                                delayed = false;
                        }

                        if (token is Hero)
                        {
                            var hero = token as Hero;
                            var cd = hero.CombatData;
                            if (!cd.Delaying)
                                delayed = false;
                        }
                    }
                }

                DetailsBtn.Enabled = SelectedTokens.Count == 1;
                DamageBtn.Enabled = mob;
                HealBtn.Enabled = mob;
                EffectMenu.Enabled = mob;

                NextInitBtn.Text = _fCombatStarted ? "Next Turn" : "Start Encounter";

                DelayBtn.Visible = _fCombatStarted;
                DelayBtn.Enabled = mob;
                DelayBtn.Checked = delayed;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatForm_Shown(object sender, EventArgs e)
        {
            try
            {
                if (!Session.Preferences.Combat.CombatMapRight)
                    MapSplitter.SplitterDistance = MapSplitter.Height / 2;

                if (_fCurrentActor == null)
                {
                    // Reset PCs for the new fight
                    foreach (var hero in Session.Project.Heroes)
                        hero.CombatData.Reset(false);

                    // We do this in case they had temp HP
                    update_list();
                    update_maps();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_fPromptOnClose)
                {
                    var enemies = false;
                    foreach (var slot in _fEncounter.AllSlots)
                    {
                        var hp = slot.Card.Hp;

                        foreach (var cd in slot.CombatData)
                        {
                            // Ignore creatures that haven't been added to the encounter yet
                            if (cd.Initiative == int.MinValue)
                                continue;

                            var totalHp = hp + cd.TempHp - cd.Damage;
                            if (totalHp > 0)
                                enemies = true;
                        }
                    }

                    if (enemies)
                    {
                        var str = "There are creatures remaining; are you sure you want to end the encounter?";
                        if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) ==
                            DialogResult.No)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }

                if (PlayerMap != null || PlayerInitiative != null)
                    Session.PlayerView.ShowDefault();

                Session.CurrentEncounter = null;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            try
            {
                if (SelectedTokens.Count != 1)
                    return;

                var token = SelectedTokens[0];

                if (token is CreatureToken)
                {
                    var ct = token as CreatureToken;
                    if (ct.Data.Location == CombatData.NoPoint)
                    {
                        DoDragDrop(ct, DragDropEffects.Move);

                        update_list();
                        update_preview_panel();
                        update_maps();
                    }
                }

                if (token is Hero)
                {
                    var hero = token as Hero;

                    if (hero.CombatData.Location == CombatData.NoPoint)
                    {
                        DoDragDrop(hero, DragDropEffects.Move);

                        if (hero.CombatData.Location != CombatData.NoPoint)
                        {
                            update_list();
                            update_preview_panel();
                            update_maps();
                        }
                    }
                }

                if (token is CustomToken)
                {
                    var ct = token as CustomToken;
                    if (ct.Data.Location == CombatData.NoPoint)
                    {
                        DoDragDrop(ct, DragDropEffects.Move);

                        update_list();
                        update_preview_panel();
                        update_maps();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatList_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                if (_fUpdatingList)
                    return;

                if (SelectedTokens.Count == 0)
                {
                    MapView.SelectTokens(null, false);
                    PlayerMap?.SelectTokens(null, false);

                    update_preview_panel();
                }
                else
                {
                    MapView.SelectTokens(SelectedTokens, false);
                    PlayerMap?.SelectTokens(SelectedTokens, false);

                    update_preview_panel();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatList_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void CombatList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (SelectedTokens.Count == 1)
                    edit_token(SelectedTokens[0]);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapView_ItemMoved(object sender, MovementEventArgs e)
        {
            try
            {
                update_maps();

                foreach (var token in MapView.SelectedTokens)
                {
                    var id = Guid.Empty;

                    var ct = token as CreatureToken;
                    if (ct != null)
                        id = ct.Data.Id;

                    var hero = token as Hero;
                    if (hero != null)
                        id = hero.Id;

                    _fLog.AddMoveEntry(id, e.Distance, "");
                }

                update_log();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapView_SelectedTokensChanged(object sender, EventArgs e)
        {
            try
            {
                _fUpdatingList = true;
                CombatList.SelectedItems.Clear();
                foreach (var token in MapView.SelectedTokens)
                    select_token(token);
                _fUpdatingList = false;

                update_preview_panel();

                PlayerMap?.SelectTokens(MapView.SelectedTokens, false);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapView_HoverTokenChanged(object sender, EventArgs e)
        {
            try
            {
                set_tooltip(MapView.HoverToken, MapView);

                if (PlayerMap != null)
                    PlayerMap.HoverToken = MapView.HoverToken;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapView_TokenActivated(object sender, TokenEventArgs e)
        {
            try
            {
                if (e.Token is CreatureToken || e.Token is Hero)
                {
                    var tokens = new List<IToken>();
                    tokens.Add(e.Token);

                    do_damage(tokens);
                }

                if (e.Token is CustomToken)
                    edit_token(e.Token);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private TokenLink MapView_CreateTokenLink(object sender, TokenListEventArgs e)
        {
            try
            {
                var link = new TokenLink();
                link.Tokens.AddRange(e.Tokens);

                var dlg = new TokenLinkForm(link);
                if (dlg.ShowDialog() == DialogResult.OK)
                    return dlg.Link;

                return null;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        private TokenLink MapView_EditTokenLink(object sender, TokenLinkEventArgs e)
        {
            var dlg = new TokenLinkForm(e.Link);
            if (dlg.ShowDialog() == DialogResult.OK)
                return dlg.Link;

            return null;
        }

        private void MapView_TokenDragged(object sender, DraggedTokenEventArgs e)
        {
            PlayerMap?.SetDragInfo(e.OldLocation, e.NewLocation);
        }

        private void ZoomGauge_Scroll(object sender, EventArgs e)
        {
            try
            {
                var max = 10.0;
                var mid = 1.0;
                var min = 0.1;

                var x = (double)(ZoomGauge.Value - ZoomGauge.Minimum) / (ZoomGauge.Maximum - ZoomGauge.Minimum);
                if (x >= 0.5)
                {
                    x -= 0.5;
                    x *= 2;
                    MapView.ScalingFactor = mid + x * (max - mid);
                }
                else
                {
                    x *= 2;
                    MapView.ScalingFactor = min + x * (mid - min);
                }

                MapView.MapChanged();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Preview_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.Scheme == "power")
                {
                    e.Cancel = true;

                    var tokens = e.Url.LocalPath.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    var tokenId = new Guid(tokens[0]);

                    var cd = _fEncounter.FindCombatData(tokenId);
                    if (cd != null)
                    {
                        var slot = _fEncounter.FindSlot(cd);
                        if (slot != null)
                        {
                            var powerId = new Guid(tokens[1]);
                            var power = slot.Card.FindPower(powerId);
                            if (power == null)
                                return;

                            // If it's an attack power, roll it
                            if (power.Attack != null)
                                roll_attack(power);

                            _fLog.AddPowerEntry(cd.Id, power.Name, true);
                            update_log();

                            // If it's an encounter / daily power, ask to use it up
                            if (power.Action != null && !cd.UsedPowers.Contains(power.Id))
                                if (power.Action.Use == PowerUseType.Encounter ||
                                    power.Action.Use == PowerUseType.Daily)
                                {
                                    var usage = "per-encounter";
                                    if (power.Action.Use == PowerUseType.Daily)
                                        usage = "daily";

                                    var str = "This is a " + usage + " power. Do you want to mark it as expended?";
                                    if (MessageBox.Show(str, power.Name, MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Information) == DialogResult.Yes)
                                    {
                                        cd.UsedPowers.Add(power.Id);
                                        update_preview_panel();
                                    }
                                }
                        }
                    }
                    else
                    {
                        // This is a trap attack
                        foreach (var trap in _fEncounter.Traps)
                        {
                            var attack = trap.FindAttack(tokenId);
                            if (attack != null) roll_check(attack.Attack.ToString(), attack.Attack.Bonus);
                        }
                    }
                }

                if (e.Url.Scheme == "refresh")
                {
                    e.Cancel = true;

                    var tokens = e.Url.LocalPath.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    var tokenId = new Guid(tokens[0]);
                    var powerId = new Guid(tokens[1]);

                    var cd = _fEncounter.FindCombatData(tokenId);

                    var powerName = "";
                    var slot = _fEncounter.FindSlot(cd);
                    if (slot != null)
                    {
                        var c = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                        if (c != null)
                            foreach (var cp in c.CreaturePowers)
                                if (cp.Id == powerId)
                                {
                                    powerName = cp.Name;
                                    break;
                                }
                    }

                    if (cd.UsedPowers.Contains(powerId))
                    {
                        cd.UsedPowers.Remove(powerId);
                        _fLog.AddPowerEntry(cd.Id, powerName, false);
                    }
                    else
                    {
                        cd.UsedPowers.Add(powerId);
                        _fLog.AddPowerEntry(cd.Id, powerName, true);
                    }

                    update_preview_panel();
                    update_log();
                }

                if (e.Url.Scheme == "ability")
                {
                    e.Cancel = true;

                    var mod = int.Parse(e.Url.LocalPath);
                    roll_check("Ability", mod);
                }

                if (e.Url.Scheme == "sc")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "reset")
                    {
                        // Reset
                        var sc = SelectedChallenge;
                        if (sc != null)
                            foreach (var scd in sc.Skills)
                            {
                                scd.Results.Successes = 0;
                                scd.Results.Fails = 0;

                                update_list();
                                update_preview_panel();
                            }
                    }
                }

                if (e.Url.Scheme == "success")
                {
                    e.Cancel = true;

                    // Success
                    var sc = SelectedChallenge;
                    if (sc != null)
                    {
                        var scd = sc.FindSkill(e.Url.LocalPath);
                        scd.Results.Successes += 1;

                        _fLog.AddSkillEntry(_fCurrentActor.Id, e.Url.LocalPath);
                        _fLog.AddSkillChallengeEntry(_fCurrentActor.Id, true);

                        update_list();
                        update_preview_panel();
                        update_log();
                    }
                }

                if (e.Url.Scheme == "failure")
                {
                    e.Cancel = true;

                    // Failure
                    var sc = SelectedChallenge;
                    if (sc != null)
                    {
                        var scd = sc.FindSkill(e.Url.LocalPath);
                        scd.Results.Fails += 1;

                        _fLog.AddSkillEntry(_fCurrentActor.Id, e.Url.LocalPath);
                        _fLog.AddSkillChallengeEntry(_fCurrentActor.Id, false);

                        update_list();
                        update_preview_panel();
                        update_log();
                    }
                }

                if (e.Url.Scheme == "dmg")
                {
                    e.Cancel = true;

                    var id = new Guid(e.Url.LocalPath);

                    var tokens = new List<IToken>();

                    var hero = Session.Project.FindHero(id);
                    if (hero != null) tokens.Add(hero);

                    var cd = _fEncounter.FindCombatData(id);
                    if (cd != null)
                    {
                        var slot = _fEncounter.FindSlot(cd);
                        var ct = new CreatureToken(slot.Id, cd);
                        tokens.Add(ct);
                    }

                    if (tokens.Count != 0) do_damage(tokens);
                }

                if (e.Url.Scheme == "kill")
                {
                    e.Cancel = true;

                    var id = new Guid(e.Url.LocalPath);

                    var cd = _fEncounter.FindCombatData(id);
                    if (cd != null)
                    {
                        cd.Damage = 1;

                        _fLog.AddStateEntry(cd.Id, CreatureState.Defeated);

                        update_list();
                        update_preview_panel();
                        update_log();
                        update_maps();
                    }
                }

                if (e.Url.Scheme == "revive")
                {
                    e.Cancel = true;

                    var id = new Guid(e.Url.LocalPath);

                    var cd = _fEncounter.FindCombatData(id);
                    if (cd != null)
                    {
                        cd.Damage = 0;

                        _fLog.AddStateEntry(cd.Id, CreatureState.Active);

                        update_list();
                        update_preview_panel();
                        update_log();
                        update_maps();
                    }
                }

                if (e.Url.Scheme == "heal")
                {
                    e.Cancel = true;

                    var id = new Guid(e.Url.LocalPath);

                    var tokens = new List<IToken>();

                    var hero = Session.Project.FindHero(id);
                    if (hero != null) tokens.Add(hero);

                    var cd = _fEncounter.FindCombatData(id);
                    if (cd != null)
                    {
                        var slot = _fEncounter.FindSlot(cd);
                        var ct = new CreatureToken(slot.Id, cd);
                        tokens.Add(ct);
                    }

                    if (tokens.Count != 0) do_heal(tokens);
                }

                if (e.Url.Scheme == "init")
                {
                    e.Cancel = true;

                    var id = new Guid(e.Url.LocalPath);

                    var bonus = int.MinValue;
                    var cd = _fEncounter.FindCombatData(id);
                    if (cd != null)
                    {
                        var slot = _fEncounter.FindSlot(cd);
                        if (slot != null)
                            bonus = slot.Card.Initiative;
                    }

                    if (cd == null)
                    {
                        var hero = Session.Project.FindHero(id);
                        if (hero != null)
                        {
                            cd = hero.CombatData;
                            bonus = hero.InitBonus;
                        }
                    }

                    if (cd == null)
                    {
                        if (_fTrapData.ContainsKey(id))
                            cd = _fTrapData[id];

                        var trap = _fEncounter.FindTrap(id);
                        if (trap != null)
                            bonus = trap.Initiative;
                    }

                    if (cd != null && bonus != int.MinValue)
                    {
                        var dlg = new InitiativeForm(bonus, cd.Initiative);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            cd.Initiative = dlg.Score;

                            InitiativePanel.InitiativeScores = get_initiatives();

                            if (_fCurrentActor != null)
                                InitiativePanel.CurrentInitiative = _fCurrentActor.Initiative;

                            update_list();
                            update_preview_panel();
                            update_maps();
                        }
                    }
                }

                if (e.Url.Scheme == "effect")
                {
                    e.Cancel = true;

                    var strTokens = e.Url.LocalPath.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                    if (strTokens.Length == 2)
                    {
                        var id = new Guid(strTokens[0]);
                        var index = int.Parse(strTokens[1]);

                        // Find the CD we're working with
                        var cd = _fEncounter.FindCombatData(id);
                        if (cd == null)
                        {
                            var hero = Session.Project.FindHero(id);
                            if (hero != null) cd = hero.CombatData;
                        }

                        if (cd != null)
                            if (index >= 0 && index <= cd.Conditions.Count - 1)
                            {
                                var oc = cd.Conditions[index];

                                cd.Conditions.RemoveAt(index);
                                _fLog.AddEffectEntry(cd.Id, oc.ToString(_fEncounter, false), false);

                                update_list();
                                update_preview_panel();
                                update_log();
                                update_maps();
                            }
                    }
                }

                if (e.Url.Scheme == "addeffect")
                {
                    var hero = Session.Project.FindHero(_fCurrentActor.Id);
                    var index = int.Parse(e.Url.LocalPath);
                    var oc = hero.Effects[index];

                    apply_effect(oc.Copy(), SelectedTokens, false);

                    update_list();
                    update_preview_panel();
                    update_log();
                    update_maps();
                }

                if (e.Url.Scheme == "creatureinit")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "auto")
                    {
                        switch (Session.Preferences.Combat.InitiativeMode)
                        {
                            case InitiativeMode.ManualGroup:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.AutoGroup;
                                break;
                            case InitiativeMode.ManualIndividual:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.AutoIndividual;
                                break;
                        }

                        update_preview_panel();
                    }

                    if (e.Url.LocalPath == "manual")
                    {
                        switch (Session.Preferences.Combat.InitiativeMode)
                        {
                            case InitiativeMode.AutoGroup:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.ManualGroup;
                                break;
                            case InitiativeMode.AutoIndividual:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.ManualIndividual;
                                break;
                        }

                        update_preview_panel();
                    }

                    if (e.Url.LocalPath == "group")
                    {
                        switch (Session.Preferences.Combat.InitiativeMode)
                        {
                            case InitiativeMode.AutoIndividual:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.AutoGroup;
                                break;
                            case InitiativeMode.ManualIndividual:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.ManualGroup;
                                break;
                        }

                        update_preview_panel();
                    }

                    if (e.Url.LocalPath == "individual")
                    {
                        switch (Session.Preferences.Combat.InitiativeMode)
                        {
                            case InitiativeMode.AutoGroup:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.AutoIndividual;
                                break;
                            case InitiativeMode.ManualGroup:
                                Session.Preferences.Combat.InitiativeMode = InitiativeMode.ManualIndividual;
                                break;
                        }

                        update_preview_panel();
                    }
                }

                if (e.Url.Scheme == "heroinit")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "auto")
                    {
                        Session.Preferences.Combat.HeroInitiativeMode = InitiativeMode.AutoIndividual;
                        update_preview_panel();
                    }

                    if (e.Url.LocalPath == "manual")
                    {
                        Session.Preferences.Combat.HeroInitiativeMode = InitiativeMode.ManualIndividual;
                        update_preview_panel();
                    }
                }

                if (e.Url.Scheme == "trapinit")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "auto")
                    {
                        Session.Preferences.Combat.TrapInitiativeMode = InitiativeMode.AutoIndividual;
                        update_preview_panel();
                    }

                    if (e.Url.LocalPath == "manual")
                    {
                        Session.Preferences.Combat.TrapInitiativeMode = InitiativeMode.ManualIndividual;
                        update_preview_panel();
                    }
                }

                if (e.Url.Scheme == "combat")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "hp")
                    {
                        var dlg = new GroupHealthForm();
                        dlg.ShowDialog();

                        update_list();
                        update_preview_panel();
                        update_maps();
                    }

                    if (e.Url.LocalPath == "rename")
                    {
                        var list = new List<CombatData>();

                        foreach (var slot in _fEncounter.AllSlots)
                            list.AddRange(slot.CombatData);

                        // Enter display names for these combatants
                        var dlg = new DisplayNameForm(list, _fEncounter);
                        dlg.ShowDialog();

                        update_list();
                        update_preview_panel();
                        update_maps();
                    }

                    if (e.Url.LocalPath == "start") start_combat();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void InitiativePanel_InitiativeChanged(object sender, EventArgs e)
        {
            try
            {
                var previousActor = _fCurrentActor.Id;

                _fCurrentActor = null;
                _fCurrentActor = get_next_actor(null);

                if (_fCurrentActor.Id != previousActor)
                    _fLog.AddStartTurnEntry(_fCurrentActor.Id);

                update_list();
                update_log();
                update_preview_panel();
                update_maps();

                highlight_current_actor();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CombatantsEffects_Click(object sender, EventArgs e)
        {
            var dlg = new EffectListForm(_fEncounter, _fCurrentActor, _fCurrentRound);
            dlg.ShowDialog();

            update_list();
            update_preview_panel();
            update_maps();
        }

        private void CombatantsLinks_Click(object sender, EventArgs e)
        {
            var dlg = new TokenLinkListForm(MapView.TokenLinks);
            dlg.ShowDialog();

            update_list();
            update_preview_panel();
            update_maps();
        }

        private void add_in_command_clicked(object sender, EventArgs e)
        {
            try
            {
                var tsmi = sender as ToolStripMenuItem;
                var command = tsmi.Tag as ICommand;

                command.Execute();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListDelay_Click(object sender, EventArgs e)
        {
            set_delay(SelectedTokens);
        }

        private void MapDelay_Click(object sender, EventArgs e)
        {
            set_delay(MapView.SelectedTokens);
        }

        private void OptionsMapRight_Click(object sender, EventArgs e)
        {
            if (MapSplitter.Orientation == Orientation.Vertical)
                return;

            var oneCol = ListSplitter.Orientation == Orientation.Horizontal;

            MapSplitter.Orientation = Orientation.Vertical;
            MapSplitter.SplitterDistance = oneCol ? 355 : 700;
            MapSplitter.FixedPanel = FixedPanel.Panel1;

            Session.Preferences.Combat.CombatMapRight = true;
        }

        private void OptionsMapBelow_Click(object sender, EventArgs e)
        {
            if (MapSplitter.Orientation == Orientation.Horizontal)
                return;

            MapSplitter.Orientation = Orientation.Horizontal;
            MapSplitter.SplitterDistance = MapSplitter.Height / 2;
            MapSplitter.FixedPanel = FixedPanel.None;

            Session.Preferences.Combat.CombatMapRight = false;
        }

        private void OptionsLandscape_Click(object sender, EventArgs e)
        {
            SuspendLayout();

            OneColumn_Click(sender, e);
            OptionsMapRight_Click(sender, e);

            ResumeLayout();
        }

        private void OptionsPortrait_Click(object sender, EventArgs e)
        {
            SuspendLayout();

            TwoColumns_Click(sender, e);
            OptionsMapBelow_Click(sender, e);

            ResumeLayout();
        }

        private void MapDrawing_Click(object sender, EventArgs e)
        {
            MapView.AllowDrawing = !MapView.AllowDrawing;

            if (PlayerMap != null)
                PlayerMap.AllowDrawing = MapView.AllowDrawing;
        }

        private void MapClearDrawings_Click(object sender, EventArgs e)
        {
            MapView.Sketches.Clear();
            MapView.Invalidate();

            if (PlayerMap != null)
            {
                PlayerMap.Sketches.Clear();
                PlayerMap.Invalidate();
            }
        }

        private void MapView_SketchCreated(object sender, MapSketchEventArgs e)
        {
            if (PlayerMap != null)
            {
                PlayerMap.Sketches.Add(e.Sketch);
                PlayerMap.Invalidate();
            }
        }

        private void MapContextOverlay_Click(object sender, EventArgs e)
        {
            var overlay = new CustomToken();
            overlay.Name = "New Overlay";
            overlay.Type = CustomTokenType.Overlay;

            if (MapView.SelectedTokens.Count == 1)
            {
                var token = MapView.SelectedTokens[0];

                var creature = token as CreatureToken;
                if (creature != null)
                {
                    overlay.Name = "Zone: " + creature.Data.DisplayName;
                    overlay.CreatureId = creature.Data.Id;
                    overlay.Colour = Color.Red;
                }

                var hero = token as Hero;
                if (hero != null)
                {
                    overlay.Name = hero.Name + " zone";
                    overlay.CreatureId = hero.Id;
                    overlay.Colour = Color.DarkGreen;
                }
            }

            var dlg = new CustomOverlayForm(overlay);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                overlay = dlg.Token;

                if (overlay.CreatureId == Guid.Empty)
                {
                    var menuPt = new Point(MapContext.Left, MapContext.Top);
                    var mouse = MapView.PointToClient(menuPt);
                    var square = MapView.LayoutData.GetSquareAtPoint(mouse);

                    var x = square.X - (overlay.OverlaySize.Width - 1) / 2;
                    var y = square.Y - (overlay.OverlaySize.Height - 1) / 2;
                    overlay.Data.Location = new Point(x, y);
                }

                _fEncounter.CustomTokens.Add(overlay);

                update_list();
                update_maps();
            }
        }

        private void MapHeal_Click(object sender, EventArgs e)
        {
            try
            {
                do_heal(MapView.SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListHeal_Click(object sender, EventArgs e)
        {
            try
            {
                do_heal(SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListCreateCopy_Click(object sender, EventArgs e)
        {
            copy_custom_token();
        }

        private void MapCreateCopy_Click(object sender, EventArgs e)
        {
            copy_custom_token();
        }

        private void MapSetPicture_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTokens.Count != 1)
                return;

            var ct = MapView.SelectedTokens[0] as CreatureToken;
            if (ct != null)
            {
                var slot = _fEncounter.FindSlot(ct.SlotId);

                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);
                if (creature != null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filter = Program.ImageFilter;
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        creature.Image = Image.FromFile(dlg.FileName);
                        Program.SetResolution(creature.Image);

                        if (creature is Creature)
                        {
                            var c = creature as Creature;
                            var lib = Session.FindLibrary(c);
                            if (lib != null)
                            {
                                var filename = Session.GetLibraryFilename(lib);
                                Serialisation<Library>.Save(filename, lib, SerialisationMode.Binary);
                            }
                        }
                        else
                        {
                            Session.Modified = true;
                        }

                        update_list();
                    }
                }
            }

            var hero = MapView.SelectedTokens[0] as Hero;
            if (hero != null)
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = Program.ImageFilter;
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    hero.Portrait = Image.FromFile(dlg.FileName);
                    Program.SetResolution(hero.Portrait);

                    Session.Modified = true;

                    update_list();
                }
            }
        }

        private void PlayerViewNoMapShowInitiativeList_Click(object sender, EventArgs e)
        {
            try
            {
                show_player_view(false, PlayerInitiative == null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewNoMapShowLabels_Click(object sender, EventArgs e)
        {
            Session.Preferences.Combat.PlayerViewCreatureLabels = !Session.Preferences.Combat.PlayerViewCreatureLabels;

            if (PlayerInitiative != null) PlayerInitiative.DocumentText = InitiativeView();
        }

        private void TemplateList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var ct = TemplateList.SelectedItems[0].Tag as CustomToken;
            ct = ct.Copy();

            if (ct.Data.Location == CombatData.NoPoint)
                if (DoDragDrop(ct, DragDropEffects.Move) == DragDropEffects.Move)
                {
                    _fEncounter.CustomTokens.Add(ct);

                    update_list();
                    update_preview_panel();
                    update_maps();
                }
        }

        private void OptionsShowInit_Click(object sender, EventArgs e)
        {
            InitiativePanel.Visible = !InitiativePanel.Visible;
        }

        private void ListSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            list_splitter_changed();
        }

        private void ListSplitter_Resize(object sender, EventArgs e)
        {
            list_splitter_changed();
        }

        private void MapView_MouseZoomed(object sender, MouseEventArgs e)
        {
            ZoomGauge.Visible = true;
            ZoomGauge.Value -= Math.Sign(e.Delta);
            ZoomGauge_Scroll(sender, e);
        }

        private void MapView_CancelledScrolling(object sender, EventArgs e)
        {
            cancelled_scrolling();
        }

        private void CombatList_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.DrawDefault = true;
        }

        private void CombatList_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
        }

        private void CombatList_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            var text = e.Item.Selected ? SystemBrushes.HighlightText : new SolidBrush(e.Item.ForeColor);
            var bg = e.Item.Selected ? SystemBrushes.Highlight : new SolidBrush(e.Item.BackColor);

            var format = e.Header.TextAlign == HorizontalAlignment.Left ? _fLeft : _fRight;

            e.Graphics.FillRectangle(bg, e.Bounds);

            if (e.ColumnIndex == 0)
            {
                var state = CreatureState.Defeated;
                var maxValue = 0;
                var currentValue = 0;
                var tempValue = 0;

                if (e.Item.Tag is CreatureToken)
                {
                    var ct = e.Item.Tag as CreatureToken;
                    var cd = ct.Data;

                    var slot = _fEncounter.FindSlot(cd);

                    state = slot.GetState(cd);
                    maxValue = slot.Card.Hp;
                    currentValue = maxValue - cd.Damage;
                    tempValue = cd.TempHp;
                }

                if (e.Item.Tag is Hero)
                {
                    var hero = e.Item.Tag as Hero;
                    var cd = hero.CombatData;

                    state = CreatureState.Active;
                    maxValue = hero.Hp;
                    currentValue = maxValue - cd.Damage;
                    tempValue = cd.TempHp;
                }

                if (e.Item.Tag is SkillChallenge)
                {
                    var sc = e.Item.Tag as SkillChallenge;

                    if (sc.Results.Fails >= 3)
                    {
                        state = CreatureState.Bloodied;
                        currentValue = 3;
                        maxValue = 3;
                    }
                    else if (sc.Results.Successes >= sc.Successes)
                    {
                        state = CreatureState.Defeated;
                        currentValue = sc.Successes;
                        maxValue = sc.Successes;
                    }
                    else
                    {
                        state = CreatureState.Active;
                        maxValue = sc.Successes;
                        currentValue = sc.Successes - sc.Results.Successes;
                    }
                }

                if (currentValue < 0)
                    currentValue = 0;
                if (currentValue > maxValue)
                    currentValue = maxValue;

                if (maxValue > 1 && state != CreatureState.Defeated)
                {
                    var width = e.Bounds.Width - 1;
                    var height = e.Bounds.Height / 4;

                    var rect = new Rectangle(e.Bounds.X, e.Bounds.Bottom - height, width, height);

                    var c = state == CreatureState.Bloodied ? Color.Red : Color.DarkGray;
                    Brush b = new LinearGradientBrush(rect, Color.White, Color.FromArgb(10, c),
                        LinearGradientMode.Vertical);
                    e.Graphics.FillRectangle(b, rect);
                    e.Graphics.DrawRectangle(Pens.DarkGray, rect);

                    var hpWidth = width * currentValue / (maxValue + tempValue);
                    var hpRect = new Rectangle(rect.X, rect.Y, hpWidth, height);

                    Brush hpB = new LinearGradientBrush(hpRect, Color.Transparent, c, LinearGradientMode.Vertical);

                    e.Graphics.FillRectangle(hpB, hpRect);

                    if (tempValue > 0)
                    {
                        var tempWidth = width * tempValue / (maxValue + tempValue);
                        var tempRect = new Rectangle(hpRect.Right, hpRect.Y, tempWidth, height);

                        Brush tempB = new LinearGradientBrush(tempRect, Color.Transparent, Color.Blue,
                            LinearGradientMode.Vertical);

                        e.Graphics.FillRectangle(tempB, tempRect);
                    }
                }
                else
                {
                    e.Graphics.DrawLine(Pens.DarkGray, e.Bounds.Left, e.Bounds.Bottom, e.Bounds.Right, e.Bounds.Bottom);
                }
            }

            e.Graphics.DrawString(e.SubItem.Text, e.Item.Font, text, e.Bounds, format);
        }

        private void ToolsColumnsInit_Click(object sender, EventArgs e)
        {
            InitHdr.Width = InitHdr.Width > 0 ? 0 : 60;
            Session.Preferences.Combat.CombatColumnInitiative = InitHdr.Width > 0;
        }

        private void ToolsColumnsHP_Click(object sender, EventArgs e)
        {
            HPHdr.Width = HPHdr.Width > 0 ? 0 : 60;
            Session.Preferences.Combat.CombatColumnHp = HPHdr.Width > 0;
        }

        private void ToolsColumnsDefences_Click(object sender, EventArgs e)
        {
            DefHdr.Width = DefHdr.Width > 0 ? 0 : 200;
            Session.Preferences.Combat.CombatColumnDefences = DefHdr.Width > 0;
        }

        private void ToolsColumnsConditions_Click(object sender, EventArgs e)
        {
            EffectsHdr.Width = EffectsHdr.Width > 0 ? 0 : 175;
            Session.Preferences.Combat.CombatColumnEffects = EffectsHdr.Width > 0;
        }

        private void ListContext_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                var mob = false;
                var delayed = false;
                var onMap = false;
                var custom = false;

                if (SelectedTokens.Count != 0)
                {
                    mob = true;
                    delayed = true;
                    onMap = true;

                    foreach (var token in SelectedTokens)
                    {
                        var tokenIsMob = token is CreatureToken || token is Hero;
                        if (!tokenIsMob)
                            mob = false;

                        if (token is CreatureToken)
                        {
                            var ct = token as CreatureToken;

                            if (!ct.Data.Delaying)
                                delayed = false;

                            if (MapView.Map != null && ct.Data.Location == CombatData.NoPoint)
                                onMap = false;
                        }

                        if (token is Hero)
                        {
                            var hero = token as Hero;
                            var cd = hero.CombatData;

                            if (!cd.Delaying)
                                delayed = false;

                            if (MapView.Map != null && cd.Location == CombatData.NoPoint)
                                onMap = false;
                        }

                        if (token is CustomToken)
                        {
                            var ct = token as CustomToken;

                            custom = true;

                            if (MapView.Map != null && ct.Data.Location == CombatData.NoPoint)
                                onMap = false;

                            if (ct.CreatureId != Guid.Empty)
                                onMap = false;
                        }
                    }
                }

                var nonHero = false;
                foreach (var token in SelectedTokens)
                    if (!(token is Hero))
                        nonHero = true;

                ListDetails.Enabled = SelectedTokens.Count == 1;
                ListDamage.Enabled = mob;
                ListHeal.Enabled = mob;
                ListCondition.Enabled = mob;
                ListRemoveEffect.Enabled = SelectedTokens.Count == 1;
                ListRemoveMap.Enabled = onMap;
                ListRemoveCombat.Enabled = SelectedTokens.Count != 0;
                ListCreateCopy.Enabled = custom;
                ListVisible.Enabled = nonHero;

                if (ListVisible.Enabled && SelectedTokens.Count == 1)
                {
                    if (SelectedTokens[0] is CreatureToken)
                    {
                        var ct = SelectedTokens[0] as CreatureToken;
                        ListVisible.Checked = ct.Data.Visible;
                    }

                    if (SelectedTokens[0] is CustomToken)
                    {
                        var ct = SelectedTokens[0] as CustomToken;
                        ListVisible.Checked = ct.Data.Visible;
                    }
                }
                else
                {
                    ListVisible.Checked = false;
                }

                ListDelay.Enabled = mob;
                ListDelay.Checked = delayed;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListDetails_Click(object sender, EventArgs e)
        {
            try
            {
                edit_token(SelectedTokens[0]);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListDamage_Click(object sender, EventArgs e)
        {
            try
            {
                do_damage(SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ListRemoveMap_Click(object sender, EventArgs e)
        {
            remove_from_map(SelectedTokens);
        }

        private void ListRemoveCombat_Click(object sender, EventArgs e)
        {
            remove_from_combat(SelectedTokens);
        }

        private void ListVisible_Click(object sender, EventArgs e)
        {
            toggle_visibility(SelectedTokens);
        }

        private void MapContext_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                var mob = false;
                var delayed = false;
                var onMap = false;
                var custom = false;

                if (MapView.SelectedTokens.Count != 0)
                {
                    mob = true;
                    delayed = true;
                    onMap = true;

                    foreach (var token in MapView.SelectedTokens)
                    {
                        var tokenIsMob = token is CreatureToken || token is Hero;
                        if (!tokenIsMob)
                            mob = false;

                        if (token is CreatureToken)
                        {
                            var ct = token as CreatureToken;

                            if (!ct.Data.Delaying)
                                delayed = false;

                            if (ct.Data.Location == CombatData.NoPoint)
                                onMap = false;
                        }

                        if (token is Hero)
                        {
                            var hero = token as Hero;
                            var cd = hero.CombatData;

                            if (!cd.Delaying)
                                delayed = false;

                            if (cd.Location == CombatData.NoPoint)
                                onMap = false;
                        }

                        if (token is CustomToken)
                        {
                            var ct = token as CustomToken;

                            custom = true;

                            if (ct.Data.Location == CombatData.NoPoint)
                                onMap = false;
                        }
                    }
                }

                var nonHero = false;
                foreach (var token in MapView.SelectedTokens)
                    if (!(token is Hero))
                        nonHero = true;

                MapDetails.Enabled = MapView.SelectedTokens.Count == 1;
                MapDamage.Enabled = mob;
                MapHeal.Enabled = mob;
                MapAddEffect.Enabled = mob;
                MapRemoveEffect.Enabled = mob;
                MapRemoveMap.Enabled = onMap;
                MapRemoveCombat.Enabled = MapView.SelectedTokens.Count != 0;
                MapCreateCopy.Enabled = custom;
                MapVisible.Enabled = nonHero;

                if (MapVisible.Enabled && MapView.SelectedTokens.Count == 1)
                {
                    if (MapView.SelectedTokens[0] is CreatureToken)
                    {
                        var ct = MapView.SelectedTokens[0] as CreatureToken;
                        MapVisible.Checked = ct.Data.Visible;
                    }

                    if (MapView.SelectedTokens[0] is CustomToken)
                    {
                        var ct = MapView.SelectedTokens[0] as CustomToken;
                        MapVisible.Checked = ct.Data.Visible;
                    }
                }
                else
                {
                    MapVisible.Checked = false;
                }

                MapDelay.Enabled = mob;
                MapDelay.Checked = delayed;

                MapContextDrawing.Checked = MapView.AllowDrawing;
                MapContextClearDrawings.Enabled = MapView.Sketches.Count != 0;

                MapContextLOS.Checked = MapView.LineOfSight;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapDetails_Click(object sender, EventArgs e)
        {
            try
            {
                if (MapView.SelectedTokens.Count == 0)
                    return;

                edit_token(MapView.SelectedTokens[0]);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapDamage_Click(object sender, EventArgs e)
        {
            try
            {
                do_damage(MapView.SelectedTokens);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapRemoveMap_Click(object sender, EventArgs e)
        {
            remove_from_map(MapView.SelectedTokens);
        }

        private void MapRemoveCombat_Click(object sender, EventArgs e)
        {
            remove_from_combat(MapView.SelectedTokens);
        }

        private void MapVisible_Click(object sender, EventArgs e)
        {
            toggle_visibility(MapView.SelectedTokens);
        }

        private void CombatantsBtn_DropDownOpening(object sender, EventArgs e)
        {
            CombatantsAddToken.Visible = _fEncounter.MapId != Guid.Empty;
            CombatantsAddOverlay.Visible = _fEncounter.MapId != Guid.Empty;
            CombatantsRemove.Enabled = SelectedTokens.Count != 0;

            CombatantsWaves.DropDownItems.Clear();

            foreach (var ew in _fEncounter.Waves)
            {
                if (ew.Count == 0)
                    continue;

                var tsmi = new ToolStripMenuItem(ew.Name);
                tsmi.Checked = ew.Active;
                tsmi.Tag = ew;
                tsmi.Click += wave_activated;
                CombatantsWaves.DropDownItems.Add(tsmi);
            }

            if (CombatantsWaves.DropDownItems.Count == 0)
            {
                var tsmi = new ToolStripMenuItem("(none set)");
                tsmi.Enabled = false;
                CombatantsWaves.DropDownItems.Add(tsmi);
            }
        }

        private void wave_activated(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            var ew = tsmi.Tag as EncounterWave;
            ew.Active = !ew.Active;

            update_list();
            update_maps();
            update_statusbar();
        }

        private void MapMenu_DropDownOpening(object sender, EventArgs e)
        {
            ShowMap.Checked = !MapSplitter.Panel2Collapsed;
            MapLOS.Checked = MapView.LineOfSight;
            MapGrid.Checked = MapView.ShowGrid != MapGridMode.None;
            MapGridLabels.Checked = MapView.ShowGridLabels;
            MapHealth.Checked = MapView.ShowHealthBars;
            MapConditions.Checked = MapView.ShowConditions;
            MapPictureTokens.Checked = MapView.ShowPictureTokens;
            MapNavigate.Checked = MapView.AllowScrolling;

            MapFogAllCreatures.Checked = MapView.ShowCreatures == CreatureViewMode.All;
            MapFogVisibleCreatures.Checked = MapView.ShowCreatures == CreatureViewMode.Visible;
            MapFogHideCreatures.Checked = MapView.ShowCreatures == CreatureViewMode.None;

            MapDrawing.Checked = MapView.AllowDrawing;
            MapClearDrawings.Enabled = MapView.Sketches.Count != 0;
        }

        private void PlayerViewMapMenu_DropDownOpening(object sender, EventArgs e)
        {
            PlayerViewMap.Checked = PlayerMap != null;
            PlayerViewInitList.Checked = PlayerInitiative != null;

            PlayerViewLOS.Enabled = PlayerMap != null;
            PlayerViewLOS.Checked = PlayerMap != null && PlayerMap.LineOfSight;
            PlayerViewGrid.Enabled = PlayerMap != null;
            PlayerViewGrid.Checked = PlayerMap != null && PlayerMap.ShowGrid != MapGridMode.None;
            PlayerViewGridLabels.Enabled = PlayerMap != null;
            PlayerViewGridLabels.Checked = PlayerMap != null && PlayerMap.ShowGridLabels;
            PlayerHealth.Enabled = PlayerMap != null;
            PlayerHealth.Checked = PlayerMap != null && PlayerMap.ShowHealthBars;
            PlayerConditions.Enabled = PlayerMap != null;
            PlayerConditions.Checked = PlayerMap != null && PlayerMap.ShowConditions;
            PlayerPictureTokens.Enabled = PlayerMap != null;
            PlayerPictureTokens.Checked = PlayerMap != null && PlayerMap.ShowPictureTokens;
            PlayerLabels.Enabled = PlayerMap != null || PlayerInitiative != null;
            PlayerLabels.Checked = PlayerMap != null && PlayerMap.ShowCreatureLabels ||
                                   PlayerInitiative != null && Session.Preferences.Combat.PlayerViewCreatureLabels;

            PlayerViewFog.Enabled = PlayerMap != null;
            PlayerFogAll.Checked = PlayerMap != null && PlayerMap.ShowCreatures == CreatureViewMode.All;
            PlayerFogVisible.Checked = PlayerMap != null && PlayerMap.ShowCreatures == CreatureViewMode.Visible;
            PlayerFogNone.Checked = PlayerMap != null && PlayerMap.ShowCreatures == CreatureViewMode.None;
        }

        private void ToolsMenu_DopDownOpening(object sender, EventArgs e)
        {
            ToolsAddIns.DropDownItems.Clear();
            foreach (var addin in Session.AddIns)
            {
                var addinItem = new ToolStripMenuItem(addin.Name);
                addinItem.ToolTipText = TextHelper.Wrap(addin.Description);
                addinItem.Tag = addin;

                ToolsAddIns.DropDownItems.Add(addinItem);

                foreach (var command in addin.CombatCommands)
                {
                    var commandItem = new ToolStripMenuItem(command.Name);
                    commandItem.ToolTipText = TextHelper.Wrap(command.Description);
                    commandItem.Enabled = command.Available;
                    commandItem.Checked = command.Active;
                    commandItem.Click += add_in_command_clicked;
                    commandItem.Tag = command;

                    addinItem.DropDownItems.Add(commandItem);
                }

                if (addin.Commands.Count == 0)
                {
                    var commandItem = ToolsAddIns.DropDownItems.Add("(no commands)");
                    commandItem.Enabled = false;
                }
            }

            if (Session.AddIns.Count == 0)
            {
                var addinItem = ToolsAddIns.DropDownItems.Add("(none)");
                addinItem.Enabled = false;
            }
        }

        private void OptionsMenu_DropDownOpening(object sender, EventArgs e)
        {
            var showMap = !MapSplitter.Panel2Collapsed;

            OptionsShowInit.Checked = InitiativePanel.Visible;

            OneColumn.Checked = ListSplitter.Orientation == Orientation.Horizontal;
            TwoColumns.Checked = ListSplitter.Orientation == Orientation.Vertical;
            OneColumn.Enabled = showMap;
            TwoColumns.Enabled = showMap;

            MapRight.Enabled = showMap;
            MapBelow.Enabled = showMap;
            MapRight.Checked = MapSplitter.Orientation == Orientation.Vertical;
            MapBelow.Checked = MapSplitter.Orientation == Orientation.Horizontal;

            OptionsLandscape.Enabled = showMap;
            OptionsPortrait.Enabled = showMap;
            OptionsLandscape.Checked = OneColumn.Checked && MapRight.Checked;
            OptionsPortrait.Checked = TwoColumns.Checked && MapBelow.Checked;

            ToolsAutoRemove.Checked = Session.Preferences.Combat.CreatureAutoRemove;
        }

        private void EffectMenu_DropDownOpening(object sender, EventArgs e)
        {
            update_effects_list(EffectMenu, true);
        }

        private void ListCondition_DropDownOpening(object sender, EventArgs e)
        {
            update_effects_list(ListCondition, true);
        }

        private void MapCondition_DropDownOpening(object sender, EventArgs e)
        {
            update_effects_list(MapAddEffect, false);
        }

        private void ListRemoveEffect_DropDownOpening(object sender, EventArgs e)
        {
            update_remove_effect_list(ListRemoveEffect, true);
        }

        private void MapRemoveEffect_DropDownOpening(object sender, EventArgs e)
        {
            update_remove_effect_list(MapRemoveEffect, false);
        }

        private void PlayerViewNoMapMenu_DropDownOpening(object sender, EventArgs e)
        {
            PlayerViewNoMapShowInitiativeList.Checked = PlayerInitiative != null;

            PlayerViewNoMapShowLabels.Enabled = PlayerInitiative != null;
            PlayerViewNoMapShowLabels.Checked = Session.Preferences.Combat.PlayerViewCreatureLabels;
        }

        private void ToolsColumns_DropDownOpening(object sender, EventArgs e)
        {
            ToolsColumnsInit.Checked = InitHdr.Width > 0;
            ToolsColumnsHP.Checked = HPHdr.Width > 0;
            ToolsColumnsDefences.Checked = DefHdr.Width > 0;
            ToolsColumnsConditions.Checked = EffectsHdr.Width > 0;
        }

        private void CloseBtn_Click(object sender, EventArgs e)
        {
            _fPromptOnClose = false;
            Close();
        }

        private void PauseBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Ask to save encounter

                var msg = "Would you like to be able to resume this encounter later?";
                msg += Environment.NewLine;
                msg += Environment.NewLine;
                msg +=
                    "If you click Yes, the encounter can be restarted by selecting Paused Encounters from the Project menu.";

                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.Yes)
                {
                    var cs = new CombatState();

                    _fLog.AddPauseEntry();

                    var heroData = new Dictionary<Guid, CombatData>();
                    foreach (var hero in Session.Project.Heroes)
                        heroData[hero.Id] = hero.CombatData;

                    cs.Encounter = _fEncounter;
                    cs.CurrentRound = _fCurrentRound;
                    cs.PartyLevel = _fPartyLevel;
                    cs.HeroData = heroData;
                    cs.TrapData = _fTrapData;
                    cs.TokenLinks = MapView.TokenLinks;
                    cs.RemovedCreatureXp = _fRemovedCreatureXp;
                    cs.Viewpoint = MapView.Viewpoint;
                    cs.Log = _fLog;

                    if (_fCurrentActor != null)
                        cs.CurrentActor = _fCurrentActor.Id;

                    foreach (var sketch in MapView.Sketches)
                        cs.Sketches.Add(sketch.Copy());

                    foreach (var oc in _fEffects)
                        cs.QuickEffects.Add(oc.Copy());

                    Session.Project.SavedCombats.Add(cs);
                    Session.Modified = true;

                    foreach (Form form in Application.OpenForms)
                    {
                        var dlg = form as PausedCombatListForm;
                        dlg?.UpdateEncounters();
                    }

                    _fPromptOnClose = false;
                    Close();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void InfoBtn_Click(object sender, EventArgs e)
        {
            var dlg = new InfoForm();
            dlg.Level = _fPartyLevel;
            dlg.ShowDialog();
        }

        private void DieRollerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new DieRollerForm();
            dlg.ShowDialog();
        }

        private void ReportBtn_Click(object sender, EventArgs e)
        {
            var dlg = new EncounterReportForm(_fLog, _fEncounter);
            dlg.ShowDialog();
        }

        private void start_combat()
        {
            roll_initiative();

            // Set the first actor
            var scores = get_initiatives();
            if (scores.Count != 0)
            {
                var maxInit = scores[0];
                var combatants = get_combatants(maxInit, false);
                if (combatants.Count != 0)
                    _fCurrentActor = combatants[0];

                if (_fCurrentActor != null)
                {
                    _fCombatStarted = true;

                    InitiativePanel.CurrentInitiative = _fCurrentActor.Initiative;
                    select_current_actor();

                    update_list();
                    update_maps();
                    update_statusbar();
                    update_preview_panel();

                    highlight_current_actor();

                    _fLog.Active = true;
                    _fLog.AddStartRoundEntry(_fCurrentRound);
                    _fLog.AddStartTurnEntry(_fCurrentActor.Id);
                    update_log();
                }
            }
        }

        private void roll_initiative()
        {
            var toRoll = new List<Pair<List<CombatData>, int>>();
            var toEnter = new Dictionary<string, List<CombatData>>();

            foreach (var hero in Session.Project.Heroes)
            {
                if (hero.CombatData.Initiative != int.MinValue)
                    continue;

                switch (Session.Preferences.Combat.HeroInitiativeMode)
                {
                    case InitiativeMode.AutoIndividual:
                        var list = new List<CombatData>();
                        list.Add(hero.CombatData);
                        toRoll.Add(new Pair<List<CombatData>, int>(list, hero.InitBonus));
                        break;
                    case InitiativeMode.ManualIndividual:
                        toEnter[hero.Name] = new List<CombatData>();
                        toEnter[hero.Name].Add(hero.CombatData);
                        break;
                }
            }

            foreach (var slot in _fEncounter.Slots)
                switch (Session.Preferences.Combat.InitiativeMode)
                {
                    case InitiativeMode.AutoGroup:
                    {
                        var list = new List<CombatData>();
                        foreach (var cd in slot.CombatData)
                        {
                            if (cd.Initiative != int.MinValue)
                                continue;

                            list.Add(cd);
                        }

                        if (list.Count != 0)
                            toRoll.Add(new Pair<List<CombatData>, int>(list, slot.Card.Initiative));
                    }
                        break;
                    case InitiativeMode.AutoIndividual:
                        foreach (var cd in slot.CombatData)
                        {
                            if (cd.Initiative != int.MinValue)
                                continue;

                            var list = new List<CombatData>();
                            list.Add(cd);
                            toRoll.Add(new Pair<List<CombatData>, int>(list, slot.Card.Initiative));
                        }

                        break;
                    case InitiativeMode.ManualGroup:
                    {
                        var list = new List<CombatData>();
                        foreach (var cd in slot.CombatData)
                        {
                            if (cd.Initiative != int.MinValue)
                                continue;

                            list.Add(cd);
                        }

                        if (list.Count != 0)
                            toEnter[slot.Card.Title] = list;
                    }
                        break;
                    case InitiativeMode.ManualIndividual:
                        foreach (var cd in slot.CombatData)
                        {
                            if (cd.Initiative != int.MinValue)
                                continue;

                            toEnter[cd.DisplayName] = new List<CombatData>();
                            toEnter[cd.DisplayName].Add(cd);
                        }

                        break;
                }

            foreach (var trap in _fEncounter.Traps)
            {
                var hasInit = trap.Initiative != int.MinValue;
                if (!hasInit)
                    continue;

                var cd = _fTrapData[trap.Id];
                if (cd.Initiative != int.MinValue)
                    continue;

                switch (Session.Preferences.Combat.TrapInitiativeMode)
                {
                    case InitiativeMode.AutoIndividual:
                        var list = new List<CombatData>();
                        list.Add(cd);
                        toRoll.Add(new Pair<List<CombatData>, int>(list, trap.Initiative));
                        break;
                    case InitiativeMode.ManualIndividual:
                        toEnter[trap.Name] = new List<CombatData>();
                        toEnter[trap.Name].Add(cd);
                        break;
                }
            }

            // Roll
            foreach (var item in toRoll)
            {
                var roll = Session.Dice(1, 20) + item.Second;
                foreach (var cd in item.First)
                    cd.Initiative = roll;
            }

            // Enter
            if (toEnter.Count != 0)
            {
                var dlg = new GroupInitiativeForm(toEnter, _fEncounter);
                dlg.ShowDialog();
            }

            InitiativePanel.InitiativeScores = get_initiatives();
        }

        private void select_current_actor()
        {
            foreach (ListViewItem lvi in CombatList.Items)
                lvi.Selected = false;

            var currentLvi = get_combatant(_fCurrentActor.Id);
            if (currentLvi != null)
                currentLvi.Selected = true;
        }

        private void set_map(List<TokenLink> tokenLinks, Rectangle viewpoint, List<MapSketch> sketches)
        {
            var m = Session.Project.FindTacticalMap(_fEncounter.MapId);
            MapView.Map = m;

            if (tokenLinks != null)
            {
                MapView.TokenLinks = tokenLinks;

                // Update token links to point to the new CreatureToken items
                foreach (var link in MapView.TokenLinks)
                foreach (var token in link.Tokens)
                {
                    var ct = token as CreatureToken;
                    if (ct != null)
                    {
                        var slot = _fEncounter.FindSlot(ct.SlotId);
                        if (slot != null)
                            ct.Data = slot.FindCombatData(ct.Data.Location);
                    }
                }
            }
            else
            {
                MapView.TokenLinks = new List<TokenLink>();
            }

            if (viewpoint != Rectangle.Empty)
            {
                MapView.Viewpoint = viewpoint;
            }
            else
            {
                if (_fEncounter.MapAreaId != Guid.Empty)
                {
                    var area = m.FindArea(_fEncounter.MapAreaId);
                    if (area != null)
                        MapView.Viewpoint = area.Region;
                }
            }

            foreach (var sketch in sketches)
                MapView.Sketches.Add(sketch.Copy());

            MapView.Encounter = _fEncounter;

            MapView.ShowHealthBars = Session.Preferences.Combat.CombatHealthBars;
            MapView.ShowCreatures = Session.Preferences.Combat.CombatFog;
            MapView.ShowPictureTokens = Session.Preferences.Combat.CombatPictureTokens;
            MapView.ShowGrid = Session.Preferences.Combat.CombatGrid ? MapGridMode.Overlay : MapGridMode.None;
            MapView.ShowGridLabels = Session.Preferences.Combat.CombatGridLabels;

            if (_fEncounter.MapId == Guid.Empty)
            {
                MapSplitter.Panel2Collapsed = true;
                CombatList.Groups[5].Header = "Non-Combatants";
            }
            else
            {
                if (!Session.Preferences.Combat.CombatMapRight)
                    OptionsMapBelow_Click(null, null);
            }

            if (_fEncounter.MapId != Guid.Empty && Session.Preferences.Combat.CombatTwoColumns)
                TwoColumns_Click(null, null);
            if (_fEncounter.MapId == Guid.Empty && Session.Preferences.Combat.CombatTwoColumnsNoMap)
                TwoColumns_Click(null, null);
        }

        private void do_damage(List<IToken> tokens)
        {
            var list = new List<Pair<CombatData, EncounterCard>>();
            foreach (var token in tokens)
            {
                CombatData cd = null;
                EncounterCard card = null;

                if (token is CreatureToken)
                {
                    var ct = token as CreatureToken;
                    cd = ct.Data;

                    var slot = _fEncounter.FindSlot(ct.SlotId);
                    card = slot.Card;
                }

                if (token is Hero)
                {
                    var h = token as Hero;
                    cd = h.CombatData;
                }

                list.Add(new Pair<CombatData, EncounterCard>(cd, card));
            }

            // Remember their starting HP / state
            var hpDic = new Dictionary<CombatData, int>();
            var stateDic = new Dictionary<CombatData, CreatureState>();
            foreach (var pair in list)
                hpDic[pair.First] = pair.First.Damage;
            foreach (var pair in list)
                stateDic[pair.First] = get_state(pair.First);

            var dlg = new DamageForm(list, 0);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var pair in list)
                {
                    // Has HP changed?
                    var hp = pair.First.Damage - hpDic[pair.First];
                    if (hp != 0)
                        _fLog.AddDamageEntry(pair.First.Id, hp, dlg.Types);

                    // Has state changed?
                    var newState = get_state(pair.First);
                    if (newState != stateDic[pair.First])
                        _fLog.AddStateEntry(pair.First.Id, newState);
                }

                update_list();
                update_log();
                update_preview_panel();
                update_maps();
            }
        }

        private void do_heal(List<IToken> tokens)
        {
            var list = new List<Pair<CombatData, EncounterCard>>();
            foreach (var token in tokens)
            {
                CombatData cd = null;
                EncounterCard card = null;

                if (token is CreatureToken)
                {
                    var ct = token as CreatureToken;
                    cd = ct.Data;

                    var slot = _fEncounter.FindSlot(ct.SlotId);
                    card = slot.Card;
                }

                if (token is Hero)
                {
                    var h = token as Hero;
                    cd = h.CombatData;
                }

                list.Add(new Pair<CombatData, EncounterCard>(cd, card));
            }

            // Remember their starting HP / state
            var hpDic = new Dictionary<CombatData, int>();
            var stateDic = new Dictionary<CombatData, CreatureState>();
            foreach (var pair in list)
                hpDic[pair.First] = pair.First.Damage;
            foreach (var pair in list)
                stateDic[pair.First] = get_state(pair.First);

            var dlg = new HealForm(list);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var pair in list)
                {
                    // Has HP changed?
                    var hp = pair.First.Damage - hpDic[pair.First];
                    if (hp != 0)
                        _fLog.AddDamageEntry(pair.First.Id, hp, null);

                    // Has state changed?
                    var newState = get_state(pair.First);
                    if (newState != stateDic[pair.First])
                        _fLog.AddStateEntry(pair.First.Id, newState);
                }

                update_list();
                update_log();
                update_preview_panel();
                update_maps();
            }
        }

        private void copy_custom_token()
        {
            foreach (var token in SelectedTokens)
                if (token is CustomToken)
                {
                    var ct = token as CustomToken;

                    var copy = ct.Copy();
                    copy.Id = Guid.NewGuid();
                    copy.Data.Location = CombatData.NoPoint;

                    _fEncounter.CustomTokens.Add(copy);
                }

            update_list();
        }

        private void show_player_view(bool map, bool initiative)
        {
            try
            {
                if (!map && !initiative)
                {
                    // Turn it off
                    Session.PlayerView.ShowDefault();
                }
                else
                {
                    // Do we have a player view already?
                    if (Session.PlayerView == null)
                        Session.PlayerView = new PlayerViewForm(this);

                    if (PlayerMap == null && PlayerInitiative == null)
                        // We're not showing anything; turn it on
                        Session.PlayerView.ShowTacticalMap(MapView, InitiativeView());

                    var splitter = Session.PlayerView.Controls[0] as SplitContainer;
                    if (splitter != null)
                    {
                        splitter.Panel1Collapsed = !map;
                        splitter.Panel2Collapsed = !initiative;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void list_splitter_changed()
        {
            try
            {
                if (Visible) update_preview_panel();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void select_token(IToken token)
        {
            foreach (ListViewItem lvi in CombatList.Items)
            {
                if (token is CreatureToken && lvi.Tag is CreatureToken)
                {
                    var ct1 = token as CreatureToken;
                    var ct2 = lvi.Tag as CreatureToken;

                    if (ct1.Data == ct2.Data)
                        lvi.Selected = true;
                }

                if (token is CustomToken && lvi.Tag is CustomToken)
                {
                    var ct1 = token as CustomToken;
                    var ct2 = lvi.Tag as CustomToken;

                    if (ct1.Data == ct2.Data)
                        lvi.Selected = true;
                }

                if (token is Hero && lvi.Tag is Hero)
                {
                    var h1 = token as Hero;
                    var h2 = lvi.Tag as Hero;

                    if (h1 == h2)
                        lvi.Selected = true;
                }
            }
        }

        private void set_delay(List<IToken> tokens)
        {
            try
            {
                foreach (var token in tokens)
                {
                    CombatData cd = null;

                    if (token is CreatureToken)
                    {
                        var ct = token as CreatureToken;
                        cd = ct.Data;
                    }

                    if (token is Hero)
                    {
                        var hero = token as Hero;
                        cd = hero.CombatData;
                    }

                    if (cd != null)
                    {
                        cd.Delaying = !cd.Delaying;

                        if (cd.Delaying)
                            InitiativePanel.InitiativeScores = get_initiatives();
                        else
                            cd.Initiative = InitiativePanel.CurrentInitiative;
                    }
                }

                update_list();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private string get_info(CreatureToken token)
        {
            var str = "";

            var slot = _fEncounter.FindSlot(token.SlotId);
            var content = slot.Card.AsText(token.Data, CardMode.Text, true);
            foreach (var line in content)
            {
                if (str != "")
                    str += Environment.NewLine;

                str += line;
            }

            if (token.Data.Conditions.Count != 0)
            {
                str += Environment.NewLine;

                foreach (var oc in token.Data.Conditions)
                {
                    str += Environment.NewLine;
                    str += oc.ToString(_fEncounter, false);
                }
            }

            return str;
        }

        private string get_info(Hero hero)
        {
            var str = hero.Race + " " + hero.Class;
            if (hero.Player != "")
            {
                str += Environment.NewLine;
                str += "Player: " + hero.Player;
            }

            var cd = hero.CombatData;
            if (cd != null)
                if (cd.Conditions.Count != 0)
                {
                    str += Environment.NewLine;

                    foreach (var oc in cd.Conditions)
                    {
                        str += Environment.NewLine;
                        str += oc.ToString(_fEncounter, false);
                    }
                }

            return str;
        }

        private string get_info(CustomToken token)
        {
            return token.Details != "" ? token.Details : "(no details)";
        }

        private void edit_token(IToken token)
        {
            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                var slot = _fEncounter.FindSlot(ct.SlotId);

                var index = slot.CombatData.IndexOf(ct.Data);

                var dmg = ct.Data.Damage;
                var state = slot.GetState(ct.Data);

                var previousConditions = new List<string>();
                foreach (var oc in ct.Data.Conditions)
                    previousConditions.Add(oc.ToString(_fEncounter, false));

                var dlg = new CombatDataForm(ct.Data, slot.Card, _fEncounter, _fCurrentActor, _fCurrentRound, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    slot.CombatData[index] = dlg.Data;

                    if (dmg != dlg.Data.Damage)
                    {
                        dmg = dlg.Data.Damage - dmg;
                        _fLog.AddDamageEntry(dlg.Data.Id, dmg, null);
                    }

                    if (slot.GetState(dlg.Data) != state)
                    {
                        state = slot.GetState(dlg.Data);
                        _fLog.AddStateEntry(dlg.Data.Id, state);
                    }

                    // Look for new / removed conditions
                    var newConditions = new List<string>();
                    foreach (var oc in dlg.Data.Conditions)
                        newConditions.Add(oc.ToString(_fEncounter, false));
                    foreach (var str in previousConditions)
                        if (!newConditions.Contains(str))
                            _fLog.AddEffectEntry(dlg.Data.Id, str, false);
                    foreach (var str in newConditions)
                        if (!previousConditions.Contains(str))
                            _fLog.AddEffectEntry(dlg.Data.Id, str, true);

                    update_list();
                    update_log();
                    update_preview_panel();
                    update_maps();

                    InitiativePanel.InitiativeScores = get_initiatives();
                }
            }

            if (token is Hero)
            {
                var h = token as Hero;

                if (h.CombatData.Initiative == int.MinValue)
                {
                    edit_initiative(h);
                }
                else
                {
                    var cd = h.CombatData;

                    var dmg = cd.Damage;
                    var state = h.GetState(cd.Damage);

                    var previousConditions = new List<string>();
                    foreach (var oc in cd.Conditions)
                        previousConditions.Add(oc.ToString(_fEncounter, false));

                    var dlg = new CombatDataForm(cd, null, _fEncounter, _fCurrentActor, _fCurrentRound, false);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        h.CombatData = dlg.Data;

                        if (dmg != dlg.Data.Damage)
                        {
                            dmg = dlg.Data.Damage - dmg;
                            _fLog.AddDamageEntry(dlg.Data.Id, dmg, null);
                        }

                        if (h.GetState(dlg.Data.Damage) != state)
                        {
                            state = h.GetState(dlg.Data.Damage);
                            _fLog.AddStateEntry(dlg.Data.Id, state);
                        }

                        // Look for new / removed conditions
                        var newConditions = new List<string>();
                        foreach (var oc in dlg.Data.Conditions)
                            newConditions.Add(oc.ToString(_fEncounter, false));
                        foreach (var str in previousConditions)
                            if (!newConditions.Contains(str))
                                _fLog.AddEffectEntry(dlg.Data.Id, str, false);
                        foreach (var str in newConditions)
                            if (!previousConditions.Contains(str))
                                _fLog.AddEffectEntry(dlg.Data.Id, str, true);

                        update_list();
                        update_log();
                        update_preview_panel();
                        update_maps();

                        InitiativePanel.InitiativeScores = get_initiatives();
                    }
                }
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;
                var index = _fEncounter.CustomTokens.IndexOf(ct);
                if (index != -1)
                    switch (ct.Type)
                    {
                        case CustomTokenType.Token:
                        {
                            var dlg = new CustomTokenForm(ct);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                _fEncounter.CustomTokens[index] = dlg.Token;

                                update_list();
                                update_preview_panel();
                                update_maps();
                            }
                        }
                            break;
                        case CustomTokenType.Overlay:
                        {
                            var dlg = new CustomOverlayForm(ct);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                _fEncounter.CustomTokens[index] = dlg.Token;

                                update_list();
                                update_preview_panel();
                                update_maps();
                            }
                        }
                            break;
                    }
            }
        }

        private void set_tooltip(IToken token, Control ctrl)
        {
            var title = "";
            string info = null;

            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;

                title = ct.Data.DisplayName;
                info = get_info(ct);
            }

            if (token is Hero)
            {
                var hero = token as Hero;

                title = hero.Name;
                info = get_info(hero);
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;

                title = ct.Name;
                info = get_info(ct);
            }

            MapTooltip.ToolTipTitle = title;
            MapTooltip.SetToolTip(ctrl, info);
        }

        private void remove_from_map(List<IToken> tokens)
        {
            try
            {
                foreach (var token in tokens)
                {
                    if (token is CreatureToken)
                    {
                        var ct = token as CreatureToken;
                        ct.Data.Location = CombatData.NoPoint;

                        remove_effects(token);
                        remove_links(token);
                    }

                    if (token is Hero)
                    {
                        var hero = token as Hero;
                        hero.CombatData.Location = CombatData.NoPoint;

                        remove_effects(token);
                        remove_links(token);
                    }

                    if (token is CustomToken)
                    {
                        var ct = token as CustomToken;
                        ct.Data.Location = CombatData.NoPoint;

                        if (ct.Type == CustomTokenType.Token)
                            remove_links(token);
                    }
                }

                update_list();
                update_preview_panel();
                update_maps();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void remove_from_combat(List<IToken> tokens)
        {
            try
            {
                foreach (var token in tokens)
                {
                    if (token is CreatureToken)
                    {
                        var ct = token as CreatureToken;

                        var slot = _fEncounter.FindSlot(ct.SlotId);
                        slot.CombatData.Remove(ct.Data);

                        _fRemovedCreatureXp += slot.Card.Xp;

                        remove_effects(token);
                        remove_links(token);
                    }

                    if (token is Hero)
                    {
                        var h = token as Hero;

                        h.CombatData.Initiative = int.MinValue;
                        h.CombatData.Location = CombatData.NoPoint;

                        remove_effects(token);
                        remove_links(token);
                    }

                    if (token is CustomToken)
                    {
                        var ct = token as CustomToken;

                        _fEncounter.CustomTokens.Remove(ct);

                        if (ct.Type == CustomTokenType.Token)
                            remove_links(token);
                    }
                }

                update_list();
                update_preview_panel();
                update_maps();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void remove_effects(IToken token)
        {
            var tokenId = Guid.Empty;

            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                tokenId = ct.Data.Id;
            }

            if (token is Hero)
            {
                var hero = token as Hero;
                tokenId = hero.Id;
            }

            if (tokenId == Guid.Empty)
                return;

            foreach (var hero in Session.Project.Heroes)
            {
                var cd = hero.CombatData;
                remove_effects(tokenId, cd);
            }

            foreach (var slot in _fEncounter.AllSlots)
            foreach (var cd in slot.CombatData)
                remove_effects(tokenId, cd);
        }

        private void remove_effects(Guid tokenId, CombatData data)
        {
            var obsolete = new List<OngoingCondition>();
            foreach (var oc in data.Conditions)
            {
                if (oc.DurationCreatureId != tokenId)
                    continue;

                if (oc.Duration == DurationType.BeginningOfTurn || oc.Duration == DurationType.EndOfTurn)
                    obsolete.Add(oc);
            }

            foreach (var oc in obsolete)
                data.Conditions.Remove(oc);
        }

        private void remove_links(IToken token)
        {
            var location = get_location(token);
            var obsolete = new List<TokenLink>();

            foreach (var tl in MapView.TokenLinks)
            foreach (var t in tl.Tokens)
                if (get_location(t) == location)
                {
                    obsolete.Add(tl);
                    break;
                }

            foreach (var tl in obsolete)
                MapView.TokenLinks.Remove(tl);

            update_maps();
        }

        private Point get_location(IToken token)
        {
            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                return ct.Data.Location;
            }

            if (token is Hero)
            {
                var h = token as Hero;
                return h.CombatData.Location;
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;
                return ct.Data.Location;
            }

            return CombatData.NoPoint;
        }

        private void toggle_visibility(List<IToken> tokens)
        {
            try
            {
                foreach (var token in tokens)
                {
                    if (token is CreatureToken)
                    {
                        var ct = token as CreatureToken;
                        ct.Data.Visible = !ct.Data.Visible;
                    }

                    if (token is CustomToken)
                    {
                        var ct = token as CustomToken;
                        ct.Data.Visible = !ct.Data.Visible;
                    }
                }

                update_list();
                update_preview_panel();
                update_maps();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void show_or_hide_all(bool visible)
        {
            foreach (var slot in _fEncounter.AllSlots)
            foreach (var cd in slot.CombatData)
                cd.Visible = visible;

            foreach (var ct in _fEncounter.CustomTokens) ct.Data.Visible = visible;

            update_list();
            update_preview_panel();
            update_maps();
        }

        private void roll_attack(CreaturePower power)
        {
            var dlg = new AttackRollForm(power, _fEncounter);
            dlg.ShowDialog();

            update_list();
            update_log();
            update_preview_panel();
            update_maps();
        }

        private void roll_check(string name, int mod)
        {
            var roll = Session.Dice(1, 20);
            var result = roll + mod;

            var rollStr = roll.ToString();
            if (roll == 1 || roll == 20)
                rollStr = "Natural " + rollStr;

            var str = "Bonus:\t" + mod + Environment.NewLine + "Roll:\t" + rollStr + Environment.NewLine +
                      Environment.NewLine + "Result:\t" + result;
            MessageBox.Show(str, name, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private bool edit_initiative(Hero hero)
        {
            var dlg = new InitiativeForm(hero.InitBonus, hero.CombatData.Initiative);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                hero.CombatData.Initiative = dlg.Score;

                update_list();
                update_preview_panel();
                update_maps();
                update_statusbar();

                var initScores = get_initiatives();
                InitiativePanel.InitiativeScores = initScores;

                return true;
            }

            return false;
        }

        private int next_init(int currentInit)
        {
            var scores = get_initiatives();

            if (!scores.Contains(currentInit))
                scores.Add(currentInit);

            scores.Sort();
            scores.Reverse();

            var initIndex = scores.IndexOf(currentInit) + 1;
            if (initIndex == scores.Count)
                initIndex = 0;

            return scores[initIndex];
        }

        private int find_max_init()
        {
            var scores = get_initiatives();

            if (scores.Count != 0)
                return scores[0];

            return 0;
        }

        private int find_min_init()
        {
            var scores = get_initiatives();

            if (scores.Count != 0)
                return scores[scores.Count - 1];

            return 0;
        }

        private List<int> get_initiatives()
        {
            var scores = new List<int>();

            foreach (var slot in _fEncounter.AllSlots)
            foreach (var cd in slot.CombatData)
            {
                if (slot.GetState(cd) == CreatureState.Defeated)
                    continue;

                var score = cd.Initiative;
                if (score == int.MinValue)
                    continue;

                if (!scores.Contains(score))
                    scores.Add(score);
            }

            foreach (var hero in Session.Project.Heroes)
            {
                var cd = hero.CombatData;

                var score = cd.Initiative;
                if (score == int.MinValue)
                    continue;

                if (!scores.Contains(score))
                    scores.Add(score);
            }

            foreach (var cd in _fTrapData.Values)
            {
                if (cd.Delaying)
                    continue;

                var score = cd.Initiative;
                if (score == int.MinValue)
                    continue;

                if (!scores.Contains(score))
                    scores.Add(score);
            }

            scores.Sort();
            scores.Reverse();

            return scores;
        }

        private void handle_regen()
        {
            if (_fCurrentActor == null)
                return;

            if (_fCurrentActor.Damage <= 0)
                return;

            var slot = _fEncounter.FindSlot(_fCurrentActor);
            if (slot == null)
                return;

            var regen = new Regeneration();
            regen.Value = 0;

            if (slot.Card.Regeneration != null)
            {
                regen.Value = slot.Card.Regeneration.Value;
                regen.Details = slot.Card.Regeneration.Details;
            }

            foreach (var oc in _fCurrentActor.Conditions)
            {
                if (oc.Type != OngoingType.Regeneration)
                    continue;

                // Take the highest regen value
                regen.Value = Math.Max(regen.Value, oc.Regeneration.Value);

                if (oc.Regeneration.Details != "")
                {
                    if (regen.Details != "")
                        regen.Details += Environment.NewLine;

                    regen.Details += oc.Regeneration.Details;
                }
            }

            if (regen.Value == 0)
                return;

            var str = _fCurrentActor.DisplayName + " has regeneration:";
            str += Environment.NewLine + Environment.NewLine;
            str += "Value: " + regen.Value + Environment.NewLine;
            if (regen.Details != "")
                str += regen.Details + Environment.NewLine;
            str += Environment.NewLine;
            str += "Do you want to apply it now?";

            if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Information) ==
                DialogResult.Yes)
            {
                _fCurrentActor.Damage -= regen.Value;
                _fCurrentActor.Damage = Math.Max(0, _fCurrentActor.Damage);
            }
        }

        private void handle_ended_effects(bool beginningOfTurn)
        {
            if (_fCurrentActor == null)
                return;

            var dt = beginningOfTurn ? DurationType.BeginningOfTurn : DurationType.EndOfTurn;

            var endedConditions = new List<Pair<CombatData, OngoingCondition>>();

            foreach (var slot in _fEncounter.AllSlots)
            foreach (var cd in slot.CombatData)
            foreach (var oc in cd.Conditions)
            {
                if (oc.Duration != dt)
                    continue;

                if (oc.DurationRound > _fCurrentRound)
                    continue;

                if (_fCurrentActor.Id == oc.DurationCreatureId)
                    endedConditions.Add(new Pair<CombatData, OngoingCondition>(cd, oc));
            }

            foreach (var hero in Session.Project.Heroes)
            {
                var cd = hero.CombatData;

                foreach (var oc in cd.Conditions)
                {
                    if (oc.Duration != dt)
                        continue;

                    if (oc.DurationRound > _fCurrentRound)
                        continue;

                    if (_fCurrentActor.Id == oc.DurationCreatureId)
                        endedConditions.Add(new Pair<CombatData, OngoingCondition>(cd, oc));
                }
            }

            foreach (var trapId in _fTrapData.Keys)
            {
                var cd = _fTrapData[trapId];

                foreach (var oc in cd.Conditions)
                {
                    if (oc.Duration != dt)
                        continue;

                    if (oc.DurationRound > _fCurrentRound)
                        continue;

                    if (_fCurrentActor.Id == oc.DurationCreatureId)
                        endedConditions.Add(new Pair<CombatData, OngoingCondition>(cd, oc));
                }
            }

            if (endedConditions.Count > 0)
            {
                var dlg = new EndedEffectsForm(endedConditions, _fEncounter);
                dlg.ShowDialog();

                update_list();
            }
        }

        private void handle_saves()
        {
            if (_fCurrentActor == null)
                return;

            if (_fCurrentActor.Delaying)
                return;

            var conditions = new List<OngoingCondition>();
            foreach (var oc in _fCurrentActor.Conditions)
                if (oc.Duration == DurationType.SaveEnds)
                    conditions.Add(oc);

            if (conditions.Count == 0)
                return;

            EncounterCard card = null;
            var slot = _fEncounter.FindSlot(_fCurrentActor);
            if (slot != null)
                card = slot.Card;

            var dlg = new SavingThrowForm(_fCurrentActor, card, _fEncounter);
            if (dlg.ShowDialog() == DialogResult.OK) update_list();
        }

        private void handle_ongoing_damage()
        {
            if (_fCurrentActor == null)
                return;

            var conditions = new List<OngoingCondition>();
            foreach (var oc in _fCurrentActor.Conditions)
                if (oc.Type == OngoingType.Damage && oc.Value > 0)
                    conditions.Add(oc);

            if (conditions.Count == 0)
                return;

            EncounterCard card = null;
            var slot = _fEncounter.FindSlot(_fCurrentActor);
            if (slot != null)
                card = slot.Card;

            var dmg = _fCurrentActor.Damage;
            var state = CreatureState.Active;
            if (slot != null)
                state = slot.GetState(_fCurrentActor);
            if (slot == null)
            {
                var h = Session.Project.FindHero(_fCurrentActor.Id);
                state = h.GetState(dmg);
            }

            var dlg = new OngoingDamageForm(_fCurrentActor, card, _fEncounter);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (_fCurrentActor.Damage != dmg)
                    _fLog.AddDamageEntry(_fCurrentActor.Id, _fCurrentActor.Damage - dmg, null);
                if (slot != null)
                {
                    if (slot.GetState(_fCurrentActor) != state)
                        _fLog.AddStateEntry(_fCurrentActor.Id, slot.GetState(_fCurrentActor));
                }
                else
                {
                    var h = Session.Project.FindHero(_fCurrentActor.Id);
                    if (h.GetState(_fCurrentActor.Damage) != state)
                        _fLog.AddStateEntry(_fCurrentActor.Id, h.GetState(_fCurrentActor.Damage));
                }

                update_list();
                update_log();
            }
        }

        private void handle_recharge()
        {
            if (_fCurrentActor == null)
                return;

            var slot = _fEncounter.FindSlot(_fCurrentActor);
            if (slot == null)
                return;

            var powers = slot.Card.CreaturePowers;
            var rechargablePowers = new List<CreaturePower>();
            foreach (var powerId in _fCurrentActor.UsedPowers)
            foreach (var power in powers)
                if (power.Id == powerId && power.Action != null && power.Action.Recharge != "")
                    rechargablePowers.Add(power);

            if (rechargablePowers.Count == 0)
                return;

            var dlg = new RechargeForm(_fCurrentActor, slot.Card);
            if (dlg.ShowDialog() == DialogResult.OK) update_list();
        }

        private CombatData get_next_actor(CombatData currentActor)
        {
            // What's the current initiative?
            var init = currentActor?.Initiative ?? InitiativePanel.CurrentInitiative;

            var scores = get_initiatives();
            if (!scores.Contains(init))
                init = next_init(init);

            CombatData nextActor = null;

            // What combatants are at this init value?
            var combatants = get_combatants(init, true);

            var index = combatants.IndexOf(currentActor);
            if (index == -1)
            {
                // Use first
                nextActor = combatants[0];
            }
            else if (index == combatants.Count - 1)
            {
                init = next_init(init);
                combatants = get_combatants(init, false);
                nextActor = combatants[0];
            }
            else
            {
                // Use next
                nextActor = combatants[index + 1];
            }

            // If defeated or delaying, get the next one
            var defeated = get_state(nextActor) == CreatureState.Defeated;
            var delaying = nextActor != null && nextActor.Delaying;
            if (defeated || delaying)
                nextActor = get_next_actor(nextActor);

            return nextActor;
        }

        private CreatureState get_state(CombatData cd)
        {
            var slot = _fEncounter.FindSlot(cd);
            if (slot != null)
                return slot.GetState(cd);

            var hero = Session.Project.FindHero(cd.Id);
            if (hero != null)
                return hero.GetState(cd.Damage);

            var trap = _fEncounter.FindTrap(cd.Id);
            if (trap != null)
                return CreatureState.Active;

            return CreatureState.Active;
        }

        private List<CombatData> get_combatants(int init, bool includeDefeated)
        {
            var data = new Dictionary<int, List<CombatData>>();

            foreach (var slot in _fEncounter.AllSlots)
            {
                var slotInit = slot.Card.Initiative;
                if (!data.ContainsKey(slotInit))
                    data[slotInit] = new List<CombatData>();

                foreach (var cd in slot.CombatData)
                {
                    if (slot.GetState(cd) == CreatureState.Defeated && !includeDefeated)
                        continue;

                    if (cd.Initiative == init)
                        data[slotInit].Add(cd);
                }
            }

            foreach (var hero in Session.Project.Heroes)
            {
                if (!data.ContainsKey(hero.InitBonus))
                    data[hero.InitBonus] = new List<CombatData>();

                var cd = hero.CombatData;

                if (cd.Initiative == init)
                    data[hero.InitBonus].Add(cd);
            }

            foreach (var trap in _fEncounter.Traps)
            {
                if (trap.Initiative == int.MinValue)
                    continue;

                if (!_fTrapData.ContainsKey(trap.Id))
                    continue;

                if (!data.ContainsKey(trap.Initiative))
                    data[trap.Initiative] = new List<CombatData>();

                var cd = _fTrapData[trap.Id];

                if (cd.Initiative == init)
                    data[trap.Initiative].Add(cd);
            }

            var bonuses = new List<int>();
            bonuses.AddRange(data.Keys);
            bonuses.Sort();
            bonuses.Reverse();

            var list = new List<CombatData>();
            foreach (var bonus in bonuses)
            {
                data[bonus].Sort();
                list.AddRange(data[bonus]);
            }

            return list;
        }

        private void highlight_current_actor()
        {
            MapView.BoxedTokens.Clear();
            if (_fCurrentActor != null)
            {
                var hero = Session.Project.FindHero(_fCurrentActor.Id);
                if (hero != null)
                {
                    MapView.BoxedTokens.Add(hero);
                }
                else
                {
                    var slot = _fEncounter.FindSlot(_fCurrentActor);
                    if (slot != null)
                    {
                        var ct = new CreatureToken(slot.Id, _fCurrentActor);
                        MapView.BoxedTokens.Add(ct);
                    }
                }

                MapView.MapChanged();
            }
        }

        private ListViewItem get_combatant(Guid id)
        {
            foreach (ListViewItem lvi in CombatList.Items)
            {
                var ct = lvi.Tag as CreatureToken;
                if (ct != null)
                    if (ct.Data.Id == id)
                        return lvi;

                var hero = lvi.Tag as Hero;
                if (hero != null)
                    if (hero.Id == id)
                        return lvi;

                var trap = lvi.Tag as Trap;
                if (trap != null)
                    if (trap.Id == id)
                        return lvi;
            }

            return null;
        }

        private void cancelled_scrolling()
        {
            var map = Session.Preferences.Combat.PlayerViewMap ? MapView : null;
            var init = Session.Preferences.Combat.PlayerViewInitiative ? InitiativeView() : null;
            Session.PlayerView.ShowTacticalMap(map, init);

            PlayerMap.ScalingFactor = MapView.ScalingFactor;
        }

        private void update_list()
        {
            var defeated = new List<CombatData>();
            if (Session.Preferences.Combat.CreatureAutoRemove)
                // Remove creatures at 0 HP or below

                foreach (var slot in _fEncounter.AllSlots)
                {
                    var fullHp = slot.Card.Hp;
                    if (fullHp == 0)
                        continue;

                    var obsolete = new List<CombatData>();
                    foreach (var cd in slot.CombatData)
                    {
                        var state = slot.GetState(cd);
                        if (state == CreatureState.Defeated)
                        {
                            obsolete.Add(cd);
                            defeated.Add(cd);
                        }
                    }

                    foreach (var cd in obsolete)
                    {
                        if (cd == _fCurrentActor)
                        {
                            var previousActor = _fCurrentActor.Id;

                            _fCurrentActor = get_next_actor(_fCurrentActor);

                            if (_fCurrentActor.Id != previousActor)
                            {
                                _fLog.AddStartTurnEntry(_fCurrentActor.Id);
                                update_log();
                            }
                        }

                        var ct = new CreatureToken(slot.Id, cd);

                        remove_effects(ct);
                        remove_links(ct);

                        cd.Location = CombatData.NoPoint;
                    }
                }


            var selectedTokens = SelectedTokens;
            var selectedTrap = SelectedTrap;
            var selectedChallenge = SelectedChallenge;

            CombatList.BeginUpdate();

            CombatList.Items.Clear();

            CombatList.SmallImageList = new ImageList();
            CombatList.SmallImageList.ImageSize = new Size(16, 16);

            foreach (var slot in _fEncounter.AllSlots)
            {
                var ew = _fEncounter.FindWave(slot);
                if (ew != null && ew.Active == false)
                    continue;

                var fullHp = slot.Card.Hp;
                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                foreach (var cd in slot.CombatData)
                {
                    var hp = fullHp - cd.Damage;
                    var hpStr = hp.ToString();

                    if (cd.TempHp > 0)
                        hpStr += " (+" + cd.TempHp + ")";

                    if (hp != fullHp)
                        hpStr += " / " + fullHp;

                    var initStr = cd.Initiative.ToString();
                    if (cd.Delaying)
                        initStr = "(" + initStr + ")";

                    var lvi = CombatList.Items.Add(cd.DisplayName);
                    lvi.Tag = new CreatureToken(slot.Id, cd);

                    if (cd.Initiative == int.MinValue)
                    {
                        lvi.ForeColor = SystemColors.GrayText;
                        initStr = "-";
                    }

                    var ac = slot.Card.Ac;
                    var fort = slot.Card.Fortitude;
                    var reflex = slot.Card.Reflex;
                    var will = slot.Card.Will;

                    foreach (var oc in cd.Conditions)
                    {
                        if (oc.Type != OngoingType.DefenceModifier)
                            continue;

                        if (oc.Defences.Contains(DefenceType.Ac))
                            ac += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Fortitude))
                            fort += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Reflex))
                            reflex += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Will))
                            will += oc.DefenceMod;
                    }

                    var defStr = "AC " + ac + ", Fort " + fort + ", Ref " + reflex + ", Will " + will;
                    var conditionsStr = get_conditions(cd);

                    lvi.SubItems.Add(initStr);
                    lvi.SubItems.Add(hpStr);
                    lvi.SubItems.Add(defStr);
                    lvi.SubItems.Add(conditionsStr);

                    switch (slot.GetState(cd))
                    {
                        case CreatureState.Bloodied:
                            lvi.ForeColor = Color.Maroon;
                            break;
                        case CreatureState.Defeated:
                            lvi.ForeColor = SystemColors.GrayText;
                            break;
                    }

                    if (!cd.Visible)
                    {
                        lvi.ForeColor = Color.FromArgb(80, lvi.ForeColor);
                        lvi.Text += " (hidden)";
                    }

                    if (creature?.Image != null)
                    {
                        CombatList.SmallImageList.Images.Add(new Bitmap(creature.Image, 16, 16));
                        lvi.ImageIndex = CombatList.SmallImageList.Images.Count - 1;
                    }
                    else
                    {
                        add_icon(lvi, lvi.ForeColor);
                    }

                    if (cd.Conditions.Count != 0)
                        add_condition_hint(lvi);

                    var groupIndex = (int)GroupIndexes.Combatants;
                    if (cd.Initiative == int.MinValue)
                        groupIndex = (int)GroupIndexes.Inactive;
                    if (cd.Delaying)
                        groupIndex = (int)GroupIndexes.Delayed;
                    if (MapView.Map != null && cd.Location == CombatData.NoPoint)
                        groupIndex = (int)GroupIndexes.Inactive;
                    if (slot.GetState(cd) == CreatureState.Defeated)
                        groupIndex = (int)GroupIndexes.Defeated;

                    lvi.Group = CombatList.Groups[groupIndex];

                    if (cd == _fCurrentActor)
                    {
                        lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                        lvi.UseItemStyleForSubItems = false;
                        lvi.BackColor = Color.LightBlue;

                        add_initiative_hint(lvi);
                    }

                    foreach (var token in selectedTokens)
                    {
                        var ct = token as CreatureToken;
                        if (ct != null)
                            if (ct.Data == cd)
                                lvi.Selected = true;
                    }
                }
            }

            foreach (var trap in _fEncounter.Traps)
            {
                var lvi = CombatList.Items.Add(trap.Name);
                lvi.Tag = trap;

                add_icon(lvi, Color.White);

                if (trap.Initiative != int.MinValue)
                {
                    var cd = _fTrapData[trap.Id];

                    if (cd != null && cd.Initiative != int.MinValue)
                    {
                        var initStr = cd.Initiative.ToString();
                        lvi.SubItems.Add(initStr);

                        lvi.Group = CombatList.Groups[(int)GroupIndexes.Combatants];
                    }
                    else
                    {
                        lvi.SubItems.Add("-");

                        lvi.Group = CombatList.Groups[(int)GroupIndexes.Inactive];
                    }

                    if (cd == _fCurrentActor)
                    {
                        lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                        lvi.UseItemStyleForSubItems = false;
                        lvi.BackColor = Color.LightBlue;

                        add_initiative_hint(lvi);
                    }
                }
                else
                {
                    lvi.SubItems.Add("-");
                    lvi.Group = CombatList.Groups[(int)GroupIndexes.Traps];
                }

                // HP, defences, conditions
                lvi.SubItems.Add("-");
                lvi.SubItems.Add("-");
                lvi.SubItems.Add("-");

                if (trap == selectedTrap)
                    lvi.Selected = true;
            }

            foreach (var sc in _fEncounter.SkillChallenges)
            {
                var lvi = CombatList.Items.Add(sc.Name);

                // Init, HP, defences, conditions
                lvi.SubItems.Add("-");
                lvi.SubItems.Add("-");
                lvi.SubItems.Add("-");
                lvi.SubItems.Add(sc.Results.Successes + " / " + sc.Successes + " successes; " + sc.Results.Fails +
                                 " / 3 failures");

                add_icon(lvi, Color.White);

                lvi.Tag = sc;
                lvi.Group = CombatList.Groups[(int)GroupIndexes.SkillChallenges];

                if (sc == selectedChallenge)
                    lvi.Selected = true;
            }

            foreach (var hero in Session.Project.Heroes)
            {
                var groupIndex = (int)GroupIndexes.Combatants;

                var lvi = CombatList.Items.Add(hero.Name);
                lvi.Tag = hero;

                var cd = hero.CombatData;

                switch (hero.GetState(cd.Damage))
                {
                    case CreatureState.Active:
                        lvi.ForeColor = SystemColors.WindowText;
                        break;
                    case CreatureState.Bloodied:
                        lvi.ForeColor = Color.Maroon;
                        break;
                    case CreatureState.Defeated:
                        lvi.ForeColor = SystemColors.GrayText;
                        break;
                }

                if (hero.Portrait != null)
                {
                    CombatList.SmallImageList.Images.Add(new Bitmap(hero.Portrait, 16, 16));
                    lvi.ImageIndex = CombatList.SmallImageList.Images.Count - 1;
                }
                else
                {
                    add_icon(lvi, Color.Green);
                }

                if (cd.Conditions.Count != 0)
                    add_condition_hint(lvi);

                var initStr = "";
                var init = cd.Initiative;
                if (init == int.MinValue)
                {
                    lvi.ForeColor = SystemColors.GrayText;
                    groupIndex = (int)GroupIndexes.Inactive;

                    initStr = "-";
                }
                else
                {
                    initStr = init.ToString();
                    if (cd.Delaying)
                    {
                        initStr = "(" + initStr + ")";
                        groupIndex = (int)GroupIndexes.Delayed;
                    }

                    if (cd == _fCurrentActor)
                    {
                        lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                        lvi.UseItemStyleForSubItems = false;
                        lvi.BackColor = Color.LightBlue;

                        add_initiative_hint(lvi);
                    }
                }

                var hpStr = "";
                if (hero.Hp != 0)
                {
                    var hp = hero.Hp - cd.Damage;
                    hpStr = hp.ToString();

                    if (cd.TempHp > 0)
                        hpStr += " (+" + cd.TempHp + ")";

                    if (hp != hero.Hp)
                        hpStr += " / " + hero.Hp;
                }
                else
                {
                    hpStr = "-";
                }

                lvi.SubItems.Add(initStr);
                lvi.SubItems.Add(hpStr);

                if (hero.Ac != 0 && hero.Fortitude != 0 && hero.Reflex != 0 && hero.Will != 0)
                {
                    var ac = hero.Ac;
                    var fort = hero.Fortitude;
                    var reflex = hero.Reflex;
                    var will = hero.Will;

                    foreach (var oc in cd.Conditions)
                    {
                        if (oc.Type != OngoingType.DefenceModifier)
                            continue;

                        if (oc.Defences.Contains(DefenceType.Ac))
                            ac += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Fortitude))
                            fort += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Reflex))
                            reflex += oc.DefenceMod;
                        if (oc.Defences.Contains(DefenceType.Will))
                            will += oc.DefenceMod;
                    }

                    var defStr = "AC " + ac + ", Fort " + fort + ", Ref " + reflex + ", Will " + will;
                    lvi.SubItems.Add(defStr);
                }
                else
                {
                    lvi.SubItems.Add("-");
                }

                lvi.SubItems.Add(get_conditions(cd));

                if (MapView.Map != null && hero.CombatData.Location == CombatData.NoPoint)
                    groupIndex = (int)GroupIndexes.Inactive;

                lvi.Group = CombatList.Groups[groupIndex];

                if (selectedTokens.Contains(hero))
                    lvi.Selected = true;
            }

            foreach (var ct in _fEncounter.CustomTokens)
            {
                var lvi = CombatList.Items.Add(ct.Name);
                lvi.Tag = ct;

                add_icon(lvi, Color.Blue);

                var groupIndex = (int)GroupIndexes.Custom;
                if (MapView.Map != null && ct.Data.Location == CombatData.NoPoint && ct.CreatureId == Guid.Empty)
                {
                    groupIndex = (int)GroupIndexes.Inactive;
                    lvi.ForeColor = SystemColors.GrayText;
                }

                lvi.Group = CombatList.Groups[groupIndex];

                if (selectedTokens.Contains(ct))
                    lvi.Selected = true;
            }

            CombatList.Sort();

            CombatList.EndUpdate();

            if (PlayerInitiative != null)
                PlayerInitiative.DocumentText = InitiativeView();
        }

        private string get_conditions(CombatData cd)
        {
            var str = "";

            // Are there any ongoing damage conditions?
            var ongoing = false;
            foreach (var oc in cd.Conditions)
                if (oc.Type == OngoingType.Damage)
                {
                    ongoing = true;
                    break;
                }

            if (ongoing)
            {
                if (str != "")
                    str += "; ";

                str += "Damage";
            }

            foreach (var oc in cd.Conditions)
            {
                // Ignore damage conditions
                if (oc.Type == OngoingType.Damage)
                    continue;

                if (str != "")
                    str += "; ";

                switch (oc.Type)
                {
                    case OngoingType.Condition:
                        str += oc.Data;
                        break;
                    case OngoingType.DefenceModifier:
                        str += oc.ToString(_fEncounter, false);
                        break;
                }
            }

            return str;
        }

        private void add_icon(ListViewItem lvi, Color c)
        {
            Image img = new Bitmap(16, 16);

            var g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.FillEllipse(new SolidBrush(c), 2, 2, 12, 12);
            if (c == Color.White)
                g.DrawEllipse(Pens.Black, 2, 2, 12, 12);

            CombatList.SmallImageList.Images.Add(img);
            lvi.ImageIndex = CombatList.SmallImageList.Images.Count - 1;
        }

        private void add_condition_hint(ListViewItem lvi)
        {
            if (lvi.ImageIndex == -1)
                return;

            var img = CombatList.SmallImageList.Images[lvi.ImageIndex];

            var g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.FillEllipse(Brushes.White, 5, 5, 6, 6);
            g.DrawEllipse(Pens.DarkGray, 5, 5, 6, 6);

            CombatList.SmallImageList.Images[lvi.ImageIndex] = img;
        }

        private void add_initiative_hint(ListViewItem lvi)
        {
            if (lvi.ImageIndex == -1)
                return;

            var img = CombatList.SmallImageList.Images[lvi.ImageIndex];

            var g = Graphics.FromImage(img);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            var p = new Pen(Color.Blue, 3);
            g.DrawRectangle(p, 0, 0, 16, 16);

            CombatList.SmallImageList.Images[lvi.ImageIndex] = img;
        }

        private void update_log()
        {
            var html = EncounterLogView(false);
            LogBrowser.Document.OpenNew(true);
            LogBrowser.Document.Write(html);
        }

        private void update_preview_panel()
        {
            var html = "";

            html += "<HTML>";
            html += Html.Concatenate(Html.GetHead("", "", Session.Preferences.TextSize));
            html += "<BODY>";

            if (_fCombatStarted)
            {
                var tokens = SelectedTokens;
                if (TwoColumnPreview)
                {
                    // Remove the current actor, cos that'll be shown anyway

                    var obsolete = new List<IToken>();

                    foreach (var token in tokens)
                    {
                        var ct = token as CreatureToken;
                        if (ct != null && ct.Data.Id == _fCurrentActor.Id)
                            obsolete.Add(token);

                        var hero = token as Hero;
                        if (hero != null && hero.Id == _fCurrentActor.Id)
                            obsolete.Add(token);
                    }

                    foreach (var token in obsolete)
                        tokens.Remove(token);
                }

                if (TwoColumnPreview)
                {
                    html += "<P class=table>";
                    html += "<TABLE class=clear>";
                    html += "<TR class=clear>";
                    html += "<TD class=clear>";

                    var slot = _fEncounter.FindSlot(_fCurrentActor);
                    if (slot != null)
                        html += Html.StatBlock(slot.Card, _fCurrentActor, _fEncounter, false, true, true,
                            CardMode.Combat, Session.Preferences.TextSize);

                    var hero = Session.Project.FindHero(_fCurrentActor.Id);
                    if (hero != null)
                    {
                        var showEffects = tokens.Count != 0;
                        html += Html.StatBlock(hero, _fEncounter, false, true, showEffects,
                            Session.Preferences.TextSize);
                    }

                    var trap = _fEncounter.FindTrap(_fCurrentActor.Id);
                    if (trap != null)
                        html += Html.Trap(trap, _fCurrentActor, false, true, false, Session.Preferences.TextSize);

                    html += "</TD>";
                    html += "<TD class=clear>";
                }

                var statblock = "";
                if (tokens.Count != 0)
                    statblock = html_tokens(tokens);
                else if (SelectedTrap != null)
                    statblock = html_trap();
                else if (SelectedChallenge != null) statblock = html_skill_challenge();

                if (statblock != "")
                    html += statblock;
                else
                    html += html_encounter_overview();

                if (TwoColumnPreview)
                {
                    html += "</TD>";
                    html += "</TR>";
                    html += "</TABLE>";
                    html += "</P>";
                }
            }
            else
            {
                html += html_encounter_start();
            }

            html += "</BODY>";
            html += "</HTML>";

            Preview.Document.OpenNew(true);
            Preview.Document.Write(html);
        }

        private void update_maps()
        {
            MapView.Invalidate();

            PlayerMap?.Invalidate();
        }

        private void update_statusbar()
        {
            var xp = _fEncounter.GetXp() + _fRemovedCreatureXp;
            XPLbl.Text = xp + " XP";

            var count = 0;
            foreach (var hero in Session.Project.Heroes)
                if (hero.CombatData.Initiative != int.MinValue)
                    count += 1;
            if (count > 1)
                XPLbl.Text += " (" + xp / count + " XP each)";

            var level = _fEncounter.GetLevel(count);
            LevelLbl.Text = level != -1 ? "Encounter Level: " + level : "";
        }

        private void add_quick_effect(OngoingCondition effect)
        {
            var effectStr = effect.ToString(_fEncounter, false);

            foreach (var oc in _fEffects)
                if (oc.ToString(_fEncounter, false) == effectStr)
                    return;

            _fEffects.Add(effect.Copy());
            _fEffects.Sort();
        }

        private void update_effects_list(ToolStripDropDownItem tsddi, bool useListSelection)
        {
            tsddi.DropDownItems.Clear();

            // Add effects for each standard condition
            var tsmiStd = new ToolStripMenuItem("Standard Conditions");
            tsddi.DropDownItems.Add(tsmiStd);

            foreach (var con in Conditions.GetConditions())
            {
                var effect = new OngoingCondition();
                effect.Data = con;
                effect.Duration = DurationType.Encounter;

                var tsmiEffect = new ToolStripMenuItem(effect.ToString(_fEncounter, false));
                tsmiEffect.Tag = effect;
                if (useListSelection)
                    tsmiEffect.Click += apply_quick_effect_from_toolbar;
                else
                    tsmiEffect.Click += apply_quick_effect_from_map;
                tsmiStd.DropDownItems.Add(tsmiEffect);
            }

            tsddi.DropDownItems.Add(new ToolStripSeparator());

            // Add effects for each PC
            var addedHeroEffect = false;
            foreach (var hero in Session.Project.Heroes)
                if (hero.Effects.Count != 0)
                {
                    var tsmiHero = new ToolStripMenuItem(hero.Name);
                    tsddi.DropDownItems.Add(tsmiHero);

                    foreach (var effect in hero.Effects)
                    {
                        var tsmiHeroEffect = new ToolStripMenuItem(effect.ToString(_fEncounter, false));
                        tsmiHeroEffect.Tag = effect.Copy();
                        if (useListSelection)
                            tsmiHeroEffect.Click += apply_quick_effect_from_toolbar;
                        else
                            tsmiHeroEffect.Click += apply_quick_effect_from_map;
                        tsmiHero.DropDownItems.Add(tsmiHeroEffect);

                        addedHeroEffect = true;
                    }
                }

            if (addedHeroEffect)
                tsddi.DropDownItems.Add(new ToolStripSeparator());

            // Add defined quick effects
            foreach (var oc in _fEffects)
            {
                var tsmiQuick = new ToolStripMenuItem(oc.ToString(_fEncounter, false));
                tsmiQuick.Tag = oc.Copy();
                if (useListSelection)
                    tsmiQuick.Click += apply_quick_effect_from_toolbar;
                else
                    tsmiQuick.Click += apply_quick_effect_from_map;
                tsddi.DropDownItems.Add(tsmiQuick);
            }

            if (_fEffects.Count != 0)
                tsddi.DropDownItems.Add(new ToolStripSeparator());

            // Add other effect
            var otherMenu = new ToolStripMenuItem("Add a New Effect...");
            if (useListSelection)
                otherMenu.Click += apply_effect_from_toolbar;
            else
                otherMenu.Click += apply_effect_from_map;
            tsddi.DropDownItems.Add(otherMenu);
        }

        private void update_remove_effect_list(ToolStripDropDownItem tsddi, bool useListSelection)
        {
            tsddi.DropDownItems.Clear();

            var tokens = useListSelection ? SelectedTokens : MapView.SelectedTokens;

            if (tokens.Count != 1)
            {
                var tsmi = new ToolStripMenuItem("(multiple selection)");
                tsmi.Enabled = false;
                tsddi.DropDownItems.Add(tsmi);

                return;
            }

            CombatData cd = null;

            var ct = tokens[0] as CreatureToken;
            if (ct != null)
                cd = ct.Data;

            var hero = tokens[0] as Hero;
            if (hero != null) cd = hero.CombatData;

            if (cd != null)
                foreach (var oc in cd.Conditions)
                {
                    var tsmi = new ToolStripMenuItem(oc.ToString(_fEncounter, false));
                    tsmi.Tag = oc;
                    if (useListSelection)
                        tsmi.Click += remove_effect_from_list;
                    else
                        tsmi.Click += remove_effect_from_map;
                    tsddi.DropDownItems.Add(tsmi);
                }

            if (tsddi.DropDownItems.Count == 0)
            {
                var tsmi = new ToolStripMenuItem("(no effects)");
                tsmi.Enabled = false;
                tsddi.DropDownItems.Add(tsmi);
            }
        }

        private void apply_quick_effect_from_toolbar(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            var oc = tsi.Tag as OngoingCondition;
            if (oc == null)
                return;

            apply_effect(oc.Copy(), SelectedTokens, false);
        }

        private void apply_quick_effect_from_map(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            var oc = tsi.Tag as OngoingCondition;
            if (oc == null)
                return;

            apply_effect(oc.Copy(), MapView.SelectedTokens, false);
        }

        private void apply_effect_from_toolbar(object sender, EventArgs e)
        {
            var oc = new OngoingCondition();

            var dlg = new EffectForm(oc, _fEncounter, _fCurrentActor, _fCurrentRound);
            if (dlg.ShowDialog() == DialogResult.OK)
                apply_effect(dlg.Effect, SelectedTokens, true);
        }

        private void apply_effect_from_map(object sender, EventArgs e)
        {
            var oc = new OngoingCondition();

            var dlg = new EffectForm(oc, _fEncounter, _fCurrentActor, _fCurrentRound);
            if (dlg.ShowDialog() == DialogResult.OK)
                apply_effect(dlg.Effect, MapView.SelectedTokens, true);
        }

        private void apply_effect(OngoingCondition oc, List<IToken> tokens, bool addToQuickList)
        {
            try
            {
                if (oc.Duration == DurationType.BeginningOfTurn || oc.Duration == DurationType.EndOfTurn)
                {
                    if (oc.DurationCreatureId == Guid.Empty)
                    {
                        // Choose a creature
                        var dlg = new CombatantSelectForm(_fEncounter, _fTrapData);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            if (dlg.SelectedCombatant != null)
                                oc.DurationCreatureId = dlg.SelectedCombatant.Id;
                            else
                                return;
                        }
                    }

                    oc.DurationRound = _fCurrentRound;
                    if (_fCurrentActor != null && oc.DurationCreatureId == _fCurrentActor.Id)
                        oc.DurationRound += 1;
                }

                foreach (var token in tokens)
                {
                    var ct = token as CreatureToken;
                    if (ct != null)
                    {
                        var cd = ct.Data;
                        cd.Conditions.Add(oc.Copy());

                        _fLog.AddEffectEntry(cd.Id, oc.ToString(_fEncounter, false), true);
                    }

                    var hero = token as Hero;
                    if (hero != null)
                    {
                        var cd = hero.CombatData;
                        cd.Conditions.Add(oc.Copy());

                        _fLog.AddEffectEntry(cd.Id, oc.ToString(_fEncounter, false), true);
                    }
                }

                if (addToQuickList)
                {
                    var addedToHero = false;
                    var copy = oc.Copy();

                    if (Session.Project.Heroes.Count != 0)
                    {
                        var selectedHero = Session.Project.FindHero(_fCurrentActor.Id);
                        var dlg = new HeroSelectForm(selectedHero);
                        if (dlg.ShowDialog() == DialogResult.OK)
                            if (dlg.SelectedHero != null)
                            {
                                // If it refers to someone in this encounter, make it generic
                                if (copy.DurationCreatureId != dlg.SelectedHero.Id)
                                    copy.DurationCreatureId = Guid.Empty;

                                dlg.SelectedHero.Effects.Add(copy);
                                Session.Modified = true;
                                addedToHero = true;
                            }
                    }

                    if (!addedToHero)
                        add_quick_effect(copy);
                }

                update_list();
                update_log();
                update_preview_panel();
                MapView.MapChanged();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void remove_effect_from_list(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            var oc = tsi.Tag as OngoingCondition;
            if (oc == null)
                return;

            if (SelectedTokens.Count != 1)
                return;

            CombatData cd = null;

            var ct = SelectedTokens[0] as CreatureToken;
            if (ct != null)
                cd = ct.Data;

            var hero = SelectedTokens[0] as Hero;
            if (hero != null) cd = hero.CombatData;

            if (cd == null)
                return;

            cd.Conditions.Remove(oc);
            _fLog.AddEffectEntry(cd.Id, oc.ToString(_fEncounter, false), false);

            update_list();
            update_log();
            update_preview_panel();
        }

        private void remove_effect_from_map(object sender, EventArgs e)
        {
            var tsi = sender as ToolStripItem;
            var oc = tsi.Tag as OngoingCondition;
            if (oc == null)
                return;

            if (MapView.SelectedTokens.Count != 1)
                return;

            CombatData cd = null;

            var ct = MapView.SelectedTokens[0] as CreatureToken;
            if (ct != null)
                cd = ct.Data;

            var hero = MapView.SelectedTokens[0] as Hero;
            if (hero != null) cd = hero.CombatData;

            if (cd == null)
                return;

            cd.Conditions.Remove(oc);
            _fLog.AddEffectEntry(cd.Id, oc.ToString(_fEncounter, false), false);

            update_list();
            update_log();
            update_preview_panel();
        }

        private string html_tokens(List<IToken> tokens)
        {
            var html = "";

            if (tokens.Count == 1)
            {
                var token = tokens[0];
                html = html_token(token, true);
            }
            else
            {
                var lines = new List<string>();

                foreach (var token in tokens) lines.Add(html_token(token, false));

                html = Html.Concatenate(lines);
            }

            return html;
        }

        private string html_token(IToken token, bool full)
        {
            var html = "";

            if (token is Hero)
            {
                var hero = token as Hero;
                var cd = hero.CombatData;

                if (TwoColumnPreview && cd == _fCurrentActor)
                    html = "";
                else
                    html = Html.StatBlock(hero, _fEncounter, false, false, false, Session.Preferences.TextSize);
            }

            if (token is CreatureToken)
            {
                var ct = token as CreatureToken;
                var slot = _fEncounter.FindSlot(ct.SlotId);
                var cd = ct.Data;

                if (TwoColumnPreview && cd == _fCurrentActor)
                    html = "";
                else
                    html = Html.StatBlock(slot.Card, ct.Data, _fEncounter, false, false, full, CardMode.Combat,
                        Session.Preferences.TextSize);
            }

            if (token is CustomToken)
            {
                var ct = token as CustomToken;
                var drag = _fEncounter.MapId != Guid.Empty && ct.Data.Location == CombatData.NoPoint;

                html = Html.CustomMapToken(ct, drag, false, Session.Preferences.TextSize);
            }

            return html;
        }

        private string html_trap()
        {
            CombatData cd = null;

            if (_fTrapData.ContainsKey(SelectedTrap.Id))
            {
                cd = _fTrapData[SelectedTrap.Id];
                if (TwoColumnPreview && cd == _fCurrentActor)
                    return "";
            }

            return Html.Trap(SelectedTrap, cd, false, false, false, Session.Preferences.TextSize);
        }

        private string html_skill_challenge()
        {
            return Html.SkillChallenge(SelectedChallenge, true, false, Session.Preferences.TextSize);
        }

        private string html_encounter_start()
        {
            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD>");
            lines.Add("<B>Starting the Encounter</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");


            lines.Add("<TR class=shaded>");
            lines.Add("<TD>");
            lines.Add("<B>How do you want to roll initiative?</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (Session.Project.Heroes.Count != 0)
            {
                var mode = "";
                var auto = Inits.Automatic;
                var manual = Inits.Manual;
                switch (Session.Preferences.Combat.HeroInitiativeMode)
                {
                    case InitiativeMode.AutoIndividual:
                    case InitiativeMode.AutoGroup:
                        mode = Inits.Rolled;
                        manual = "<A href=heroinit:manual>" + manual + "</A>";
                        break;
                    case InitiativeMode.ManualIndividual:
                    case InitiativeMode.ManualGroup:
                        mode = Inits.Entered;
                        auto = "<A href=heroinit:auto>" + auto + "</A>";
                        break;
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("For <B>PCs</B>: " + mode);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add(auto + " / " + manual);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            if (_fEncounter.Count != 0)
            {
                var mode = "";
                var auto = Inits.Automatic;
                var manual = Inits.Manual;
                var individual = Inits.Individual;
                var group = Inits.InGroups;
                switch (Session.Preferences.Combat.InitiativeMode)
                {
                    case InitiativeMode.AutoIndividual:
                        mode = Inits.Rolled;
                        manual = "<A href=creatureinit:manual>" + manual + "</A>";
                        group = "<A href=creatureinit:group>" + group + "</A>";
                        break;
                    case InitiativeMode.AutoGroup:
                        mode = Inits.Rolled + Inits.InGroups;
                        manual = "<A href=creatureinit:manual>" + manual + "</A>";
                        individual = "<A href=creatureinit:individual>" + individual + "</A>";
                        break;
                    case InitiativeMode.ManualIndividual:
                        mode = Inits.Entered;
                        auto = "<A href=creatureinit:auto>" + auto + "</A>";
                        group = "<A href=creatureinit:group>" + group + "</A>";
                        break;
                    case InitiativeMode.ManualGroup:
                        mode = Inits.Entered + Inits.InGroups;
                        auto = "<A href=creatureinit:auto>" + auto + "</A>";
                        individual = "<A href=creatureinit:individual>" + individual + "</A>";
                        break;
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("For <B>creatures</B>: " + mode);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add(auto + " / " + manual);
                lines.Add("</TD>");
                lines.Add("</TR>");

                var groups = false;
                foreach (var slot in _fEncounter.AllSlots)
                    if (slot.CombatData.Count > 1)
                    {
                        groups = true;
                        break;
                    }

                if (groups)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD class=indent>");
                    lines.Add(individual + " / " + group);
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }
            }

            var trapsWithInit = false;
            foreach (var trap in _fEncounter.Traps)
                if (trap.Initiative != int.MinValue)
                {
                    trapsWithInit = true;
                    break;
                }

            if (trapsWithInit)
            {
                var mode = "";
                var auto = Inits.Automatic;
                var manual = Inits.Manual;
                switch (Session.Preferences.Combat.TrapInitiativeMode)
                {
                    case InitiativeMode.AutoIndividual:
                    case InitiativeMode.AutoGroup:
                        mode = Inits.Rolled;
                        manual = "<A href=trapinit:manual>" + manual + "</A>";
                        break;
                    case InitiativeMode.ManualIndividual:
                    case InitiativeMode.ManualGroup:
                        mode = Inits.Entered;
                        auto = "<A href=trapinit:auto>" + auto + "</A>";
                        break;
                }

                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("For <B>traps</B>: " + mode);
                lines.Add("</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD class=indent>");
                lines.Add(auto + " / " + manual);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<TR class=shaded>");
            lines.Add("<TD>");
            lines.Add("<B>Preparing for the encounter</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add("<A href=combat:hp>Update PC hit points</A>");
            lines.Add("- if they've healed or taken damage since their last encounter");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add("<A href=combat:rename>Rename combatants</A>");
            lines.Add("- if you need to indicate which mini is which creature");
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (_fEncounter.MapId != Guid.Empty)
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("Place PCs on the map - drag PCs from the list into their starting positions on the map");
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<TR class=shaded>");
            lines.Add("<TD>");
            lines.Add("<B>Everything ready?</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");
            lines.Add("<TR>");
            lines.Add("<TD>");
            lines.Add("<A href=combat:start>Click here to roll initiative and start the encounter!</A>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return Html.Concatenate(lines);
        }

        private string html_encounter_overview()
        {
            var lines = new List<string>();

            lines.Add("<P class=instruction>Select a combatant from the list to see its stat block here.</P>");
            lines.Add("<P class=instruction></P>");

            var withAuras = new List<EncounterCard>();
            var withTactics = new List<EncounterCard>();
            var withReactions = new List<EncounterCard>();
            foreach (var slot in _fEncounter.AllSlots)
            {
                if (slot.Card.Auras.Count != 0)
                    withAuras.Add(slot.Card);

                if (slot.Card.Tactics != "")
                    withTactics.Add(slot.Card);

                var hasReactions = false;
                var powers = slot.Card.CreaturePowers;
                foreach (var cp in powers)
                    if (cp.Action != null && cp.Action.Trigger != "")
                        hasReactions = true;
                if (hasReactions)
                    withReactions.Add(slot.Card);
            }

            if (withAuras.Count != 0 || withTactics.Count != 0 || withReactions.Count != 0)
            {
                lines.Add("<P class=table>");

                lines.Add("<TABLE>");

                lines.Add("<TR class=heading>");
                lines.Add("<TD>");
                lines.Add("<B>Remember</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (withAuras.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Auras</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var card in withAuras)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("<B>" + card.Title + "</B>");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        foreach (var aura in card.Auras)
                        {
                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>");
                            lines.Add("<B>" + aura.Name + "</B>: " + aura.Details);
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }
                }

                if (withTactics.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Tactics</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var card in withTactics)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD class=indent>");
                        lines.Add("<B>" + card.Title + "</B>: " + card.Tactics);
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                if (withReactions.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD>");
                    lines.Add("<B>Triggered Powers</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var card in withReactions)
                    {
                        lines.Add("<TR>");
                        lines.Add("<TD>");
                        lines.Add("<B>" + card.Title + "</B>:");
                        lines.Add("</TD>");
                        lines.Add("</TR>");

                        var powers = card.CreaturePowers;
                        foreach (var power in powers)
                        {
                            if (power.Action == null || power.Action.Trigger == "")
                                continue;

                            lines.Add("<TR>");
                            lines.Add("<TD class=indent>");
                            lines.Add("<B>" + power.Name + "</B>: " + power.Action.Trigger);
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (_fEncounter.MapAreaId != Guid.Empty)
            {
                var area = MapView.Map.FindArea(_fEncounter.MapAreaId);
                if (area != null && area.Details != "")
                {
                    lines.Add("<P class=encounter_note><B>" + Html.Process(area.Name, true) + "</B>:</P>");
                    lines.Add("<P class=encounter_note>" + Html.Process(area.Details, true) + "</P>");
                }
            }

            foreach (var note in _fEncounter.Notes)
            {
                if (note.Contents == "")
                    continue;

                lines.Add("<P class=encounter_note><B>" + Html.Process(note.Title, true) + "</B>:</P>");
                lines.Add("<P class=encounter_note>" + Html.Process(note.Contents, false) + "</P>");
            }

            return Html.Concatenate(lines);
        }

        public string InitiativeView()
        {
            var items = new List<ListViewItem>();
            foreach (ListViewItem lvi in CombatList.Groups[0].Items)
                items.Add(lvi);
            items.Sort(CombatList.ListViewItemSorter as IComparer<ListViewItem>);

            var lines = new List<string>();
            var previous = new List<string>();

            var active = false;

            lines.AddRange(Html.GetHead(null, null, Session.Preferences.PlayerViewTextSize));

            lines.Add("<BODY bgcolor=black>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE class=initiative>");
            foreach (var lvi in items)
            {
                CombatData cd = null;
                var name = "";

                if (lvi.Tag is CreatureToken)
                {
                    var ct = lvi.Tag as CreatureToken;
                    cd = ct.Data;

                    name = cd.DisplayName;
                    if (!Session.Preferences.Combat.PlayerViewCreatureLabels)
                    {
                        var slot = _fEncounter.FindSlot(ct.SlotId);
                        var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                        name = creature.Category;
                        if (name == "")
                            name = "Creature";
                    }
                }

                if (lvi.Tag is Trap)
                {
                    var trap = lvi.Tag as Trap;
                    if (trap.Initiative != int.MinValue)
                    {
                        cd = _fTrapData[trap.Id];

                        name = cd.DisplayName;
                        if (!Session.Preferences.Combat.PlayerViewCreatureLabels)
                            name = trap.Type.ToString();
                    }
                }

                if (lvi.Tag is Hero)
                {
                    var hero = lvi.Tag as Hero;
                    cd = hero.CombatData;

                    name = hero.Name;
                }

                if (lvi.Tag is CustomToken)
                {
                    var ct = lvi.Tag as CustomToken;
                    cd = ct.Data;

                    name = cd.DisplayName;
                }

                if (cd != null)
                {
                    if (!cd.Visible)
                        continue;

                    if (cd.Initiative == int.MinValue)
                        continue;

                    var colour = "white";
                    if (cd == _fCurrentActor)
                    {
                        active = true;

                        name = "<B>" + name + "</B>";
                    }

                    var slot = _fEncounter.FindSlot(cd);
                    if (slot != null)
                    {
                        var state = slot.GetState(cd);
                        switch (state)
                        {
                            case CreatureState.Bloodied:
                                colour = "red";
                                break;
                            case CreatureState.Defeated:
                                colour = "darkgrey";
                                name = "<S>" + name + "</S>";
                                break;
                        }
                    }

                    var text = "<FONT color=" + colour + ">" + name + "</FONT>";

                    if (cd.Conditions.Count != 0)
                    {
                        var conditions = "";
                        foreach (var oc in cd.Conditions)
                        {
                            if (conditions != "")
                                conditions += "; ";

                            conditions += oc.ToString(_fEncounter, true);
                        }

                        text += "<BR><FONT color=grey>" + conditions + "</FONT>";
                    }

                    var list = active ? lines : previous;

                    list.Add("<TR>");
                    list.Add("<TD align=center bgcolor=black width=50><FONT color=lightgrey>" + cd.Initiative +
                             "</FONT></TD>");
                    list.Add("<TD bgcolor=black>" + text + "</TD>");
                    list.Add("</TR>");
                }
            }

            lines.AddRange(previous);
            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("<HR>");

            lines.Add(EncounterLogView(true));

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Html.Concatenate(lines);
        }

        public string EncounterLogView(bool playerView)
        {
            var lines = new List<string>();

            if (!playerView)
            {
                lines.AddRange(Html.GetHead("Encounter Log", "", Session.Preferences.TextSize));
                lines.Add("<BODY>");
            }

            if (_fLog != null)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE class=wide>");

                lines.Add("<TR class=encounterlog>");
                lines.Add("<TD colspan=2>");
                lines.Add("<B>Encounter Log</B>");
                lines.Add("</TD>");
                lines.Add("<TD align=right>");
                lines.Add("<B>Round " + _fCurrentRound + "</B>");
                lines.Add("</TD>");
                lines.Add("</TR>");

                if (!_fLog.Active)
                {
                    lines.Add("<TR class=warning>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("The log is not yet active as the encounter has not started.");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                }

                var report = _fLog.CreateReport(_fEncounter, !playerView);
                foreach (var round in report.Rounds)
                {
                    lines.Add("<TR class=shaded>");
                    if (playerView)
                        lines.Add("<TD class=pvlogentry colspan=3>");
                    else
                        lines.Add("<TD colspan=3>");
                    lines.Add("<B>Round " + round.Round + "</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    if (round.Count == 0)
                    {
                        lines.Add("<TR>");
                        if (playerView)
                            lines.Add("<TD class=pvlogentry align=center colspan=3>");
                        else
                            lines.Add("<TD align=center colspan=3>");
                        lines.Add("(nothing)");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }

                    var detailedNames = !playerView || Session.Preferences.Combat.PlayerViewCreatureLabels;

                    foreach (var turn in round.Turns)
                    {
                        if (turn.Entries.Count == 0)
                            continue;

                        lines.Add("<TR>");
                        if (playerView)
                            lines.Add("<TD class=pvlogentry colspan=3>");
                        else
                            lines.Add("<TD colspan=2>");
                        lines.Add("<B>" + EncounterLog.GetName(turn.Id, _fEncounter, detailedNames) + "</B>");
                        lines.Add("</TD>");
                        if (!playerView)
                        {
                            lines.Add("<TD align=right>");
                            lines.Add(turn.Start.ToString("h:mm:ss"));
                            lines.Add("</TD>");
                        }

                        lines.Add("</TR>");

                        foreach (var entry in turn.Entries)
                        {
                            lines.Add("<TR>");
                            if (playerView)
                                lines.Add("<TD class=pvlogindent colspan=3>");
                            else
                                lines.Add("<TD class=indent colspan=3>");
                            lines.Add(entry.Description(_fEncounter, detailedNames));
                            lines.Add("</TD>");
                            lines.Add("</TR>");
                        }
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (!playerView)
            {
                lines.Add("</BODY>");
                lines.Add("</HTML>");
            }

            return Html.Concatenate(lines);
        }

        private class InitiativeSorter : IComparer, IComparer<ListViewItem>
        {
            private readonly Encounter _fEncounter;

            private readonly Dictionary<Guid, CombatData> _fTrapData;

            public InitiativeSorter(Dictionary<Guid, CombatData> trapData, Encounter enc)
            {
                _fTrapData = trapData;
                _fEncounter = enc;
            }

            private int get_score(ListViewItem lvi)
            {
                try
                {
                    if (lvi.Tag is Hero)
                    {
                        var hero = lvi.Tag as Hero;
                        return hero.CombatData.Initiative;
                    }

                    if (lvi.Tag is CreatureToken)
                    {
                        var data = lvi.Tag as CreatureToken;
                        return data.Data.Initiative;
                    }

                    if (lvi.Tag is Trap)
                    {
                        var trap = lvi.Tag as Trap;
                        if (_fTrapData.ContainsKey(trap.Id))
                            return _fTrapData[trap.Id].Initiative;
                        return int.MinValue;
                    }
                }
                catch (Exception ex)
                {
                    LogSystem.Trace(ex);
                }

                return 0;
            }

            private int get_bonus(ListViewItem lvi)
            {
                try
                {
                    if (lvi.Tag is Hero)
                    {
                        var hero = lvi.Tag as Hero;
                        return hero.InitBonus;
                    }

                    if (lvi.Tag is CreatureToken)
                    {
                        var ct = lvi.Tag as CreatureToken;
                        var slot = _fEncounter.FindSlot(ct.SlotId);
                        return slot != null ? slot.Card.Initiative : 0;
                    }

                    if (lvi.Tag is Trap)
                    {
                        var trap = lvi.Tag as Trap;
                        return trap.Initiative != int.MinValue ? trap.Initiative : 0;
                    }
                }
                catch (Exception ex)
                {
                    LogSystem.Trace(ex);
                }

                return 0;
            }

            public int Compare(object x, object y)
            {
                var lviX = x as ListViewItem;
                var lviY = y as ListViewItem;

                if (lviX == null || lviY == null)
                    return 0;

                return Compare(lviX, lviY);
            }

            public int Compare(ListViewItem lviX, ListViewItem lviY)
            {
                var scoreX = get_score(lviX);
                var scoreY = get_score(lviY);

                var result = scoreX.CompareTo(scoreY);

                if (result == 0)
                {
                    var bonusX = get_bonus(lviX);
                    var bonusY = get_bonus(lviY);

                    result = bonusX.CompareTo(bonusY);
                }

                if (result == 0)
                {
                    var textX = lviX.Text;
                    var textY = lviY.Text;

                    result = textX.CompareTo(textY) * -1;
                }

                return -result;
            }
        }

        public class CombatListControl : ListView
        {
            public CombatListControl()
            {
                DoubleBuffered = true;
            }
        }

        private enum GroupIndexes
        {
            Combatants = -1,
            Delayed = 0,
            Traps = 1,
            SkillChallenges = 2,
            Custom = 3,
            Inactive = 4,
            Defeated = 5
        }

        private static class Inits
        {
            public static readonly string Automatic = "automatically";
            public static readonly string Manual = "manually";
            public static readonly string Individual = "individually";
            public static readonly string InGroups = "in groups";
            public static readonly string Rolled = "calculated automatically";
            public static readonly string Entered = "entered manually";
            public static string Ingroups = " (grouped by type)";
        }
    }
}
