using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RegionalMapForm : Form
    {
        private PointF _fRightClickLocation = PointF.Empty;

        public RegionalMap Map { get; }

        public RegionalMapForm(RegionalMap map, MapLocation loc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Map = map.Copy();

            NameBox.Text = Map.Name;
            MapPanel.Map = Map;

            if (loc != null)
            {
                // Disable editing

                NameBox.Enabled = false;
                Toolbar.Visible = false;

                OKBtn.Visible = false;
                CancelBtn.Text = "Close";

                MapPanel.Mode = MapViewMode.Plain;
                MapPanel.HighlightedLocation = loc;
            }
        }

        ~RegionalMapForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PasteBtn.Enabled = Clipboard.ContainsImage();
            RemoveBtn.Enabled = MapPanel.SelectedLocation != null;
            EditBtn.Enabled = MapPanel.SelectedLocation != null;
            EntryBtn.Enabled = MapPanel.SelectedLocation != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Map.Name = NameBox.Text;
        }

        private void BrowseBtn_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            openDlg.Filter = Program.ImageFilter;
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            MapPanel.Map.Image = Image.FromFile(openDlg.FileName);
            Program.SetResolution(MapPanel.Map.Image);
            MapPanel.Invalidate();
        }

        private void PasteBtn_Click(object sender, EventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                MapPanel.Map.Image = Clipboard.GetImage();
                Program.SetResolution(MapPanel.Map.Image);
                MapPanel.Invalidate();
            }
        }

        private void MapContext_Opening(object sender, CancelEventArgs e)
        {
            set_click_location();

            MapContextAddLocation.Enabled = _fRightClickLocation != PointF.Empty;
            MapContextRemove.Enabled = MapPanel.SelectedLocation != null;
            MapContextEdit.Enabled = MapPanel.SelectedLocation != null;
        }

        private void MapContextAddLocation_Click(object sender, EventArgs e)
        {
            if (_fRightClickLocation == PointF.Empty)
                return;

            var loc = new MapLocation();
            loc.Name = "New Location";
            loc.Point = _fRightClickLocation;

            var dlg = new MapLocationForm(loc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Map.Locations.Add(loc);
                MapPanel.Invalidate();

                Session.Modified = true;
            }
        }

        private void MapContextRemove_Click(object sender, EventArgs e)
        {
            if (MapPanel.SelectedLocation != null)
            {
                MapPanel.Map.Locations.Remove(MapPanel.SelectedLocation);
                MapPanel.Invalidate();

                Session.Modified = true;
            }
        }

        private void MapContextEdit_Click(object sender, EventArgs e)
        {
            if (MapPanel.SelectedLocation != null)
            {
                var index = Map.Locations.IndexOf(MapPanel.SelectedLocation);

                var dlg = new MapLocationForm(MapPanel.SelectedLocation);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Map.Locations[index] = dlg.MapLocation;
                    MapPanel.Invalidate();

                    Session.Modified = true;
                }
            }
        }

        private void MapPanel_DoubleClick(object sender, EventArgs e)
        {
            if (MapPanel.SelectedLocation == null)
            {
                set_click_location();
                MapContextAddLocation_Click(sender, e);
            }
            else
            {
                MapContextEdit_Click(sender, e);
            }
        }

        private void set_click_location()
        {
            _fRightClickLocation = PointF.Empty;

            var mouse = MapPanel.PointToClient(Cursor.Position);
            var rect = MapPanel.MapRectangle;
            if (rect.Contains(mouse))
            {
                var dx = (mouse.X - rect.X) / rect.Width;
                var dy = (mouse.Y - rect.Y) / rect.Height;

                _fRightClickLocation = new PointF(dx, dy);
            }
        }

        private void EntryBtn_Click(object sender, EventArgs e)
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
    }
}
