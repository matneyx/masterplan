using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Tools;
using Masterplan.Tools.Generators;
using Masterplan.Tools.IO;
using Masterplan.Wizards;

namespace Masterplan.UI
{
    internal enum ListMode
    {
        Creatures,
        Templates,
        NpCs,
        Traps,
        SkillChallenges
    }

    internal partial class EncounterBuilderForm : Form
    {
        private readonly bool _fAddingThreats;

        private ListMode _fMode = ListMode.Creatures;

        private int _fPartyLevel = Session.Project.Party.Level;
        private int _fPartySize = Session.Project.Party.Size;

        public Encounter Encounter { get; } = new Encounter();

        public EncounterSlot SelectedSlot
        {
            get
            {
                if (SlotList.SelectedItems.Count != 0)
                    return SlotList.SelectedItems[0].Tag as EncounterSlot;

                return null;
            }
        }

        public Trap SelectedSlotTrap
        {
            get
            {
                if (SlotList.SelectedItems.Count != 0)
                    return SlotList.SelectedItems[0].Tag as Trap;

                return null;
            }
        }

        public SkillChallenge SelectedSlotSkillChallenge
        {
            get
            {
                if (SlotList.SelectedItems.Count != 0)
                    return SlotList.SelectedItems[0].Tag as SkillChallenge;

                return null;
            }
        }

        public ICreature SelectedCreature
        {
            get
            {
                if (SourceItemList.SelectedItems.Count != 0)
                    return SourceItemList.SelectedItems[0].Tag as ICreature;

                return null;
            }
        }

        public CreatureTemplate SelectedTemplate
        {
            get
            {
                if (SourceItemList.SelectedItems.Count != 0)
                    return SourceItemList.SelectedItems[0].Tag as CreatureTemplate;

                return null;
            }
        }

        public Npc SelectedNpc
        {
            get
            {
                if (SourceItemList.SelectedItems.Count != 0)
                    return SourceItemList.SelectedItems[0].Tag as Npc;

                return null;
            }
        }

        public Trap SelectedTrap
        {
            get
            {
                if (SourceItemList.SelectedItems.Count != 0)
                    return SourceItemList.SelectedItems[0].Tag as Trap;

                return null;
            }
        }

        public SkillChallenge SelectedSkillChallenge
        {
            get
            {
                if (SourceItemList.SelectedItems.Count != 0)
                    return SourceItemList.SelectedItems[0].Tag as SkillChallenge;

                return null;
            }
        }

        public IToken SelectedMapThreat
        {
            get
            {
                if (MapThreatList.SelectedItems.Count != 0)
                    return MapThreatList.SelectedItems[0].Tag as IToken;

                return null;
            }
        }

        private EncounterNote SelectedNote
        {
            get
            {
                if (NoteList.SelectedItems.Count != 0)
                    return NoteList.SelectedItems[0].Tag as EncounterNote;

                return null;
            }
            set
            {
                NoteList.SelectedItems.Clear();

                if (value != null)
                    foreach (ListViewItem lvi in NoteList.Items)
                    {
                        var en = lvi.Tag as EncounterNote;
                        if (en != null && en.Id == value.Id)
                            lvi.Selected = true;
                    }

                update_selected_note();
            }
        }

        public EncounterBuilderForm(Encounter enc, int partyLevel, bool addingThreats)
        {
            InitializeComponent();

            _fMode = ListMode.Creatures;
            Encounter = enc.Copy() as Encounter;
            _fPartyLevel = partyLevel;
            _fAddingThreats = addingThreats;

            SourceItemList.ListViewItemSorter = new SourceSorter();
            NoteDetails.DocumentText = "";

            ToolsUseDeck.Visible = Session.Project.Decks.Count != 0;

            FilterPanel.Mode = _fMode;
            FilterPanel.PartyLevel = _fPartyLevel;
            FilterPanel.FilterByPartyLevel();

            Application.Idle += Application_Idle;

            if (_fAddingThreats)
            {
                Pages.TabPages.Remove(MapPage);
                Pages.TabPages.Remove(NotesPage);

                VSplitter.Panel2Collapsed = true;
            }
            else
            {
                var m = Encounter.MapId != Guid.Empty ? Session.Project.FindTacticalMap(Encounter.MapId) : null;
                if (m != null)
                {
                    MapView.Map = m;
                    MapView.Encounter = Encounter;

                    var ma = Encounter.MapAreaId != Guid.Empty ? m.FindArea(Encounter.MapAreaId) : null;
                    MapView.Viewpoint = ma?.Region ?? Rectangle.Empty;
                }

                update_difficulty_list();
                update_mapthreats();
                update_notes();
                update_selected_note();
            }

            update_source_list();
            update_encounter();
            update_party_label();
        }

        ~EncounterBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                if (Pages.SelectedTab == ThreatsPage)
                {
                    var selected = SelectedSlot != null || SelectedSlotTrap != null ||
                                   SelectedSlotSkillChallenge != null;

                    AddBtn.Enabled = selected;
                    AddBtn.Visible = SelectedSlot != null || SelectedSlotTrap != null;
                    RemoveBtn.Enabled = selected;
                    StatBlockBtn.Enabled = selected;

                    if (SelectedSlotTrap != null || SelectedSlotSkillChallenge != null)
                        RemoveBtn.Text = "Remove";
                    else
                        RemoveBtn.Text = "-";

                    CreaturesBtn.Visible = Session.Creatures.Count != 0;
                    TrapsBtn.Visible = Session.Traps.Count != 0;
                    ChallengesBtn.Visible = Session.SkillChallenges.Count != 0;

                    CreaturesBtn.Checked = _fMode == ListMode.Creatures;
                    TrapsBtn.Checked = _fMode == ListMode.Traps;
                    ChallengesBtn.Checked = _fMode == ListMode.SkillChallenges;
                }

                if (Pages.SelectedTab == MapPage)
                {
                    MapToolsLOS.Checked = MapView.LineOfSight;
                    MapToolsGridlines.Checked = MapView.ShowGrid != MapGridMode.None;
                    MapToolsGridLabels.Checked = MapView.ShowGridLabels;
                    MapToolsPictureTokens.Checked = MapView.ShowPictureTokens;
                    MapToolsPrint.Enabled = MapView.Map != null;
                    MapToolsScreenshot.Enabled = MapView.Map != null;

                    MapSplitter.Panel2Collapsed = MapView.Map == null || MapThreatList.Items.Count == 0;

                    MapContextView.Enabled = MapView.SelectedTokens.Count == 1;
                    MapContextSetPicture.Enabled = MapView.SelectedTokens.Count == 1;
                    MapContextRemove.Enabled = MapView.SelectedTokens.Count != 0;
                    MapContextRemoveEncounter.Enabled = MapView.SelectedTokens.Count != 0;
                    MapContextCopy.Enabled =
                        MapView.SelectedTokens.Count == 1 && MapView.SelectedTokens[0] is CustomToken;

                    if (MapView.SelectedTokens.Count == 1)
                    {
                        MapContextVisible.Enabled = true;

                        var token = MapView.SelectedTokens[0];

                        if (token is CreatureToken)
                        {
                            var ct = token as CreatureToken;
                            MapContextVisible.Checked = ct.Data.Visible;
                        }

                        if (token is CustomToken)
                        {
                            var ct = token as CustomToken;
                            MapContextVisible.Checked = ct.Data.Visible;
                        }
                    }
                    else
                    {
                        MapContextVisible.Enabled = false;
                        MapContextVisible.Checked = false;
                    }
                }

                if (Pages.SelectedTab == NotesPage)
                {
                    NoteRemoveBtn.Enabled = SelectedNote != null;
                    NoteEditBtn.Enabled = SelectedNote != null;
                    NoteUpBtn.Enabled = SelectedNote != null && Encounter.Notes.IndexOf(SelectedNote) != 0;
                    NoteDownBtn.Enabled = SelectedNote != null &&
                                          Encounter.Notes.IndexOf(SelectedNote) != Encounter.Notes.Count - 1;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    SelectedSlot.Card.LevelAdjustment += 1;

                    update_encounter();
                }
                else
                {
                    var cd = new CombatData();
                    SelectedSlot.CombatData.Add(cd);

                    update_encounter();
                    update_mapthreats();
                }
            }

            if (SelectedSlotTrap != null)
            {
                var trap = SelectedSlotTrap.Copy();
                trap.Id = Guid.NewGuid();

                Encounter.Traps.Add(trap);

                update_encounter();
            }

