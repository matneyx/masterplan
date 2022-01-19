using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Controls.Elements;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.Tools.Generators;

namespace Masterplan.UI
{
    internal partial class PlotPointForm : Form
    {
        private readonly Plot _fPlot;
        private readonly bool _fStartAtElement;

        public PlotPoint PlotPoint { get; set; }

        public Parcel SelectedParcel
        {
            get
            {
                if (ParcelList.SelectedItems.Count != 0)
                    return ParcelList.SelectedItems[0].Tag as Parcel;

                return null;
            }
        }

        private EncyclopediaEntry SelectedEntry
        {
            get
            {
                if (EncyclopediaList.SelectedItems.Count != 0)
                    return EncyclopediaList.SelectedItems[0].Tag as EncyclopediaEntry;

                return null;
            }
        }

        private PlotPoint SelectedLink
        {
            get
            {
                if (LinkList.SelectedItems.Count != 0)
                    return LinkList.SelectedItems[0].Tag as PlotPoint;

                return null;
            }
        }

        public PlotPointForm(PlotPoint pp, Plot p, bool startAtElementPage)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            ParcelList_SizeChanged(null, null);
            LinkList_SizeChanged(null, null);
            EncBrowser.DocumentText = "";

            PlotPoint = pp.Copy();
            _fPlot = p;
            _fStartAtElement = startAtElementPage;

            NameBox.Text = PlotPoint.Name;
            DetailsBox.Text = PlotPoint.Details;
            ReadAloudBox.Text = PlotPoint.ReadAloud;
            XPBox.Value = PlotPoint.AdditionalXp;

            if (_fPlot.FindPoint(PlotPoint.Id) != null)
            {
                StartXPLbl.Text = "Start at: " + Workspace.GetTotalXp(PlotPoint) + " XP";
            }
            else
            {
                StartXPLbl.Visible = false;
                XPSeparator.Visible = false;
            }

            update_element();
            update_parcels();
            update_encyclopedia_entries();
            update_links();

            if (Session.Project.Encyclopedia.Entries.Count == 0)
                Pages.TabPages.Remove(EncyclopediaPage);
            else
                EncyclopediaList_SelectedIndexChanged(null, null);

            if (startAtElementPage)
                Pages.SelectedTab = RPGPage;
        }

        ~PlotPointForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            MapLocation loc = null;
            if (PlotPoint.RegionalMapId != Guid.Empty)
            {
                var map = Session.Project.FindRegionalMap(PlotPoint.RegionalMapId);
                if (map != null && PlotPoint.MapLocationId != Guid.Empty)
                    loc = map.FindLocation(PlotPoint.MapLocationId);
            }

            var count = 0;
            foreach (var map in Session.Project.RegionalMaps)
                count += map.Locations.Count;

            LocationBtn.Enabled = count != 0;
            LocationBtn.Text = loc != null ? loc.Name : "Set Location";
            ClearLocationLbl.Visible = loc != null;

            DateBtn.Enabled = Session.Project.Calendars.Count != 0;
            DateBtn.Text = PlotPoint.Date != null ? PlotPoint.Date.ToString() : "Set Date";
            ClearDateLbl.Visible = PlotPoint.Date != null;

            CopyElementBtn.Visible = PlotPoint.Element != null;
            RemoveElementBtn.Visible = PlotPoint.Element != null;

            ParcelAddPredefined.Enabled = Session.Project.TreasureParcels.Count != 0;
            ParcelAddItem.Enabled = Session.MagicItems.Count != 0;
            ParcelAddArtifact.Enabled = Session.Artifacts.Count != 0;
            ParcelRemoveBtn.Enabled = SelectedParcel != null;
            ParcelEditBtn.Enabled = SelectedParcel != null;

            if (SelectedParcel != null)
            {
                var isItem = SelectedParcel.MagicItemId != Guid.Empty;
                var isRealItem = isItem && !Treasure.PlaceholderIDs.Contains(SelectedParcel.MagicItemId);
                var isArtifact = SelectedParcel.ArtifactId != Guid.Empty;
                var isRealArtifact = isArtifact && !Treasure.PlaceholderIDs.Contains(SelectedParcel.ArtifactId);

                ChangeItemBtn.Enabled = isItem || isArtifact;
                ItemStatBlockBtn.Enabled = isRealItem || isRealArtifact;

                if (isItem)
                    ChangeItemBtn.Text = "Select Magic Item";
                else if (isArtifact)
                    ChangeItemBtn.Text = "Select Artifact";
                else
                    ChangeItemBtn.Text = "Select";
            }
            else
            {
                ChangeItemBtn.Enabled = false;
                ItemStatBlockBtn.Enabled = false;
            }

