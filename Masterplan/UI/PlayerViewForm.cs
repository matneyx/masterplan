using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Controls.Elements;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal enum PlayerViewMode
    {
        Blank,
        Html,
        RichText,
        Image,
        Calendar,
        Combat,
        RegionalMap
    }

    internal partial class PlayerViewForm : Form
    {
        public static bool UseOtherDisplay = true;

        public PlayerViewMode Mode { get; set; } = PlayerViewMode.Blank;

        public MapView ParentMap { get; set; }

        public PlayerViewForm(Form parent)
        {
            InitializeComponent();

            set_location(parent);
        }

        private void set_location(Form parent)
        {
            if (!UseOtherDisplay)
                return;

            if (Screen.AllScreens.Length < 2)
                return;

            // See if we can find an external monitor
            var otherScreens = new List<Screen>();
            foreach (var screen in Screen.AllScreens)
            {
                var rect = screen.Bounds;

                if (rect.Contains(parent.ClientRectangle))
                    continue;

                otherScreens.Add(screen);
            }

            if (otherScreens.Count == 0)
                return;

            StartPosition = FormStartPosition.Manual;
            Location = otherScreens[0].WorkingArea.Location;
            WindowState = FormWindowState.Maximized;
            FormBorderStyle = FormBorderStyle.None;
        }

        private void PlayerViewForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Session.PlayerView = null;
        }

        public void ShowDefault()
        {
            var ctrl = new TitlePanel();
            ctrl.Title = "Masterplan";
            ctrl.Zooming = true;
            ctrl.Mode = TitlePanel.TitlePanelMode.PlayerView;
            ctrl.BackColor = Color.Black;
            ctrl.ForeColor = Color.White;
            ctrl.MouseMove += mouse_move;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Blank;

            Show();
        }

        private void mouse_move(object sender, MouseEventArgs e)
        {
            var title = Controls[0] as TitlePanel;
            title.Wake();
        }

        public void ShowMessage(string message)
        {
            var html = Html.Text(message, true, true, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowPlainText(Attachment att)
        {
            var str = new ASCIIEncoding().GetString(att.Contents);
            var html = Html.Text(str, true, false, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowRichText(Attachment att)
        {
            var str = new ASCIIEncoding().GetString(att.Contents);

            var ctrl = new RichTextBox();
            ctrl.Rtf = str;
            ctrl.ReadOnly = true;
            ctrl.Multiline = true;
            ctrl.ScrollBars = RichTextBoxScrollBars.Vertical;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.RichText;

            Show();
        }

        public void ShowWebPage(Attachment att)
        {
            var ctrl = new WebBrowser();
            ctrl.IsWebBrowserContextMenuEnabled = false;
            ctrl.ScriptErrorsSuppressed = true;
            ctrl.WebBrowserShortcutsEnabled = false;

            switch (att.Type)
            {
                case AttachmentType.Url:
                {
                    var str = new ASCIIEncoding().GetString(att.Contents);
                    var lines = str.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    var url = "";
                    foreach (var line in lines)
                        if (line.StartsWith("URL="))
                        {
                            url = line.Substring(4);
                            break;
                        }

                    if (url != "")
                        ctrl.Navigate(url);
                }
                    break;
                case AttachmentType.Html:
                {
                    var str = new ASCIIEncoding().GetString(att.Contents);
                    ctrl.DocumentText = str;
                }
                    break;
            }

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Html;

            Show();
        }

        public void ShowImage(Attachment att)
        {
            var img = Image.FromStream(new MemoryStream(att.Contents));

            var ctrl = new PictureBox();
            ctrl.Image = img;
            ctrl.SizeMode = PictureBoxSizeMode.Zoom;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Image;

            Show();
        }

        public void ShowImage(Image img)
        {
            var ctrl = new PictureBox();
            ctrl.Image = img;
            ctrl.SizeMode = PictureBoxSizeMode.Zoom;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Image;

            Show();
        }

        public void ShowPlotPoint(PlotPoint pp)
        {
            var html = Html.Text(pp.ReadAloud, false, false, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowBackground(Background background)
        {
            var html = Html.Background(background, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowBackground(List<Background> backgrounds)
        {
            var html = Html.Background(backgrounds, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowEncyclopediaItem(IEncyclopediaItem item)
        {
            if (item is EncyclopediaEntry)
            {
                var html = Html.EncyclopediaEntry(item as EncyclopediaEntry, Session.Project.Encyclopedia,
                    Session.Preferences.PlayerViewTextSize, false, false, false, false);
                set_html(html);
            }

            if (item is EncyclopediaGroup)
            {
                var html = Html.EncyclopediaGroup(item as EncyclopediaGroup, Session.Project.Encyclopedia,
                    Session.Preferences.PlayerViewTextSize, false, false);
                set_html(html);
            }

            Show();
        }

        public void ShowEncyclopediaGroup(EncyclopediaGroup group)
        {
            var html = Html.EncyclopediaGroup(group, Session.Project.Encyclopedia,
                Session.Preferences.PlayerViewTextSize, false, false);
            set_html(html);

            Show();
        }

        public void ShowTacticalMap(MapView mapview, string initiative)
        {
            ParentMap = mapview;

            MapView mv = null;
            if (ParentMap != null)
            {
                mv = new MapView();
                mv.Map = ParentMap.Map;
                mv.Viewpoint = ParentMap.Viewpoint;
                mv.BorderSize = ParentMap.BorderSize;
                mv.ScalingFactor = ParentMap.ScalingFactor;
                mv.Encounter = ParentMap.Encounter;
                mv.Plot = ParentMap.Plot;
                mv.TokenLinks = ParentMap.TokenLinks;
                mv.AllowDrawing = ParentMap.AllowDrawing;
                mv.Mode = MapViewMode.PlayerView;
                mv.Tactical = true;
                mv.HighlightAreas = false;
                mv.FrameType = MapDisplayType.Opaque;
                mv.ShowCreatures = Session.Preferences.Combat.PlayerViewFog;
                mv.ShowHealthBars = Session.Preferences.Combat.PlayerViewHealthBars;
                mv.ShowCreatureLabels = Session.Preferences.Combat.PlayerViewCreatureLabels;
                mv.ShowGrid = Session.Preferences.Combat.PlayerViewGrid ? MapGridMode.Overlay : MapGridMode.None;
                mv.ShowGridLabels = Session.Preferences.Combat.PlayerViewGridLabels;
                mv.ShowAuras = false;
                mv.ShowGrid = MapGridMode.None;

                foreach (var sketch in mapview.Sketches)
                    mv.Sketches.Add(sketch.Copy());

                mv.SelectedTokensChanged += selected_tokens_changed;
                mv.HoverTokenChanged += hover_token_changed;
                mv.ItemMoved += item_moved;
                mv.TokenDragged += token_dragged;
                mv.SketchCreated += sketch_created;
                mv.Dock = DockStyle.Fill;
            }

            var dicebtn = new Button();
            dicebtn.Text = "Die Roller";
            dicebtn.BackColor = SystemColors.Control;
            dicebtn.Dock = DockStyle.Bottom;
            dicebtn.Click += dicebtn_click;

            var browser = new WebBrowser();
            browser.IsWebBrowserContextMenuEnabled = false;
            browser.ScriptErrorsSuppressed = true;
            browser.WebBrowserShortcutsEnabled = false;
            browser.Dock = DockStyle.Fill;
            browser.DocumentText = initiative;

            var splitter = new SplitContainer();
            splitter.Panel1.Controls.Add(mv);
            splitter.Panel2.Controls.Add(browser);
            splitter.Panel2.Controls.Add(dicebtn);

            Controls.Clear();
            Controls.Add(splitter);
            splitter.Dock = DockStyle.Fill;

            if (mapview == null)
            {
                splitter.Panel1Collapsed = true;
            }
            else if (initiative == null)
            {
                splitter.Panel2Collapsed = true;
            }
            else
            {
                splitter.BackColor = Color.FromArgb(10, 10, 10);
                splitter.SplitterDistance = (int)(Width * 0.65);
                splitter.FixedPanel = FixedPanel.Panel2;

                splitter.Panel2Collapsed = !Session.Preferences.Combat.PlayerViewInitiative;
            }

            Mode = PlayerViewMode.Combat;

            Show();
        }

        private void selected_tokens_changed(object sender, EventArgs e)
        {
            var splitter = Controls[0] as SplitContainer;
            var map = splitter.Panel1.Controls[0] as MapView;

            ParentMap.SelectTokens(map.SelectedTokens, true);
        }

        private void hover_token_changed(object sender, EventArgs e)
        {
            var splitter = Controls[0] as SplitContainer;
            var map = splitter.Panel1.Controls[0] as MapView;

            ParentMap.HoverToken = map.HoverToken;

            var title = "";
            string info = null;

            if (map.HoverToken is CreatureToken)
            {
                var ct = map.HoverToken as CreatureToken;
                var slot = map.Encounter.FindSlot(ct.SlotId);
                var creature = Session.FindCreature(slot.Card.CreatureId, SearchType.Global);

                var hpTotal = slot.Card.Hp;
                var hpCurrent = hpTotal - ct.Data.Damage;
                var hpBloodied = hpTotal / 2;

                if (map.ShowCreatureLabels)
                {
                    title = ct.Data.DisplayName;
                }
                else
                {
                    title = creature.Category;
                    if (title == "")
                        title = "Creature";
                }

                if (ct.Data.Damage == 0)
                    info = "Not wounded";
                if (hpCurrent < hpTotal)
                    info = "Wounded";
                if (hpCurrent < hpBloodied)
                    info = "Bloodied";
                if (hpCurrent <= 0)
                    info = "Dead";

                if (ct.Data.Conditions.Count != 0)
                {
                    info += Environment.NewLine;

                    foreach (var oc in ct.Data.Conditions)
                    {
                        info += Environment.NewLine;
                        info += oc.ToString(ParentMap.Encounter, false);
                    }
                }
            }

            if (map.HoverToken is Hero)
            {
                var hero = map.HoverToken as Hero;

                title = hero.Name;

                info = hero.Race + " " + hero.Class;
                info += Environment.NewLine;
                info += "Player: " + hero.Player;
            }

            if (map.HoverToken is CustomToken)
            {
                var ct = map.HoverToken as CustomToken;

                if (map.ShowCreatureLabels)
                {
                    title = ct.Name;
                    info = "(custom token)";
                }
            }

            Tooltip.ToolTipTitle = title;
            Tooltip.ToolTipIcon = ToolTipIcon.Info;
            Tooltip.SetToolTip(map, info);
        }

        private void item_moved(object sender, MovementEventArgs e)
        {
            ParentMap.Invalidate();
        }

        private void token_dragged(object sender, DraggedTokenEventArgs e)
        {
            ParentMap.SetDragInfo(e.OldLocation, e.NewLocation);
        }

        private void sketch_created(object sender, MapSketchEventArgs e)
        {
            ParentMap.Sketches.Add(e.Sketch.Copy());
            ParentMap.Invalidate();
        }

        private void dicebtn_click(object sender, EventArgs e)
        {
            var dlg = new DieRollerForm();
            dlg.ShowDialog();
        }

        public void ShowRegionalMap(RegionalMapPanel panel)
        {
            var ctrl = new RegionalMapPanel();
            ctrl.Map = panel.Map;
            ctrl.Mode = MapViewMode.PlayerView;

            if (panel.SelectedLocation == null)
            {
                ctrl.ShowLocations = false;
            }
            else
            {
                ctrl.ShowLocations = true;
                ctrl.HighlightedLocation = panel.SelectedLocation;
            }

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.RegionalMap;

            Show();
        }

        public void ShowHandout(List<object> items, bool includeDmInfo)
        {
            var html = Html.Handout(items, Session.Preferences.PlayerViewTextSize, includeDmInfo);
            set_html(html);

            Show();
        }

        public void ShowPCs()
        {
            var html = Html.PartyBreakdown(Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowPlayerOption(IPlayerOption option)
        {
            var html = Html.PlayerOption(option, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowCalendar(Calendar calendar, int monthIndex, int year)
        {
            var ctrl = new CalendarPanel();
            ctrl.Calendar = calendar;
            ctrl.MonthIndex = monthIndex;
            ctrl.Year = year;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Calendar;

            Show();
        }

        public void ShowHero(Hero h)
        {
            var html = Html.StatBlock(h, null, true, false, false, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowEncounterCard(EncounterCard card)
        {
            var html = Html.StatBlock(card, null, null, true, false, true, CardMode.View,
                Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowCreatureTemplate(CreatureTemplate template)
        {
            var html = Html.CreatureTemplate(template, Session.Preferences.PlayerViewTextSize, false);
            set_html(html);

            Show();
        }

        public void ShowTrap(Trap trap)
        {
            var html = Html.Trap(trap, null, true, false, false, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowSkillChallenge(SkillChallenge sc)
        {
            var html = Html.SkillChallenge(sc, false, true, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowMagicItem(MagicItem item)
        {
            var html = Html.MagicItem(item, Session.Preferences.PlayerViewTextSize, false, true);
            set_html(html);

            Show();
        }

        public void ShowArtifact(Artifact artifact)
        {
            var html = Html.Artifact(artifact, Session.Preferences.PlayerViewTextSize, false, true);
            set_html(html);

            Show();
        }

        public void ShowTerrainPower(TerrainPower tp)
        {
            var html = Html.TerrainPower(tp, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        public void ShowEncounterReportTable(ReportTable table)
        {
            var html = Html.EncounterReportTable(table, Session.Preferences.PlayerViewTextSize);
            set_html(html);

            Show();
        }

        private void set_html(string html)
        {
            var ctrl = new WebBrowser();
            ctrl.IsWebBrowserContextMenuEnabled = false;
            ctrl.ScriptErrorsSuppressed = true;
            ctrl.WebBrowserShortcutsEnabled = false;
            ctrl.DocumentText = html;

            Controls.Clear();
            Controls.Add(ctrl);
            ctrl.Dock = DockStyle.Fill;

            Mode = PlayerViewMode.Html;
        }
    }
}
