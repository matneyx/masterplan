using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Controls.Elements;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Extensibility;
using Masterplan.Tools;
using Masterplan.Tools.Generators;
using Masterplan.UI.PlayerOptions;

namespace Masterplan.UI
{
    internal partial class MainForm : Form
    {
        public enum ViewType
        {
            Flowchart,
            Delve,
            Map
        }

        private readonly ExtensibilityManager _fExtensibility;

        private MapView _fDelveView;
        private RegionalMapPanel _fMapView;

        private string _fPartyBreakdownSecondary = "";

        private bool _fUpdating;

        private ViewType _fView = ViewType.Flowchart;

        private WelcomePanel _fWelcome;

        public Background SelectedBackground
        {
            get
            {
                if (BackgroundList.SelectedItems.Count != 0)
                    return BackgroundList.SelectedItems[0].Tag as Background;

                return null;
            }
            set
            {
                BackgroundList.SelectedItems.Clear();

                if (value != null)
                    foreach (ListViewItem lvi in BackgroundList.Items)
                    {
                        var bg = lvi.Tag as Background;
                        if (bg != null && bg.Id == value.Id)
                            lvi.Selected = true;
                    }

                update_background_item();
            }
        }

        public IEncyclopediaItem SelectedEncyclopediaItem
        {
            get
            {
                if (EntryList.SelectedItems.Count != 0)
                    return EntryList.SelectedItems[0].Tag as IEncyclopediaItem;

                return null;
            }
            set
            {
                EntryList.SelectedItems.Clear();

                if (value != null)
                    foreach (ListViewItem lvi in EntryList.Items)
                    {
                        var entry = lvi.Tag as IEncyclopediaItem;
                        if (entry != null && entry.Id == value.Id)
                            lvi.Selected = true;
                    }

                update_encyclopedia_entry();
            }
        }

        public EncyclopediaImage SelectedEncyclopediaImage
        {
            get
            {
                if (EntryImageList.SelectedItems.Count != 0)
                    return EntryImageList.SelectedItems[0].Tag as EncyclopediaImage;

                return null;
            }
        }

        public IPlayerOption SelectedRule
        {
            get
            {
                if (RulesList.SelectedItems.Count != 0)
                    return RulesList.SelectedItems[0].Tag as IPlayerOption;

                return null;
            }
            set
            {
                RulesList.SelectedItems.Clear();

                if (value != null)
                    foreach (ListViewItem lvi in RulesList.Items)
                    {
                        var n = lvi.Tag as IPlayerOption;
                        if (n != null && n.Id == value.Id)
                            lvi.Selected = true;
                    }
            }
        }

        public List<Attachment> SelectedAttachments
        {
            get
            {
                var attachments = new List<Attachment>();

                foreach (ListViewItem lvi in AttachmentList.SelectedItems)
                {
                    var att = lvi.Tag as Attachment;
                    if (att != null)
                        attachments.Add(att);
                }

                return attachments;
            }
        }

        public Note SelectedNote
        {
            get
            {
                if (NoteList.SelectedItems.Count != 0)
                    return NoteList.SelectedItems[0].Tag as Note;

                return null;
            }
            set
            {
                NoteList.SelectedItems.Clear();

                if (value != null)
                    foreach (ListViewItem lvi in NoteList.Items)
                    {
                        var n = lvi.Tag as Note;
                        if (n != null && n.Id == value.Id)
                            lvi.Selected = true;
                    }
            }
        }