            EncyclopediaRemoveBtn.Enabled = SelectedEntry != null;
            EncPlayerViewBtn.Enabled = SelectedEntry != null;

            RemoveBtn.Enabled = SelectedLink != null;
        }

        private void RemoveElementBtn_Click(object sender, EventArgs e)
        {
            if (PlotPoint.Element != null)
            {
                PlotPoint.Element = null;
                update_element();
            }
        }

        private void CopyElementBtn_Click(object sender, EventArgs e)
        {
            if (PlotPoint.Element != null)
            {
                var type = PlotPoint.Element.GetType().ToString();
                Clipboard.SetData(type, PlotPoint.Element);
            }
        }

        private void CutElementBtn_Click(object sender, EventArgs e)
        {
            if (PlotPoint.Element != null)
            {
                var type = PlotPoint.Element.GetType().ToString();
                Clipboard.SetData(type, PlotPoint.Element);

                PlotPoint.Element = null;
                update_element();
            }
        }

        private void update_element()
        {
            RPGPanel.Controls.Clear();

            Control ctrl = null;
            var level = get_party_level();

            if (PlotPoint.Element is Encounter)
            {
                var panel = new EncounterPanel();
                panel.Encounter = PlotPoint.Element as Encounter;
                panel.PartyLevel = level;

                ctrl = panel;
            }

            if (PlotPoint.Element is SkillChallenge)
            {
                var panel = new SkillChallengePanel();
                panel.Challenge = PlotPoint.Element as SkillChallenge;
                panel.PartyLevel = level;

                ctrl = panel;
            }

            if (PlotPoint.Element is TrapElement)
            {
                var panel = new TrapElementPanel();
                panel.Trap = PlotPoint.Element as TrapElement;

                ctrl = panel;
            }

            if (PlotPoint.Element is Quest)
            {
                var panel = new QuestPanel();
                panel.Quest = PlotPoint.Element as Quest;

                ctrl = panel;
            }

            if (PlotPoint.Element is MapElement)
            {
                var panel = new MapElementPanel();
                panel.MapElement = PlotPoint.Element as MapElement;

                ctrl = panel;
            }

            if (ctrl == null)
            {
                var browser = new WebBrowser();
                browser.IsWebBrowserContextMenuEnabled = false;
                browser.ScriptErrorsSuppressed = true;
                browser.WebBrowserShortcutsEnabled = false;
                browser.ScrollBarsEnabled = false;
                browser.DocumentText = get_element_html();
                browser.Navigating += element_select;
                ctrl = browser;
            }

            if (ctrl != null)
            {
                ctrl.Dock = DockStyle.Fill;
                RPGPanel.Controls.Add(ctrl);
            }
        }

        private string get_element_html()
        {
            var lines = new List<string>();

            lines.Add("<HTML>");
            lines.AddRange(Html.GetHead("Plot Point", "Plot Point", Session.Preferences.TextSize));
            lines.Add("<BODY>");

            lines.Add("<P>");
            lines.Add("This plot point does not contain a game element (such as an encounter or a skill challenge).");
            lines.Add("The list of available game elements is below.");
            lines.Add("You can add a game element to this plot point by clicking on one of the links.");
            lines.Add("</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Select a Game Element</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Add a <A href=\"element:encounter\">combat encounter</A></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Add a <A href=\"element:challenge\">skill challenge</A></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Add a <A href=\"element:trap\">trap or hazard</A></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Add a <A href=\"element:quest\">quest</A></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Add a <A href=\"element:map\">tactical map</A></TD>");
            lines.Add("</TR>");

            if (Clipboard.ContainsData(typeof(Encounter).ToString()))
            {
                lines.Add("<TR>");
                lines.Add("<TD><A href=\"element:pasteencounter\">Paste the encounter from the clipboard</A></TD>");
                lines.Add("</TR>");
            }

            if (Clipboard.ContainsData(typeof(SkillChallenge).ToString()))
            {
                lines.Add("<TR>");
                lines.Add(
                    "<TD><A href=\"element:pastechallenge\">Paste the skill challenge from the clipboard</A></TD>");
                lines.Add("</TR>");
            }

            if (Clipboard.ContainsData(typeof(TrapElement).ToString()))
            {
                lines.Add("<TR>");
                lines.Add("<TD><A href=\"element:pastetrap\">Paste the trap from the clipboard</A></TD>");
                lines.Add("</TR>");
            }

            if (Clipboard.ContainsData(typeof(Quest).ToString()))
            {
                lines.Add("<TR>");
                lines.Add("<TD><A href=\"element:pastequest\">Paste the quest from the clipboard</A></TD>");
                lines.Add("</TR>");
            }

            if (Clipboard.ContainsData(typeof(MapElement).ToString()))
            {
                lines.Add("<TR>");
                lines.Add("<TD><A href=\"element:pastemap\">Paste the map from the clipboard</A></TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            return Html.Concatenate(lines);
        }

        private void element_select(object sender, WebBrowserNavigatingEventArgs e)
        {
            var level = get_party_level();

            if (e.Url.LocalPath == "encounter")
            {
                // Add an encounter
                var enc = new Encounter();
                enc.SetStandardEncounterNotes();
                PlotPoint.Element = enc;

                update_element();
            }

            if (e.Url.LocalPath == "pasteencounter")
            {
                // Add an encounter
                var enc = Clipboard.GetData(typeof(Encounter).ToString()) as Encounter;
                PlotPoint.Element = enc;

                update_element();
            }

            if (e.Url.LocalPath == "challenge")
            {
                // Add a skill challenge
                var sc = new SkillChallenge();
                sc.Name = "Unnamed Skill Challenge";
                sc.Level = level;
                PlotPoint.Element = sc;

                update_element();
            }

            if (e.Url.LocalPath == "pastechallenge")
            {
                // Add a skill challenge
                var sc = Clipboard.GetData(typeof(SkillChallenge).ToString()) as SkillChallenge;
                sc.Level = level;
                PlotPoint.Element = sc;

                update_element();
            }

            if (e.Url.LocalPath == "trap")
            {
                // Add a trap
                var te = new TrapElement();
                te.Trap.Name = "Unnamed Trap";
                te.Trap.Level = level;
                PlotPoint.Element = te;

                update_element();
            }

            if (e.Url.LocalPath == "pastetrap")
            {
                // Add a trap
                var te = Clipboard.GetData(typeof(TrapElement).ToString()) as TrapElement;
                te.Trap.Level = level;
                PlotPoint.Element = te;

                update_element();
            }

            if (e.Url.LocalPath == "quest")
            {
                // Add a quest
                var q = new Quest();
                q.Level = level;
                PlotPoint.Element = q;

                update_element();
            }

            if (e.Url.LocalPath == "pastequest")
            {
                // Add a quest
                var q = Clipboard.GetData(typeof(Quest).ToString()) as Quest;
                q.Level = level;
                PlotPoint.Element = q;

                update_element();
            }

            if (e.Url.LocalPath == "map")
            {
                // Add a map
                var me = new MapElement();
                PlotPoint.Element = me;

                update_element();
            }

            if (e.Url.LocalPath == "pastemap")
            {
                // Add a map
                var me = Clipboard.GetData(typeof(MapElement).ToString()) as MapElement;
                PlotPoint.Element = me;

                update_element();
            }
        }

        private void ParcelAddPredefined_Click(object sender, EventArgs e)
        {
            var dlg = new ParcelSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.Parcels.Add(dlg.Parcel);
                Session.Project.TreasureParcels.Remove(dlg.Parcel);

                update_parcels();
            }
        }

        private void ParcelAddParcel_Click(object sender, EventArgs e)
        {
            var p = new Parcel();
            p.Name = "New Treasure Parcel";

            var dlg = new ParcelForm(p);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.Parcels.Add(dlg.Parcel);
                update_parcels();
            }
        }

        private void ParcelAddItem_Click(object sender, EventArgs e)
        {
            var dlg = new MagicItemSelectForm(Session.Project.Party.Level);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.Parcels.Add(new Parcel(dlg.MagicItem));
                update_parcels();
            }
        }

        private void ParcelAddArtifact_Click(object sender, EventArgs e)
        {
            var dlg = new ArtifactSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.Parcels.Add(new Parcel(dlg.Artifact));
                update_parcels();
            }
        }

        private void ParcelRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                PlotPoint.Parcels.Remove(SelectedParcel);
                Session.Project.TreasureParcels.Add(SelectedParcel);

                update_parcels();
            }
        }

        private void ParcelEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                var index = PlotPoint.Parcels.IndexOf(SelectedParcel);

                var dlg = new ParcelForm(SelectedParcel);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotPoint.Parcels[index] = dlg.Parcel;
                    update_parcels();
                }
            }
        }

        private void ChangeItemBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                if (SelectedParcel.MagicItemId != Guid.Empty)
                {
                    var level = SelectedParcel.FindItemLevel();
                    if (level != -1)
                    {
                        var dlg = new MagicItemSelectForm(level);
                        if (dlg.ShowDialog() == DialogResult.OK)
                            SelectedParcel.SetAsMagicItem(dlg.MagicItem);
                    }
                }
                else if (SelectedParcel.ArtifactId != Guid.Empty)
                {
                    var dlg = new ArtifactSelectForm();
                    if (dlg.ShowDialog() == DialogResult.OK)
                        SelectedParcel.SetAsArtifact(dlg.Artifact);
                }

                update_parcels();
            }
        }

        private void ItemStatBlockBtn_Click(object sender, EventArgs e)
        {
            if (SelectedParcel != null)
            {
                if (SelectedParcel.MagicItemId != Guid.Empty)
                {
                    var item = Session.FindMagicItem(SelectedParcel.MagicItemId, SearchType.Global);
                    if (item != null)
                    {
                        var dlg = new MagicItemDetailsForm(item);
                        dlg.ShowDialog();
                    }
                }
                else if (SelectedParcel.ArtifactId != Guid.Empty)
                {
                    var item = Session.FindArtifact(SelectedParcel.ArtifactId, SearchType.Global);
                    if (item != null)
                    {
                        var dlg = new ArtifactDetailsForm(item);
                        dlg.ShowDialog();
                    }
                }
            }
        }

        private void ParcelList_SizeChanged(object sender, EventArgs e)
        {
            var width = ParcelList.Width - (SystemInformation.VerticalScrollBarWidth + 6);
            ParcelList.TileSize = new Size(width, ParcelList.TileSize.Height);
        }

        private void update_parcels()
        {
            ParcelList.Items.Clear();

            foreach (var p in PlotPoint.Parcels)
            {
                var name = p.Name;
                if (name == "")
                    name = "(undefined parcel)";

                if (p.MagicItemId != Guid.Empty)
                {
                    if (Treasure.PlaceholderIDs.Contains(p.MagicItemId))
                    {
                        // Placeholder
                    }
                    else
                    {
                        var item = Session.FindMagicItem(p.MagicItemId, SearchType.Global);
                        if (item != null)
                            name += " (" + item.Info.ToLower() + ")";
                    }
                }

                if (p.ArtifactId != Guid.Empty)
                {
                    if (Treasure.PlaceholderIDs.Contains(p.ArtifactId))
                    {
                        // Placeholder
                    }
                    else
                    {
                        var item = Session.FindArtifact(p.ArtifactId, SearchType.Global);
                        if (item != null)
                            name += " (" + item.Tier.ToString().ToLower() + " tier)";
                    }
                }

                var lvi = ParcelList.Items.Add(name);
                lvi.Tag = p;

                if (p.Details != "")
                    lvi.SubItems.Add(p.Details);
                else
                    lvi.SubItems.Add("(no details)");

                Hero hero = null;
                if (p.HeroId != Guid.Empty)
                    hero = Session.Project.FindHero(p.HeroId);

                if (hero != null)
                    lvi.SubItems.Add("Allocated to " + hero.Name);
                else
                    lvi.SubItems.Add("(not allocated to a PC)");
            }

            if (ParcelList.Items.Count == 0)
            {
                var lvi = ParcelList.Items.Add("(no parcels)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void EncyclopediaAddBtn_Click(object sender, EventArgs e)
        {
            var dlg = new EncyclopediaEntrySelectForm(PlotPoint.EncyclopediaEntryIDs);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.EncyclopediaEntryIDs.Add(dlg.EncyclopediaEntry.Id);
                update_encyclopedia_entries();
            }
        }

        private void EncyclopediaRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEntry != null)
            {
                PlotPoint.EncyclopediaEntryIDs.Remove(SelectedEntry.Id);
                update_encyclopedia_entries();
            }
        }

        private void EncPlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEntry != null)
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowEncyclopediaItem(SelectedEntry);
            }
        }

        private void EncyclopediaList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var text = Html.EncyclopediaEntry(SelectedEntry, Session.Project.Encyclopedia, Session.Preferences.TextSize,
                true, false, false, true);

            EncBrowser.Document.OpenNew(true);
            EncBrowser.Document.Write(text);
        }

        private void update_encyclopedia_entries()
        {
            EncyclopediaList.BeginUpdate();

            EncyclopediaList.Items.Clear();
            foreach (var entryId in PlotPoint.EncyclopediaEntryIDs)
            {
                var entry = Session.Project.Encyclopedia.FindEntry(entryId);
                if (entry == null)
                    continue;

                var lvi = EncyclopediaList.Items.Add(entry.Name);
                lvi.Tag = entry;
            }

            if (EncyclopediaList.Items.Count == 0)
            {
                var lvi = EncyclopediaList.Items.Add("(no encyclopedia entries)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            EncyclopediaList.EndUpdate();
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLink != null)
            {
                if (SelectedLink.Links.Contains(PlotPoint.Id))
                    // Remove the link to this item
                    while (SelectedLink.Links.Contains(PlotPoint.Id))
                        SelectedLink.Links.Remove(PlotPoint.Id);
                else if (PlotPoint.Links.Contains(SelectedLink.Id))
                    // Remove the link from this item
                    while (PlotPoint.Links.Contains(SelectedLink.Id))
                        PlotPoint.Links.Remove(SelectedLink.Id);

                update_links();
            }
        }

        private void LinkList_SizeChanged(object sender, EventArgs e)
        {
            var width = LinkList.Width - (SystemInformation.VerticalScrollBarWidth + 6);
            LinkList.TileSize = new Size(width, LinkList.TileSize.Height);
        }

        private void update_links()
        {
            LinkList.Items.Clear();

            // Links to this item
            foreach (var pp in _fPlot.Points)
                if (pp.Links.Contains(PlotPoint.Id))
                {
                    var lvi = LinkList.Items.Add(pp.Name);
                    lvi.SubItems.Add(pp.Details != "" ? pp.Details : "(no details)");

                    lvi.Tag = pp;
                    lvi.Group = LinkList.Groups[0];
                }

            // Links from this item
            foreach (var id in PlotPoint.Links)
            {
                var pp = _fPlot.FindPoint(id);
                if (pp != null)
                {
                    var lvi = LinkList.Items.Add(pp.Name);
                    lvi.SubItems.Add(pp.Details != "" ? pp.Details : "(no details)");

                    lvi.Tag = pp;
                    lvi.Group = LinkList.Groups[1];
                }
            }

            foreach (ListViewGroup lvg in LinkList.Groups)
                if (lvg.Items.Count == 0)
                {
                    var lvi = LinkList.Items.Add("(none)");
                    lvi.ForeColor = SystemColors.GrayText;
                    lvi.Group = lvg;
                }
        }

        private void LocationBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MapLocationSelectForm(PlotPoint.RegionalMapId, PlotPoint.MapLocationId);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                PlotPoint.RegionalMapId = dlg.Map?.Id ?? Guid.Empty;
                PlotPoint.MapLocationId = dlg.MapLocation?.Id ?? Guid.Empty;
            }
        }

        private void ClearLocationLbl_Click(object sender, EventArgs e)
        {
            PlotPoint.RegionalMapId = Guid.Empty;
            PlotPoint.MapLocationId = Guid.Empty;
        }

        private void DateBtn_Click(object sender, EventArgs e)
        {
            var date = PlotPoint.Date;
            if (date == null)
            {
                date = new CalendarDate();

                var cal = Session.Project.Calendars[0];
                date.CalendarId = cal.Id;
                date.Year = cal.CampaignYear;
            }

            var dlg = new DateForm(date);
            if (dlg.ShowDialog() == DialogResult.OK)
                PlotPoint.Date = dlg.Date;
        }

        private void ClearDateLbl_Click(object sender, EventArgs e)
        {
            PlotPoint.Date = null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            PlotPoint.Name = NameBox.Text;
            PlotPoint.Details = DetailsBox.Text != DetailsBox.DefaultText ? DetailsBox.Text : "";
            PlotPoint.ReadAloud = ReadAloudBox.Text != ReadAloudBox.DefaultText ? ReadAloudBox.Text : "";
            PlotPoint.AdditionalXp = (int)XPBox.Value;
        }

        private void AllocateBtn_DropDownOpening(object sender, EventArgs e)
        {
            AllocateBtn.DropDownItems.Clear();
            foreach (var hero in Session.Project.Heroes)
            {
                var tsmi = new ToolStripMenuItem(hero.Name);
                tsmi.Tag = hero;
                tsmi.Click += assign_to_hero;

                if (SelectedParcel != null)
                    tsmi.Checked = SelectedParcel.HeroId == hero.Id;

                AllocateBtn.DropDownItems.Add(tsmi);
            }

            if (Session.Project.Heroes.Count != 0)
                AllocateBtn.DropDownItems.Add(new ToolStripSeparator());

            var tsmiNone = new ToolStripMenuItem("Not Allocated");
            tsmiNone.Tag = null;
            tsmiNone.Click += assign_to_hero;

            if (SelectedParcel != null)
                tsmiNone.Checked = SelectedParcel.HeroId == Guid.Empty;

            AllocateBtn.DropDownItems.Add(tsmiNone);
        }

        private void assign_to_hero(object sender, EventArgs e)
        {
            if (SelectedParcel == null)
                return;

            var tsi = sender as ToolStripItem;
            if (tsi == null)
                return;

            var hero = tsi.Tag as Hero;
            SelectedParcel.HeroId = hero?.Id ?? Guid.Empty;

            update_parcels();
        }

        private void PlotPointForm_Shown(object sender, EventArgs e)
        {
            if (_fStartAtElement)
            {
                if (RPGPanel.Controls.Count == 0)
                    return;

                var encPnl = RPGPanel.Controls[0] as EncounterPanel;
                encPnl?.Edit();

                var scPnl = RPGPanel.Controls[0] as SkillChallengePanel;
                scPnl?.Edit();
            }
        }

        private int get_party_level()
        {
            // Work out the approximate party level here
            var level = Session.Project.Party.Level;
            if (_fPlot.FindPoint(PlotPoint.Id) != null)
            {
                level = Workspace.GetPartyLevel(PlotPoint);
            }
            else
            {
                if (_fPlot.Points.Count > 0)
                {
                    var layers = Workspace.FindLayers(_fPlot);
                    level = Workspace.GetPartyLevel(layers[0][0]);
                }
            }

            return level;
        }

        private void SettingsMenu_DropDownOpening(object sender, EventArgs e)
        {
            SettingsMenu.DropDownItems.Clear();

            var states = Enum.GetValues(typeof(PlotPointState));
            foreach (PlotPointState state in states)
            {
                var tsmi = SettingsMenu.DropDownItems.Add(state.ToString()) as ToolStripMenuItem;
                tsmi.Tag = state;
                tsmi.Checked = PlotPoint.State == state;
                tsmi.Click += select_state;
            }

            SettingsMenu.DropDownItems.Add(new ToolStripSeparator());

            var colours = Enum.GetValues(typeof(PlotPointColour));
            foreach (PlotPointColour colour in colours)
            {
                var text = colour.ToString();
                if (colour == PlotPointColour.Yellow)
                    text += " (default)";

                var img = new Bitmap(16, 16);
                var imgRect = new Rectangle(0, 0, 16, 16);
                var gradient = PlotView.GetColourGradient(colour, 255);
                var g = Graphics.FromImage(img);
                g.FillRectangle(
                    new LinearGradientBrush(imgRect, gradient.First, gradient.Second, LinearGradientMode.Vertical),
                    imgRect);

                var tsmi = SettingsMenu.DropDownItems.Add(text) as ToolStripMenuItem;
                tsmi.Image = img;
                tsmi.Tag = colour;
                tsmi.Checked = PlotPoint.Colour == colour;
                tsmi.Click += select_colour;
            }
        }

        private void select_state(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            if (tsmi != null)
            {
                var state = (PlotPointState)tsmi.Tag;
                PlotPoint.State = state;
            }
        }

        private void select_colour(object sender, EventArgs e)
        {
            var tsmi = sender as ToolStripMenuItem;
            if (tsmi != null)
            {
                var colour = (PlotPointColour)tsmi.Tag;
                PlotPoint.Colour = colour;
            }
        }

        private void InfoBtn_Click(object sender, EventArgs e)
        {
            var dlg = new InfoForm();
            dlg.Level = get_party_level();
            dlg.ShowDialog();
        }

        private void DieRollerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new DieRollerForm();
            dlg.ShowDialog();
        }
    }
}
