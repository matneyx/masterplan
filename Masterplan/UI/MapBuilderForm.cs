using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Events;
using Masterplan.Tools;
using Masterplan.Tools.Generators;
using Masterplan.Wizards;

namespace Masterplan.UI
{
    /// <summary>
    ///     Enumeration containing ways in which tiles can be grouped.
    /// </summary>
    public enum TileView
    {
        /// <summary>
        ///     Sort tiles by library.
        /// </summary>
        Library,

        /// <summary>
        ///     Sort tiles by size.
        /// </summary>
        Size,

        /// <summary>
        ///     Sort tiles by category.
        /// </summary>
        Category
    }

    /// <summary>
    ///     Enumeration containing tile display sizes.
    /// </summary>
    public enum TileSize
    {
        /// <summary>
        ///     Small size.
        /// </summary>
        Small,

        /// <summary>
        ///     Medium size.
        /// </summary>
        Medium,

        /// <summary>
        ///     Large size.
        /// </summary>
        Large
    }

    internal partial class MapBuilderForm : Form
    {
        private readonly Dictionary<Guid, bool> _fTileSets = new Dictionary<Guid, bool>();

        public Map Map { get; }

        public Tile SelectedTile
        {
            get
            {
                if (TileList.SelectedItems.Count != 0)
                    return TileList.SelectedItems[0].Tag as Tile;

                return null;
            }
        }

        public MapArea SelectedArea
        {
            get
            {
                if (AreaList.SelectedItems.Count != 0)
                    return AreaList.SelectedItems[0].Tag as MapArea;

                return null;
            }
        }

        public MapBuilderForm(Map m, bool autobuild)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            var libs = new List<Library>();
            libs.AddRange(Session.Libraries);
            if (Session.Project != null)
                libs.Add(Session.Project.Library);
            var tilesShown = 0;
            foreach (var lib in libs)
            {
                if (Session.Preferences.TileLibraries != null)
                    _fTileSets[lib.Id] = Session.Preferences.TileLibraries.Contains(lib.Id);
                else
                    _fTileSets[lib.Id] = true;

                if (_fTileSets[lib.Id])
                    tilesShown = lib.Tiles.Count;
            }

            if (tilesShown == 0)
                foreach (var lib in libs)
                    _fTileSets[lib.Id] = true;

            MapFilterPanel.Visible = false;
            populate_tiles();

            Map = m.Copy();
            MapView.Map = Map;
            NameBox.Text = Map.Name;

            if (autobuild)
            {
                Cursor.Current = Cursors.WaitCursor;

                ToolsAutoBuild_Click(null, null);

                foreach (var area in Map.Areas)
                {
                    area.Name = RoomBuilder.Name();
                    area.Details = RoomBuilder.Details();
                }

                Cursor.Current = Cursors.Default;
            }

            update_areas();
        }

        ~MapBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = MapView.SelectedTiles != null && MapView.SelectedTiles.Count != 0;
            RotateLeftBtn.Enabled = MapView.SelectedTiles != null && MapView.SelectedTiles.Count != 0;
            RotateRightBtn.Enabled = MapView.SelectedTiles != null && MapView.SelectedTiles.Count != 0;
            OrderingBtn.Enabled = MapView.SelectedTiles != null && MapView.SelectedTiles.Count == 1;

            ToolsHighlightAreas.Checked = MapView.HighlightAreas;
            ToolsNavigate.Checked = MapView.AllowScrolling;
            ToolsClearBackground.Enabled = MapView.BackgroundMap != null;

            AreaRemoveBtn.Enabled = SelectedArea != null;
            AreaEditBtn.Enabled = SelectedArea != null;
            FullMapBtn.Enabled = MapView.Viewpoint != Rectangle.Empty;

            GroupByTileSet.Checked = Session.Preferences.MapBuilder.TileView == TileView.Library;
            GroupBySize.Checked = Session.Preferences.MapBuilder.TileView == TileView.Size;