            if (SelectedSlotSkillChallenge != null)
            {
                var sc = SelectedSlotSkillChallenge.Copy() as SkillChallenge;
                sc.Id = Guid.NewGuid();

                Encounter.SkillChallenges.Add(sc);

                update_encounter();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                {
                    if (SelectedSlot.Card.Level > 1)
                    {
                        SelectedSlot.Card.LevelAdjustment -= 1;

                        update_encounter();
                    }
                }
                else
                {
                    SelectedSlot.CombatData.RemoveAt(SelectedSlot.CombatData.Count - 1);

                    if (SelectedSlot.CombatData.Count == 0)
                        Encounter.Slots.Remove(SelectedSlot);

                    update_encounter();
                    update_mapthreats();
                }
            }

            if (SelectedSlotTrap != null)
            {
                Encounter.Traps.Remove(SelectedSlotTrap);

                update_encounter();
            }

            if (SelectedSlotSkillChallenge != null)
            {
                Encounter.SkillChallenges.Remove(SelectedSlotSkillChallenge);

                update_encounter();
            }
        }

        private void StatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                var dlg = new CreatureDetailsForm(SelectedSlot.Card);
                dlg.ShowDialog();
            }

            if (SelectedSlotTrap != null)
            {
                var dlg = new TrapDetailsForm(SelectedSlotTrap);
                dlg.ShowDialog();
            }

            if (SelectedSlotSkillChallenge != null)
            {
                var dlg = new SkillChallengeDetailsForm(SelectedSlotSkillChallenge);
                dlg.ShowDialog();
            }
        }

        private void EditStatBlock_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                var id = SelectedSlot.Card.CreatureId;

                var cc = Session.Project.FindCustomCreature(id);
                var npc = Session.Project.FindNpc(id);

                if (cc != null)
                {
                    var index = Session.Project.CustomCreatures.IndexOf(cc);

                    var dlg = new CreatureBuilderForm(cc);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        SelectedSlot.SetDefaultDisplayNames();

                        Session.Project.CustomCreatures[index] = dlg.Creature as CustomCreature;
                        Session.Modified = true;

                        update_encounter();
                        update_source_list();
                        update_mapthreats();
                    }
                }
                else if (npc != null)
                {
                    var index = Session.Project.NpCs.IndexOf(npc);

                    var dlg = new CreatureBuilderForm(npc);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        SelectedSlot.SetDefaultDisplayNames();

                        Session.Project.NpCs[index] = dlg.Creature as Npc;
                        Session.Modified = true;

                        update_encounter();
                        update_source_list();
                        update_mapthreats();
                    }
                }
                else
                {
                    var msg =
                        "You're about to edit a creature's stat block. Do you want to change this creature globally?";
                    msg += Environment.NewLine;
                    msg += Environment.NewLine;
                    msg +=
                        "Press Yes to apply your changes to this creature, everywhere it appears, even in other projects. Select this option if you're correcting an error in the creature's stat block.";
                    msg += Environment.NewLine;
                    msg += Environment.NewLine;
                    msg +=
                        "Press No to apply your changes to a copy of this creature. Select this option if you're modifying or re-skinning the creature for this encounter only, leaving other encounters as they are.";

                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Information);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                        {
                            // Edit the base creature
                            var original = Session.FindCreature(id, SearchType.Global) as Creature;
                            var lib = Session.FindLibrary(original);
                            var index = lib.Creatures.IndexOf(original);

                            var dlg = new CreatureBuilderForm(original);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                SelectedSlot.SetDefaultDisplayNames();

                                lib.Creatures[index] = dlg.Creature as Creature;

                                // Save the library
                                var filename = Session.GetLibraryFilename(lib);
                                Serialisation<Library>.Save(filename, lib, SerialisationMode.Binary);

                                update_encounter();
                                update_source_list();
                                update_mapthreats();
                            }
                        }
                            break;
                        case DialogResult.No:
                        {
                            // Make it into a custom creature
                            var original = Session.FindCreature(id, SearchType.Global);
                            var creature = new CustomCreature(original);
                            CreatureHelper.AdjustCreatureLevel(creature, SelectedSlot.Card.LevelAdjustment);
                            creature.Id = Guid.NewGuid();

                            var dlg = new CreatureBuilderForm(creature);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Session.Project.CustomCreatures.Add(dlg.Creature as CustomCreature);
                                Session.Modified = true;

                                SelectedSlot.Card.CreatureId = dlg.Creature.Id;
                                SelectedSlot.Card.LevelAdjustment = 0;
                                SelectedSlot.SetDefaultDisplayNames();

                                update_encounter();
                                update_source_list();
                                update_mapthreats();
                            }
                        }
                            break;
                    }
                }
            }

            if (SelectedSlotTrap != null)
            {
                var index = Encounter.Traps.IndexOf(SelectedSlotTrap);

                var dlg = new TrapBuilderForm(SelectedSlotTrap);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    dlg.Trap.Id = Guid.NewGuid();

                    Encounter.Traps[index] = dlg.Trap;

                    update_encounter();
                }
            }

            if (SelectedSlotSkillChallenge != null)
            {
                var index = Encounter.SkillChallenges.IndexOf(SelectedSlotSkillChallenge);

                var dlg = new SkillChallengeBuilderForm(SelectedSlotSkillChallenge);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    dlg.SkillChallenge.Id = Guid.NewGuid();

                    Encounter.SkillChallenges[index] = dlg.SkillChallenge;

                    update_encounter();
                }
            }
        }

        private void count_slot_as(object sender, EventArgs e)
        {
            if (SelectedSlot != null)
            {
                var tsmi = sender as ToolStripMenuItem;
                var type = (EncounterSlotType)tsmi.Tag;

                SelectedSlot.Type = type;
                update_encounter();
            }
        }

        private void EditRemoveTemplate_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null && SelectedSlot.Card.TemplateIDs.Count != 0)
            {
                SelectedSlot.Card.TemplateIDs.Clear();

                update_encounter();
                update_mapthreats();
            }
        }

        private void EditRemoveLevelAdj_Click(object sender, EventArgs e)
        {
            if (SelectedSlot != null && SelectedSlot.Card.LevelAdjustment != 0)
            {
                SelectedSlot.Card.LevelAdjustment = 0;

                update_encounter();
                update_mapthreats();
            }
        }

        private void SwapStandard_Click(object sender, EventArgs e)
        {
            var card = SelectedSlot.Card;
            var selected = Session.FindCreature(card.CreatureId, SearchType.Global);

            // Work out how many to use
            var count = 1;
            if (selected.Role is Minion)
                count = SelectedSlot.CombatData.Count / 4;
            else
                switch (card.Flag)
                {
                    case RoleFlag.Standard:
                        count = SelectedSlot.CombatData.Count;
                        break;
                    case RoleFlag.Elite:
                        count = SelectedSlot.CombatData.Count * 2;
                        break;
                    case RoleFlag.Solo:
                        count = SelectedSlot.CombatData.Count * 5;
                        break;
                }

            // Find all standard creatures of this level and role
            var creatures = find_creatures(RoleFlag.Standard, card.Level, card.Roles);
            if (creatures.Count == 0)
            {
                var msg = "There are no creatures of this type.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var creature = choose_creature(creatures, selected.Category);
            if (creature == null)
                return;

            perform_swap(creature, count, SelectedSlot);
        }

        private void SwapElite_Click(object sender, EventArgs e)
        {
            var card = SelectedSlot.Card;
            var selected = Session.FindCreature(card.CreatureId, SearchType.Global);

            // Work out how many to use
            var count = 1;
            if (selected.Role is Minion)
                count = SelectedSlot.CombatData.Count / 8;
            else
                switch (card.Flag)
                {
                    case RoleFlag.Standard:
                        count = SelectedSlot.CombatData.Count / 2;
                        break;
                    case RoleFlag.Elite:
                        count = SelectedSlot.CombatData.Count;
                        break;
                    case RoleFlag.Solo:
                        count = SelectedSlot.CombatData.Count * 5 / 2;
                        break;
                }

            // Find all elite creatures of this level and role
            var creatures = find_creatures(RoleFlag.Elite, card.Level, card.Roles);
            if (creatures.Count == 0)
            {
                var msg = "There are no creatures of this type.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var creature = choose_creature(creatures, selected.Category);
            if (creature == null)
                return;

            perform_swap(creature, count, SelectedSlot);
        }

        private void SwapSolo_Click(object sender, EventArgs e)
        {
            var card = SelectedSlot.Card;
            var selected = Session.FindCreature(card.CreatureId, SearchType.Global);

            // Work out how many to use
            var count = 1;
            if (selected.Role is Minion)
                count = SelectedSlot.CombatData.Count / 20;
            else
                switch (card.Flag)
                {
                    case RoleFlag.Standard:
                        count = SelectedSlot.CombatData.Count / 5;
                        break;
                    case RoleFlag.Elite:
                        count = SelectedSlot.CombatData.Count * 2 / 5;
                        break;
                    case RoleFlag.Solo:
                        count = SelectedSlot.CombatData.Count;
                        break;
                }

            // Find all solo creatures of this level and role
            var creatures = find_creatures(RoleFlag.Solo, card.Level, card.Roles);
            if (creatures.Count == 0)
            {
                var msg = "There are no creatures of this type.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var creature = choose_creature(creatures, selected.Category);
            if (creature == null)
                return;

            perform_swap(creature, count, SelectedSlot);
        }

        private void SwapMinions_Click(object sender, EventArgs e)
        {
            var card = SelectedSlot.Card;
            var selected = Session.FindCreature(card.CreatureId, SearchType.Global);

            // Work out how many to use
            var count = 1;
            if (selected.Role is Minion)
                count = SelectedSlot.CombatData.Count / 4;
            else
                switch (card.Flag)
                {
                    case RoleFlag.Standard:
                        count = SelectedSlot.CombatData.Count * 4;
                        break;
                    case RoleFlag.Elite:
                        count = SelectedSlot.CombatData.Count * 8;
                        break;
                    case RoleFlag.Solo:
                        count = SelectedSlot.CombatData.Count * 20;
                        break;
                }

            // Find all minions of this level
            var creatures = find_minions(card.Level);
            if (creatures.Count == 0)
            {
                var msg = "There are no creatures of this type.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var creature = choose_creature(creatures, selected.Category);
            if (creature == null)
                return;

            perform_swap(creature, count, SelectedSlot);
        }

        private List<Creature> find_creatures(RoleFlag flag, int level, List<RoleType> roles)
        {
            var creatures = new List<Creature>();

            foreach (var lib in Session.Libraries)
            foreach (var c in lib.Creatures)
            {
                if (c.Role is Minion)
                    continue;

                var role = c.Role as ComplexRole;

                if (role.Flag == flag && c.Level == level)
                    if (roles.Count == 0 || roles.Contains(role.Type))
                        creatures.Add(c);
            }

            return creatures;
        }

        private List<Creature> find_minions(int level)
        {
            var creatures = new List<Creature>();

            foreach (var lib in Session.Libraries)
            foreach (var c in lib.Creatures)
                if (c.Role is Minion && c.Level == level)
                    creatures.Add(c);

            return creatures;
        }

        private Creature choose_creature(List<Creature> creatures, string category)
        {
            var dlg = new CreatureSelectForm(creatures);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var card = dlg.Creature;
                return Session.FindCreature(card.CreatureId, SearchType.Global) as Creature;
            }

            return null;
        }

        private void perform_swap(Creature creature, int count, EncounterSlot oldSlot)
        {
            var newslot = new EncounterSlot();
            newslot.Card.CreatureId = creature.Id;
            for (var n = 0; n != count; ++n)
            {
                var ccd = new CombatData();
                newslot.CombatData.Add(ccd);
            }

            Encounter.Slots.Remove(oldSlot);
            Encounter.Slots.Add(newslot);

            update_encounter();
            update_mapthreats();
        }

        private void EditApplyTheme_Click(object sender, EventArgs e)
        {
            if (SelectedSlot == null)
                return;

            var dlg = new ThemeForm(SelectedSlot.Card);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedSlot.Card = dlg.Card;

                update_encounter();
                update_mapthreats();
            }
        }

        private void EditClearTheme_Click(object sender, EventArgs e)
        {
            if (SelectedSlot == null)
                return;

            SelectedSlot.Card.ThemeId = Guid.Empty;

            SelectedSlot.Card.ThemeAttackPowerId = Guid.Empty;
            SelectedSlot.Card.ThemeUtilityPowerId = Guid.Empty;

            update_encounter();
            update_mapthreats();
        }

        private void ToolsClearAll_Click(object sender, EventArgs e)
        {
            Encounter.Slots.Clear();
            Encounter.Traps.Clear();
            Encounter.SkillChallenges.Clear();

            update_encounter();
            update_mapthreats();
        }

        private void ToolsUseTemplate_Click(object sender, EventArgs e)
        {
            var templates = EncounterBuilder.FindTemplates(Encounter, _fPartyLevel, true);

            if (templates.Count == 0)
            {
                var msg = "There are no encounter templates which match the creatures already in the encounter.";
                msg += Environment.NewLine;
                msg += "This does not mean there is a problem with your encounter.";

                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return;
            }

            var wizard = new EncounterTemplateWizard(templates, Encounter, _fPartyLevel);
            if (wizard.Show() == DialogResult.OK)
            {
                update_encounter();
                update_mapthreats();
            }
        }

        private void ToolsMenu_DropDownOpening(object sender, EventArgs e)
        {
            ToolsUseDeck.DropDownItems.Clear();

            foreach (var deck in Session.Project.Decks)
            {
                if (deck.Cards.Count == 0)
                    continue;

                var tsmi = new ToolStripMenuItem(deck.Name + " (" + deck.Cards.Count + " cards)");
                tsmi.Tag = deck;
                tsmi.Click += use_deck;

                ToolsUseDeck.DropDownItems.Add(tsmi);
            }

            if (ToolsUseDeck.DropDownItems.Count == 0)
            {
                var tsmi = new ToolStripMenuItem("(no decks)");
                tsmi.ForeColor = SystemColors.GrayText;

                ToolsUseDeck.DropDownItems.Add(tsmi);
            }
        }

        private void use_deck(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            var deck = tsmi.Tag as EncounterDeck;

            deck.DrawEncounter(Encounter);

            if (deck.Cards.Count == 0)
                Session.Project.Decks.Remove(deck);

            update_encounter();
            update_mapthreats();
        }

        private void ToolsAddCreature_Click(object sender, EventArgs e)
        {
            try
            {
                var creature = new CustomCreature();
                creature.Name = "Custom Creature";
                creature.Level = _fPartyLevel;

                var dlg = new CreatureBuilderForm(creature);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.CustomCreatures.Add(dlg.Creature as CustomCreature);
                    Session.Modified = true;

                    add_opponent(dlg.Creature);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsAddTrap_Click(object sender, EventArgs e)
        {
            try
            {
                var trap = new Trap();
                trap.Name = "Custom Trap";
                trap.Level = _fPartyLevel;
                trap.Attacks.Add(new TrapAttack());

                var dlg = new TrapBuilderForm(trap);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Encounter.Traps.Add(dlg.Trap);
                    update_encounter();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsAddChallenge_Click(object sender, EventArgs e)
        {
            try
            {
                var sc = new SkillChallenge();
                sc.Name = "Custom Skill Challenge";
                sc.Level = _fPartyLevel;

                var dlg = new SkillChallengeBuilderForm(sc);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Encounter.SkillChallenges.Add(dlg.SkillChallenge);
                    update_encounter();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsApplyTheme_Click(object sender, EventArgs e)
        {
            var dlg = new MonsterThemeSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
                if (dlg.MonsterTheme != null)
                {
                    foreach (var slot in Encounter.Slots)
                    {
                        slot.Card.ThemeId = dlg.MonsterTheme.Id;

                        slot.Card.ThemeAttackPowerId = Guid.Empty;
                        slot.Card.ThemeUtilityPowerId = Guid.Empty;

                        var attacks = dlg.MonsterTheme.ListPowers(slot.Card.Roles, PowerType.Attack);
                        if (attacks.Count != 0)
                        {
                            var index = Session.Random.Next() % attacks.Count;
                            var power = attacks[index];
                            slot.Card.ThemeAttackPowerId = power.Power.Id;
                        }

                        var utilities = dlg.MonsterTheme.ListPowers(slot.Card.Roles, PowerType.Utility);
                        if (utilities.Count != 0)
                        {
                            var index = Session.Random.Next() % utilities.Count;
                            var power = utilities[index];
                            slot.Card.ThemeUtilityPowerId = power.Power.Id;
                        }
                    }

                    update_encounter();
                    update_mapthreats();
                }
        }

        private void AutoBuildBtn_Click(object sender, EventArgs e)
        {
            Autobuild(false);
        }

        private void AutoBuildAdvanced_Click(object sender, EventArgs e)
        {
            Autobuild(true);
        }

        private void Autobuild(bool advanced)
        {
            AutoBuildData data = null;

            if (advanced)
            {
                var dlg = new AutoBuildForm(AutoBuildForm.Mode.Encounter);
                if (dlg.ShowDialog() == DialogResult.OK)
                    data = dlg.Data;
                else
                    return;
            }
            else
            {
                data = new AutoBuildData();
            }

            data.Level = _fPartyLevel;

            var ok = EncounterBuilder.Build(data, Encounter, false);

            update_encounter();
            update_mapthreats();

            if (!ok)
            {
                var str =
                    "AutoBuild was unable to find enough creatures of the appropriate type to build an encounter.";
                MessageBox.Show(str, "Encounter Builder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            ViewTemplates.Enabled = Session.Templates.Count != 0;
            ViewNPCs.Enabled = Session.Project.NpCs.Count != 0;

            ViewNPCs.Checked = _fMode == ListMode.NpCs;
            ViewTemplates.Checked = _fMode == ListMode.Templates;

            ViewGroups.Checked = SourceItemList.ShowGroups;
        }

        private void ViewCreatures_Click(object sender, EventArgs e)
        {
            _fMode = ListMode.Creatures;
            FilterPanel.Mode = ListMode.Creatures;

            update_source_list();
        }

        private void ViewTemplates_Click(object sender, EventArgs e)
        {
            _fMode = ListMode.Templates;
            FilterPanel.Mode = ListMode.Templates;

            update_source_list();
        }

        private void ViewNPCs_Click(object sender, EventArgs e)
        {
            _fMode = ListMode.NpCs;
            FilterPanel.Mode = ListMode.NpCs;

            update_source_list();
        }

        private void ViewTraps_Click(object sender, EventArgs e)
        {
            _fMode = ListMode.Traps;
            FilterPanel.Mode = ListMode.Traps;

            update_source_list();
        }

        private void ViewChallenges_Click(object sender, EventArgs e)
        {
            _fMode = ListMode.SkillChallenges;
            FilterPanel.Mode = ListMode.SkillChallenges;

            update_source_list();
        }

        private void FilterPanel_FilterChanged(object sender, EventArgs e)
        {
            update_source_list();
        }

        private void SlotList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (SelectedSlot != null)
                {
                    Encounter.Slots.Remove(SelectedSlot);

                    update_encounter();

                    e.Handled = true;
                }
                else if (SelectedSlotSkillChallenge != null)
                {
                    Encounter.SkillChallenges.Remove(SelectedSlotSkillChallenge);

                    update_encounter();

                    e.Handled = true;
                }
                else if (SelectedSlotTrap != null)
                {
                    Encounter.Traps.Remove(SelectedSlotTrap);

                    update_encounter();

                    e.Handled = true;
                }
            }
        }

        private void SlotList_DoubleClick(object sender, EventArgs e)
        {
            StatBlockBtn_Click(sender, e);
        }

        private void ThreatList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCreature != null)
            {
                var card = new EncounterCard();
                card.CreatureId = SelectedCreature.Id;

                var dlg = new CreatureDetailsForm(card);
                dlg.ShowDialog();
            }

            if (SelectedTemplate != null)
            {
                var dlg = new CreatureTemplateDetailsForm(SelectedTemplate);
                dlg.ShowDialog();
            }

            if (SelectedTrap != null)
            {
                var dlg = new TrapDetailsForm(SelectedTrap);
                dlg.ShowDialog();
            }

            if (SelectedSkillChallenge != null)
            {
                var dlg = new SkillChallengeDetailsForm(SelectedSkillChallenge);
                dlg.ShowDialog();
            }
        }

        private void OpponentList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedCreature != null)
                DoDragDrop(SelectedCreature, DragDropEffects.All);

            if (SelectedTemplate != null)
                DoDragDrop(SelectedTemplate, DragDropEffects.All);

            if (SelectedNpc != null)
                DoDragDrop(SelectedNpc, DragDropEffects.All);

            if (SelectedTrap != null)
                DoDragDrop(SelectedTrap, DragDropEffects.All);

            if (SelectedSkillChallenge != null)
                DoDragDrop(SelectedSkillChallenge, DragDropEffects.All);
        }

        private void SlotList_DragOver(object sender, DragEventArgs e)
        {
            var creature = e.Data.GetData(typeof(Creature)) as Creature;
            if (creature != null)
                e.Effect = DragDropEffects.Copy;

            var custom = e.Data.GetData(typeof(CustomCreature)) as CustomCreature;
            if (custom != null)
                e.Effect = DragDropEffects.Copy;

            var npc = e.Data.GetData(typeof(Npc)) as Npc;
            if (npc != null)
                e.Effect = DragDropEffects.Copy;

            var trap = e.Data.GetData(typeof(Trap)) as Trap;
            if (trap != null)
                e.Effect = DragDropEffects.Copy;

            var sc = e.Data.GetData(typeof(SkillChallenge)) as SkillChallenge;
            if (sc != null)
                e.Effect = DragDropEffects.Copy;

            var template = e.Data.GetData(typeof(CreatureTemplate)) as CreatureTemplate;
            if (template != null)
            {
                var mouse = SlotList.PointToClient(Cursor.Position);
                var lvi = SlotList.GetItemAt(mouse.X, mouse.Y);
                lvi.Selected = true;

                var slot = lvi.Tag as EncounterSlot;
                if (slot != null && allow_template_drop(slot, template))
                    e.Effect = DragDropEffects.Copy;
                else
                    e.Effect = DragDropEffects.None;
            }
        }

        private void SlotList_DragDrop(object sender, DragEventArgs e)
        {
            var creature = e.Data.GetData(typeof(Creature)) as Creature;
            if (creature != null)
                add_opponent(creature);

            var custom = e.Data.GetData(typeof(CustomCreature)) as CustomCreature;
            if (custom != null)
                add_opponent(custom);

            var npc = e.Data.GetData(typeof(Npc)) as Npc;
            if (npc != null)
                add_opponent(npc);

            var trap = e.Data.GetData(typeof(Trap)) as Trap;
            if (trap != null)
                add_trap(trap);

            var sc = e.Data.GetData(typeof(SkillChallenge)) as SkillChallenge;
            if (sc != null)
                add_challenge(sc);

            var template = e.Data.GetData(typeof(CreatureTemplate)) as CreatureTemplate;
            if (template != null && SelectedSlot != null && allow_template_drop(SelectedSlot, template))
                add_template(template, SelectedSlot);
        }

        private bool allow_template_drop(EncounterSlot slot, CreatureTemplate template)
        {
            // You can't add the same template twice
            if (slot.Card.TemplateIDs.Contains(template.Id))
                return false;

            var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

            // You can't add a template to a minion
            if (creature.Role is Minion)
                return false;

            // You can't add a template to something which is already effectively a solo
            var role = creature.Role as ComplexRole;
            var steps = slot.Card.TemplateIDs.Count;
            switch (role.Flag)
            {
                case RoleFlag.Elite:
                    steps += 1;
                    break;
                case RoleFlag.Solo:
                    steps += 2;
                    break;
            }

            return steps < 2;
        }

        private void add_opponent(ICreature creature)
        {
            EncounterSlot slot = null;
            foreach (var es in Encounter.Slots)
                if (es.Card.CreatureId == creature.Id && es.Card.TemplateIDs.Count == 0)
                {
                    slot = es;
                    break;
                }

            if (slot == null)
            {
                slot = new EncounterSlot();
                slot.Card.CreatureId = creature.Id;

                Encounter.Slots.Add(slot);
            }

            var cd = new CombatData();
            cd.DisplayName = slot.Card.Title;
            slot.CombatData.Add(cd);

            update_encounter();
            update_mapthreats();
        }

        private void add_template(CreatureTemplate template, EncounterSlot slot)
        {
            slot.Card.TemplateIDs.Add(template.Id);

            update_encounter();
            update_mapthreats();
        }

        private void add_trap(Trap trap)
        {
            Encounter.Traps.Add(trap.Copy());
            update_encounter();
        }

        private void add_challenge(SkillChallenge sc)
        {
            var challenge = sc.Copy() as SkillChallenge;
            challenge.Level = _fPartyLevel;

            Encounter.SkillChallenges.Add(challenge);
            update_encounter();
        }

        private void update_difficulty_list()
        {
            var easyXp = _fPartySize *
                (Experience.GetCreatureXp(_fPartyLevel - 3) + Experience.GetCreatureXp(_fPartyLevel - 2)) / 2;
            var modXp = _fPartySize *
                (Experience.GetCreatureXp(_fPartyLevel - 1) + Experience.GetCreatureXp(_fPartyLevel - 0)) / 2;
            var hardXp = _fPartySize *
                (Experience.GetCreatureXp(_fPartyLevel + 1) + Experience.GetCreatureXp(_fPartyLevel + 2)) / 2;
            var extXp = _fPartySize *
                (Experience.GetCreatureXp(_fPartyLevel + 4) + Experience.GetCreatureXp(_fPartyLevel + 5)) / 2;

            easyXp = Math.Max(1, easyXp);
            modXp = Math.Max(1, modXp);
            hardXp = Math.Max(1, hardXp);
            extXp = Math.Max(1, extXp);

            DifficultyList.Items.Clear();

            var lviEasy = DifficultyList.Items.Add("Easy");
            lviEasy.SubItems.Add(easyXp + " - " + modXp);
            var minEasy = Math.Max(1, _fPartyLevel - 4);
            lviEasy.SubItems.Add(minEasy + " - " + (_fPartyLevel + 3));
            lviEasy.Tag = Difficulty.Easy;

            var lviMod = DifficultyList.Items.Add("Moderate");
            lviMod.SubItems.Add(modXp + " - " + hardXp);
            var minMod = Math.Max(1, _fPartyLevel - 3);
            lviMod.SubItems.Add(minMod + " - " + (_fPartyLevel + 3));
            lviMod.Tag = Difficulty.Moderate;

            var lviHard = DifficultyList.Items.Add("Hard");
            lviHard.SubItems.Add(hardXp + " - " + extXp);
            var minHard = Math.Max(1, _fPartyLevel - 3);
            lviHard.SubItems.Add(minHard + " - " + (_fPartyLevel + 5));
            lviHard.Tag = Difficulty.Hard;

            XPGauge.Party = new Party(_fPartySize, _fPartyLevel);
        }

        private void update_encounter()
        {
            SlotList.BeginUpdate();
            var state = ListState.GetState(SlotList);

            SlotList.Groups.Clear();
            SlotList.Items.Clear();

            SlotList.ShowGroups = Encounter.Count != 0 || Encounter.Traps.Count != 0 ||
                                  Encounter.SkillChallenges.Count != 0;

            if (Encounter.Count != 0)
            {
                foreach (var slot in Encounter.AllSlots)
                    slot.SetDefaultDisplayNames();

                SlotList.Groups.Add("Combatants", "Combatants");
                foreach (var ew in Encounter.Waves)
                    SlotList.Groups.Add(ew.Name, ew.Name);

                foreach (var slot in Encounter.AllSlots)
                {
                    var name = slot.Card.Title;

                    var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                    var lvi = SlotList.Items.Add(name);
                    lvi.SubItems.Add(slot.Card.Info);
                    lvi.SubItems.Add(slot.CombatData.Count.ToString());
                    lvi.SubItems.Add(slot.Xp.ToString());
                    lvi.Tag = slot;

                    if (creature != null)
                    {
                        var ew = Encounter.FindWave(slot);
                        lvi.Group = SlotList.Groups[ew == null ? "Combatants" : ew.Name];
                    }

                    if (creature == null)
                    {
                        lvi.ForeColor = Color.Red;
                    }
                    else
                    {
                        var diff = Ai.GetThreatDifficulty(creature.Level + slot.Card.LevelAdjustment, _fPartyLevel);
                        if (diff == Difficulty.Trivial)
                            lvi.ForeColor = Color.Green;
                        if (diff == Difficulty.Extreme)
                            lvi.ForeColor = Color.Red;
                    }
                }
            }
            else
            {
                SlotList.Groups.Add("Creatures", "Creatures");

                var lvi = SlotList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
                lvi.Group = SlotList.Groups["Creatures"];
            }

            if (Encounter.Traps.Count != 0)
            {
                SlotList.Groups.Add("Traps / Hazards", "Traps / Hazards");

                foreach (var trap in Encounter.Traps)
                {
                    var lvi = SlotList.Items.Add(trap.Name);
                    lvi.SubItems.Add(trap.Info);
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(trap.Xp.ToString());
                    lvi.Tag = trap;

                    lvi.Group = SlotList.Groups["Traps / Hazards"];

                    var diff = Ai.GetThreatDifficulty(trap.Level, _fPartyLevel);
                    if (diff == Difficulty.Trivial)
                        lvi.ForeColor = Color.Green;
                    if (diff == Difficulty.Extreme)
                        lvi.ForeColor = Color.Red;
                }
            }

            if (Encounter.SkillChallenges.Count != 0)
            {
                SlotList.Groups.Add("Skill Challenges", "Skill Challenges");

                foreach (var sc in Encounter.SkillChallenges)
                {
                    var lvi = SlotList.Items.Add(sc.Name);
                    lvi.SubItems.Add(sc.Info);
                    lvi.SubItems.Add("");
                    lvi.SubItems.Add(sc.GetXp().ToString());
                    lvi.Tag = sc;

                    lvi.Group = SlotList.Groups["Skill Challenges"];

                    var diff = sc.GetDifficulty(_fPartyLevel, _fPartySize);
                    if (diff == Difficulty.Trivial)
                        lvi.ForeColor = Color.Green;
                    if (diff == Difficulty.Extreme)
                        lvi.ForeColor = Color.Red;
                }
            }

            ListState.SetState(SlotList, state);
            SlotList.EndUpdate();

            var encDiff = Encounter.GetDifficulty(_fPartyLevel, _fPartySize);
            foreach (ListViewItem lvi in DifficultyList.Items)
            {
                var lviDiff = (Difficulty)lvi.Tag;

                lvi.BackColor = encDiff == lviDiff ? Color.Gray : SystemColors.Window;
                lvi.Font = encDiff == lviDiff ? new Font(Font, Font.Style | FontStyle.Bold) : Font;
            }

            var xp = Encounter.GetXp();
            XPGauge.Xp = xp;
            XPLbl.Text = "XP: " + xp;

            var level = Experience.GetCreatureLevel(xp / _fPartySize);
            LevelLbl.Text = "Level: " + Math.Max(level, 1);

            DiffLbl.Text = "Difficulty: " + Encounter.GetDifficulty(_fPartyLevel, _fPartySize);
            CountLbl.Text = "Opponents: " + Encounter.Count;
        }

        private void update_source_list()
        {
            Cursor.Current = Cursors.WaitCursor;
            SourceItemList.BeginUpdate();

            try
            {
                SourceItemList.Items.Clear();
                SourceItemList.Groups.Clear();
                SourceItemList.ShowGroups = true;

                switch (_fMode)
                {
                    case ListMode.Creatures:
                    {
                        var creatures = Session.Creatures;

                        var bst = new BinarySearchTree<string>();
                        foreach (var c in creatures)
                            if (c.Category != null && c.Category != "")
                                bst.Add(c.Category);

                        var cats = bst.SortedList;
                        cats.Insert(0, "Custom Creatures");
                        cats.Add("Miscellaneous Creatures");

                        foreach (var cat in cats)
                            SourceItemList.Groups.Add(cat, cat);

                        var listItems = new List<ListViewItem>();

                        foreach (var c in Session.Project.CustomCreatures)
                        {
                            var lvi = add_creature_to_list(c);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        foreach (var c in creatures)
                        {
                            var lvi = add_creature_to_list(c);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        SourceItemList.Items.AddRange(listItems.ToArray());

                        if (SourceItemList.Items.Count == 0)
                        {
                            SourceItemList.ShowGroups = false;

                            var lvi = SourceItemList.Items.Add("(no creatures)");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                    }
                        break;
                    case ListMode.Templates:
                    {
                        var templates = Session.Templates;

                        var lvgFunctional = SourceItemList.Groups.Add("Functional Templates", "Functional Templates");
                        var lvgClass = SourceItemList.Groups.Add("Class Templates", "Class Templates");

                        var listItems = new List<ListViewItem>();

                        foreach (var ct in templates)
                        {
                            var lvg = ct.Type == CreatureTemplateType.Functional ? lvgFunctional : lvgClass;
                            var lvi = add_template_to_list(ct, lvg);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        SourceItemList.Items.AddRange(listItems.ToArray());

                        if (SourceItemList.Items.Count == 0)
                        {
                            SourceItemList.ShowGroups = false;

                            var lvi = SourceItemList.Items.Add("(no templates)");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                    }
                        break;
                    case ListMode.NpCs:
                    {
                        var lvg = SourceItemList.Groups.Add("NPCs", "NPCs");

                        var listItems = new List<ListViewItem>();

                        foreach (var npc in Session.Project.NpCs)
                        {
                            var lvi = add_npc_to_list(npc, lvg);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        SourceItemList.Items.AddRange(listItems.ToArray());

                        if (SourceItemList.Items.Count == 0)
                        {
                            SourceItemList.ShowGroups = false;

                            var lvi = SourceItemList.Items.Add("(no npcs)");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                    }
                        break;
                    case ListMode.Traps:
                    {
                        var traps = Session.Traps;

                        var trapGroup = SourceItemList.Groups.Add("Traps", "Traps");
                        var hazardGroup = SourceItemList.Groups.Add("Hazards", "Hazards");
                        var terrainGroup = SourceItemList.Groups.Add("Terrain", "Terrain");

                        var listItems = new List<ListViewItem>();

                        foreach (var trap in traps)
                        {
                            ListViewGroup lvg = null;
                            switch (trap.Type)
                            {
                                case TrapType.Trap:
                                    lvg = trapGroup;
                                    break;
                                case TrapType.Hazard:
                                    lvg = hazardGroup;
                                    break;
                                case TrapType.Terrain:
                                    lvg = terrainGroup;
                                    break;
                            }

                            var lvi = add_trap_to_list(trap, lvg);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        SourceItemList.Items.AddRange(listItems.ToArray());

                        if (SourceItemList.Items.Count == 0)
                        {
                            SourceItemList.ShowGroups = false;

                            var lvi = SourceItemList.Items.Add("(no traps)");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                    }
                        break;
                    case ListMode.SkillChallenges:
                    {
                        var challenges = Session.SkillChallenges;

                        var listItems = new List<ListViewItem>();

                        foreach (var sc in challenges)
                        {
                            var lvi = add_challenge_to_list(sc);
                            if (lvi != null)
                                listItems.Add(lvi);
                        }

                        SourceItemList.Items.AddRange(listItems.ToArray());

                        if (SourceItemList.Items.Count == 0)
                        {
                            SourceItemList.ShowGroups = false;

                            var lvi = SourceItemList.Items.Add("(no skill challenges)");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                    }
                        break;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            SourceItemList.EndUpdate();
            Cursor.Current = Cursors.Default;
        }

        private ListViewItem add_creature_to_list(ICreature c)
        {
            if (c == null)
                return null;

            Difficulty diff;
            if (!FilterPanel.AllowItem(c, out diff))
                return null;

            var lvi = new ListViewItem(c.Name);
            var lvsi = lvi.SubItems.Add(c.Info);
            lvi.Tag = c;

            lvi.UseItemStyleForSubItems = false;
            lvsi.ForeColor = SystemColors.GrayText;

            switch (diff)
            {
                case Difficulty.Trivial:
                    lvi.ForeColor = Color.Green;
                    break;
                case Difficulty.Extreme:
                    lvi.ForeColor = Color.Maroon;
                    break;
            }

            if (c is CustomCreature)
            {
                lvi.Group = SourceItemList.Groups["Custom Creatures"];
            }
            else
            {
                if (c.Category != null && c.Category != "")
                    lvi.Group = SourceItemList.Groups[c.Category];
                else
                    lvi.Group = SourceItemList.Groups["Miscellaneous Creatures"];
            }

            return lvi;
        }

        private ListViewItem add_template_to_list(CreatureTemplate ct, ListViewGroup group)
        {
            if (ct == null)
                return null;

            Difficulty diff;
            if (!FilterPanel.AllowItem(ct, out diff))
                return null;

            var lvi = new ListViewItem(ct.Name);
            var lvsi = lvi.SubItems.Add(ct.Info);
            lvi.Group = group;
            lvi.Tag = ct;

            lvi.UseItemStyleForSubItems = false;
            lvsi.ForeColor = SystemColors.GrayText;

            return lvi;
        }

        private ListViewItem add_npc_to_list(Npc npc, ListViewGroup group)
        {
            if (npc == null)
                return null;

            Difficulty diff;
            if (!FilterPanel.AllowItem(npc, out diff))
                return null;

            var lvi = new ListViewItem(npc.Name);
            var lvsi = lvi.SubItems.Add(npc.Info);
            lvi.Group = group;
            lvi.Tag = npc;

            lvi.UseItemStyleForSubItems = false;
            lvsi.ForeColor = SystemColors.GrayText;

            if (diff == Difficulty.Trivial)
                lvi.ForeColor = Color.Green;

            if (diff == Difficulty.Extreme)
                lvi.ForeColor = Color.Red;

            return lvi;
        }

        private ListViewItem add_trap_to_list(Trap trap, ListViewGroup lvg)
        {
            if (trap == null)
                return null;

            Difficulty diff;
            if (!FilterPanel.AllowItem(trap, out diff))
                return null;

            var lvi = new ListViewItem(trap.Name);
            var lvsi = lvi.SubItems.Add(trap.Info);
            lvi.Group = lvg;
            lvi.Tag = trap;

            lvi.UseItemStyleForSubItems = false;
            lvsi.ForeColor = SystemColors.GrayText;

            if (diff == Difficulty.Trivial)
                lvi.ForeColor = Color.Green;

            if (diff == Difficulty.Extreme)
                lvi.ForeColor = Color.Red;

            return lvi;
        }

        private ListViewItem add_challenge_to_list(SkillChallenge sc)
        {
            if (sc == null)
                return null;

            Difficulty diff;
            if (!FilterPanel.AllowItem(sc, out diff))
                return null;

            var lvi = new ListViewItem(sc.Name);
            var lvsi = lvi.SubItems.Add(sc.Info);
            lvi.Tag = sc;

            lvi.UseItemStyleForSubItems = false;
            lvsi.ForeColor = SystemColors.GrayText;

            return lvi;
        }

        private void AddToken_Click(object sender, EventArgs e)
        {
            try
            {
                var ct = new CustomToken();
                ct.Name = "Custom Map Token";
                ct.Type = CustomTokenType.Token;

                var dlg = new CustomTokenForm(ct);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Encounter.CustomTokens.Add(dlg.Token);

                    update_encounter();
                    update_mapthreats();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CreaturesAddOverlay_Click(object sender, EventArgs e)
        {
            try
            {
                var ct = new CustomToken();
                ct.Name = "Custom Overlay";
                ct.Type = CustomTokenType.Overlay;

                var dlg = new CustomOverlayForm(ct);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Encounter.CustomTokens.Add(dlg.Token);
                    update_encounter();
                    update_mapthreats();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapCreaturesRemoveAll_Click(object sender, EventArgs e)
        {
            foreach (var slot in Encounter.Slots)
            foreach (var cd in slot.CombatData)
                cd.Location = CombatData.NoPoint;

            foreach (var ct in Encounter.CustomTokens)
                ct.Data.Location = CombatData.NoPoint;

            MapView.MapChanged();
            update_mapthreats();
        }

        private void MapCreaturesShowAll_Click(object sender, EventArgs e)
        {
            foreach (var slot in Encounter.Slots)
            foreach (var cd in slot.CombatData)
                cd.Visible = true;

            foreach (var ct in Encounter.CustomTokens)
                ct.Data.Visible = true;

            MapView.MapChanged();
        }

        private void MapCreaturesHideAll_Click(object sender, EventArgs e)
        {
            foreach (var slot in Encounter.Slots)
            foreach (var cd in slot.CombatData)
                cd.Visible = false;

            foreach (var ct in Encounter.CustomTokens)
                ct.Data.Visible = false;

            MapView.MapChanged();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            if (MapView.Map != null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = MapView.Name;
                dlg.Filter = "Bitmap Image|*.bmp|JPEG Image|*.jpg|GIF Image|*.gif|PNG Image|*.png";

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
        }

        private void MapThreatList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedMapThreat != null)
            {
                var creature = SelectedMapThreat as CreatureToken;
                if (creature != null)
                {
                    var slot = Encounter.FindSlot(creature.SlotId);

                    var dlg = new CreatureDetailsForm(slot.Card);
                    dlg.ShowDialog();
                }

                var custom = SelectedMapThreat as CustomToken;
                if (custom != null)
                {
                    var index = Encounter.CustomTokens.IndexOf(custom);

                    switch (custom.Type)
                    {
                        case CustomTokenType.Token:
                        {
                            var dlg = new CustomTokenForm(custom);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Encounter.CustomTokens[index] = dlg.Token;

                                update_encounter();
                                update_mapthreats();
                            }
                        }
                            break;
                        case CustomTokenType.Overlay:
                        {
                            var dlg = new CustomOverlayForm(custom);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Encounter.CustomTokens[index] = dlg.Token;

                                update_encounter();
                                update_mapthreats();
                            }
                        }
                            break;
                    }
                }
            }
        }

        private void MapThreatList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedMapThreat != null) DoDragDrop(SelectedMapThreat, DragDropEffects.Move);
        }

        private void MapView_ItemDropped(object sender, EventArgs e)
        {
            update_mapthreats();
        }

        private void MapView_ItemMoved(object sender, MovementEventArgs e)
        {
        }

        private void MapView_SelectedTokensChanged(object sender, EventArgs e)
        {
        }

        private void MapView_HoverTokenChanged(object sender, EventArgs e)
        {
            var title = "";
            var info = "";

            var token = MapView.HoverToken as CreatureToken;
            if (token != null)
            {
                var slot = Encounter.FindSlot(token.SlotId);

                title = slot.Card.Title;

                info = slot.Card.Info;
                info += Environment.NewLine;
                info += "Double-click for more details";
            }

            var custom = MapView.HoverToken as CustomToken;
            if (custom != null)
            {
                title = custom.Name;
                info = "Double-click to edit";
            }

            Tooltip.ToolTipTitle = title;
            Tooltip.ToolTipIcon = ToolTipIcon.Info;
            Tooltip.SetToolTip(MapView, info);
        }

        private void MapView_TokenActivated(object sender, TokenEventArgs e)
        {
            var token = e.Token as CreatureToken;
            if (token != null)
            {
                var slot = Encounter.FindSlot(token.SlotId);

                var dlg = new CreatureDetailsForm(slot.Card);
                dlg.ShowDialog();
            }

            var custom = e.Token as CustomToken;
            if (custom != null)
            {
                var index = Encounter.CustomTokens.IndexOf(custom);
                if (index != -1)
                    switch (custom.Type)
                    {
                        case CustomTokenType.Token:
                        {
                            var dlg = new CustomTokenForm(custom);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Encounter.CustomTokens[index] = dlg.Token;

                                update_encounter();
                                update_mapthreats();
                            }
                        }
                            break;
                        case CustomTokenType.Overlay:
                        {
                            var dlg = new CustomOverlayForm(custom);
                            if (dlg.ShowDialog() == DialogResult.OK)
                            {
                                Encounter.CustomTokens[index] = dlg.Token;

                                update_encounter();
                                update_mapthreats();
                            }
                        }
                            break;
                    }
            }
        }

        private void MapView_DoubleClick(object sender, EventArgs e)
        {
            if (Encounter.MapId == Guid.Empty)
                MapBtn_Click(sender, e);
        }

        private void MapBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MapAreaSelectForm(Encounter.MapId, Encounter.MapAreaId);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var mapId = dlg.Map?.Id ?? Guid.Empty;
                var mapAreaId = dlg.MapArea?.Id ?? Guid.Empty;

                // Have we changed anything?
                if (mapId == Encounter.MapId && mapAreaId == Encounter.MapAreaId)
                    return;

                // Take everything off the map
                foreach (var slot in Encounter.Slots)
                foreach (var cd in slot.CombatData)
                    cd.Location = CombatData.NoPoint;
                foreach (var ct in Encounter.CustomTokens)
                    ct.Data.Location = CombatData.NoPoint;

                Encounter.MapId = mapId;
                Encounter.MapAreaId = mapAreaId;

                MapView.Map = dlg.Map;
                MapView.Viewpoint = dlg.MapArea?.Region ?? Rectangle.Empty;
                MapView.Encounter = Encounter;

                update_mapthreats();
            }
        }

        private void PrintBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MapPrintingForm(MapView);
            dlg.ShowDialog();
        }

        private void MapToolsLOS_Click(object sender, EventArgs e)
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

        private void MapToolsGridlines_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowGrid = MapView.ShowGrid == MapGridMode.None ? MapGridMode.Overlay : MapGridMode.None;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapToolsGridLabels_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.ShowGridLabels = !MapView.ShowGridLabels;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MapToolsPictureTokens_Click(object sender, EventArgs e)
        {
            MapView.ShowPictureTokens = !MapView.ShowPictureTokens;
        }

        private void MapContextView_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTokens.Count != 1)
                return;

            var ct = MapView.SelectedTokens[0] as CreatureToken;
            if (ct != null)
            {
                var slot = Encounter.FindSlot(ct.SlotId);

                var dlg = new CreatureDetailsForm(slot.Card);
                dlg.ShowDialog();
            }

            var custom = MapView.SelectedTokens[0] as CustomToken;
            if (custom != null)
            {
                var index = Encounter.CustomTokens.IndexOf(custom);

                switch (custom.Type)
                {
                    case CustomTokenType.Token:
                    {
                        var dlg = new CustomTokenForm(custom);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Encounter.CustomTokens[index] = dlg.Token;

                            update_encounter();
                            update_mapthreats();
                        }
                    }
                        break;
                    case CustomTokenType.Overlay:
                    {
                        var dlg = new CustomOverlayForm(custom);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Encounter.CustomTokens[index] = dlg.Token;

                            update_encounter();
                            update_mapthreats();
                        }
                    }
                        break;
                }
            }
        }

        private void MapContextRemove_Click(object sender, EventArgs e)
        {
            foreach (var token in MapView.SelectedTokens)
            {
                var ct = token as CreatureToken;
                if (ct != null)
                    ct.Data.Location = CombatData.NoPoint;

                var custom = token as CustomToken;
                if (custom != null)
                    custom.Data.Location = CombatData.NoPoint;
            }

            update_mapthreats();
        }

        private void MapContextRemoveEncounter_Click(object sender, EventArgs e)
        {
            foreach (var token in MapView.SelectedTokens)
            {
                var ct = token as CreatureToken;
                if (ct != null)
                {
                    var slot = Encounter.FindSlot(ct.SlotId);
                    slot.CombatData.Remove(ct.Data);

                    if (slot.CombatData.Count == 0)
                        Encounter.Slots.Remove(slot);
                }

                var custom = token as CustomToken;
                if (custom != null)
                    Encounter.CustomTokens.Remove(custom);
            }

            update_encounter();
            update_mapthreats();
        }

        private void MapContextVisible_Click(object sender, EventArgs e)
        {
            foreach (var token in MapView.SelectedTokens)
            {
                var ct = token as CreatureToken;
                if (ct != null)
                {
                    ct.Data.Visible = !ct.Data.Visible;

                    MapView.Invalidate();
                }

                var custom = token as CustomToken;
                if (custom != null)
                {
                    custom.Data.Visible = !custom.Data.Visible;

                    MapView.Invalidate();
                }
            }
        }

        private void MapContextSetPicture_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTokens.Count != 1)
                return;

            var ct = MapView.SelectedTokens[0] as CreatureToken;
            if (ct != null)
            {
                var slot = Encounter.FindSlot(ct.SlotId);

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

                        MapView.Invalidate();
                    }
                }
            }
        }

        private void MapContextCopy_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTokens.Count != 1)
                return;

            var ct = MapView.SelectedTokens[0] as CustomToken;
            if (ct != null)
            {
                var copy = ct.Copy();
                copy.Id = Guid.NewGuid();
                copy.Data.Location = CombatData.NoPoint;

                Encounter.CustomTokens.Add(copy);
            }

            update_mapthreats();
        }

        private void update_mapthreats()
        {
            MapThreatList.Items.Clear();
            MapThreatList.Groups.Clear();

            SlotList.Groups.Add("Combatants", "Combatants");
            foreach (var ew in Encounter.Waves)
                SlotList.Groups.Add(ew.Name, ew.Name);
            SlotList.Groups.Add("Custom Tokens / Overlays", "Custom Tokens / Overlays");

            foreach (var slot in Encounter.AllSlots)
            foreach (var cd in slot.CombatData)
                if (cd.Location == CombatData.NoPoint)
                {
                    var mapLvi = MapThreatList.Items.Add(cd.DisplayName);
                    mapLvi.Tag = new CreatureToken(slot.Id, cd);

                    var ew = Encounter.FindWave(slot);
                    mapLvi.Group = MapThreatList.Groups[ew == null ? "Combatants" : ew.Name];
                }

            foreach (var ct in Encounter.CustomTokens)
                if (ct.Data.Location == CombatData.NoPoint)
                {
                    var mapLvi = MapThreatList.Items.Add(ct.Name);
                    mapLvi.Tag = ct;
                    mapLvi.Group = MapThreatList.Groups["Custom Tokens / Overlays"];
                }

            if (MapThreatList.Items.Count == 0)
                MapView.Caption = "";
            else
                MapView.Caption = "Drag creatures from the list to place them on the map";
        }

        private void NoteAddBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var en = new EncounterNote("New Note");
                var dlg = new EncounterNoteForm(en);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Encounter.Notes.Add(dlg.Note);

                    update_notes();
                    SelectedNote = dlg.Note;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteRemoveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null)
                {
                    var str = "Remove encounter note: are you sure?";
                    if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                        DialogResult.No)
                        return;

                    Encounter.Notes.Remove(SelectedNote);

                    update_notes();
                    SelectedNote = null;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteEditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null)
                {
                    var index = Encounter.Notes.IndexOf(SelectedNote);

                    var dlg = new EncounterNoteForm(SelectedNote);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Encounter.Notes[index] = dlg.Note;

                        update_notes();
                        SelectedNote = dlg.Note;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteUpBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null && Encounter.Notes.IndexOf(SelectedNote) != 0)
                {
                    var index = Encounter.Notes.IndexOf(SelectedNote);
                    var tmp = Encounter.Notes[index - 1];
                    Encounter.Notes[index - 1] = SelectedNote;
                    Encounter.Notes[index] = tmp;

                    update_notes();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteDownBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null && Encounter.Notes.IndexOf(SelectedNote) != Encounter.Notes.Count - 1)
                {
                    var index = Encounter.Notes.IndexOf(SelectedNote);
                    var tmp = Encounter.Notes[index + 1];
                    Encounter.Notes[index + 1] = SelectedNote;
                    Encounter.Notes[index] = tmp;

                    update_notes();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                update_selected_note();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteDetails_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.Scheme == "note")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "edit") NoteEditBtn_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_notes()
        {
            try
            {
                var selection = SelectedNote;

                NoteList.Items.Clear();
                foreach (var en in Encounter.Notes)
                {
                    var lvi = NoteList.Items.Add(en.Title);
                    lvi.Tag = en;

                    if (en.Contents == "")
                        lvi.ForeColor = SystemColors.GrayText;

                    if (en == selection)
                        lvi.Selected = true;
                }

                if (NoteList.Items.Count == 0)
                {
                    var lvi = NoteList.Items.Add("(no notes)");
                    lvi.ForeColor = SystemColors.GrayText;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_selected_note()
        {
            try
            {
                NoteDetails.Document.OpenNew(true);
                NoteDetails.Document.Write(Html.EncounterNote(SelectedNote, Session.Preferences.TextSize));
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void SourceItemList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var sorter = SourceItemList.ListViewItemSorter as SourceSorter;
            sorter.Set(e.Column);

            SourceItemList.Sort();
        }

        private void ViewGroups_Click(object sender, EventArgs e)
        {
            SourceItemList.ShowGroups = !SourceItemList.ShowGroups;
        }

        private void ToolsExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "Encounter";
            dlg.Filter = Program.EncounterFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var xml = EncounterExporter.ExportXml(Encounter);
                File.WriteAllText(dlg.FileName, xml);
            }
        }

        private void ThreatContextMenu_Opening(object sender, CancelEventArgs e)
        {
            EditStatBlock.Enabled =
                SelectedSlot != null || SelectedSlotTrap != null || SelectedSlotSkillChallenge != null;

            EditSetFaction.Enabled = SelectedSlot != null;
            EditSetFaction.DropDownItems.Clear();

            // Factions
            foreach (EncounterSlotType type in Enum.GetValues(typeof(EncounterSlotType)))
            {
                var tsmi = new ToolStripMenuItem(type.ToString());
                tsmi.Tag = type;
                tsmi.Enabled = SelectedSlot != null;
                tsmi.Checked = SelectedSlot != null && SelectedSlot.Type == type;
                tsmi.Click += count_slot_as;

                EditSetFaction.DropDownItems.Add(tsmi);
            }

            EditRemoveTemplate.Enabled = SelectedSlot != null && SelectedSlot.Card.TemplateIDs.Count != 0;
            EditRemoveLevelAdj.Enabled = SelectedSlot != null && SelectedSlot.Card.LevelAdjustment != 0;
            EditSwap.Enabled = SelectedSlot != null;

            EditSetWave.Enabled = SelectedSlot != null;
            EditSetWave.DropDownItems.Clear();

            // Initial wave
            var tsmiInitialWave = new ToolStripMenuItem("Initial Wave");
            tsmiInitialWave.Tag = Encounter;
            tsmiInitialWave.Enabled = SelectedSlot != null;
            tsmiInitialWave.Checked = SelectedSlot != null && Encounter.Slots.Contains(SelectedSlot);
            tsmiInitialWave.Click += wave_initial;
            EditSetWave.DropDownItems.Add(tsmiInitialWave);

            // Waves
            foreach (var ew in Encounter.Waves)
            {
                var tsmiWave = new ToolStripMenuItem(ew.Name);
                tsmiWave.Tag = ew;
                tsmiWave.Enabled = SelectedSlot != null;
                tsmiWave.Checked = SelectedSlot != null && Encounter.FindWave(SelectedSlot) == ew;
                tsmiWave.Click += wave_subsequent;
                EditSetWave.DropDownItems.Add(tsmiWave);
            }

            // Add a wave
            var tsmiNewWave = new ToolStripMenuItem("New Wave...");
            tsmiNewWave.Tag = null;
            tsmiNewWave.Enabled = SelectedSlot != null;
            tsmiNewWave.Checked = false;
            tsmiNewWave.Click += wave_new;
            EditSetWave.DropDownItems.Add(tsmiNewWave);

            if (SelectedSlot == null)
            {
                SwapStandard.Enabled = false;
                SwapElite.Enabled = false;
                SwapSolo.Enabled = false;
                SwapMinions.Enabled = false;
            }
            else
            {
                var creature = Session.FindCreature(SelectedSlot.Card.CreatureId, SearchType.Global);
                if (creature == null)
                {
                    SwapStandard.Enabled = false;
                    SwapElite.Enabled = false;
                    SwapSolo.Enabled = false;
                    SwapMinions.Enabled = false;
                }
                else
                {
                    var minion = creature.Role is Minion;
                    if (minion)
                    {
                        SwapStandard.Enabled =
                            SelectedSlot.CombatData.Count >= 4 && SelectedSlot.CombatData.Count % 4 == 0;
                        SwapElite.Enabled =
                            SelectedSlot.CombatData.Count >= 8 && SelectedSlot.CombatData.Count % 8 == 0;
                        SwapSolo.Enabled = SelectedSlot.CombatData.Count >= 20 &&
                                           SelectedSlot.CombatData.Count % 20 == 0;
                        SwapMinions.Enabled = false;
                    }
                    else
                    {
                        SwapStandard.Enabled = true;
                        SwapElite.Enabled =
                            SelectedSlot.CombatData.Count >= 2 && SelectedSlot.CombatData.Count % 2 == 0;
                        SwapSolo.Enabled = SelectedSlot.CombatData.Count >= 5 && SelectedSlot.CombatData.Count % 5 == 0;
                        SwapMinions.Enabled = true;
                    }
                }
            }

            if (SelectedSlot != null)
            {
                EditApplyTheme.Enabled = SelectedSlot.Card != null;
                EditClearTheme.Enabled = SelectedSlot.Card != null && SelectedSlot.Card.ThemeId != Guid.Empty;
            }
            else
            {
                EditApplyTheme.Enabled = false;
                EditClearTheme.Enabled = false;
            }
        }

        private void wave_initial(object sender, EventArgs e)
        {
            // Move slot to encounter
            var initial = Encounter.FindWave(SelectedSlot);
            if (initial != null)
            {
                initial.Slots.Remove(SelectedSlot);
                Encounter.Slots.Add(SelectedSlot);
            }

            update_encounter();
            update_mapthreats();
        }

        private void wave_subsequent(object sender, EventArgs e)
        {
            // Move slot to this wave
            var tsmi = sender as ToolStripMenuItem;
            var ew = tsmi.Tag as EncounterWave;
            if (ew != null)
            {
                var initial = Encounter.FindWave(SelectedSlot);
                if (initial == null)
                    Encounter.Slots.Remove(SelectedSlot);
                else
                    initial.Slots.Remove(SelectedSlot);
                ew.Slots.Add(SelectedSlot);
            }

            update_encounter();
            update_mapthreats();
        }

        private void wave_new(object sender, EventArgs e)
        {
            // Create a new wave
            var ew = new EncounterWave();
            ew.Name = "Wave " + (Encounter.Waves.Count + 2);
            Encounter.Waves.Add(ew);

            // Move slot to new wave
            var initial = Encounter.FindWave(SelectedSlot);
            if (initial == null)
                Encounter.Slots.Remove(SelectedSlot);
            else
                initial.Slots.Remove(SelectedSlot);
            ew.Slots.Add(SelectedSlot);

            update_encounter();
            update_mapthreats();
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

        private void PartyLbl_Click(object sender, EventArgs e)
        {
            var party = new Party(_fPartySize, _fPartyLevel);
            var dlg = new PartyForm(party);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fPartySize = dlg.Party.Size;
                _fPartyLevel = dlg.Party.Level;

                update_difficulty_list();
                update_encounter();
                update_party_label();
            }
        }

        private void update_party_label()
        {
            PartyLbl.Text = _fPartySize + " PCs at level " + _fPartyLevel;
        }

        private class SourceSorter : IComparer
        {
            private bool _fAscending = true;
            private int _fColumn;

            public void Set(int column)
            {
                if (_fColumn == column)
                    _fAscending = !_fAscending;

                _fColumn = column;
            }

            public int Compare(object x, object y)
            {
                var lviX = x as ListViewItem;
                var lviY = y as ListViewItem;

                var result = 0;

                if (_fColumn == 1)
                {
                    if (lviX.Tag is ICreature)
                    {
                        var creatureX = lviX.Tag as ICreature;
                        var creatureY = lviY.Tag as ICreature;

                        var levelX = creatureX.Level;
                        var levelY = creatureY.Level;

                        result = levelX.CompareTo(levelY);
                    }

                    if (lviX.Tag is Trap)
                    {
                        var trapX = lviX.Tag as Trap;
                        var trapY = lviY.Tag as Trap;

                        var levelX = trapX.Level;
                        var levelY = trapY.Level;

                        result = levelX.CompareTo(levelY);
                    }
                }

                if (result == 0)
                {
                    var lvsiX = lviX.SubItems[_fColumn];
                    var lvsiY = lviY.SubItems[_fColumn];

                    var strX = lvsiX.Text;
                    var strY = lvsiY.Text;

                    result = strX.CompareTo(strY);
                }

                if (!_fAscending)
                    result *= -1;

                return result;
            }
        }
    }
}