        public IIssue SelectedIssue
        {
            get
            {
                if (NoteList.SelectedItems.Count != 0)
                    return NoteList.SelectedItems[0].Tag as IIssue;

                return null;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            try
            {
                Preview.DocumentText = "";
                BackgroundDetails.DocumentText = "";
                EntryDetails.DocumentText = "";
                RulesBrowser.DocumentText = "";
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            try
            {
                _fExtensibility = new ExtensibilityManager(this);
                foreach (var addin in _fExtensibility.AddIns)
                {
                    foreach (var addinPage in addin.Pages)
                    {
                        var page = new TabPage(addinPage.Name);
                        Pages.TabPages.Add(page);

                        page.Controls.Add(addinPage.Control);
                        addinPage.Control.Dock = DockStyle.Fill;
                    }

                    foreach (var page in addin.QuickReferencePages)
                    {
                        var tabpage = new TabPage();
                        tabpage.Text = page.Name;

                        tabpage.Controls.Add(page.Control);
                        page.Control.Dock = DockStyle.Fill;

                        ReferencePages.TabPages.Add(tabpage);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            try
            {
                if (Session.Project == null)
                {
                    Controls.Clear();

                    _fWelcome = new WelcomePanel();
                    _fWelcome.Dock = DockStyle.Fill;

                    _fWelcome.NewProjectClicked += Welcome_NewProjectClicked;
                    _fWelcome.OpenProjectClicked += Welcome_OpenProjectClicked;
                    _fWelcome.OpenLastProjectClicked += Welcome_OpenLastProjectClicked;
                    _fWelcome.DelveClicked += Welcome_DelveClicked;

                    Controls.Add(_fWelcome);
                    Controls.Add(MainMenu);
                }
                else
                {
                    PlotView.Plot = Session.Project.Plot;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            try
            {
                NavigationSplitter.Panel1Collapsed = !Session.Preferences.Workspace.ShowNavigation;
                PreviewSplitter.Panel2Collapsed = !Session.Preferences.Workspace.ShowPreview;
                PlotView.LinkStyle = Session.Preferences.Workspace.LinkStyle;
                WorkspaceSearchBar.Visible = false;

                update_encyclopedia_templates();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            try
            {
                if (Session.Preferences.Window.Maximised)
                {
                    WindowState = FormWindowState.Maximized;
                }
                else if (Session.Preferences.Window.Size != Size.Empty &&
                         Session.Preferences.Window.Location != Point.Empty)
                {
                    StartPosition = FormStartPosition.Manual;

                    var width = Math.Max(Width, Session.Preferences.Window.Size.Width);
                    var height = Math.Max(Height, Session.Preferences.Window.Size.Height);
                    Size = new Size(width, height);

                    var x = Math.Max(Left, Session.Preferences.Window.Location.X);
                    var y = Math.Max(Top, Session.Preferences.Window.Location.Y);
                    Location = new Point(x, y);
                }
                else
                {
                    StartPosition = FormStartPosition.CenterScreen;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            update_title();
            UpdateView();
        }

        ~MainForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            try
            {
                if (Pages.SelectedTab == WorkspacePage)
                {
                    var selectedPoint = get_selected_point();

                    RemoveBtn.Enabled = selectedPoint != null;
                    PlotCutBtn.Enabled = selectedPoint != null;
                    PlotCopyBtn.Enabled = selectedPoint != null;
                    PlotPasteBtn.Enabled =
                        Clipboard.ContainsData(typeof(PlotPoint).ToString()) || Clipboard.ContainsText();
                    SearchBtn.Checked = WorkspaceSearchBar.Visible;
                    PlotClearBtn.Visible = PlotSearchBox.Text != "";

                    EditBtn.Enabled = selectedPoint != null;
                    ExploreBtn.Enabled = selectedPoint != null;
                    PlotPointMenu.Enabled = selectedPoint != null;
                    PlotPointPlayerView.Enabled = selectedPoint != null && selectedPoint.ReadAloud != "";
                    PlotPointExportHTML.Enabled = selectedPoint != null;

                    ContextRemove.Enabled = RemoveBtn.Enabled;
                    ContextEdit.Enabled = EditBtn.Enabled;
                    ContextExplore.Enabled = EditBtn.Enabled;
                    ContextState.Enabled = selectedPoint != null;

                    FlowchartAllXP.Checked = Session.Preferences.AllXp;
                }

                if (Pages.SelectedTab == BackgroundPage)
                {
                    BackgroundRemoveBtn.Enabled = SelectedBackground != null;
                    BackgroundEditBtn.Enabled = SelectedBackground != null;

                    BackgroundUpBtn.Enabled = SelectedBackground != null &&
                                              Session.Project.Backgrounds.IndexOf(SelectedBackground) != 0;
                    BackgroundDownBtn.Enabled = SelectedBackground != null &&
                                                Session.Project.Backgrounds.IndexOf(SelectedBackground) !=
                                                Session.Project.Backgrounds.Count - 1;

                    BackgroundPlayerViewSelected.Enabled =
                        SelectedBackground != null && SelectedBackground.Details != "";
                    BackgroundPlayerViewAll.Enabled = Session.Project != null && Session.Project.Backgrounds.Count != 0;
                }

                if (Pages.SelectedTab == EncyclopediaPage)
                {
                    EncAddGroup.Enabled = Session.Project != null && Session.Project.Encyclopedia.Entries.Count != 0;
                    EncRemoveBtn.Enabled = SelectedEncyclopediaItem != null;
                    EncEditBtn.Enabled = SelectedEncyclopediaItem != null;

                    EncCutBtn.Enabled = SelectedEncyclopediaItem is EncyclopediaEntry;
                    EncCopyBtn.Enabled = SelectedEncyclopediaItem is EncyclopediaEntry;
                    EncPasteBtn.Enabled = Clipboard.ContainsData(typeof(EncyclopediaEntry).ToString()) ||
                                          Clipboard.ContainsText();

                    EncPlayerView.Enabled = SelectedEncyclopediaItem != null;

                    EncShareExport.Enabled = Session.Project != null && Session.Project.Encyclopedia.Entries.Count != 0;
                    EncSharePublish.Enabled =
                        Session.Project != null && Session.Project.Encyclopedia.Entries.Count != 0;

                    EncClearLbl.Visible = EncSearchBox.Text != "";
                }

                if (Pages.SelectedTab == RulesPage)
                {
                    RulesRemoveBtn.Enabled = SelectedRule != null;
                    RulesEditBtn.Enabled = SelectedRule != null;

                    RulesPlayerViewBtn.Enabled = SelectedRule != null;
                    RuleEncyclopediaBtn.Enabled = SelectedRule != null;

                    RulesShareExport.Enabled = Session.Project != null && Session.Project.PlayerOptions.Count != 0;
                    RulesSharePublish.Enabled = Session.Project != null && Session.Project.PlayerOptions.Count != 0;
                }

                if (Pages.SelectedTab == AttachmentsPage)
                {
                    AttachmentImportBtn.Enabled = true;
                    AttachmentRemoveBtn.Enabled = SelectedAttachments.Count != 0;
                    AttachmentExtract.Enabled = SelectedAttachments.Count != 0;
                    AttachmentPlayerView.Enabled = SelectedAttachments.Count == 1 &&
                                                   SelectedAttachments[0].Type != AttachmentType.Miscellaneous;
                }

                if (Pages.SelectedTab == JotterPage)
                {
                    NoteRemoveBtn.Enabled = SelectedNote != null;
                    NoteCategoryBtn.Enabled = SelectedNote != null;

                    NoteCutBtn.Enabled = SelectedNote != null;
                    NoteCopyBtn.Enabled = SelectedNote != null;
                    NotePasteBtn.Enabled = Clipboard.ContainsData(typeof(Note).ToString()) || Clipboard.ContainsText();
                    NoteClearLbl.Visible = NoteSearchBox.Text != "";
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys key)
        {
            if (Session.Project != null)
            {
                if (Pages.SelectedTab == WorkspacePage)
                {
                    if (key == (Keys.Control | Keys.A))
                    {
                        AddBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.X))
                    {
                        CutBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.C))
                    {
                        CopyBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.V))
                    {
                        PasteBtn_Click(null, null);
                        return true;
                    }

                    var handled = PlotView.Navigate(key);
                    if (handled)
                        return true;
                }

                if (Pages.SelectedTab == BackgroundPage)
                    if (key == (Keys.Control | Keys.A))
                    {
                        BackgroundAddBtn_Click(null, null);
                        return true;
                    }

                if (Pages.SelectedTab == EncyclopediaPage)
                {
                    if (key == (Keys.Control | Keys.A))
                    {
                        EncAddEntry_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.X))
                    {
                        EncCutBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.C))
                    {
                        EncCopyBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.V))
                    {
                        EncPasteBtn_Click(null, null);
                        return true;
                    }
                }

                if (Pages.SelectedTab == AttachmentsPage)
                    if (key == (Keys.Control | Keys.A))
                    {
                        AttachmentImportBtn_Click(null, null);
                        return true;
                    }

                if (Pages.SelectedTab == JotterPage)
                {
                    if (key == (Keys.Control | Keys.X))
                    {
                        NoteCutBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.C))
                    {
                        NoteCopyBtn_Click(null, null);
                        return true;
                    }

                    if (key == (Keys.Control | Keys.V))
                    {
                        NotePasteBtn_Click(null, null);
                        return true;
                    }
                }
            }

            return base.ProcessCmdKey(ref msg, key);
        }

        private void MainForm_Layout(object sender, LayoutEventArgs e)
        {
            try
            {
                //
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MainForm_Shown(object sender, EventArgs e)
        {
            try
            {
                Session.MainForm = this;

                if (Program.SplashScreen != null)
                {
                    Program.SplashScreen.Close();
                    Program.SplashScreen = null;
                }

                PlotView_SelectionChanged(null, null);
                NoteList_SelectedIndexChanged(null, null);

                if (Session.DisabledLibraries != null && Session.DisabledLibraries.Count != 0)
                {
                    var str = "Due to copy protection, some libraries were not loaded:";
                    str += Environment.NewLine;

                    var libs = new List<string>(Session.DisabledLibraries);
                    var count = Math.Min(libs.Count, 6);
                    for (var n = 0; n != count; ++n)
                    {
                        var index = Session.Random.Next(libs.Count);
                        var item = libs[index];
                        libs.Remove(item);

                        str += Environment.NewLine;
                        str += "* " + item;
                    }

                    if (libs.Count > 0)
                    {
                        str += Environment.NewLine + Environment.NewLine;
                        str += "... and " + libs.Count + " others.";
                    }

                    MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (Session.Project == null && Session.Creatures.Count == 0)
                {
                    var mpDir = FileName.Directory(Application.ExecutablePath);
                    if (mpDir.Contains("Program Files"))
                    {
                        var str = "You're running Masterplan from the Program Files folder.";
                        str += Environment.NewLine + Environment.NewLine;
                        str +=
                            "Although Masterplan will run, this is a protected folder, and Masterplan won't be able to save any changes that you make to your libraries.";
                        str += Environment.NewLine + Environment.NewLine;
                        str +=
                            "If you move Masterplan to a new location (like My Documents or the Desktop), you won't have this problem.";

                        MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (!check_modified())
                    e.Cancel = true;

                if (Session.FileName != "")
                    Session.Preferences.LastFile = Session.FileName;

                Session.Preferences.Window.Maximised = WindowState == FormWindowState.Maximized;
                if (!Session.Preferences.Window.Maximised)
                {
                    Session.Preferences.Window.Maximised = false;
                    Session.Preferences.Window.Size = Size;
                    Session.Preferences.Window.Location = Location;
                }
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
                if (e.Url.Scheme == "plot")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "add")
                        AddBtn_Click(sender, e);

                    if (e.Url.LocalPath == "encounter")
                    {
                        if (PlotView.SelectedPoint == null)
                        {
                            AddEncounter_Click(sender, e);
                        }
                        else
                        {
                            PlotView.SelectedPoint.Element = new Encounter();
                            if (!edit_element(null, null))
                                PlotView.SelectedPoint.Element = null;
                        }
                    }

                    if (e.Url.LocalPath == "challenge")
                    {
                        if (PlotView.SelectedPoint == null)
                        {
                            AddChallenge_Click(sender, e);
                        }
                        else
                        {
                            var sc = new SkillChallenge();
                            sc.Level = Session.Project.Party.Level;

                            PlotView.SelectedPoint.Element = sc;
                            if (!edit_element(null, null))
                                PlotView.SelectedPoint.Element = null;
                        }
                    }

                    if (e.Url.LocalPath == "edit")
                        EditBtn_Click(sender, e);

                    if (e.Url.LocalPath == "explore")
                        ExploreBtn_Click(sender, e);

                    if (e.Url.LocalPath == "properties")
                        ProjectProject_Click(sender, e);

                    if (e.Url.LocalPath == "up")
                    {
                        var pp = Session.Project.FindParent(PlotView.Plot);
                        if (pp != null)
                        {
                            var plot = Session.Project.FindParent(pp);
                            if (plot != null)
                            {
                                if (_fView != ViewType.Flowchart)
                                    flowchart_view();

                                PlotView.Plot = plot;
                                PlotView.SelectedPoint = pp;

                                UpdateView();
                            }
                        }
                    }

                    if (e.Url.LocalPath == "element")
                        edit_element(sender, e);

                    if (e.Url.LocalPath == "run")
                        run_encounter(sender, e);

                    if (e.Url.LocalPath == "maparea")
                    {
                        var point = get_selected_point();

                        Map map = null;
                        MapArea mapArea = null;
                        point.GetTacticalMapArea(ref map, ref mapArea);

                        edit_map_area(map, mapArea, null);
                    }

                    if (e.Url.LocalPath == "maploc")
                    {
                        var point = get_selected_point();

                        RegionalMap map = null;
                        MapLocation loc = null;
                        point.GetRegionalMapArea(ref map, ref loc, Session.Project);

                        show_map_location(map, loc);
                    }
                }

                if (e.Url.Scheme == "entry")
                {
                    e.Cancel = true;

                    var entryId = new Guid(e.Url.LocalPath);
                    var entry = Session.Project.Encyclopedia.FindEntry(entryId);
                    if (entry != null)
                    {
                        var dlg = new EncyclopediaEntryDetailsForm(entry);
                        dlg.ShowDialog();
                    }
                }

                if (e.Url.Scheme == "item")
                {
                    e.Cancel = true;

                    var itemId = new Guid(e.Url.LocalPath);
                    var item = Session.FindMagicItem(itemId, SearchType.Global);
                    if (item != null)
                    {
                        var dlg = new MagicItemDetailsForm(item);
                        dlg.ShowDialog();
                    }
                }

                if (e.Url.Scheme == "delveview")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "select")
                    {
                        var dlg = new MapSelectForm(Session.Project.Maps, null, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                            delve_view(dlg.Map);
                    }
                    else if (e.Url.LocalPath == "off")
                    {
                        flowchart_view();
                    }
                    else if (e.Url.LocalPath == "edit")
                    {
                        delve_view_edit();
                    }
                    else if (e.Url.LocalPath == "build")
                    {
                        var m = new Map();
                        m.Name = "New Map";

                        var dlg = new MapBuilderForm(m, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Session.Project.Maps.Add(dlg.Map);
                            delve_view(dlg.Map);
                        }
                    }
                    else if (e.Url.LocalPath == "playerview")
                    {
                        var view = new MapView();
                        view.Map = _fDelveView.Map;
                        view.Plot = PlotView.Plot;
                        view.Mode = MapViewMode.PlayerView;
                        view.LineOfSight = false;
                        view.BorderSize = 1;
                        view.HighlightAreas = false;

                        // Show only the explored area
                        var explored = false;
                        var left = int.MaxValue;
                        var right = int.MinValue;
                        var top = int.MaxValue;
                        var bottom = int.MinValue;
                        foreach (var area in _fDelveView.Map.Areas)
                        {
                            var point = PlotView.Plot.FindPointForMapArea(_fDelveView.Map, area);
                            if (point != null)
                                if (point.State == PlotPointState.Completed)
                                {
                                    explored = true;

                                    left = Math.Min(left, area.Region.Left);
                                    right = Math.Max(right, area.Region.Right);
                                    top = Math.Min(top, area.Region.Top);
                                    bottom = Math.Max(bottom, area.Region.Bottom);
                                }
                        }

                        if (explored)
                            view.Viewpoint = new Rectangle(left, top, right - left, bottom - top);

                        if (Session.PlayerView == null)
                            Session.PlayerView = new PlayerViewForm(this);
                        Session.PlayerView.ShowTacticalMap(view, null);
                    }
                    else
                    {
                        var mapId = new Guid(e.Url.LocalPath);
                        var map = Session.Project.FindTacticalMap(mapId);
                        if (map != null)
                            delve_view(map);
                    }
                }

                if (e.Url.Scheme == "mapview")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "select")
                    {
                        var dlg = new RegionalMapSelectForm(Session.Project.RegionalMaps, null, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                            map_view(dlg.Map);
                    }
                    else if (e.Url.LocalPath == "off")
                    {
                        flowchart_view();
                    }
                    else if (e.Url.LocalPath == "edit")
                    {
                        map_view_edit();
                    }
                    else if (e.Url.LocalPath == "build")
                    {
                        var m = new RegionalMap();
                        m.Name = "New Map";

                        var dlg = new RegionalMapForm(m, null);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            Session.Project.RegionalMaps.Add(dlg.Map);
                            map_view(dlg.Map);
                        }
                    }
                    else if (e.Url.LocalPath == "playerview")
                    {
                        var view = new RegionalMapPanel();
                        view.Map = _fMapView.Map;
                        view.Plot = PlotView.Plot;
                        view.Mode = MapViewMode.PlayerView;

                        if (Session.PlayerView == null)
                            Session.PlayerView = new PlayerViewForm(this);
                        Session.PlayerView.ShowRegionalMap(view);
                    }
                    else
                    {
                        var mapId = new Guid(e.Url.LocalPath);
                        var map = Session.Project.FindRegionalMap(mapId);
                        if (map != null)
                            map_view(map);
                    }
                }

                if (e.Url.Scheme == "maparea")
                {
                    e.Cancel = true;

                    MapView mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView)
                        {
                            mapview = ctrl as MapView;
                            break;
                        }

                    if (mapview?.SelectedArea == null)
                        return;

                    if (e.Url.LocalPath == "edit") edit_map_area(mapview.Map, mapview.SelectedArea, mapview);

                    if (e.Url.LocalPath == "create")
                    {
                        var pp = new PlotPoint(mapview.SelectedArea.Name);
                        pp.Element = new MapElement(mapview.Map.Id, mapview.SelectedArea.Id);

                        var dlg = new PlotPointForm(pp, PlotView.Plot, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "encounter")
                    {
                        var enc = new Encounter();
                        enc.MapId = mapview.Map.Id;
                        enc.MapAreaId = mapview.SelectedArea.Id;
                        enc.SetStandardEncounterNotes();

                        var pp = new PlotPoint(mapview.SelectedArea.Name);
                        pp.Element = enc;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "trap")
                    {
                        var te = new TrapElement();
                        te.Trap.Name = mapview.SelectedArea.Name;
                        te.MapId = mapview.Map.Id;
                        te.MapAreaId = mapview.SelectedArea.Id;

                        var pp = new PlotPoint(mapview.SelectedArea.Name);
                        pp.Element = te;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "challenge")
                    {
                        var sc = new SkillChallenge();
                        sc.Name = mapview.SelectedArea.Name;
                        sc.MapId = mapview.Map.Id;
                        sc.MapAreaId = mapview.SelectedArea.Id;
                        sc.Level = Session.Project.Party.Level;

                        var pp = new PlotPoint(mapview.SelectedArea.Name);
                        pp.Element = sc;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }
                }

                if (e.Url.Scheme == "maploc")
                {
                    e.Cancel = true;

                    RegionalMapPanel mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel)
                        {
                            mapview = ctrl as RegionalMapPanel;
                            break;
                        }

                    if (mapview?.SelectedLocation == null)
                        return;

                    if (e.Url.LocalPath == "edit") edit_map_location(mapview.Map, mapview.SelectedLocation, mapview);

                    if (e.Url.LocalPath == "create")
                    {
                        var pp = new PlotPoint(mapview.SelectedLocation.Name);
                        pp.RegionalMapId = mapview.Map.Id;
                        pp.MapLocationId = mapview.SelectedLocation.Id;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "encounter")
                    {
                        var enc = new Encounter();
                        enc.SetStandardEncounterNotes();

                        var pp = new PlotPoint(mapview.SelectedLocation.Name);
                        pp.RegionalMapId = mapview.Map.Id;
                        pp.MapLocationId = mapview.SelectedLocation.Id;
                        pp.Element = enc;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "trap")
                    {
                        var te = new TrapElement();
                        te.Trap.Name = mapview.SelectedLocation.Name;

                        var pp = new PlotPoint(mapview.SelectedLocation.Name);
                        pp.RegionalMapId = mapview.Map.Id;
                        pp.MapLocationId = mapview.SelectedLocation.Id;
                        pp.Element = te;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }

                    if (e.Url.LocalPath == "challenge")
                    {
                        var sc = new SkillChallenge();
                        sc.Name = mapview.SelectedLocation.Name;
                        sc.Level = Session.Project.Party.Level;

                        var pp = new PlotPoint(mapview.SelectedLocation.Name);
                        pp.RegionalMapId = mapview.Map.Id;
                        pp.MapLocationId = mapview.SelectedLocation.Id;
                        pp.Element = sc;

                        var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            PlotView.Plot.Points.Add(dlg.PlotPoint);
                            UpdateView();

                            Session.Modified = true;
                        }
                    }
                }

                if (e.Url.Scheme == "sc")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "reset")
                    {
                        // Reset
                        var point = get_selected_point();
                        var sc = point.Element as SkillChallenge;
                        if (sc != null)
                            foreach (var scd in sc.Skills)
                            {
                                scd.Results.Successes = 0;
                                scd.Results.Fails = 0;

                                Session.Modified = true;
                                UpdateView();
                            }
                    }
                }

                if (e.Url.Scheme == "success")
                {
                    e.Cancel = true;

                    // Success
                    var point = get_selected_point();
                    var sc = point.Element as SkillChallenge;
                    if (sc != null)
                    {
                        var scd = sc.FindSkill(e.Url.LocalPath);
                        scd.Results.Successes += 1;

                        Session.Modified = true;
                        UpdateView();
                    }
                }

                if (e.Url.Scheme == "failure")
                {
                    e.Cancel = true;

                    // Failure
                    var point = get_selected_point();
                    var sc = point.Element as SkillChallenge;
                    if (sc != null)
                    {
                        var scd = sc.FindSkill(e.Url.LocalPath);
                        scd.Results.Fails += 1;

                        Session.Modified = true;
                        UpdateView();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private PlotPoint get_selected_point()
        {
            switch (_fView)
            {
                case ViewType.Flowchart:
                    return PlotView.SelectedPoint;
                case ViewType.Delve:
                {
                    // Work out what map area is selected, and what plot point it corresponds to
                    MapView mapview = null;
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView)
                            mapview = ctrl as MapView;

                    var area = mapview?.SelectedArea;
                    if (area != null)
                        foreach (var pp in PlotView.Plot.Points)
                            if (pp.Element != null)
                            {
                                if (pp.Element is Encounter)
                                {
                                    var enc = pp.Element as Encounter;
                                    if (enc.MapId == mapview.Map.Id && enc.MapAreaId == area.Id)
                                        return pp;
                                }

                                if (pp.Element is TrapElement)
                                {
                                    var te = pp.Element as TrapElement;
                                    if (te.MapId == mapview.Map.Id && te.MapAreaId == area.Id)
                                        return pp;
                                }

                                if (pp.Element is SkillChallenge)
                                {
                                    var sc = pp.Element as SkillChallenge;
                                    if (sc.MapId == mapview.Map.Id && sc.MapAreaId == area.Id)
                                        return pp;
                                }

                                if (pp.Element is MapElement)
                                {
                                    var me = pp.Element as MapElement;
                                    if (me.MapId == mapview.Map.Id && me.MapAreaId == area.Id)
                                        return pp;
                                }
                            }
                }
                    break;
                case ViewType.Map:
                {
                    RegionalMapPanel mapview = null;
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel)
                            mapview = ctrl as RegionalMapPanel;

                    if (mapview != null)
                    {
                        if (mapview.SelectedLocation == null)
                            return null;

                        foreach (var pp in PlotView.Plot.Points)
                            if (pp.RegionalMapId == mapview.Map.Id && pp.MapLocationId == mapview.SelectedLocation.Id)
                                return pp;
                    }
                }
                    break;
            }

            return null;
        }

        private void set_selected_point(PlotPoint point)
        {
            switch (_fView)
            {
                case ViewType.Flowchart:
                {
                    PlotView.SelectedPoint = point;
                }
                    break;
                case ViewType.Delve:
                {
                    MapView mapview = null;
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView)
                            mapview = ctrl as MapView;

                    if (mapview != null)
                    {
                        mapview.SelectedArea = null;

                        if (point.Element != null)
                        {
                            if (point.Element is Encounter)
                            {
                                var enc = point.Element as Encounter;
                                if (enc.MapId == mapview.Map.Id)
                                {
                                    var area = mapview.Map.FindArea(enc.MapAreaId);
                                    mapview.SelectedArea = area;
                                }
                            }

                            if (point.Element is TrapElement)
                            {
                                var te = point.Element as TrapElement;
                                if (te.MapId == mapview.Map.Id)
                                {
                                    var area = mapview.Map.FindArea(te.MapAreaId);
                                    mapview.SelectedArea = area;
                                }
                            }

                            if (point.Element is SkillChallenge)
                            {
                                var sc = point.Element as SkillChallenge;
                                if (sc.MapId == mapview.Map.Id)
                                {
                                    var area = mapview.Map.FindArea(sc.MapAreaId);
                                    mapview.SelectedArea = area;
                                }
                            }

                            if (point.Element is MapElement)
                            {
                                var me = point.Element as MapElement;
                                if (me.MapId == mapview.Map.Id)
                                {
                                    var area = mapview.Map.FindArea(me.MapAreaId);
                                    mapview.SelectedArea = area;
                                }
                            }
                        }
                    }
                }
                    break;
                case ViewType.Map:
                {
                    RegionalMapPanel mapview = null;
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel)
                            mapview = ctrl as RegionalMapPanel;

                    if (mapview != null)
                    {
                        mapview.SelectedLocation = null;

                        if (point.RegionalMapId != mapview.Map.Id)
                        {
                            var loc = mapview.Map.FindLocation(point.MapLocationId);
                            mapview.SelectedLocation = loc;
                        }
                    }
                }
                    break;
            }
        }

        private void update_preview()
        {
            try
            {
                Preview.Document.OpenNew(true);
                var showPreview = false;

                var point = get_selected_point();

                Map map = null;
                MapArea mapArea = null;

                RegionalMap rmap = null;
                MapLocation loc = null;

                if (point == null && _fView == ViewType.Delve)
                {
                    MapView mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView)
                        {
                            mapview = ctrl as MapView;
                            break;
                        }

                    if (mapview != null)
                    {
                        map = mapview.Map;
                        mapArea = mapview.SelectedArea;
                    }
                }

                if (point == null && _fView == ViewType.Map)
                {
                    RegionalMapPanel mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel)
                        {
                            mapview = ctrl as RegionalMapPanel;
                            break;
                        }

                    if (mapview != null)
                    {
                        rmap = mapview.Map;
                        loc = mapview.SelectedLocation;
                    }
                }

                if (mapArea != null)
                {
                    Preview.Document.Write(Html.MapArea(mapArea, Session.Preferences.TextSize));
                }
                else if (loc != null)
                {
                    Preview.Document.Write(Html.MapLocation(loc, Session.Preferences.TextSize));
                }
                else
                {
                    var partyLevel = point != null ? Workspace.GetPartyLevel(point) : 0;
                    Preview.Document.Write(Html.PlotPoint(point, PlotView.Plot, partyLevel, true, _fView,
                        Session.Preferences.TextSize));
                }

                PreviewInfoSplitter.Panel2.Controls.Clear();