            SizeSmall.Checked = Session.Preferences.MapBuilder.TileSize == TileSize.Small;
            SizeMedium.Checked = Session.Preferences.MapBuilder.TileSize == TileSize.Medium;
            SizeLarge.Checked = Session.Preferences.MapBuilder.TileSize == TileSize.Large;

            FilterBtn.Checked = MapFilterPanel.Visible;

            OKBtn.Enabled = Map.Name != "" && Map.Tiles.Count != 0;
        }

        private void MapForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Session.Preferences.TileLibraries = new List<Guid>();
            foreach (var id in _fTileSets.Keys)
                if (_fTileSets[id])
                    Session.Preferences.TileLibraries.Add(id);
        }

        protected override bool IsInputKey(Keys key)
        {
            return base.IsInputKey(key) || MapView.HandleKey(key);
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count != 0)
            {
                foreach (var td in MapView.SelectedTiles)
                    Map.Tiles.Remove(td);
                MapView.SelectedTiles.Clear();

                MapView.MapChanged();
            }
        }

        private void RotateLeftBtn_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count != 0)
            {
                // Rotate selected tile
                foreach (var td in MapView.SelectedTiles)
                    td.Rotations -= 1;

                MapView.MapChanged();
            }
        }

        private void RotateRightBtn_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count != 0)
            {
                // Rotate selected tile
                foreach (var td in MapView.SelectedTiles)
                    td.Rotations += 1;

                MapView.MapChanged();
            }
        }

        private void OrderingFront_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count == 1)
            {
                var td = MapView.SelectedTiles[0];
                Map.Tiles.Remove(td);
                Map.Tiles.Add(td);

                MapView.MapChanged();
            }
        }

        private void OrderingBack_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count == 1)
            {
                var td = MapView.SelectedTiles[0];
                Map.Tiles.Remove(td);
                Map.Tiles.Insert(0, td);

                MapView.MapChanged();
            }
        }

        private void RotateMapLeft_Click(object sender, EventArgs e)
        {
            // Rotate whole map
            foreach (var td in Map.Tiles)
            {
                if (!MapView.LayoutData.Tiles.ContainsKey(td))
                    continue;

                // Change location
                var x = td.Location.X - MapView.LayoutData.MinX;
                var y = td.Location.Y - MapView.LayoutData.MinY;
                var t = MapView.LayoutData.Tiles[td];
                var tilewidth = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
                td.Location = new Point(y, MapView.LayoutData.Width - x - tilewidth + 1);

                // Rotate
                td.Rotations -= 1;
            }

            // Rotate areas
            foreach (var area in Map.Areas)
            {
                var x = area.Region.X - MapView.LayoutData.MinX;
                var y = area.Region.Y - MapView.LayoutData.MinY;
                var loc = new Point(y, MapView.LayoutData.Width - x - area.Region.Width + 1);
                var size = new Size(area.Region.Height, area.Region.Width);
                area.Region = new Rectangle(loc, size);
            }

            if (SelectedArea != null)
                MapView.Viewpoint = SelectedArea.Region;

            MapView.MapChanged();
        }

        private void RotateMapRight_Click(object sender, EventArgs e)
        {
            // Rotate whole map
            foreach (var td in Map.Tiles)
            {
                if (!MapView.LayoutData.Tiles.ContainsKey(td))
                    continue;

                // Change location
                var x = td.Location.X - MapView.LayoutData.MinX;
                var y = td.Location.Y - MapView.LayoutData.MinY;
                var t = MapView.LayoutData.Tiles[td];
                var tileheight = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;
                td.Location = new Point(MapView.LayoutData.Height - y - tileheight + 1, x);

                // Rotate
                td.Rotations += 1;
            }

            // Rotate areas
            foreach (var area in Map.Areas)
            {
                var x = area.Region.X - MapView.LayoutData.MinX;
                var y = area.Region.Y - MapView.LayoutData.MinY;
                var loc = new Point(MapView.LayoutData.Height - y - area.Region.Height + 1, x);
                var size = new Size(area.Region.Height, area.Region.Width);
                area.Region = new Rectangle(loc, size);
            }

            if (SelectedArea != null)
                MapView.Viewpoint = SelectedArea.Region;

            MapView.MapChanged();
        }

        private void ToolsHighlightAreas_Click(object sender, EventArgs e)
        {
            MapView.HighlightAreas = !MapView.HighlightAreas;
        }

        private void ToolsClearAll_Click(object sender, EventArgs e)
        {
            Map.Tiles.Clear();

            MapView.MapChanged();
        }

        private void NameBox_TextChanged(object sender, EventArgs e)
        {
            Map.Name = NameBox.Text;
        }

        private void AreaRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedArea != null)
            {
                Map.Areas.Remove(SelectedArea);
                update_areas();

                MapView.Viewpoint = Rectangle.Empty;
            }
        }

        private void AreaEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedArea != null)
            {
                var index = Map.Areas.IndexOf(SelectedArea);

                var dlg = new MapAreaForm(SelectedArea, Map);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Map.Areas[index] = dlg.Area;
                    update_areas();

                    MapView.Viewpoint = Map.Areas[index].Region;
                }
            }
        }

        private void FullMapBtn_Click(object sender, EventArgs e)
        {
            MapView.Viewpoint = Rectangle.Empty;
        }

        private void AreaList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectedArea != null)
                MapView.Viewpoint = SelectedArea.Region;
            else
                MapView.Viewpoint = Rectangle.Empty;
        }

        private void TileSet_Click(object sender, EventArgs e)
        {
            // Show / hide this tile set
            var tsmi = sender as ToolStripMenuItem;
            var lib = tsmi.Tag as Library;

            _fTileSets[lib.Id] = !_fTileSets[lib.Id];
            tsmi.Checked = _fTileSets[lib.Id];

            populate_tiles();
        }

        private void GroupByTileSet_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileView = TileView.Library;

            populate_tiles();
        }

        private void GroupBySize_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileView = TileView.Size;

            populate_tiles();
        }

        private void GroupByCategory_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileView = TileView.Category;

            populate_tiles();
        }

        private void TileList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedTile != null)
                DoDragDrop(SelectedTile, DragDropEffects.All);
        }

        private void populate_tiles()
        {
            var libraries = new List<Library>();
            libraries.AddRange(Session.Libraries);
            libraries.Add(Session.Project.Library);

            var sets = new List<string>();
            switch (Session.Preferences.MapBuilder.TileView)
            {
                case TileView.Library:
                {
                    foreach (var lib in libraries)
                    {
                        if (!_fTileSets[lib.Id])
                            continue;

                        if (!sets.Contains(lib.Name))
                            sets.Add(lib.Name);
                    }

                    sets.Sort();
                }
                    break;
                case TileView.Size:
                {
                    var areas = new List<int>();
                    foreach (var lib in libraries)
                    foreach (var t in lib.Tiles)
                        if (!areas.Contains(t.Area))
                            areas.Add(t.Area);

                    areas.Sort();

                    foreach (var area in areas)
                        sets.Add("Size: " + area);
                }
                    break;
                case TileView.Category:
                {
                    foreach (TileCategory cat in Enum.GetValues(typeof(TileCategory)))
                        sets.Add(cat.ToString());
                }
                    break;
            }

            var size = 32;
            switch (Session.Preferences.MapBuilder.TileSize)
            {
                case TileSize.Small:
                    size = 16;
                    break;
                case TileSize.Medium:
                    size = 32;
                    break;
                case TileSize.Large:
                    size = 64;
                    break;
            }

            TileList.BeginUpdate();

            TileList.Groups.Clear();
            foreach (var set in sets)
                TileList.Groups.Add(set, set);

            TileList.ShowGroups = TileList.Groups.Count != 0;

            var itemList = new List<ListViewItem>();
            var imageList = new List<Image>();

            foreach (var lib in libraries)
            {
                if (!_fTileSets[lib.Id])
                    continue;

                foreach (var t in lib.Tiles)
                {
                    if (!Match(t, SearchBox.Text))
                        continue;

                    var lvi = new ListViewItem(t.ToString());
                    lvi.Tag = t;

                    switch (Session.Preferences.MapBuilder.TileView)
                    {
                        case TileView.Library:
                            lvi.Group = TileList.Groups[lib.Name];
                            break;
                        case TileView.Size:
                            lvi.Group = TileList.Groups["Size: " + t.Area];
                            break;
                        case TileView.Category:
                            lvi.Group = TileList.Groups[t.Category.ToString()];
                            break;
                    }

                    // Get tile image
                    var img = t.Image ?? t.BlankImage;
                    if (img == null)
                        continue;

                    try
                    {
                        var bmp = new Bitmap(size, size);
                        if (t.Size.Width > t.Size.Height)
                        {
                            var height = t.Size.Height * size / t.Size.Width;
                            var rect = new Rectangle(0, (size - height) / 2, size, height);

                            var g = Graphics.FromImage(bmp);
                            g.DrawImage(img, rect);
                        }
                        else
                        {
                            var width = t.Size.Width * size / t.Size.Height;
                            var rect = new Rectangle((size - width) / 2, 0, width, size);

                            var g = Graphics.FromImage(bmp);
                            g.DrawImage(img, rect);
                        }

                        imageList.Add(bmp);
                        lvi.ImageIndex = imageList.Count - 1;

                        itemList.Add(lvi);
                    }
                    catch (Exception ex)
                    {
                        LogSystem.Trace(ex);
                    }
                }
            }

            TileList.LargeImageList = new ImageList();
            TileList.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
            TileList.LargeImageList.ImageSize = new Size(size, size);
            TileList.LargeImageList.Images.AddRange(imageList.ToArray());

            TileList.Items.Clear();
            TileList.Items.AddRange(itemList.ToArray());

            if (TileList.Items.Count == 0)
            {
                var lvi = TileList.Items.Add("(no tiles)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            TileList.EndUpdate();
        }

        private bool Match(Tile t, string query)
        {
            var tokens = query.ToLower().Split();
            foreach (var token in tokens)
                if (!match_token(t, token))
                    return false;

            return true;
        }

        private bool match_token(Tile t, string token)
        {
            return t.Keywords.ToLower().Contains(token);
        }

        private void update_areas()
        {
            AreaList.Items.Clear();

            foreach (var area in Map.Areas)
            {
                var lvi = AreaList.Items.Add(area.Name);
                lvi.Tag = area;
            }

            if (AreaList.Items.Count == 0)
            {
                var lvi = AreaList.Items.Add("(no areas defined)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void MapView_ItemDropped(object sender, EventArgs e)
        {
        }

        private void MapView_ItemMoved(object sender, MovementEventArgs e)
        {
        }

        private void MapView_ItemRemoved(object sender, EventArgs e)
        {
        }

        private void MapView_RegionSelected(object sender, EventArgs e)
        {
            var mouse = MapView.PointToClient(Cursor.Position);
            MapContextMenu.Show(MapView, mouse);
        }

        private void MapView_TileContext(object sender, EventArgs e)
        {
            var mouse = MapView.PointToClient(Cursor.Position);
            TileContextMenu.Show(MapView, mouse);
        }

        private void ContextCreate_Click(object sender, EventArgs e)
        {
            try
            {
                if (MapView.Selection != Rectangle.Empty)
                {
                    var area = new MapArea();
                    area.Name = "New Area";
                    area.Region = MapView.Selection;

                    var dlg = new MapAreaForm(area, Map);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Map.Areas.Add(dlg.Area);
                        update_areas();

                        MapView.Selection = Rectangle.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextClear_Click(object sender, EventArgs e)
        {
            try
            {
                var obsolete = new List<TileData>();
                foreach (var td in Map.Tiles)
                {
                    if (!MapView.LayoutData.TileSquares.ContainsKey(td))
                        continue;

                    var rect = MapView.LayoutData.TileSquares[td];

                    if (MapView.Selection.IntersectsWith(rect))
                        obsolete.Add(td);
                }

                foreach (var td in obsolete)
                    Map.Tiles.Remove(td);

                MapView.Selection = Rectangle.Empty;
                MapView.MapChanged();
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ContextSelect_Click(object sender, EventArgs e)
        {
            try
            {
                MapView.SelectedTiles.Clear();
                foreach (var td in Map.Tiles)
                {
                    if (!MapView.LayoutData.TileSquares.ContainsKey(td))
                        continue;

                    var rect = MapView.LayoutData.TileSquares[td];

                    if (MapView.Selection.IntersectsWith(rect))
                        MapView.SelectedTiles.Add(td);
                }

                MapView.Selection = Rectangle.Empty;
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        private void ToolsAutoBuild_Click(object sender, EventArgs e)
        {
            var delveOnly = sender == null;
            var wizard = new MapWizard(delveOnly);
            if (sender == null)
            {
                var mbd = wizard.Data as MapBuilderData;
                mbd.MaxAreaCount = 3;
                mbd.MinAreaCount = 3;
            }

            if (wizard.Show() == DialogResult.OK)
            {
                var data = wizard.Data as MapBuilderData;

                MapView.Viewpoint = Rectangle.Empty;

                var attempts = 0;
                while (attempts != 20)
                {
                    attempts += 1;

                    MapBuilder.BuildMap(data, Map, OnAutoBuild);

                    if (data.Type == MapAutoBuildType.FilledArea || data.Type == MapAutoBuildType.Freeform)
                        break;

                    if (Map.Areas.Count >= data.MinAreaCount)
                        break;
                }

                if (data.Type == MapAutoBuildType.Warren && MapView.LayoutData.Height > MapView.LayoutData.Width)
                    RotateMapLeft_Click(null, null);

                MapView.MapChanged();
                update_areas();
            }
        }

        private void OnAutoBuild(object sender, EventArgs e)
        {
            MapView.MapChanged();
            Application.DoEvents();
        }

        private void MapView_HoverAreaChanged(object sender, EventArgs e)
        {
            var title = "";
            var info = "";

            if (MapView.HighlightedArea != null)
            {
                title = MapView.HighlightedArea.Name;
                info = TextHelper.Wrap(MapView.HighlightedArea.Details);

                if (info != "")
                    info += Environment.NewLine;

                info += MapView.HighlightedArea.Region.Width + " sq x " + MapView.HighlightedArea.Region.Height + " sq";
            }

            Tooltip.ToolTipTitle = title;
            Tooltip.ToolTipIcon = ToolTipIcon.Info;
            Tooltip.SetToolTip(MapView, info);
        }

        private void SizeSmall_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileSize = TileSize.Small;

            populate_tiles();
        }

        private void SizeMedium_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileSize = TileSize.Medium;

            populate_tiles();
        }

        private void SizeLarge_Click(object sender, EventArgs e)
        {
            Session.Preferences.MapBuilder.TileSize = TileSize.Large;

            populate_tiles();
        }

        private void MapForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                RemoveBtn_Click(sender, e);

            switch (e.KeyCode)
            {
                case Keys.Delete:
                    RemoveBtn_Click(sender, e);
                    break;
                case Keys.Up:
                case Keys.Down:
                case Keys.Left:
                case Keys.Right:
                    MapView.Nudge(e);
                    break;
            }
        }

        private void ZoomGauge_Scroll(object sender, EventArgs e)
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

        private void ToolsNavigate_Click(object sender, EventArgs e)
        {
            MapView.AllowScrolling = !MapView.AllowScrolling;
            ZoomGauge.Visible = MapView.AllowScrolling;
        }

        private void ToolsReset_Click(object sender, EventArgs e)
        {
            ZoomGauge.Value = 50;
            MapView.ScalingFactor = 1.0;
            MapView.Viewpoint = Rectangle.Empty;

            MapView.MapChanged();
        }

        private void TileContextSwap_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count != 1)
                return;

            var td = MapView.SelectedTiles[0];
            var t = Session.FindTile(td.TileId, SearchType.Global);
            var dlg = new TileSelectForm(t.Size, t.Category);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                // Swap tile
                var width = td.Rotations % 2 == 0 ? t.Size.Width : t.Size.Height;
                var height = td.Rotations % 2 == 0 ? t.Size.Height : t.Size.Width;

                var rot = 0;
                if (dlg.Tile.Size.Width != width || dlg.Tile.Size.Height != height)
                    rot = 1;

                td.TileId = dlg.Tile.Id;
                td.Rotations = rot;

                MapView.MapChanged();
            }
        }

        private void TileContextDuplicate_Click(object sender, EventArgs e)
        {
            if (MapView.SelectedTiles.Count != 1)
                return;

            var td = MapView.SelectedTiles[0];
            td = td.Copy();
            td.Id = Guid.NewGuid();
            td.Location = new Point(td.Location.X + 1, td.Location.Y + 1);

            Map.Tiles.Add(td);

            MapView.MapChanged();
        }

        private void ViewSelectLibraries_Click(object sender, EventArgs e)
        {
            var libraries = new List<Library>();
            libraries.AddRange(Session.Libraries);
            libraries.Add(Session.Project.Library);

            var libs = new List<Library>();
            foreach (var lib in libraries)
                if (_fTileSets[lib.Id])
                    libs.Add(lib);

            var dlg = new TileLibrarySelectForm(libs);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var lib in libraries)
                    _fTileSets[lib.Id] = dlg.Libraries.Contains(lib);

                populate_tiles();
            }
        }

        private void ToolsSelectBackground_Click(object sender, EventArgs e)
        {
            var exclude = new List<Guid>();
            exclude.Add(Map.Id);

            var dlg = new MapSelectForm(Session.Project.Maps, exclude, false);
            if (dlg.ShowDialog() == DialogResult.OK)
                MapView.BackgroundMap = dlg.Map;
        }

        private void ToolsClearBackground_Click(object sender, EventArgs e)
        {
            MapView.BackgroundMap = null;
        }

        private void MapView_AreaActivated(object sender, MapAreaEventArgs e)
        {
            var index = Map.Areas.IndexOf(e.MapArea);

            var dlg = new MapAreaForm(e.MapArea, Map);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Map.Areas[index] = dlg.Area;
                update_areas();
            }
        }

        private void ToolsImportMap_Click(object sender, EventArgs e)
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

            var tileDlg = new TileForm(tile);
            if (tileDlg.ShowDialog() != DialogResult.OK)
                return;

            Session.Project.Library.Tiles.Add(tileDlg.Tile);

            var td = new TileData();
            td.TileId = tile.Id;

            Map.Tiles.Add(td);
            MapView.MapChanged();
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedTile != null)
            {
                var lib = Session.FindLibrary(SelectedTile);
                var index = lib.Tiles.IndexOf(SelectedTile);

                var dlg = new TileForm(SelectedTile);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    lib.Tiles[index] = dlg.Tile;
                    populate_tiles();
                }
            }
        }

        private void MapForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK)
                return;

            if (Map.Tiles.Count == 0)
                return;

            if (Session.Project.FindTacticalMap(Map.Id) != null)
                return;

            var msg = "Do you want to save this new map?";
            if (MessageBox.Show(msg, "Masterplan", MessageBoxButtons.YesNo, MessageBoxIcon.Question) ==
                DialogResult.Yes)
                DialogResult = DialogResult.OK;
        }

        private void MapView_MouseZoomed(object sender, MouseEventArgs e)
        {
            ZoomGauge.Visible = true;
            ZoomGauge.Value -= Math.Sign(e.Delta);
            ZoomGauge_Scroll(sender, e);
        }

        private void FilterBtn_Click(object sender, EventArgs e)
        {
            MapFilterPanel.Visible = !MapFilterPanel.Visible;
        }

        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            populate_tiles();
        }
    }
}
