using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class RegionalMapListForm : Form
    {
        public RegionalMap SelectedMap
        {
            get
            {
                if (MapList.SelectedItems.Count != 0)
                    return MapList.SelectedItems[0].Tag as RegionalMap;

                return null;
            }
            set
            {
                MapList.SelectedItems.Clear();

                foreach (ListViewItem lvi in MapList.Items)
                    if (lvi.Tag == value)
                        lvi.Selected = true;
            }
        }

        public RegionalMapListForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_maps();
        }

        ~RegionalMapListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedMap != null;
            EditBtn.Enabled = SelectedMap != null;

            LocationMenu.Enabled = MapPanel.SelectedLocation != null;
            ToolsMenu.Enabled = SelectedMap != null;
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var m = new RegionalMap();
            m.Name = "New Map";

            var dlg = new RegionalMapForm(m, null);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.RegionalMaps.Add(dlg.Map);
                Session.Modified = true;

                update_maps();

                SelectedMap = dlg.Map;
            }
        }

        private void AddImportProject_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ProjectFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var p = Serialisation<Project>.Load(dlg.FileName, SerialisationMode.Binary);
                if (p != null)
                {
                    var mapDlg = new RegionalMapSelectForm(p.RegionalMaps, null, true);
                    if (mapDlg.ShowDialog(this) != DialogResult.OK)
                        return;

                    Session.Project.RegionalMaps.AddRange(mapDlg.Maps);
                    Session.Modified = true;

                    update_maps();
                }
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMap != null)
            {
                var msg = "Are you sure you want to delete this map?";
                var dr = MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.No)
                    return;

                Session.Project.RegionalMaps.Remove(SelectedMap);
                Session.Modified = true;

                update_maps();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedMap != null)
            {
                var index = Session.Project.RegionalMaps.IndexOf(SelectedMap);

                var dlg = new RegionalMapForm(SelectedMap, null);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Session.Project.RegionalMaps[index] = dlg.Map;
                    Session.Modified = true;

                    update_maps();

                    SelectedMap = dlg.Map;
                }
            }
        }

        private void LocationEntry_Click(object sender, EventArgs e)
        {
            if (MapPanel.SelectedLocation == null)
                return;

            var entry = Session.Project.Encyclopedia.FindEntryForAttachment(MapPanel.SelectedLocation.Id);

            if (entry == null)
            {
                // If there is no entry, ask to create it
                var msg = "There is no encyclopedia entry associated with this location.";
                msg += Environment.NewLine;
                msg += "Would you like to create one now?";
                if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                    DialogResult.No)
                    return;

                entry = new EncyclopediaEntry();
                entry.Name = MapPanel.SelectedLocation.Name;
                entry.AttachmentId = MapPanel.SelectedLocation.Id;
                entry.Category = MapPanel.SelectedLocation.Category;
                if (entry.Category == "")
                    entry.Category = "Places";

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
            }
        }

        private void ToolsExport_Click(object sender, EventArgs e)
        {
            if (SelectedMap != null)
            {
                var dlg = new SaveFileDialog();
                dlg.FileName = SelectedMap.Name;
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

                    MapPanel.Map.Image.Save(dlg.FileName, format);
                }
            }
        }

        private void ToolsPlayerView_Click(object sender, EventArgs e)
        {
            if (SelectedMap != null)
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowRegionalMap(MapPanel);
            }
        }

        private void MapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            MapPanel.Map = SelectedMap;
        }

        private void update_maps()
        {
            MapList.Items.Clear();

            foreach (var m in Session.Project.RegionalMaps)
            {
                var lvi = MapList.Items.Add(m.Name);
                lvi.Tag = m;
            }

            if (MapList.Items.Count == 0)
            {
                var lvi = MapList.Items.Add("(no maps)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