                if (point != null)
                {
                    if (point.Element is Encounter)
                    {
                        var enc = point.Element as Encounter;
                        if (enc.MapId != Guid.Empty)
                        {
                            set_tmap_preview(enc.MapId, enc.MapAreaId, enc);
                            showPreview = true;
                        }
                    }

                    if (point.Element is MapElement)
                    {
                        var me = point.Element as MapElement;
                        if (me.MapId != Guid.Empty)
                        {
                            set_tmap_preview(me.MapId, me.MapAreaId, null);
                            showPreview = true;
                        }
                    }

                    if (!showPreview)
                    {
                        RegionalMap regionalMap = null;
                        MapLocation location = null;
                        point.GetRegionalMapArea(ref regionalMap, ref location, Session.Project);

                        if (location != null)
                        {
                            set_rmap_preview(regionalMap, location);
                            showPreview = true;
                        }
                    }

                    if (!showPreview && point.Subplot.Points.Count > 0)
                    {
                        set_subplot_preview(point.Subplot);
                        showPreview = true;
                    }
                }
                else if (mapArea != null)
                {
                    set_tmap_preview(map.Id, mapArea.Id, null);
                    showPreview = true;
                }

                if (!showPreview)
                    PreviewInfoSplitter.Panel2.Controls.Clear();

