using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MapAreaSelectForm : Form
    {
        public Map Map => MapBox.SelectedItem as Map;

        public MapArea MapArea => AreaBox.SelectedItem as MapArea;

        public MapAreaSelectForm(Guid mapId, Guid mapAreaId)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            UseTileBtn.Visible = map_tiles_exist();

            MapBox.Items.Add("(no map)");
            foreach (var m in Session.Project.Maps)
                MapBox.Items.Add(m);

            var map = Session.Project.FindTacticalMap(mapId);
            if (map != null)
            {
                MapBox.SelectedItem = map;

                var ma = map.FindArea(mapAreaId);
                if (ma != null)
                    AreaBox.SelectedItem = ma;
                else
                    AreaBox.SelectedIndex = 0;
            }
            else
            {
                MapBox.SelectedIndex = 0;

                AreaBox.Items.Add("(no map)");
                AreaBox.SelectedIndex = 0;
            }
        }

        ~MapAreaSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private bool map_tiles_exist()
        {
            var libs = new List<Library>();
            libs.AddRange(Session.Libraries);
            if (Session.Project != null)
                libs.Add(Session.Project.Library);
            foreach (var lib in libs)
            foreach (var tile in lib.Tiles)
                if (tile.Category == TileCategory.Map)
                    return true;

            return false;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            MapLbl.Enabled = Session.Project.Maps.Count != 0;
            MapBox.Enabled = Session.Project.Maps.Count != 0;

            var m = MapBox.SelectedItem as Map;
            var areas = m != null && m.Areas.Count != 0;

            AreaLbl.Enabled = areas;
            AreaBox.Enabled = areas;
        }

        private void MapBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            AreaBox.Items.Clear();

            var m = MapBox.SelectedItem as Map;
            if (m != null)
            {
                AreaBox.Items.Add("(entire map)");

                foreach (var ma in m.Areas)
                    AreaBox.Items.Add(ma);

                AreaBox.SelectedIndex = 0;
            }

            show_map();
        }

        private void AreaBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            show_map();
        }

        private void show_map()
        {
            var m = MapBox.SelectedItem as Map;
            if (m != null)
            {
                MapView.Map = m;

                var ma = AreaBox.SelectedItem as MapArea;
                if (ma != null)
                    MapView.Viewpoint = ma.Region;
                else
                    MapView.Viewpoint = Rectangle.Empty;
            }
            else
            {
                MapView.Map = null;
            }
        }

        private void NewBtn_Click(object sender, EventArgs e)
        {
            var m = new Map();
            m.Name = "New Map";

            var dlg = new MapBuilderForm(m, false);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Maps.Add(dlg.Map);
                Session.Modified = true;

                MapBox.Items.Add(dlg.Map);
                MapBox.SelectedItem = dlg.Map;
            }
        }

        private void ImportBtn_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            openDlg.Filter = Program.ImageFilter;
            if (openDlg.ShowDialog() != DialogResult.OK)
                return;

            var img = Image.FromFile(openDlg.FileName);
            if (img == null)
                return;

            var tile = new Tile();
            tile.Image = img;
            tile.Category = TileCategory.Map;

            var tileDlg = new TileForm(tile);
            if (tileDlg.ShowDialog() != DialogResult.OK)
                return;

            Session.Project.Library.Tiles.Add(tileDlg.Tile);

            var td = new TileData();
            td.TileId = tile.Id;

            var map = new Map();
            map.Name = FileName.Name(openDlg.FileName);
            map.Tiles.Add(td);

            Session.Project.Maps.Add(map);
            Session.Modified = true;

            MapBox.Items.Add(map);
            MapBox.SelectedItem = map;
        }

        private void UseTileBtn_Click(object sender, EventArgs e)
        {
            var dlg = new TileSelectForm(Size.Empty, TileCategory.Map);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var td = new TileData();
                td.TileId = dlg.Tile.Id;

                var map = new Map();
                map.Name = FileName.Name("New Map");
                map.Tiles.Add(td);

                Session.Project.Maps.Add(map);
                Session.Modified = true;

                MapBox.Items.Add(map);
                MapBox.SelectedItem = map;
            }
        }
    }
}