                PreviewInfoSplitter.Panel2Collapsed = !showPreview;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void set_tmap_preview(Guid mapId, Guid areaId, Encounter enc)
        {
            try
            {
                var m = Session.Project.FindTacticalMap(mapId);
                if (m == null)
                    return;

                var mapview = new MapView();
                mapview.Mode = MapViewMode.Plain;
                mapview.FrameType = MapDisplayType.Dimmed;
                mapview.LineOfSight = false;
                mapview.ShowGrid = MapGridMode.None;
                mapview.BorderSize = 1;
                mapview.Map = m;

                if (areaId != Guid.Empty)
                {
                    var area = m.FindArea(areaId);
                    if (area != null)
                        mapview.Viewpoint = area.Region;
                }

                if (enc != null)
                {
                    mapview.Encounter = enc;
                    mapview.DoubleClick += run_encounter;
                }
                else
                {
                    mapview.DoubleClick += show_tmap;
                }

                mapview.BorderStyle = BorderStyle.Fixed3D;
                mapview.Dock = DockStyle.Fill;
                PreviewInfoSplitter.Panel2.Controls.Add(mapview);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void set_rmap_preview(RegionalMap map, MapLocation loc)
        {
            try
            {
                var mapview = new RegionalMapPanel();
                mapview.Mode = MapViewMode.Plain;
                mapview.Map = map;
                mapview.HighlightedLocation = loc;

                mapview.DoubleClick += show_rmap;

                mapview.BorderStyle = BorderStyle.Fixed3D;
                mapview.Dock = DockStyle.Fill;
                PreviewInfoSplitter.Panel2.Controls.Add(mapview);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void set_subplot_preview(Plot p)
        {
            try
            {
                var plotview = new PlotView();
                plotview.Plot = p;
                plotview.Mode = PlotViewMode.Plain;
                plotview.LinkStyle = Session.Preferences.Workspace.LinkStyle;

                plotview.DoubleClick += ExploreBtn_Click;

                plotview.BorderStyle = BorderStyle.Fixed3D;
                plotview.Dock = DockStyle.Fill;
                PreviewInfoSplitter.Panel2.Controls.Add(plotview);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool edit_element(object sender, EventArgs e)
        {
            try
            {
                var point = get_selected_point();
                if (point != null)
                {
                    var partyLevel = Workspace.GetPartyLevel(point);

                    var enc = point.Element as Encounter;
                    if (enc != null)
                    {
                        var dlg = new EncounterBuilderForm(enc, partyLevel, false);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            point.Element = dlg.Encounter;
                            Session.Modified = true;

                            UpdateView();

                            return true;
                        }
                    }

                    var sc = point.Element as SkillChallenge;
                    if (sc != null)
                    {
                        var dlg = new SkillChallengeBuilderForm(sc);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            point.Element = dlg.SkillChallenge;
                            Session.Modified = true;

                            UpdateView();

                            return true;
                        }
                    }

                    var te = point.Element as TrapElement;
                    if (te != null)
                    {
                        var dlg = new TrapBuilderForm(te.Trap);
                        if (dlg.ShowDialog() == DialogResult.OK)
                        {
                            te.Trap = dlg.Trap;
                            Session.Modified = true;

                            UpdateView();

                            return true;
                        }
                    }

                    // Map - can't edit
                    // Quest - can't edit
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private void run_encounter(object sender, EventArgs e)
        {
            try
            {
                var point = get_selected_point();
                var enc = point?.Element as Encounter;
                if (enc != null)
                {
                    var cs = new CombatState();
                    cs.Encounter = enc;
                    cs.PartyLevel = Workspace.GetPartyLevel(point);

                    var dlg = new CombatForm(cs);
                    dlg.Show();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void edit_map_area(Map map, MapArea mapArea, MapView mapview)
        {
            try
            {
                if (map != null && mapArea != null)
                {
                    var index = map.Areas.IndexOf(mapArea);

                    var dlg = new MapAreaForm(mapArea, map);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        map.Areas[index] = dlg.Area;
                        Session.Modified = true;

                        if (mapview != null)
                            mapview.SelectedArea = dlg.Area;

                        UpdateView();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void edit_map_location(RegionalMap map, MapLocation loc, RegionalMapPanel mappanel)
        {
            try
            {
                if (map != null && loc != null)
                {
                    var index = map.Locations.IndexOf(loc);

                    var dlg = new MapLocationForm(loc);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        map.Locations[index] = dlg.MapLocation;
                        Session.Modified = true;

                        if (mappanel != null)
                            mappanel.SelectedLocation = dlg.MapLocation;

                        UpdateView();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void show_map_location(RegionalMap map, MapLocation loc)
        {
            var dlg = new RegionalMapForm(map, loc);
            dlg.ShowDialog();
        }

        private void show_tmap(object sender, EventArgs e)
        {
            var mapview = sender as MapView;

            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowTacticalMap(mapview, null);
        }

        private void show_rmap(object sender, EventArgs e)
        {
            var mapview = sender as RegionalMapPanel;

            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowRegionalMap(mapview);
        }

        private void NavigationTree_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                e.Effect = DragDropEffects.None;

                var pp = e.Data.GetData(typeof(PlotPoint)) as PlotPoint;
                if (pp != null)
                {
                    var mouse = NavigationTree.PointToClient(Cursor.Position);
                    var tn = NavigationTree.GetNodeAt(mouse);

                    if (tn == null)
                        return;

                    var target = tn.Tag as Plot;

                    if (target.Points.Contains(pp))
                        return;

                    if (pp == Session.Project.FindParent(target))
                        return;

                    NavigationTree.SelectedNode = tn;
                    e.Effect = DragDropEffects.Move;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NavigationTree_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var pp = e.Data.GetData(typeof(PlotPoint)) as PlotPoint;
                if (pp != null)
                {
                    var mouse = NavigationTree.PointToClient(Cursor.Position);
                    var tn = NavigationTree.GetNodeAt(mouse);

                    if (tn == null)
                        return;

                    var target = tn.Tag as Plot;
                    NavigationTree.SelectedNode = tn;

                    if (target.Points.Contains(pp))
                        return;

                    if (pp == Session.Project.FindParent(target))
                        return;

                    // Remove all links to this point in its parent plot
                    var parent = Session.Project.FindParent(pp);
                    parent.RemovePoint(pp);
                    pp.Links.Clear();

                    // Move the plot point into the target plot
                    parent.Points.Remove(pp);
                    target.Points.Add(pp);

                    Session.Modified = true;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NavigationTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (_fUpdating)
                    return;

                var p = e.Node.Tag as Plot;

                if (PlotView.Plot != p)
                {
                    PlotView.Plot = p;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Welcome_NewProjectClicked(object sender, EventArgs e)
        {
            try
            {
                FileNew_Click(sender, e);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Welcome_OpenProjectClicked(object sender, EventArgs e)
        {
            try
            {
                FileOpen_Click(sender, e);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Welcome_OpenLastProjectClicked(object sender, EventArgs e)
        {
            try
            {
                open_file(Session.Preferences.LastFile);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Welcome_DelveClicked(object sender, EventArgs e)
        {
            try
            {
                AdvancedDelve_Click(null, null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotView_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint == null)
                {
                    // Create new point
                    AddBtn_Click(sender, e);
                }
                else
                {
                    if ((ModifierKeys & Keys.Shift) == Keys.Shift)
                        // Explore point
                        ExploreBtn_Click(sender, e);
                    else
                        // Open point
                        EditBtn_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotView_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                update_preview();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotView_PlotLayoutChanged(object sender, EventArgs e)
        {
            Session.Modified = true;
        }

        private void PlotView_PlotChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void PointMenu_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                ContextAddBetween.DropDownItems.Clear();
                ContextDisconnect.DropDownItems.Clear();

                if (PlotView.SelectedPoint != null)
                {
                    foreach (var pp in PlotView.Plot.Points)
                    {
                        if (!pp.Links.Contains(PlotView.SelectedPoint.Id))
                            continue;

                        var tsmi = new ToolStripMenuItem("After \"" + pp.Name + "\"");
                        tsmi.Click += add_between;
                        tsmi.Tag = new Pair<PlotPoint, PlotPoint>(pp, PlotView.SelectedPoint);
                        ContextAddBetween.DropDownItems.Add(tsmi);

                        var tsmiDisconnect = new ToolStripMenuItem(pp.Name);
                        tsmiDisconnect.Click += disconnect_points;
                        tsmiDisconnect.Tag = new Pair<PlotPoint, PlotPoint>(pp, PlotView.SelectedPoint);
                        ContextDisconnect.DropDownItems.Add(tsmiDisconnect);
                    }

                    foreach (var id in PlotView.SelectedPoint.Links)
                    {
                        var pp = PlotView.Plot.FindPoint(id);

                        var tsmi = new ToolStripMenuItem("Before \"" + pp.Name + "\"");
                        tsmi.Click += add_between;
                        tsmi.Tag = new Pair<PlotPoint, PlotPoint>(PlotView.SelectedPoint, pp);
                        ContextAddBetween.DropDownItems.Add(tsmi);

                        var tsmiDisconnect = new ToolStripMenuItem(pp.Name);
                        tsmiDisconnect.Click += disconnect_points;
                        tsmiDisconnect.Tag = new Pair<PlotPoint, PlotPoint>(PlotView.SelectedPoint, pp);
                        ContextDisconnect.DropDownItems.Add(tsmiDisconnect);
                    }
                }

                ContextAddBetween.Enabled = ContextAddBetween.DropDownItems.Count != 0;

                ContextDisconnect.Enabled = ContextDisconnect.DropDownItems.Count != 0;
                ContextDisconnectAll.Enabled = ContextDisconnect.Enabled;

                ContextMoveTo.DropDownItems.Clear();

                if (PlotView.SelectedPoint != null)
                {
                    foreach (var pp in PlotView.Plot.Points)
                        if (pp.Links.Contains(PlotView.SelectedPoint.Id))
                        {
                            var tsmi = new ToolStripMenuItem(pp.Name);
                            tsmi.Click += move_to_subplot;
                            tsmi.Tag = new Pair<PlotPoint, PlotPoint>(pp, PlotView.SelectedPoint);
                            ContextMoveTo.DropDownItems.Add(tsmi);
                        }

                    ContextStateNormal.Checked = PlotView.SelectedPoint.State == PlotPointState.Normal;
                    ContextStateCompleted.Checked = PlotView.SelectedPoint.State == PlotPointState.Completed;
                    ContextStateSkipped.Checked = PlotView.SelectedPoint.State == PlotPointState.Skipped;
                }

                ContextMoveTo.Enabled = ContextMoveTo.DropDownItems.Count != 0;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void delve_view(Map map)
        {
            if (map == null)
                return;

            foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                ctrl.Visible = false;

            var mapview = new MapView();

            mapview.Map = map;
            mapview.Plot = PlotView.Plot;
            mapview.Mode = MapViewMode.Thumbnail;
            mapview.HighlightAreas = true;
            mapview.LineOfSight = false;
            mapview.BorderSize = 1;
            mapview.BorderStyle = BorderStyle.FixedSingle;
            mapview.Dock = DockStyle.Fill;

            PreviewSplitter.Panel1.Controls.Add(mapview);

            mapview.AreaSelected += select_maparea;
            mapview.DoubleClick += edit_maparea;

            _fDelveView = mapview;
            _fView = ViewType.Delve;
            update_preview();
        }

        private void delve_view_edit()
        {
            var map = _fDelveView.Map;
            var index = Session.Project.Maps.IndexOf(map);

            var dlg = new MapBuilderForm(map, false);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Maps[index] = dlg.Map;
                Session.Modified = true;

                _fDelveView.Map = dlg.Map;
            }
        }

        private void select_maparea(object sender, MapAreaEventArgs e)
        {
            update_preview();
        }

        private void edit_maparea(object sender, EventArgs e)
        {
            if (_fDelveView.SelectedArea != null)
            {
                var index = _fDelveView.Map.Areas.IndexOf(_fDelveView.SelectedArea);

                var dlg = new MapAreaForm(_fDelveView.SelectedArea, _fDelveView.Map);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fDelveView.Map.Areas[index] = dlg.Area;
                    Session.Modified = true;

                    _fDelveView.MapChanged();
                }
            }
        }

        private void map_view(RegionalMap map)
        {
            if (map == null)
                return;

            foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                ctrl.Visible = false;

            var mapview = new RegionalMapPanel();

            mapview.Map = map;
            mapview.Plot = PlotView.Plot;
            mapview.Mode = MapViewMode.Thumbnail;
            mapview.BorderStyle = BorderStyle.FixedSingle;
            mapview.Dock = DockStyle.Fill;

            PreviewSplitter.Panel1.Controls.Add(mapview);

            mapview.SelectedLocationModified += select_maplocation;
            mapview.DoubleClick += edit_maplocation;

            _fMapView = mapview;
            _fView = ViewType.Map;
            update_preview();
        }

        private void map_view_edit()
        {
            var map = _fMapView.Map;
            var index = Session.Project.RegionalMaps.IndexOf(map);

            var dlg = new RegionalMapForm(map, null);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.RegionalMaps[index] = dlg.Map;
                Session.Modified = true;

                _fMapView.Map = dlg.Map;
            }
        }

        private void select_maplocation(object sender, EventArgs e)
        {
            update_preview();
        }

        private void edit_maplocation(object sender, EventArgs e)
        {
            if (_fMapView.SelectedLocation != null)
            {
                var index = _fMapView.Map.Locations.IndexOf(_fMapView.SelectedLocation);

                var dlg = new MapLocationForm(_fMapView.SelectedLocation);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _fMapView.Map.Locations[index] = dlg.MapLocation;
                    Session.Modified = true;

                    _fMapView.Invalidate();
                }
            }
        }

        private void flowchart_view()
        {
            // Turn off delve view / map view

            var controls = new List<Control>();
            foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                if (ctrl.Visible)
                    controls.Add(ctrl);
            foreach (var ctrl in controls)
                PreviewSplitter.Panel1.Controls.Remove(ctrl);

            foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                ctrl.Visible = true;

            _fView = ViewType.Flowchart;
            update_preview();
        }

        private void BackgroundList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                update_background_item();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundDetails_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.Scheme == "background")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "edit") BackgroundEditBtn_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EntryList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                update_encyclopedia_entry();
                EntryList.Focus();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EntryDetails_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            try
            {
                if (e.Url.Scheme == "entry")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "edit")
                    {
                        EncEditBtn_Click(sender, e);
                    }
                    else
                    {
                        var entryId = new Guid(e.Url.LocalPath);
                        SelectedEncyclopediaItem = Session.Project.Encyclopedia.FindEntry(entryId);
                    }
                }

                if (e.Url.Scheme == "missing")
                {
                    e.Cancel = true;

                    // Offer to create entry
                    var name = e.Url.LocalPath;
                    var entry = create_entry(name, "");

                    if (entry != null)
                    {
                        update_encyclopedia_list();
                        SelectedEncyclopediaItem = entry;
                    }
                }

                if (e.Url.Scheme == "group")
                {
                    e.Cancel = true;

                    if (e.Url.LocalPath == "edit")
                    {
                        EncEditBtn_Click(sender, e);
                    }
                    else
                    {
                        var groupId = new Guid(e.Url.LocalPath);
                        SelectedEncyclopediaItem = Session.Project.Encyclopedia.FindGroup(groupId);
                    }
                }

                if (e.Url.Scheme == "map")
                {
                    e.Cancel = true;

                    var locId = new Guid(e.Url.LocalPath);
                    foreach (var map in Session.Project.RegionalMaps)
                    {
                        var loc = map.FindLocation(locId);
                        if (loc != null)
                        {
                            var dlg = new RegionalMapForm(map, loc);
                            dlg.ShowDialog();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void RulesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            update_selected_rule();
        }

        private void AttachmentList_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                var filenames = e.Data.GetData("FileDrop") as string[];
                if (filenames != null)
                    e.Effect = DragDropEffects.All;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AttachmentList_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                var filenames = e.Data.GetData("FileDrop") as string[];
                if (filenames != null)
                {
                    foreach (var filename in filenames)
                        add_attachment(filename);

                    update_attachments();
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
                _fUpdating = true;

                NoteBox.Text = "(no note selected)";
                NoteBox.Enabled = false;
                NoteBox.ReadOnly = true;

                if (SelectedNote != null)
                {
                    NoteBox.Text = SelectedNote.Content;
                    NoteBox.Enabled = true;
                    NoteBox.ReadOnly = false;
                }

                if (SelectedIssue != null)
                {
                    NoteBox.Text = SelectedIssue.ToString();
                    NoteBox.Enabled = true;
                }

                _fUpdating = false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (_fUpdating)
                    return;

                if (SelectedNote != null)
                {
                    SelectedNote.Content = NoteBox.Text;

                    NoteList.SelectedItems[0].Text = SelectedNote.Name;
                    NoteList.SelectedItems[0].ForeColor =
                        SelectedNote.Content != "" ? SystemColors.WindowText : SystemColors.GrayText;
                    NoteList.Sort();

                    Session.Modified = true;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PartyBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "party")
                if (e.Url.LocalPath == "edit")
                {
                    e.Cancel = true;

                    ProjectPlayers_Click(sender, e);
                }

            if (e.Url.Scheme == "show")
            {
                e.Cancel = true;

                _fPartyBreakdownSecondary = e.Url.LocalPath;
                update_party();
            }
        }

        private void GeneratorBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "entry")
            {
                e.Cancel = true;

                var name = e.Url.LocalPath;

                var entry = Session.Project.Encyclopedia.FindEntry(name);
                if (entry == null)
                {
                    entry = new EncyclopediaEntry();
                    entry.Name = name;
                    entry.Category = "People";
                }

                // Edit the entry
                var dlg = new EncyclopediaEntryForm(entry);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Encyclopedia.Entries.Add(dlg.Entry);
                    Session.Modified = true;

                    update_encyclopedia_list();
                }
            }

            if (e.Url.Scheme == "parcel")
            {
                e.Cancel = true;

                var item = e.Url.LocalPath;

                var p = new Parcel();
                p.Name = "Item";
                p.Details = item;

                Session.Project.TreasureParcels.Add(p);
                Session.Modified = true;

                var dlg = new ParcelListForm();
                dlg.ShowDialog();
            }
        }

        private void ReferencePages_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void ContextStateNormal_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                {
                    var points = PlotView.SelectedPoint.Subtree;
                    foreach (var pp in points)
                        pp.State = PlotPointState.Normal;

                    Session.Modified = true;
                    update_workspace();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextStateCompleted_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                {
                    var points = PlotView.SelectedPoint.Subtree;
                    foreach (var pp in points)
                        pp.State = PlotPointState.Completed;

                    Session.Modified = true;
                    update_workspace();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextStateSkipped_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                {
                    var points = PlotView.SelectedPoint.Subtree;
                    foreach (var pp in points)
                        pp.State = PlotPointState.Skipped;

                    Session.Modified = true;
                    update_workspace();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextDisconnectAll_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.SelectedPoint.Links.Clear();

                var id = PlotView.SelectedPoint.Id;

                foreach (var pp in PlotView.Plot.Points)
                    while (pp.Links.Contains(id))
                        pp.Links.Remove(id);

                PlotView.RecalculateLayout();
                Session.Modified = true;
                update_workspace();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FileMenu_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                FileSave.Enabled = Session.Project != null;
                FileSaveAs.Enabled = Session.Project != null;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectMenu_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                ProjectProject.Enabled = Session.Project != null;
                ProjectOverview.Enabled = Session.Project != null;
                ProjectChecklist.Enabled = Session.Project != null;
                ProjectCampaignSettings.Enabled = Session.Project != null;
                ProjectPassword.Enabled = Session.Project != null;
                ProjectTacticalMaps.Enabled = Session.Project != null;
                ProjectRegionalMaps.Enabled = Session.Project != null;
                ProjectPlayers.Enabled = Session.Project != null;
                ProjectParcels.Enabled = Session.Project != null;
                ProjectDecks.Enabled = Session.Project != null;
                ProjectCustomCreatures.Enabled = Session.Project != null;
                ProjectCalendars.Enabled = Session.Project != null;
                ProjectEncounters.Enabled = Session.Project != null && Session.Project.SavedCombats.Count != 0;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlayerViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                PlayerViewShow.Enabled = Session.Project != null;
                PlayerViewShow.Checked = Session.PlayerView != null;
                PlayerViewClear.Enabled = Session.PlayerView != null && Session.PlayerView.Mode != PlayerViewMode.Blank;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsMenu_DropDownOpening(object sender, EventArgs e)
        {
            try
            {
                ToolsImportProject.Enabled = Session.Project != null;
                ToolsExportProject.Enabled = Session.Project != null;
                ToolsExportHandout.Enabled = Session.Project != null;
                ToolsIssues.Enabled = Session.Project != null;
                ToolsPreferencesTextSizeSmall.Checked = Session.Preferences.TextSize == DisplaySize.Small;
                ToolsPreferencesTextSizeMedium.Checked = Session.Preferences.TextSize == DisplaySize.Medium;
                ToolsPreferencesTextSizeLarge.Checked = Session.Preferences.TextSize == DisplaySize.Large;
                ToolsPreferencesTextSizeExtraLarge.Checked = Session.Preferences.TextSize == DisplaySize.ExtraLarge;
                ToolsPreferencesPlayerViewSmall.Checked = Session.Preferences.PlayerViewTextSize == DisplaySize.Small;
                ToolsPreferencesPlayerViewMedium.Checked = Session.Preferences.PlayerViewTextSize == DisplaySize.Medium;
                ToolsPreferencesPlayerViewLarge.Checked = Session.Preferences.PlayerViewTextSize == DisplaySize.Large;
                ToolsPreferencesPlayerViewExtraLarge.Checked =
                    Session.Preferences.PlayerViewTextSize == DisplaySize.ExtraLarge;
                ToolsPreferencesPlayerViewOtherDisplay.Enabled = Screen.AllScreens.Length > 1;
                ToolsPreferencesPlayerViewOtherDisplay.Checked =
                    Screen.AllScreens.Length > 1 && PlayerViewForm.UseOtherDisplay;

                ToolsAddIns.DropDownItems.Clear();
                foreach (var addin in Session.AddIns)
                {
                    var addinItem = new ToolStripMenuItem(addin.Name);
                    addinItem.ToolTipText = TextHelper.Wrap(addin.Description);
                    addinItem.Tag = addin;

                    ToolsAddIns.DropDownItems.Add(addinItem);

                    foreach (var command in addin.Commands)
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
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewMenu_DropDownOpening(object sender, EventArgs e)
        {
            ViewDefault.Checked = PlotView.Mode == PlotViewMode.Normal;
            ViewHighlighting.Checked = PlotView.Mode == PlotViewMode.HighlightSelected;
            ViewEncounters.Checked = PlotView.Mode == PlotViewMode.HighlightEncounter;
            ViewTraps.Checked = PlotView.Mode == PlotViewMode.HighlightTrap;
            ViewChallenges.Checked = PlotView.Mode == PlotViewMode.HighlightChallenge;
            ViewQuests.Checked = PlotView.Mode == PlotViewMode.HighlightQuest;
            ViewParcels.Checked = PlotView.Mode == PlotViewMode.HighlightParcel;
            ViewLevelling.Checked = PlotView.ShowLevels;
            ViewTooltips.Checked = PlotView.ShowTooltips;
            ViewPreview.Checked = !PreviewSplitter.Panel2Collapsed;
            ViewNavigation.Checked = !NavigationSplitter.Panel1Collapsed;
        }

        private void FileNew_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fView != ViewType.Flowchart)
                    flowchart_view();

                if (!check_modified())
                    return;

                var p = new Project();
                p.Name = sender != null ? "Untitled Campaign" : "Random Delve";
                p.Author = Environment.UserName;

                var dlg = new ProjectForm(p);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project = p;
                    Session.Project.SetStandardBackgroundItems();
                    Session.Project.TreasureParcels.AddRange(Treasure.CreateParcelSet(Session.Project.Party.Level,
                        Session.Project.Party.Size, true));

                    Session.FileName = "";

                    PlotView.Plot = Session.Project.Plot;

                    update_title();
                    UpdateView();

                    if (Controls.Contains(_fWelcome))
                    {
                        Controls.Clear();
                        _fWelcome = null;

                        Controls.Add(Pages);
                        Controls.Add(MainMenu);

                        Pages.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FileOpen_Click(object sender, EventArgs e)
        {
            try
            {
                if (_fView != ViewType.Flowchart)
                    flowchart_view();

                if (!check_modified())
                    return;

                var dlg = new OpenFileDialog();
                dlg.Filter = Program.ProjectFilter;
                dlg.FileName = Session.FileName;

                if (dlg.ShowDialog() == DialogResult.OK)
                    open_file(dlg.FileName);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void open_file(string filename)
        {
            GC.Collect();

            var p = Serialisation<Project>.Load(filename, SerialisationMode.Binary);
            if (p != null)
                Session.CreateBackup(filename);
            else
                p = Session.LoadBackup(filename);

            if (p != null)
            {
                if (Session.CheckPassword(p))
                {
                    Session.Project = p;
                    Session.FileName = filename;
                    Session.Modified = false;

                    Session.Project.Update();
                    Session.Project.SimplifyProjectLibrary();

                    PlotView.Plot = Session.Project.Plot;

                    update_title();
                    UpdateView();

                    if (Controls.Contains(_fWelcome))
                    {
                        Controls.Clear();
                        _fWelcome = null;

                        Controls.Add(Pages);
                        Controls.Add(MainMenu);

                        Pages.Focus();
                    }
                }
            }
            else
            {
                var msg = "The file '" + FileName.Name(filename) + "' could not be opened.";
                MessageBox.Show(msg, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void FileSave_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.FileName == "")
                {
                    FileSaveAs_Click(sender, e);
                    return;
                }

                GC.Collect();

                Session.Project.PopulateProjectLibrary();

                var ok = Serialisation<Project>.Save(Session.FileName, Session.Project, SerialisationMode.Binary);
                if (ok)
                {
                    Session.Modified = false;
                }
                else
                {
                    // Warn that the save failed
                    var str = "The file could not be saved; check the filename and drive permissions and try again.";
                    MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                Session.Project.SimplifyProjectLibrary();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FileSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = Program.ProjectFilter;
                dlg.FileName = FileName.TrimInvalidCharacters(Session.Project.Name);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    GC.Collect();

                    Session.Project.PopulateProjectLibrary();

                    var ok = Serialisation<Project>.Save(dlg.FileName, Session.Project, SerialisationMode.Binary);
                    if (ok)
                    {
                        Session.FileName = dlg.FileName;
                        Session.Modified = false;
                    }
                    else
                    {
                        // Warn that the save failed
                        var str =
                            "The file could not be saved; check the filename and drive permissions and try again.";
                        MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    Session.Project.SimplifyProjectLibrary();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AdvancedDelve_Click(object sender, EventArgs e)
        {
            try
            {
                var previousProject = Session.Project;
                var previousFilename = Session.FileName;
                var view = _fView;

                if (!create_delve())
                {
                    Session.Project = previousProject;
                    Session.FileName = previousFilename;
                    _fView = view;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool create_delve()
        {
            // Create a new project
            FileNew_Click(null, null);
            if (Session.Project == null)
                return false;

            // Generate 3-room map
            var map = new Map();
            map.Name = "Random Dungeon";
            var mapDlg = new MapBuilderForm(map, true);
            if (mapDlg.ShowDialog() != DialogResult.OK)
                return false;

            Cursor.Current = Cursors.WaitCursor;

            map = mapDlg.Map;

            // Build delve
            var abd = new AutoBuildData();
            var pp = DelveBuilder.AutoBuild(map, abd);
            if (pp == null)
                return false;

            Session.Project.Maps.Add(map);
            foreach (var child in pp.Subplot.Points)
                Session.Project.Plot.Points.Add(child);
            Session.Modified = true;

            UpdateView();

            // Switch to delve view
            delve_view(map);

            Cursor.Current = Cursors.Default;

            return true;
        }

        private void FileExit_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectProject_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.Project == null)
                    return;

                var dlg = new ProjectForm(Session.Project);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Modified = true;

                    update_title();
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectOverview_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OverviewForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectChecklist_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.Project != null)
                {
                    var dlg = new ProjectChecklistForm();
                    dlg.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectCampaignSettings_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.Project == null)
                    return;

                var dlg = new CampaignSettingsForm(Session.Project.CampaignSettings);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Modified = true;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectPassword_Click(object sender, EventArgs e)
        {
            if (Session.CheckPassword(Session.Project))
            {
                // Set or change the password
                var dlg = new PasswordSetForm();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Password = dlg.Password;
                    Session.Project.PasswordHint = dlg.PasswordHint;

                    Session.Modified = true;
                }
            }
        }

        private void ProjectPlayers_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new HeroListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectTacticalMaps_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new MapListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectRegionalMaps_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new RegionalMapListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectParcels_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new ParcelListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectDecks_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new DeckListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectCustomCreatures_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new CustomCreatureListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectCalendars_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new CalendarListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ProjectEncounters_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new PausedCombatListForm();
                dlg.ShowDialog();

                foreach (Form form in Application.OpenForms)
                    if (form is CombatForm)
                        form.Activate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPlayerView_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.PlayerView == null)
                {
                    Session.PlayerView = new PlayerViewForm(this);
                    Session.PlayerView.ShowDefault();
                }
                else
                {
                    Session.PlayerView.Close();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPlayerViewClear_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.PlayerView != null)
                    Session.PlayerView.ShowDefault();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsImportProject_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = Program.ProjectFilter;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    GC.Collect();

                    var p = Serialisation<Project>.Load(dlg.FileName, SerialisationMode.Binary);
                    if (p != null)
                    {
                        Session.Project.PopulateProjectLibrary();
                        Session.Project.Import(p);
                        Session.Project.SimplifyProjectLibrary();

                        Session.Modified = true;

                        PlotView.RecalculateLayout();
                        UpdateView();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsExportProject_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = Program.HtmlFilter;
                dlg.FileName = FileName.TrimInvalidCharacters(Session.Project.Name);

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var html = new Html();
                    var ok = html.ExportProject(dlg.FileName);
                    if (ok)
                    {
                        Process.Start(dlg.FileName);
                    }
                    else
                    {
                        // Warn that the save failed
                        var str =
                            "The file could not be saved; check the filename and drive permissions and try again.";
                        MessageBox.Show(str, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsExportHandout_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new HandoutForm();
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsIssues_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new IssuesForm(Session.Project.Plot);
                dlg.ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsLibraries_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new LibraryListForm();
                dlg.ShowDialog();

                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
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

        private void ToolsPreferencesTextSizeSmall_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.TextSize = DisplaySize.Small;
                update_preview();
                _fWelcome?.RefreshOptions();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesTextSizeMedium_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.TextSize = DisplaySize.Medium;
                update_preview();
                _fWelcome?.RefreshOptions();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesTextSizeLarge_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.TextSize = DisplaySize.Large;
                update_preview();
                _fWelcome?.RefreshOptions();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesTextSizeExtraLarge_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.TextSize = DisplaySize.ExtraLarge;
                update_preview();
                _fWelcome?.RefreshOptions();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesPlayerViewSmall_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.PlayerViewTextSize = DisplaySize.Small;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesPlayerViewMedium_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.PlayerViewTextSize = DisplaySize.Medium;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesPlayerViewLarge_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.PlayerViewTextSize = DisplaySize.Large;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesPlayerViewExtraLarge_Click(object sender, EventArgs e)
        {
            try
            {
                Session.Preferences.PlayerViewTextSize = DisplaySize.ExtraLarge;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsPreferencesPlayerViewOtherDisplay_Click(object sender, EventArgs e)
        {
            try
            {
                PlayerViewForm.UseOtherDisplay = !PlayerViewForm.UseOtherDisplay;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void HelpManual_Click(object sender, EventArgs e)
        {
            try
            {
                var ass = Assembly.GetEntryAssembly();
                var path = FileName.Directory(ass.FullName) + "Manual.pdf";

                if (!File.Exists(path))
                    return;

                Process.Start(path);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void HelpAbout_Click(object sender, EventArgs e)
        {
            try
            {
                new AboutBox().ShowDialog();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            try
            {
                add_point(null, null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextAdd_Click(object sender, EventArgs e)
        {
            try
            {
                add_point(PlotView.SelectedPoint, null);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void add_point(PlotPoint lhs, PlotPoint rhs)
        {
            try
            {
                var pp = new PlotPoint("New Point");

                var dlg = new PlotPointForm(pp, PlotView.Plot, false);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotView.Plot.Points.Add(dlg.PlotPoint);
                    PlotView.RecalculateLayout();

                    if (lhs != null && rhs != null)
                        lhs.Links.Remove(rhs.Id);

                    lhs?.Links.Add(dlg.PlotPoint.Id);

                    if (rhs != null)
                        dlg.PlotPoint.Links.Add(rhs.Id);

                    Session.Modified = true;

                    UpdateView();
                    PlotView.SelectedPoint = dlg.PlotPoint;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddEncounter_Click(object sender, EventArgs e)
        {
            try
            {
                var enc = new Encounter();
                enc.SetStandardEncounterNotes();

                var pp = new PlotPoint("New Encounter Point");
                pp.Element = enc;

                var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotView.Plot.Points.Add(dlg.PlotPoint);
                    PlotView.RecalculateLayout();
                    Session.Modified = true;

                    UpdateView();
                    PlotView.SelectedPoint = dlg.PlotPoint;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddChallenge_Click(object sender, EventArgs e)
        {
            try
            {
                var sc = new SkillChallenge();
                sc.Name = "Unnamed Skill Challenge";
                sc.Level = Session.Project.Party.Level;

                var pp = new PlotPoint("New Skill Challenge Point");
                pp.Element = sc;

                var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotView.Plot.Points.Add(dlg.PlotPoint);
                    PlotView.RecalculateLayout();
                    Session.Modified = true;

                    UpdateView();
                    PlotView.SelectedPoint = dlg.PlotPoint;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddTrap_Click(object sender, EventArgs e)
        {
            try
            {
                var te = new TrapElement();
                te.Trap.Name = "Unnamed Trap";
                te.Trap.Level = Session.Project.Party.Level;

                var pp = new PlotPoint("New Trap / Hazard Point");
                pp.Element = te;

                var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotView.Plot.Points.Add(dlg.PlotPoint);
                    PlotView.RecalculateLayout();
                    Session.Modified = true;

                    UpdateView();
                    PlotView.SelectedPoint = dlg.PlotPoint;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AddQuest_Click(object sender, EventArgs e)
        {
            try
            {
                var pp = new PlotPoint("New Quest Point");
                pp.Element = new Quest();

                var dlg = new PlotPointForm(pp, PlotView.Plot, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PlotView.Plot.Points.Add(dlg.PlotPoint);
                    PlotView.RecalculateLayout();
                    Session.Modified = true;

                    UpdateView();
                    PlotView.SelectedPoint = dlg.PlotPoint;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                {
                    var msg = "Are you sure you want to delete this plot point?";
                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;

                    if (PlotView.SelectedPoint.Subplot.Points.Count != 0)
                    {
                        var subplotMsg = "This plot point has a subplot.";
                        subplotMsg += Environment.NewLine;
                        subplotMsg += "Do you want to keep the subplot points?";

                        dr = MessageBox.Show(subplotMsg, "Masterplan", MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);

                        if (dr == DialogResult.Cancel)
                            return;

                        if (dr == DialogResult.Yes)
                            // Keep subplot points
                            foreach (var pp in PlotView.SelectedPoint.Subplot.Points)
                                PlotView.Plot.Points.Add(pp);
                    }

                    PlotView.Plot.RemovePoint(PlotView.SelectedPoint);
                    PlotView.RecalculateLayout();
                    PlotView.SelectedPoint = null;

                    Session.Modified = true;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CutBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                {
                    Clipboard.SetData(typeof(PlotPoint).ToString(), PlotView.SelectedPoint.Copy());

                    PlotView.Plot.RemovePoint(PlotView.SelectedPoint);
                    PlotView.RecalculateLayout();
                    PlotView.SelectedPoint = null;
                    Session.Modified = true;

                    PlotView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void CopyBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (PlotView.SelectedPoint != null)
                    Clipboard.SetData(typeof(PlotPoint).ToString(), PlotView.SelectedPoint.Copy());
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PasteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsData(typeof(PlotPoint).ToString()))
                {
                    var pp = Clipboard.GetData(typeof(PlotPoint).ToString()) as PlotPoint;
                    if (pp != null)
                    {
                        if (PlotView.Plot.FindPoint(pp.Id) != null)
                        {
                            // Paste a new copy (with a new ID)
                            pp = pp.Copy();
                            pp.Links.Clear();
                            pp.Id = Guid.NewGuid();
                        }

                        // Remove links to any point that's not in this subplot
                        var obsolete = new List<Guid>();
                        foreach (var linkId in pp.Links)
                            if (PlotView.Plot.FindPoint(linkId) == null)
                                obsolete.Add(linkId);
                        foreach (var linkId in obsolete)
                            pp.Links.Remove(linkId);

                        PlotView.Plot.Points.Add(pp);
                        PlotView.RecalculateLayout();
                        PlotView.SelectedPoint?.Links.Add(pp.Id);

                        Session.Modified = true;

                        PlotView.SelectedPoint = pp;
                        PlotView.Invalidate();
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();

                    var pp = new PlotPoint();
                    pp.Name = text.Trim().Substring(0, 12) + "...";
                    pp.Details = text;

                    PlotView.Plot.Points.Add(pp);
                    PlotView.RecalculateLayout();
                    PlotView.SelectedPoint?.Links.Add(pp.Id);

                    Session.Modified = true;

                    PlotView.SelectedPoint = pp;
                    PlotView.Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void SearchBtn_Click(object sender, EventArgs e)
        {
            try
            {
                WorkspaceSearchBar.Visible = !WorkspaceSearchBar.Visible;

                if (!WorkspaceSearchBar.Visible)
                    PlotSearchBox.Text = "";
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                PlotView.Filter = PlotSearchBox.Text;
                PlotSearchBox.Focus();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            try
            {
                PlotSearchBox.Text = "";
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewDefault_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.Normal;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewHighlighting_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightSelected;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewEncounters_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightEncounter;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewTraps_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightTrap;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewChallenges_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightChallenge;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewQuests_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightQuest;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewParcels_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.Mode = PlotViewMode.HighlightParcel;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewLevelling_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.ShowLevels = !PlotView.ShowLevels;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewTooltips_Click(object sender, EventArgs e)
        {
            try
            {
                PlotView.ShowTooltips = !PlotView.ShowTooltips;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewPreview_Click(object sender, EventArgs e)
        {
            try
            {
                PreviewSplitter.Panel2Collapsed = !PreviewSplitter.Panel2Collapsed;
                Session.Preferences.Workspace.ShowPreview = !Session.Preferences.Workspace.ShowPreview;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewNavigation_Click(object sender, EventArgs e)
        {
            try
            {
                NavigationSplitter.Panel1Collapsed = !NavigationSplitter.Panel1Collapsed;
                Session.Preferences.Workspace.ShowNavigation = !Session.Preferences.Workspace.ShowNavigation;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ViewLinks_DropDownOpening(object sender, EventArgs e)
        {
            ViewLinksCurved.Checked = PlotView.LinkStyle == PlotViewLinkStyle.Curved;
            ViewLinksAngled.Checked = PlotView.LinkStyle == PlotViewLinkStyle.Angled;
            ViewLinksStraight.Checked = PlotView.LinkStyle == PlotViewLinkStyle.Straight;
        }

        private void ViewLinksCurved_Click(object sender, EventArgs e)
        {
            PlotView.LinkStyle = PlotViewLinkStyle.Curved;
            Session.Preferences.Workspace.LinkStyle = PlotViewLinkStyle.Curved;
        }

        private void ViewLinksAngled_Click(object sender, EventArgs e)
        {
            PlotView.LinkStyle = PlotViewLinkStyle.Angled;
            Session.Preferences.Workspace.LinkStyle = PlotViewLinkStyle.Angled;
        }

        private void ViewLinksStraight_Click(object sender, EventArgs e)
        {
            PlotView.LinkStyle = PlotViewLinkStyle.Straight;
            Session.Preferences.Workspace.LinkStyle = PlotViewLinkStyle.Straight;
        }

        private void FlowchartPrint_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new PrintDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var doc = new PrintDocument();
                    doc.DocumentName = Session.Project.Name;
                    doc.PrinterSettings = dlg.PrinterSettings;

                    doc.PrintPage += print_page;
                    doc.Print();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FlowchartExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = Session.Project.Name;
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

                    var bmp = Screenshot.Plot(PlotView.Plot, new Size(800, 600));
                    bmp.Save(dlg.FileName, format);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void FlowchartAllXP_Click(object sender, EventArgs e)
        {
            Session.Preferences.AllXp = !Session.Preferences.AllXp;

            update_workspace();
            update_preview();
        }

        private void PlotAdvancedIssues_Click(object sender, EventArgs e)
        {
            var dlg = new IssuesForm(PlotView.Plot);
            dlg.ShowDialog();
        }

        private void PlotAdvancedDifficulty_Click(object sender, EventArgs e)
        {
            var dlg = new LevelAdjustmentForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var delta = dlg.LevelAdjustment;

                var points = PlotView.Plot.AllPlotPoints;
                foreach (var point in points)
                {
                    if (point.Element is Encounter)
                    {
                        var enc = point.Element as Encounter;
                        foreach (var slot in enc.Slots)
                            slot.Card.LevelAdjustment += delta;

                        foreach (var t in enc.Traps) t.AdjustLevel(delta);

                        foreach (var sc in enc.SkillChallenges)
                        {
                            sc.Level += delta;
                            sc.Level = Math.Max(1, sc.Level);
                        }
                    }

                    if (point.Element is Trap)
                    {
                        var t = point.Element as Trap;

                        t.AdjustLevel(delta);
                    }

                    if (point.Element is SkillChallenge)
                    {
                        var sc = point.Element as SkillChallenge;

                        sc.Level += delta;
                        sc.Level = Math.Max(1, sc.Level);
                    }

                    if (point.Element is Quest)
                    {
                        var q = point.Element as Quest;

                        q.Level += delta;
                        q.Level = Math.Max(1, q.Level);
                    }
                }

                Session.Modified = true;

                PlotView.Invalidate();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var pp = get_selected_point();
                if (pp != null)
                {
                    var index = PlotView.Plot.Points.IndexOf(pp);
                    var plot = Session.Project.FindParent(pp);

                    var dlg = new PlotPointForm(pp, plot, false);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        plot.Points[index] = dlg.PlotPoint;
                        Session.Modified = true;

                        set_selected_point(dlg.PlotPoint);

                        PlotView.RecalculateLayout();
                        UpdateView();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ExploreBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var pp = get_selected_point();
                if (pp != null)
                {
                    if (_fView != ViewType.Flowchart)
                        flowchart_view();

                    PlotView.Plot = pp.Subplot;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotPointPlayerView_Click(object sender, EventArgs e)
        {
            try
            {
                var point = get_selected_point();
                if (point != null)
                {
                    if (Session.PlayerView == null)
                        Session.PlayerView = new PlayerViewForm(this);

                    Session.PlayerView.ShowPlotPoint(point);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotPointExportHTML_Click(object sender, EventArgs e)
        {
            try
            {
                var point = get_selected_point();
                if (point != null)
                {
                    var dlg = new SaveFileDialog();
                    dlg.FileName = point.Name;
                    dlg.Filter = Program.HtmlFilter;

                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        var partyLevel = Workspace.GetPartyLevel(point);
                        File.WriteAllText(dlg.FileName,
                            Html.PlotPoint(point, PlotView.Plot, partyLevel, false, _fView,
                                Session.Preferences.TextSize));
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void PlotPointExportFile_Click(object sender, EventArgs e)
        {
            try
            {
                var point = get_selected_point();
                if (point != null)
                {
                    var p = new Project();
                    p.Name = point.Name;
                    p.Party.Size = Session.Project.Party.Size;
                    p.Party.Level = Workspace.GetPartyLevel(point);

                    // Add this plot point
                    p.Plot.Points.Add(point.Copy());

                    foreach (var pp in p.AllPlotPoints)
                        pp.EncyclopediaEntryIDs.Clear();

                    // Add all tactical maps
                    var tacticalMapIds = p.Plot.FindTacticalMaps();
                    foreach (var mapId in tacticalMapIds)
                    {
                        var m = Session.Project.FindTacticalMap(mapId);
                        if (m != null)
                            p.Maps.Add(m.Copy());
                    }

                    // Add all regional maps
                    var regionalMapIds = p.Plot.FindRegionalMaps();
                    foreach (var mapId in regionalMapIds)
                    {
                        var m = Session.Project.FindRegionalMap(mapId);
                        if (m != null)
                            p.RegionalMaps.Add(m.Copy());
                    }

                    GC.Collect();

                    p.PopulateProjectLibrary();

                    var dlg = new SaveFileDialog();
                    dlg.FileName = point.Name;
                    dlg.Filter = Program.ProjectFilter;

                    if (dlg.ShowDialog() == DialogResult.OK)
                        Serialisation<Project>.Save(dlg.FileName, p, SerialisationMode.Binary);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundAddBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var bg = new Background("New Background Item");
                var dlg = new BackgroundForm(bg);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Backgrounds.Add(dlg.Background);
                    Session.Modified = true;

                    update_background_list();
                    SelectedBackground = dlg.Background;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundRemoveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedBackground != null)
                {
                    var msg = "Are you sure you want to delete this background?";
                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;

                    Session.Project.Backgrounds.Remove(SelectedBackground);
                    Session.Modified = true;

                    update_background_list();
                    SelectedBackground = null;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundEditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedBackground != null)
                {
                    var index = Session.Project.Backgrounds.IndexOf(SelectedBackground);

                    var dlg = new BackgroundForm(SelectedBackground);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Session.Project.Backgrounds[index] = dlg.Background;
                        Session.Modified = true;

                        update_background_list();
                        SelectedBackground = dlg.Background;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundUpBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedBackground != null && Session.Project.Backgrounds.IndexOf(SelectedBackground) != 0)
                {
                    var index = Session.Project.Backgrounds.IndexOf(SelectedBackground);
                    var tmp = Session.Project.Backgrounds[index - 1];
                    Session.Project.Backgrounds[index - 1] = SelectedBackground;
                    Session.Project.Backgrounds[index] = tmp;

                    Session.Modified = true;

                    update_background_list();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundDownBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedBackground != null && Session.Project.Backgrounds.IndexOf(SelectedBackground) !=
                    Session.Project.Backgrounds.Count - 1)
                {
                    var index = Session.Project.Backgrounds.IndexOf(SelectedBackground);
                    var tmp = Session.Project.Backgrounds[index + 1];
                    Session.Project.Backgrounds[index + 1] = SelectedBackground;
                    Session.Project.Backgrounds[index] = tmp;

                    Session.Modified = true;

                    update_background_list();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundPlayerViewSelected_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedBackground != null)
                {
                    if (Session.PlayerView == null)
                        Session.PlayerView = new PlayerViewForm(this);

                    Session.PlayerView.ShowBackground(SelectedBackground);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundPlayerViewAll_Click(object sender, EventArgs e)
        {
            try
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowBackground(Session.Project.Backgrounds);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void BackgroundShareExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = Program.BackgroundFilter;
            dlg.FileName = Session.Project.Name;

            if (dlg.ShowDialog() == DialogResult.OK)
                Serialisation<List<Background>>.Save(dlg.FileName, Session.Project.Backgrounds, SerialisationMode.Xml);
        }

        private void BackgroundShareImport_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.BackgroundFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var list = Serialisation<List<Background>>.Load(dlg.FileName, SerialisationMode.Xml);
                Session.Project.Backgrounds.AddRange(list);
                Session.Modified = true;
                UpdateView();
            }
        }

        private void BackgroundSharePublish_Click(object sender, EventArgs e)
        {
            var dlg = new HandoutForm();
            dlg.AddBackgroundEntries();

            dlg.ShowDialog();
        }

        private void EncAddEntry_Click(object sender, EventArgs e)
        {
            try
            {
                var entry = create_entry("New Entry", "");

                if (entry != null)
                {
                    UpdateView();
                    SelectedEncyclopediaItem = entry;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private EncyclopediaEntry create_entry(string name, string content)
        {
            try
            {
                var entry = new EncyclopediaEntry();
                entry.Name = name;
                entry.Details = content;

                var dlg = new EncyclopediaEntryForm(entry);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Encyclopedia.Entries.Add(dlg.Entry);
                    Session.Project.Encyclopedia.Entries.Sort();

                    Session.Modified = true;

                    return dlg.Entry;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return null;
        }

        private void encyclopedia_template(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem)
                {
                    var tsmi = sender as ToolStripMenuItem;
                    var filename = tsmi.Tag as string;

                    // Create entry with this text
                    var name = FileName.Name(filename);
                    var text = File.ReadAllText(filename);
                    var entry = create_entry(name, text);

                    if (entry != null)
                    {
                        UpdateView();
                        SelectedEncyclopediaItem = entry;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncAddGroup_Click(object sender, EventArgs e)
        {
            try
            {
                var group = new EncyclopediaGroup();
                group.Name = "New Group";

                var dlg = new EncyclopediaGroupForm(group);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.Encyclopedia.Groups.Add(dlg.Group);
                    Session.Modified = true;

                    UpdateView();
                    SelectedEncyclopediaItem = group;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncRemoveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedEncyclopediaItem is EncyclopediaEntry)
                {
                    var msg = "Are you sure you want to delete this encyclopedia entry?";
                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;

                    Session.Project.Encyclopedia.Entries.Remove(SelectedEncyclopediaItem as EncyclopediaEntry);

                    // Remove all links to this entry
                    var obsolete = new List<EncyclopediaLink>();
                    foreach (var link in Session.Project.Encyclopedia.Links)
                        if (link.EntryIDs.Contains(SelectedEncyclopediaItem.Id))
                            obsolete.Add(link);
                    foreach (var link in obsolete)
                        Session.Project.Encyclopedia.Links.Remove(link);

                    // Remove the entry from groups
                    foreach (var group in Session.Project.Encyclopedia.Groups)
                        if (group.EntryIDs.Contains(SelectedEncyclopediaItem.Id))
                            group.EntryIDs.Remove(SelectedEncyclopediaItem.Id);

                    Session.Modified = true;

                    update_encyclopedia_list();
                    SelectedEncyclopediaItem = null;
                }

                if (SelectedEncyclopediaItem is EncyclopediaGroup)
                {
                    var msg = "Are you sure you want to delete this encyclopedia group?";
                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;

                    Session.Project.Encyclopedia.Groups.Remove(SelectedEncyclopediaItem as EncyclopediaGroup);

                    UpdateView();
                    SelectedEncyclopediaItem = null;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncEditBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedEncyclopediaItem is EncyclopediaEntry)
                {
                    var index = Session.Project.Encyclopedia.Entries.IndexOf(
                        SelectedEncyclopediaItem as EncyclopediaEntry);

                    var dlg = new EncyclopediaEntryForm(SelectedEncyclopediaItem as EncyclopediaEntry);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Session.Project.Encyclopedia.Entries[index] = dlg.Entry;
                        Session.Modified = true;

                        UpdateView();
                        SelectedEncyclopediaItem = dlg.Entry;
                    }
                }

                if (SelectedEncyclopediaItem is EncyclopediaGroup)
                {
                    var index = Session.Project.Encyclopedia.Groups.IndexOf(
                        SelectedEncyclopediaItem as EncyclopediaGroup);

                    var dlg = new EncyclopediaGroupForm(SelectedEncyclopediaItem as EncyclopediaGroup);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Session.Project.Encyclopedia.Groups[index] = dlg.Group;
                        Session.Modified = true;

                        UpdateView();
                        SelectedEncyclopediaItem = dlg.Group;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncPlayerView_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedEncyclopediaItem != null)
                {
                    if (Session.PlayerView == null)
                        Session.PlayerView = new PlayerViewForm(this);

                    Session.PlayerView.ShowEncyclopediaItem(SelectedEncyclopediaItem);
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncShareExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = Program.EncyclopediaFilter;
            dlg.FileName = Session.Project.Name;

            if (dlg.ShowDialog() == DialogResult.OK)
                Serialisation<Encyclopedia>.Save(dlg.FileName, Session.Project.Encyclopedia, SerialisationMode.Xml);
        }

        private void EncShareImport_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.EncyclopediaFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var enc = Serialisation<Encyclopedia>.Load(dlg.FileName, SerialisationMode.Xml);
                Session.Project.Encyclopedia.Import(enc);
                Session.Modified = true;
                UpdateView();
            }
        }

        private void EncSharePublish_Click(object sender, EventArgs e)
        {
            var dlg = new HandoutForm();
            dlg.AddEncyclopediaEntries();

            dlg.ShowDialog();
        }

        private void EncCutBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedEncyclopediaItem is EncyclopediaEntry)
                {
                    var entry = SelectedEncyclopediaItem as EncyclopediaEntry;

                    Clipboard.SetData(typeof(EncyclopediaEntry).ToString(), entry.Copy());

                    Session.Project.Encyclopedia.Entries.Remove(entry);
                    Session.Modified = true;

                    update_encyclopedia_list();
                    SelectedEncyclopediaItem = null;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncCopyBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedEncyclopediaItem is EncyclopediaEntry)
                {
                    var entry = SelectedEncyclopediaItem as EncyclopediaEntry;
                    Clipboard.SetData(typeof(EncyclopediaEntry).ToString(), entry.Copy());
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncPasteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsData(typeof(EncyclopediaEntry).ToString()))
                {
                    var entry = Clipboard.GetData(typeof(EncyclopediaEntry).ToString()) as EncyclopediaEntry;
                    if (entry != null)
                    {
                        if (Session.Project.Encyclopedia.FindEntry(entry.Id) != null)
                        {
                            var previousId = entry.Id;

                            // Set a new ID
                            entry.Id = Guid.NewGuid();

                            var newLinks = new List<EncyclopediaLink>();
                            foreach (var link in Session.Project.Encyclopedia.Links)
                                if (link.EntryIDs.Contains(previousId))
                                {
                                    var newLink = link.Copy();
                                    var index = newLink.EntryIDs.IndexOf(previousId);
                                    newLink.EntryIDs[index] = entry.Id;

                                    newLinks.Add(newLink);
                                }

                            Session.Project.Encyclopedia.Links.AddRange(newLinks);
                        }

                        Session.Project.Encyclopedia.Entries.Add(entry);
                        Session.Modified = true;

                        update_encyclopedia_list();
                        SelectedEncyclopediaItem = entry;
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    var text = Clipboard.GetText();

                    var entry = new EncyclopediaEntry();
                    entry.Name = text.Trim().Substring(0, 12) + "...";
                    entry.Details = text;

                    Session.Project.Encyclopedia.Entries.Add(entry);
                    Session.Modified = true;

                    update_encyclopedia_list();
                    SelectedEncyclopediaItem = entry;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                update_encyclopedia_list();

                if (EntryList.Items.Count != 0)
                    SelectedEncyclopediaItem = EntryList.Items[0].Tag as EncyclopediaEntry;
                else
                    SelectedEncyclopediaItem = null;

                EncSearchBox.Focus();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EncClearLbl_Click(object sender, EventArgs e)
        {
            try
            {
                EncSearchBox.Text = "";
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void EntryImageList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedEncyclopediaImage != null)
            {
                var entry = SelectedEncyclopediaItem as EncyclopediaEntry;
                if (entry == null)
                    return;

                var index = entry.Images.IndexOf(SelectedEncyclopediaImage);

                var dlg = new EncyclopediaImageForm(SelectedEncyclopediaImage);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    entry.Images[index] = dlg.Image;
                    update_encyclopedia_images();

                    Session.Modified = true;
                }
            }
        }

        private void AddRace_Click(object sender, EventArgs e)
        {
            var race = new Race();
            race.Name = "New Race";

            var dlg = new OptionRaceForm(race);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Race);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddClass_Click(object sender, EventArgs e)
        {
            var c = new Class();
            c.Name = "New Class";

            for (var level = 1; level <= 30; ++level)
            {
                var ld = new LevelData();
                ld.Level = level;
                c.Levels.Add(ld);
            }

            var dlg = new OptionClassForm(c);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Class);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddTheme_Click(object sender, EventArgs e)
        {
            var theme = new Theme();
            theme.Name = "New Theme";

            for (var level = 1; level <= 10; ++level)
            {
                var ld = new LevelData();
                ld.Level = level;
                theme.Levels.Add(ld);
            }

            var dlg = new OptionThemeForm(theme);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Theme);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddParagonPath_Click(object sender, EventArgs e)
        {
            var pp = new ParagonPath();
            pp.Name = "New Paragon Path";

            for (var level = 11; level <= 20; ++level)
            {
                var ld = new LevelData();
                ld.Level = level;
                pp.Levels.Add(ld);
            }

            var dlg = new OptionParagonPathForm(pp);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.ParagonPath);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddEpicDestiny_Click(object sender, EventArgs e)
        {
            var ed = new EpicDestiny();
            ed.Name = "New Epic Destiny";

            for (var level = 21; level <= 30; ++level)
            {
                var ld = new LevelData();
                ld.Level = level;
                ed.Levels.Add(ld);
            }

            var dlg = new OptionEpicDestinyForm(ed);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.EpicDestiny);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddBackground_Click(object sender, EventArgs e)
        {
            var bg = new PlayerBackground();
            bg.Name = "New Background";

            var dlg = new OptionBackgroundForm(bg);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Background);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddFeat_Click(object sender, EventArgs e)
        {
            var feat = new Feat();
            feat.Name = "New Feat";

            var dlg = new OptionFeatForm(feat);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Feat);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddWeapon_Click(object sender, EventArgs e)
        {
            var wpn = new Weapon();
            wpn.Name = "New Weapon";

            var dlg = new OptionWeaponForm(wpn);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Weapon);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddRitual_Click(object sender, EventArgs e)
        {
            var ritual = new Ritual();
            ritual.Name = "New Ritual";

            var dlg = new OptionRitualForm(ritual);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Ritual);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddCreatureLore_Click(object sender, EventArgs e)
        {
            var lore = new CreatureLore();
            lore.Name = "Creature";
            lore.SkillName = "Nature";

            var dlg = new OptionCreatureLoreForm(lore);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.CreatureLore);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddDisease_Click(object sender, EventArgs e)
        {
            var disease = new Disease();
            disease.Name = "New Disease";

            var dlg = new OptionDiseaseForm(disease);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Disease);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void AddPoison_Click(object sender, EventArgs e)
        {
            var poison = new Poison();
            poison.Name = "New Poison";

            var dlg = new OptionPoisonForm(poison);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.PlayerOptions.Add(dlg.Poison);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void RulesRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedRule != null)
            {
                Session.Project.PlayerOptions.Remove(SelectedRule);
                Session.Modified = true;

                update_rules_list();
                update_selected_rule();
            }
        }

        private void RulesEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedRule == null)
                return;

            var index = Session.Project.PlayerOptions.IndexOf(SelectedRule);

            if (SelectedRule is Race)
            {
                var dlg = new OptionRaceForm(SelectedRule as Race);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Race;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Class)
            {
                var dlg = new OptionClassForm(SelectedRule as Class);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Class;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Theme)
            {
                var dlg = new OptionThemeForm(SelectedRule as Theme);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Theme;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is ParagonPath)
            {
                var dlg = new OptionParagonPathForm(SelectedRule as ParagonPath);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.ParagonPath;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is EpicDestiny)
            {
                var dlg = new OptionEpicDestinyForm(SelectedRule as EpicDestiny);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.EpicDestiny;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is PlayerBackground)
            {
                var dlg = new OptionBackgroundForm(SelectedRule as PlayerBackground);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Background;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Feat)
            {
                var dlg = new OptionFeatForm(SelectedRule as Feat);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Feat;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Weapon)
            {
                var dlg = new OptionWeaponForm(SelectedRule as Weapon);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Weapon;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Ritual)
            {
                var dlg = new OptionRitualForm(SelectedRule as Ritual);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Ritual;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is CreatureLore)
            {
                var dlg = new OptionCreatureLoreForm(SelectedRule as CreatureLore);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.CreatureLore;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Disease)
            {
                var dlg = new OptionDiseaseForm(SelectedRule as Disease);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Disease;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }

            if (SelectedRule is Poison)
            {
                var dlg = new OptionPoisonForm(SelectedRule as Poison);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.PlayerOptions[index] = dlg.Poison;
                    Session.Modified = true;

                    update_rules_list();
                    update_selected_rule();
                }
            }
        }

        private void RulesPlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (SelectedRule != null)
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowPlayerOption(SelectedRule);
            }
        }

        private void RuleEncyclopediaBtn_Click(object sender, EventArgs e)
        {
            if (SelectedRule == null)
                return;

            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(SelectedRule.Id);

            if (entry == null)
            {
                // If there is no entry, ask to create it
                var msg = "There is no encyclopedia entry associated with this item.";
                msg += Environment.NewLine;
                msg += "Would you like to create one now?";
                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.No)
                    return;

                entry = new EncyclopediaEntry();
                entry.Name = SelectedRule.Name;
                entry.AttachmentId = SelectedRule.Id;
                entry.Category = "";

                Session.Project.Encyclopedia.Entries.Add(entry);
                Session.Modified = true;
            }

            // Edit the entry
            var index = Session.Project.Encyclopedia.Entries.IndexOf(entry);
            var dlg = new EncyclopediaEntryForm(entry);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Encyclopedia.Entries[index] = dlg.Entry;
                Session.Modified = true;

                UpdateView();

                Pages.SelectedTab = EncyclopediaPage;
                SelectedEncyclopediaItem = dlg.Entry;
            }
        }

        private void RulesShareExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Filter = Program.RulesFilter;
            dlg.FileName = Session.Project.Name;

            if (dlg.ShowDialog() == DialogResult.OK)
                Serialisation<List<IPlayerOption>>.Save(dlg.FileName, Session.Project.PlayerOptions,
                    SerialisationMode.Binary);
        }

        private void RulesShareImport_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.RulesFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var rules = Serialisation<List<IPlayerOption>>.Load(dlg.FileName, SerialisationMode.Binary);
                Session.Project.PlayerOptions.AddRange(rules);
                UpdateView();
            }
        }

        private void RulesSharePublish_Click(object sender, EventArgs e)
        {
            var dlg = new HandoutForm();
            dlg.AddRulesEntries();

            dlg.ShowDialog();
        }

        private void AttachmentImportBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.Filter = "All Files|*.*";
                dlg.Multiselect = true;

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filename in dlg.FileNames)
                        add_attachment(filename);

                    update_attachments();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AttachmentRemoveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var attachments = SelectedAttachments;

                if (attachments.Count != 0)
                {
                    var str = "You are about to remove one or more attachments from this project.";
                    str += Environment.NewLine;
                    str += "Are you sure you want to do this?";

                    if (MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                        DialogResult.Yes)
                        return;

                    foreach (var att in attachments)
                        Session.Project.Attachments.Remove(att);

                    Session.Modified = true;
                    update_attachments();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AttachmentExtractSimple_Click(object sender, EventArgs e)
        {
            try
            {
                var attachments = SelectedAttachments;
                foreach (var att in attachments) extract_attachment(att, false);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AttachmentExtractAndRun_Click(object sender, EventArgs e)
        {
            try
            {
                var attachments = SelectedAttachments;
                foreach (var att in attachments) extract_attachment(att, true);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void extract_attachment(Attachment att, bool run)
        {
            try
            {
                var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (!desktop.EndsWith("\\"))
                    desktop += "\\";

                var filename = desktop + att.Name;

                // Work out unique filename
                var n = 1;
                var uniqueFile = filename;
                while (File.Exists(uniqueFile))
                {
                    n += 1;
                    uniqueFile = desktop + FileName.Name(filename) + " " + n + "." + FileName.Extension(filename);
                }

                // Create file
                File.WriteAllBytes(uniqueFile, att.Contents);

                if (run)
                    // Open the file
                    Process.Start(uniqueFile);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void AttachmentSendBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedAttachments.Count != 1)
                    return;

                var att = SelectedAttachments[0];
                if (att.Type == AttachmentType.Miscellaneous)
                    return;

                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                if (att.Type == AttachmentType.PlainText)
                    Session.PlayerView.ShowPlainText(att);

                if (att.Type == AttachmentType.RichText)
                    Session.PlayerView.ShowRichText(att);

                if (att.Type == AttachmentType.Image)
                    Session.PlayerView.ShowImage(att);

                if (att.Type == AttachmentType.Url)
                    Session.PlayerView.ShowWebPage(att);

                if (att.Type == AttachmentType.Html)
                    Session.PlayerView.ShowWebPage(att);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteAddBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var n = new Note();

                Session.Project.Notes.Add(n);
                Session.Modified = true;

                update_notes();
                SelectedNote = n;

                NoteBox.Focus();
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
                    var msg = "Are you sure you want to delete this note?";
                    var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.No)
                        return;

                    Session.Project.Notes.Remove(SelectedNote);
                    Session.Modified = true;

                    update_notes();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteCategoryBtn_Click(object sender, EventArgs e)
        {
            if (SelectedNote == null)
                return;

            var bst = new BinarySearchTree<string>();
            foreach (var n in Session.Project.Notes)
                if (n.Category != "")
                    bst.Add(n.Category);

            var dlg = new CategoryForm(bst.SortedList, SelectedNote.Category);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedNote.Category = dlg.Category;
                Session.Modified = true;

                update_notes();
            }
        }

        private void NoteCutBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null)
                {
                    Clipboard.SetData(typeof(Note).ToString(), SelectedNote.Copy());

                    Session.Project.Notes.Remove(SelectedNote);
                    Session.Modified = true;

                    update_notes();
                    SelectedNote = null;
                }
                else if (NoteBox.SelectedText != "")
                {
                    NoteBox.Cut();

                    Session.Modified = true;

                    update_notes();
                    SelectedNote = null;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteCopyBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (SelectedNote != null)
                    Clipboard.SetData(typeof(Note).ToString(), SelectedNote.Copy());
                else if (NoteBox.SelectedText != "") NoteBox.Copy();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NotePasteBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.ContainsData(typeof(Note).ToString()))
                {
                    var n = Clipboard.GetData(typeof(Note).ToString()) as Note;
                    if (n != null)
                    {
                        if (Session.Project.FindNote(n.Id) != null)
                            // Set a new ID
                            n.Id = Guid.NewGuid();

                        Session.Project.Notes.Add(n);
                        Session.Modified = true;

                        update_notes();
                        SelectedNote = n;
                    }
                }
                else if (Clipboard.ContainsText())
                {
                    Clipboard.GetText();

                    if (NoteBox.Focused && SelectedNote != null)
                    {
                        NoteBox.Paste();

                        Session.Modified = true;

                        update_notes();

                        NoteBox.Focus();
                    }
                    else
                    {
                        var n = new Note();
                        n.Content = Clipboard.GetText();

                        Session.Project.Notes.Add(n);
                        Session.Modified = true;

                        update_notes();
                        SelectedNote = n;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                update_notes();

                if (NoteList.Groups[1].Items.Count != 0)
                {
                    var n = NoteList.Groups[1].Items[0].Tag as Note;
                    SelectedNote = n;
                }
                else
                {
                    SelectedNote = null;
                }

                NoteSearchBox.Focus();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void NoteClearLbl_Click(object sender, EventArgs e)
        {
            try
            {
                NoteSearchBox.Text = "";
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void DieRollerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new DieRollerForm();
            dlg.ShowDialog();
        }

        private void ElfNameBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Elvish Names</H3>");

            lines.Add("<P class=instruction>Click on any name to create an encyclopedia entry for it.</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Male</B></TD>");
            lines.Add("<TD><B>Female</B></TD>");
            lines.Add("</TR>");

            for (var n = 0; n != 10; ++n)
            {
                var male = ElfName.MaleName();
                var female = ElfName.FemaleName();

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + male.Replace(" ", "%20") + ">" + male + "</A></P>");
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + female.Replace(" ", "%20") + ">" + female + "</A></P>");
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void DwarfNameBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Dwarvish Names</H3>");

            lines.Add("<P class=instruction>Click on any name to create an encyclopedia entry for it.</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Male</B></TD>");
            lines.Add("<TD><B>Female</B></TD>");
            lines.Add("</TR>");

            for (var n = 0; n != 10; ++n)
            {
                var male = DwarfName.MaleName();
                var female = DwarfName.FemaleName();

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + male.Replace(" ", "%20") + ">" + male + "</A></P>");
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + female.Replace(" ", "%20") + ">" + female + "</A></P>");
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void HalflingNameBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Halfling Names</H3>");

            lines.Add("<P class=instruction>Click on any name to create an encyclopedia entry for it.</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Male</B></TD>");
            lines.Add("<TD><B>Female</B></TD>");
            lines.Add("</TR>");

            for (var n = 0; n != 10; ++n)
            {
                var male = HalflingName.MaleName();
                var female = HalflingName.FemaleName();

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + male.Replace(" ", "%20") + ">" + male + "</A></P>");
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + female.Replace(" ", "%20") + ">" + female + "</A></P>");
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void ExoticNameBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Exotic Names</H3>");

            lines.Add("<P class=instruction>Click on any name to create an encyclopedia entry for it.</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Names</B></TD>");
            lines.Add("</TR>");

            for (var n = 0; n != 10; ++n)
            {
                var name1 = ExoticName.FullName();
                var name2 = ExoticName.FullName();

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + name1.Replace(" ", "%20") + ">" + name1 + "</A></P>");
                lines.Add("</TD>");

                lines.Add("<TD>");
                lines.Add("<P><A href=entry:" + name2.Replace(" ", "%20") + ">" + name2 + "</A></P>");
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void TreasureBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Art Objects</H3>");

            lines.Add("<P class=instruction>Click on any item to make it available as a treasure parcel.</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD><B>Items</B></TD>");
            lines.Add("</TR>");

            for (var n = 0; n != 10; ++n)
            {
                var art = Treasure.ArtObject();

                lines.Add("<TR>");

                lines.Add("<TD>");
                lines.Add("<P><A href=parcel:" + art.Replace(" ", "%20") + ">" + art + "</A></P>");
                lines.Add("</TD>");

                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void BookTitleBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Book Titles</H3>");

            for (var n = 0; n != 10; ++n)
                lines.Add("<P>" + Book.Title() + "</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void PotionBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>Potions</H3>");

            for (var n = 0; n != 10; ++n)
                lines.Add("<P>" + Potion.Description() + "</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void NPCBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>NPC Description</H3>");
            lines.Add("<P>" + NpcBuilder.Description() + "</P>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>NPC Details</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            var physical = NpcBuilder.Physical();
            if (physical != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Physical Traits</B>");
                lines.Add("</TD>");
                lines.Add("<TD colspan=2>");
                lines.Add(physical);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var personality = NpcBuilder.Personality();
            if (personality != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Personality</B>");
                lines.Add("</TD>");
                lines.Add("<TD colspan=2>");
                lines.Add(personality);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            var speech = NpcBuilder.Speech();
            if (speech != "")
            {
                lines.Add("<TR>");
                lines.Add("<TD>");
                lines.Add("<B>Speech</B>");
                lines.Add("</TD>");
                lines.Add("<TD colspan=2>");
                lines.Add(speech);
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void RoomBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");
            lines.Add("<H3>" + RoomBuilder.Name() + "</H3>");
            lines.Add("<P>" + RoomBuilder.Details() + "</P>");
            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void ElfTextBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, DisplaySize.Small);

            lines.Add("<BODY>");

            var sentences = Session.Dice(1, 6);
            for (var n = 0; n != sentences; ++n)
                lines.Add("<P>" + ElfName.Sentence() + "</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void DwarfTextBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");

            var sentences = Session.Dice(1, 6);
            for (var n = 0; n != sentences; ++n)
                lines.Add("<P>" + DwarfName.Sentence() + "</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void PrimordialTextBtn_Click(object sender, EventArgs e)
        {
            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);

            lines.Add("<BODY>");

            var sentences = Session.Dice(1, 6);
            for (var n = 0; n != sentences; ++n)
                lines.Add("<P>" + ExoticName.Sentence() + "</P>");

            lines.Add("</BODY>");

            GeneratorBrowser.DocumentText = Html.Concatenate(lines);
        }

        public void UpdateView()
        {
            try
            {
                _fUpdating = true;

                update_workspace();

                update_background_list();
                update_background_item();

                update_encyclopedia_list();
                update_encyclopedia_entry();

                update_rules_list();
                update_selected_rule();

                update_attachments();

                update_notes();

                update_reference();

                foreach (var addin in _fExtensibility.AddIns)
                foreach (var addinPage in addin.Pages)
                    addinPage.UpdateView();

                if (_fView == ViewType.Delve)
                    // Update delve map
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView)
                        {
                            var mapview = ctrl as MapView;
                            mapview.Map = Session.Project.FindTacticalMap(mapview.Map.Id);

                            break;
                        }

                if (_fView == ViewType.Map)
                    // Update regional map
                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel)
                        {
                            var mapview = ctrl as RegionalMapPanel;
                            mapview.Map = Session.Project.FindRegionalMap(mapview.Map.Id);

                            break;
                        }

                _fUpdating = false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_title()
        {
            try
            {
                var title = "Masterplan";
                if (Session.Project != null)
                    title = Session.Project.Name + " - Masterplan";

                Text = title;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_workspace()
        {
            try
            {
                update_navigation();
                update_preview();
                update_breadcrumbs();

                PlotView.Invalidate();

                if (_fView == ViewType.Delve)
                {
                    MapView mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is MapView && ctrl.Visible)
                        {
                            mapview = ctrl as MapView;
                            break;
                        }

                    mapview?.MapChanged();
                }

                if (_fView == ViewType.Map)
                {
                    RegionalMapPanel mapview = null;

                    foreach (Control ctrl in PreviewSplitter.Panel1.Controls)
                        if (ctrl is RegionalMapPanel && ctrl.Visible)
                        {
                            mapview = ctrl as RegionalMapPanel;
                            break;
                        }

                    mapview?.Invalidate();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_navigation()
        {
            try
            {
                NavigationTree.BeginUpdate();

                NavigationTree.Nodes.Clear();

                if (Session.Project != null)
                {
                    add_navigation_node(null, null);
                    NavigationTree.ExpandAll();
                }

                NavigationTree.EndUpdate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void add_navigation_node(PlotPoint pp, TreeNode parent)
        {
            try
            {
                var name = pp != null ? pp.Name : Session.Project.Name;

                var tnc = parent != null ? parent.Nodes : NavigationTree.Nodes;
                var node = tnc.Add(name);

                var p = pp != null ? pp.Subplot : Session.Project.Plot;
                node.Tag = p;

                if (PlotView.Plot == p) NavigationTree.SelectedNode = node;

                var list = pp != null ? pp.Subplot.Points : Session.Project.Plot.Points;
                foreach (var child in list)
                    if (child.Subplot.Points.Count != 0 || child.Subplot == PlotView.Plot)
                        add_navigation_node(child, node);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_breadcrumbs()
        {
            try
            {
                BreadcrumbBar.Items.Clear();

                if (Session.Project != null)
                {
                    var breadcrumbs = new List<PlotPoint>();

                    var p = PlotView.Plot;
                    while (p != null)
                    {
                        var pp = Session.Project.FindParent(p);
                        p = pp != null ? Session.Project.FindParent(pp) : null;

                        breadcrumbs.Add(pp);
                    }

                    breadcrumbs.Reverse();
                    foreach (var pp in breadcrumbs)
                    {
                        // Add breadcrumb for this plot point
                        var link = breadcrumbs.IndexOf(pp) != breadcrumbs.Count - 1;
                        add_breadcrumb(pp, link);
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void add_breadcrumb(PlotPoint pp, bool link)
        {
            try
            {
                var name = pp != null ? pp.Name : Session.Project.Name;
                var tsl = new ToolStripLabel(name);
                tsl.IsLink = link;
                tsl.Tag = pp;
                tsl.Click += Breadcrumb_Click;

                BreadcrumbBar.Items.Add(tsl);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void Breadcrumb_Click(object sender, EventArgs e)
        {
            try
            {
                var tsl = sender as ToolStripLabel;
                var pp = tsl.Tag as PlotPoint;

                if (pp == null)
                {
                    PlotView.Plot = Session.Project.Plot;
                    UpdateView();
                }
                else
                {
                    PlotView.Plot = pp.Subplot;
                    UpdateView();
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_background_list()
        {
            try
            {
                var selection = SelectedBackground;

                BackgroundList.Items.Clear();

                if (Session.Project != null)
                {
                    foreach (var bg in Session.Project.Backgrounds)
                    {
                        var lvi = BackgroundList.Items.Add(bg.Title);
                        lvi.Tag = bg;

                        if (bg.Details == "")
                            lvi.ForeColor = SystemColors.GrayText;

                        if (bg == selection)
                            lvi.Selected = true;
                    }

                    if (Session.Project.Backgrounds.Count == 0)
                    {
                        var lvi = BackgroundList.Items.Add("(no backgrounds)");
                        lvi.ForeColor = SystemColors.GrayText;
                    }
                }
                else
                {
                    var lvi = BackgroundList.Items.Add("(no project)");
                    lvi.ForeColor = SystemColors.GrayText;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_background_item()
        {
            try
            {
                BackgroundDetails.Document.OpenNew(true);
                BackgroundDetails.Document.Write(Html.Background(SelectedBackground, Session.Preferences.TextSize));
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_encyclopedia_templates()
        {
            try
            {
                var dir = Application.StartupPath + "\\Encyclopedia";
                if (Directory.Exists(dir))
                {
                    var files = new List<string>();
                    files.AddRange(Directory.GetFiles(dir, "*.txt"));
                    files.AddRange(Directory.GetFiles(dir, "*.htm"));
                    files.AddRange(Directory.GetFiles(dir, "*.html"));

                    if (files.Count > 0)
                    {
                        EncAddBtn.DropDownItems.Add(new ToolStripSeparator());

                        foreach (var filename in files)
                        {
                            var name = FileName.Name(filename);
                            var tsmi = new ToolStripMenuItem(name);
                            tsmi.Tag = filename;
                            tsmi.Click += encyclopedia_template;

                            EncAddBtn.DropDownItems.Add(tsmi);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_encyclopedia_list()
        {
            try
            {
                string[] split = null;
                var tokens = EncSearchBox.Text.ToLower().Split(split, StringSplitOptions.RemoveEmptyEntries);

                EntryList.BeginUpdate();

                if (Session.Project != null)
                {
                    EntryList.ShowGroups = true;

                    var bst = new BinarySearchTree<string>();
                    foreach (var entry in Session.Project.Encyclopedia.Entries)
                        if (entry.Category != null && entry.Category != "")
                            bst.Add(entry.Category);
                    var cats = bst.SortedList;
                    cats.Insert(0, "Groups");
                    cats.Add("Miscellaneous Entries");

                    EntryList.Groups.Clear();
                    foreach (var cat in cats)
                        EntryList.Groups.Add(cat, cat);

                    var items = new List<ListViewItem>();

                    if (tokens.Length == 0)
                    {
                        var groups = new List<EncyclopediaGroup>();
                        groups.AddRange(Session.Project.Encyclopedia.Groups);
                        groups.Sort();

                        foreach (var group in groups)
                        {
                            var lvi = new ListViewItem(group.Name);
                            lvi.Tag = group;
                            lvi.Group = EntryList.Groups["Groups"];

                            items.Add(lvi);
                        }
                    }

                    foreach (var entry in Session.Project.Encyclopedia.Entries)
                    {
                        if (!Match(entry, tokens))
                            continue;

                        var lvi = new ListViewItem(entry.Name);
                        lvi.Tag = entry;
                        if (entry.Category != null && entry.Category != "")
                            lvi.Group = EntryList.Groups[entry.Category];
                        else
                            lvi.Group = EntryList.Groups["Miscellaneous Entries"];

                        if (entry.Details == "" && entry.DmInfo == "")
                            lvi.ForeColor = SystemColors.GrayText;

                        items.Add(lvi);
                    }

                    if (items.Count == 0)
                    {
                        EntryList.ShowGroups = false;

                        var str = EncSearchBox.Text == "" ? "(no entries)" : "(no matching entries)";
                        var lvi = new ListViewItem(str);
                        lvi.ForeColor = SystemColors.GrayText;

                        items.Add(lvi);
                    }

                    EntryList.Items.Clear();
                    EntryList.Items.AddRange(items.ToArray());
                }
                else
                {
                    var lvi = EntryList.Items.Add("(no project)");
                    lvi.ForeColor = SystemColors.GrayText;
                }

                EntryList.EndUpdate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool Match(EncyclopediaEntry entry, string[] tokens)
        {
            try
            {
                foreach (var token in tokens)
                    if (!Match(entry, token))
                        return false;

                return true;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private bool Match(EncyclopediaEntry entry, string token)
        {
            try
            {
                if (entry.Name.ToLower().Contains(token))
                    return true;

                if (entry.Details.ToLower().Contains(token))
                    return true;

                return false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private void update_encyclopedia_entry()
        {
            try
            {
                var enc = Session.Project != null ? Session.Project.Encyclopedia : null;
                var str = "";

                if (SelectedEncyclopediaItem != null)
                {
                    if (SelectedEncyclopediaItem is EncyclopediaEntry)
                        str = Html.EncyclopediaEntry(SelectedEncyclopediaItem as EncyclopediaEntry, enc,
                            Session.Preferences.TextSize, true, true, true, false);
                    if (SelectedEncyclopediaItem is EncyclopediaGroup)
                        str = Html.EncyclopediaGroup(SelectedEncyclopediaItem as EncyclopediaGroup, enc,
                            Session.Preferences.TextSize, true, true);
                }
                else
                {
                    str = Html.EncyclopediaEntry(null, enc, Session.Preferences.TextSize, true, true, true, false);
                }

                EntryDetails.Document.OpenNew(true);
                EntryDetails.Document.Write(str);

                update_encyclopedia_images();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_encyclopedia_images()
        {
            try
            {
                EncyclopediaEntry entry = null;
                if (SelectedEncyclopediaItem is EncyclopediaEntry)
                    entry = SelectedEncyclopediaItem as EncyclopediaEntry;

                var showSidebar = false;
                if (entry != null)
                    showSidebar = entry.Images.Count > 0;

                if (showSidebar)
                {
                    EntryImageList.Items.Clear();
                    EntryImageList.LargeImageList = null;

                    const int pictureSize = 64;

                    var images = new ImageList();
                    images.ImageSize = new Size(pictureSize, pictureSize);
                    images.ColorDepth = ColorDepth.Depth32Bit;
                    EntryImageList.LargeImageList = images;

                    foreach (var img in entry.Images)
                    {
                        if (img.Image == null)
                            continue;

                        var lvi = EntryImageList.Items.Add(img.Name);
                        lvi.Tag = img;

                        Image bmp = new Bitmap(pictureSize, pictureSize);
                        var g = Graphics.FromImage(bmp);
                        if (img.Image.Size.Width > img.Image.Size.Height)
                        {
                            var height = img.Image.Size.Height * pictureSize / img.Image.Size.Width;
                            var rect = new Rectangle(0, (pictureSize - height) / 2, pictureSize, height);

                            g.DrawImage(img.Image, rect);
                        }
                        else
                        {
                            var width = img.Image.Size.Width * pictureSize / img.Image.Size.Height;
                            var rect = new Rectangle((pictureSize - width) / 2, 0, width, pictureSize);

                            g.DrawImage(img.Image, rect);
                        }

                        images.Images.Add(bmp);
                        lvi.ImageIndex = images.Images.Count - 1;
                    }

                    EncyclopediaEntrySplitter.Panel2Collapsed = false;
                }
                else
                {
                    // Clear the sidebar
                    EntryImageList.Items.Clear();
                    EntryImageList.LargeImageList = null;

                    EncyclopediaEntrySplitter.Panel2Collapsed = true;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void update_rules_list()
        {
            RulesList.Items.Clear();
            RulesList.ShowGroups = true;

            if (Session.Project != null)
            {
                foreach (var option in Session.Project.PlayerOptions)
                {
                    var groupIndex = 0;
                    if (option is Race)
                        groupIndex = 0;
                    if (option is Class)
                        groupIndex = 1;
                    if (option is Theme)
                        groupIndex = 2;
                    if (option is ParagonPath)
                        groupIndex = 3;
                    if (option is EpicDestiny)
                        groupIndex = 4;
                    if (option is PlayerBackground)
                        groupIndex = 5;
                    if (option is Feat)
                    {
                        var ft = option as Feat;
                        switch (ft.Tier)
                        {
                            case Tier.Heroic:
                                groupIndex = 6;
                                break;
                            case Tier.Paragon:
                                groupIndex = 7;
                                break;
                            case Tier.Epic:
                                groupIndex = 8;
                                break;
                        }
                    }

                    if (option is Weapon)
                        groupIndex = 9;
                    if (option is Ritual)
                        groupIndex = 10;
                    if (option is CreatureLore)
                        groupIndex = 11;
                    if (option is Disease)
                        groupIndex = 12;
                    if (option is Poison)
                        groupIndex = 13;

                    var lvi = RulesList.Items.Add(option.Name);
                    lvi.Tag = option;
                    lvi.Group = RulesList.Groups[groupIndex];
                }

                if (RulesList.Items.Count == 0)
                {
                    RulesList.ShowGroups = false;

                    var lvi = RulesList.Items.Add("(none)");
                    lvi.ForeColor = SystemColors.GrayText;
                }
            }
        }

        private void update_selected_rule()
        {
            if (SelectedRule != null)
            {
                RulesBrowser.Document.OpenNew(true);
                RulesBrowser.Document.Write(Html.PlayerOption(SelectedRule, Session.Preferences.TextSize));
            }
            else
            {
                var lines = new List<string>();
                lines.Add("<HTML>");
                lines.AddRange(Html.GetHead(null, null, Session.Preferences.TextSize));
                lines.Add("<BODY>");
                lines.Add(
                    "<P class=instruction>On this page you can create and manage campaign-specific rules elements.</P>");
                lines.Add("</BODY>");
                lines.Add("</HTML>");

                RulesBrowser.Document.OpenNew(true);
                RulesBrowser.Document.Write(Html.Concatenate(lines));
            }
        }

        private void update_attachments()
        {
            try
            {
                if (Session.Project != null)
                {
                    var categories = new BinarySearchTree<string>();
                    foreach (var att in Session.Project.Attachments)
                    {
                        var cat = FileName.Extension(att.Name).ToUpper() + " Files";
                        categories.Add(cat);
                    }

                    var cats = categories.SortedList;
                    AttachmentList.Groups.Clear();
                    foreach (var cat in cats)
                        AttachmentList.Groups.Add(cat, cat);

                    AttachmentList.Items.Clear();
                    foreach (var att in Session.Project.Attachments)
                    {
                        var b = att.Contents.Length;
                        var size = b + " B";
                        var kb = (float)b / 1024;
                        if (kb >= 1)
                            size = kb.ToString("F1") + " KB";
                        var mb = kb / 1024;
                        if (mb >= 1)
                            size = mb.ToString("F1") + " MB";
                        var gb = mb / 1024;
                        if (gb >= 1)
                            size = gb.ToString("F1") + " GB";

                        var cat = FileName.Extension(att.Name).ToUpper() + " Files";

                        var lvi = AttachmentList.Items.Add(att.Name);
                        lvi.SubItems.Add(size);
                        lvi.Group = AttachmentList.Groups[cat];
                        lvi.Tag = att;
                    }

                    if (Session.Project.Attachments.Count == 0)
                    {
                        var lvi = AttachmentList.Items.Add("(no attachments)");
                        lvi.ForeColor = SystemColors.GrayText;
                    }
                }
                else
                {
                    var lvi = AttachmentList.Items.Add("(no project)");
                    lvi.ForeColor = SystemColors.GrayText;
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
                NoteList.BeginUpdate();

                var selectedNote = SelectedNote;

                NoteList.Items.Clear();
                NoteBox.Text = "";

                var bst = new BinarySearchTree<string>();
                if (Session.Project != null)
                    foreach (var n in Session.Project.Notes)
                        if (n.Category != "")
                            bst.Add(n.Category);
                var cats = bst.SortedList;
                cats.Add("Notes");

                NoteList.Groups.Clear();
                foreach (var cat in cats)
                    NoteList.Groups.Add(cat, cat);

                var tokens = NoteSearchBox.Text.ToLower().Split();

                if (Session.Project != null)
                    foreach (var n in Session.Project.Notes)
                    {
                        if (!Match(n, tokens))
                            continue;

                        var lvi = NoteList.Items.Add(n.Name);
                        lvi.Tag = n;

                        if (n.Category == "")
                            lvi.Group = NoteList.Groups["Notes"];
                        else
                            lvi.Group = NoteList.Groups[n.Category];

                        if (n.Content == "")
                            lvi.ForeColor = SystemColors.GrayText;

                        if (n == selectedNote)
                            lvi.Selected = true;
                    }

                if (NoteList.Groups["Notes"].Items.Count == 0)
                {
                    var str = NoteSearchBox.Text == "" ? "(no notes)" : "(no matching notes)";
                    var lvi = NoteList.Items.Add(str);
                    lvi.ForeColor = SystemColors.GrayText;
                    lvi.Group = NoteList.Groups["Notes"];
                }

                NoteList.Sort();
                NoteList.EndUpdate();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool Match(Note n, string[] tokens)
        {
            try
            {
                foreach (var token in tokens)
                    if (!Match(n, token))
                        return false;

                return true;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private bool Match(Note n, string token)
        {
            try
            {
                return n.Content.ToLower().Contains(token);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return false;
        }

        private void update_reference()
        {
            if (Session.Project != null)
                InfoPanel.Level = Session.Project.Party.Level;

            update_party();

            if (GeneratorBrowser.DocumentText == "")
            {
                var lines = new List<string>();

                lines.AddRange(Html.GetHead(null, null, Session.Preferences.TextSize));
                lines.Add("<BODY>");

                lines.Add("<P class=instruction>");
                lines.Add("Use the buttons to the left to generate random names etc.");
                lines.Add("</P>");

                lines.Add("</BODY>");

                GeneratorBrowser.DocumentText = Html.Concatenate(lines);
            }

            foreach (var addin in _fExtensibility.AddIns)
            foreach (var page in addin.QuickReferencePages)
                page.UpdateView();
        }

        private void update_party()
        {
            if (PartyBrowser.Document == null)
                PartyBrowser.DocumentText = "";

            PartyBrowser.Document.OpenNew(true);
            PartyBrowser.Document.Write(Html.PCs(_fPartyBreakdownSecondary, Session.Preferences.TextSize));
        }

        private void add_between(object sender, EventArgs e)
        {
            try
            {
                var tsmi = sender as ToolStripMenuItem;
                var points = tsmi.Tag as Pair<PlotPoint, PlotPoint>;

                // Add point
                add_point(points.First, points.Second);
                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void disconnect_points(object sender, EventArgs e)
        {
            try
            {
                var tsmi = sender as ToolStripMenuItem;
                var points = tsmi.Tag as Pair<PlotPoint, PlotPoint>;

                // Disconnect point
                var id = points.Second.Id;
                while (points.First.Links.Contains(id))
                    points.First.Links.Remove(id);

                PlotView.RecalculateLayout();
                Session.Modified = true;
                update_workspace();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void move_to_subplot(object sender, EventArgs e)
        {
            try
            {
                var tsmi = sender as ToolStripMenuItem;
                var points = tsmi.Tag as Pair<PlotPoint, PlotPoint>;

                // Move this to the subplot
                PlotView.Plot.RemovePoint(points.Second);
                points.First.Subplot.Points.Add(points.Second);
                Session.Modified = true;

                PlotView.RecalculateLayout();
                UpdateView();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void add_attachment(string filename)
        {
            try
            {
                var fi = new FileInfo(filename);

                var att = new Attachment();
                att.Name = fi.Name;
                att.Contents = File.ReadAllBytes(filename);

                var existing = Session.Project.FindAttachment(att.Name);
                if (existing != null)
                {
                    var str = "An attachment with this name already exists.";
                    str += Environment.NewLine;
                    str += "Do you want to replace it?";

                    var dr = MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            // Replace the existing attachment
                            Session.Project.Attachments.Remove(existing);
                            break;
                        case DialogResult.No:
                            // Add a new attachment with a unique name
                        {
                            var n = 1;
                            while (Session.Project.FindAttachment(att.Name) != null)
                            {
                                n += 1;
                                att.Name = FileName.Name(filename) + " " + n + "." + FileName.Extension(filename);
                            }
                        }
                            break;
                        case DialogResult.Cancel:
                            // Do nothing
                            return;
                    }
                }

                Session.Project.Attachments.Add(att);
                Session.Project.Attachments.Sort();

                Session.Modified = true;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private bool check_modified()
        {
            try
            {
                if (Session.Modified)
                {
                    var str = "The project has been modified.\nDo you want to save it now?";
                    var dr = MessageBox.Show(str, "Masterplan", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    switch (dr)
                    {
                        case DialogResult.Yes:
                            // Save
                        {
                            if (Session.FileName != "")
                            {
                                GC.Collect();

                                Session.Project.PopulateProjectLibrary();
                                var ok = Serialisation<Project>.Save(Session.FileName, Session.Project,
                                    SerialisationMode.Binary);
                                Session.Project.SimplifyProjectLibrary();

                                if (!ok)
                                    return false;

                                Session.Modified = false;
                            }
                            else
                            {
                                var dlg = new SaveFileDialog();
                                dlg.Filter = Program.ProjectFilter;
                                dlg.FileName = Session.Project.Name;
                                if (dlg.ShowDialog() == DialogResult.OK)
                                {
                                    GC.Collect();

                                    Session.Project.PopulateProjectLibrary();
                                    var ok = Serialisation<Project>.Save(dlg.FileName, Session.Project,
                                        SerialisationMode.Binary);
                                    Session.Project.SimplifyProjectLibrary();

                                    if (!ok)
                                        return false;

                                    Session.FileName = dlg.FileName;
                                    Session.Modified = false;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                            break;
                        case DialogResult.No:
                            // Don't save
                            break;
                        case DialogResult.Cancel:
                            // Cancel
                            return false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }

            return true;
        }

        private void print_page(object sender, PrintPageEventArgs e)
        {
            try
            {
                var bmp = Screenshot.Plot(PlotView.Plot, e.MarginBounds.Size);
                e.Graphics.DrawImage(bmp, e.MarginBounds);

                e.HasMorePages = false;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }
    }
}
