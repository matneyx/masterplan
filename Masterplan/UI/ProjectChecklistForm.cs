using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ProjectChecklistForm : Form
    {
        private readonly Plot _fRootPlot = Session.Project.Plot;

        public Plot SelectedPlot => PlotTree.SelectedNode?.Tag as Plot;

        public MagicItem SelectedMagicItem
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as MagicItem;

                return null;
            }
        }

        public ProjectChecklistForm()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            update_plot_tree();
            update_lists();
        }

        ~ProjectChecklistForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            SelectAll.Enabled = ItemList.Items.Count != 0;
            SelectNone.Enabled = ItemList.Items.Count != 0;
            ExportBtn.Enabled = ItemList.CheckedItems.Count != 0 && Pages.SelectedTab == MagicItemsPage;
            PagesLbl.Visible = ItemList.CheckedItems.Count > 9;
        }

        private void PlotTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            update_lists();
        }

        private void ItemList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedMagicItem != null)
            {
                var dlg = new MagicItemDetailsForm(SelectedMagicItem);
                dlg.ShowDialog();
            }
        }

        private void update_plot_tree()
        {
            PlotTree.Nodes.Clear();
            var nodes = add_nodes(PlotTree.Nodes, _fRootPlot);
            PlotTree.ExpandAll();
            PlotTree.SelectedNode = PlotTree.Nodes[0];

            Splitter.Panel1Collapsed = nodes == 1;
        }

        private int add_nodes(TreeNodeCollection tnc, Plot p)
        {
            var nodes = 1;

            var pp = Session.Project.FindParent(p);
            var plotName = pp != null ? pp.Name : Session.Project.Name;

            var tn = tnc.Add(plotName);
            tn.Tag = p;

            foreach (var child in p.Points)
                if (child.Subplot.Points.Count != 0)
                    nodes += add_nodes(tn.Nodes, child.Subplot);

            return nodes;
        }

        private void update_lists()
        {
            update_list_items();
            update_list_minis();
            update_list_tiles();
        }

        private void update_list_items()
        {
            var items = new List<MagicItem>();

            var points = get_points(SelectedPlot);
            foreach (var pp in points)
            foreach (var parcel in pp.Parcels)
            {
                if (parcel.MagicItemId == Guid.Empty)
                    continue;

                var mi = Session.FindMagicItem(parcel.MagicItemId, SearchType.Global);
                if (mi != null && !items.Contains(mi))
                    items.Add(mi);
            }

            items.Sort();

            ItemList.Items.Clear();
            foreach (var mi in items)
            {
                var lvi = ItemList.Items.Add(mi.Name);
                lvi.Tag = mi;
            }

            ItemList.CheckBoxes = ItemList.Items.Count > 0;

            if (items.Count == 0)
            {
                var lvi = ItemList.Items.Add("None");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void update_list_minis()
        {
            var encounters = new List<Encounter>();

            var points = get_points(SelectedPlot);
            foreach (var pp in points)
            {
                var enc = pp.Element as Encounter;
                if (enc != null) encounters.Add(enc);
            }

            var creatures = new Dictionary<Guid, int>();

            foreach (var enc in encounters)
            {
                // Get the mini breakdown for this encounter
                var encCreatures = new Dictionary<Guid, int>();
                foreach (var slot in enc.Slots)
                {
                    if (!encCreatures.ContainsKey(slot.Card.CreatureId))
                        encCreatures[slot.Card.CreatureId] = 0;

                    encCreatures[slot.Card.CreatureId] += slot.CombatData.Count;
                }

                // Update the running total
                foreach (var creatureId in encCreatures.Keys)
                {
                    if (!creatures.ContainsKey(creatureId))
                        creatures[creatureId] = 0;

                    if (encCreatures[creatureId] > creatures[creatureId])
                        creatures[creatureId] = encCreatures[creatureId];
                }
            }

            var creatureList = new List<ICreature>();
            foreach (var creatureId in creatures.Keys)
            {
                var c = Session.FindCreature(creatureId, SearchType.Global);
                if (c != null)
                    creatureList.Add(c);
            }

            creatureList.Sort();

            MiniList.Items.Clear();
            foreach (var c in creatureList)
            {
                var lvi = MiniList.Items.Add(c.Name);

                lvi.SubItems.Add(c.Size.ToString());

                var info = "";
                if (c.Keywords != "")
                {
                    if (info != "")
                        info += "; ";
                    info += "Keywords: " + c.Keywords;
                }

                foreach (var cp in c.CreaturePowers)
                {
                    if (info != "")
                        info += ", ";
                    info += cp.Name;
                }

                lvi.SubItems.Add(info);

                var count = creatures[c.Id];
                if (count > 1)
                    lvi.SubItems.Add("x" + count);
                else
                    lvi.SubItems.Add("");
            }

            if (creatureList.Count == 0)
            {
                var lvi = MiniList.Items.Add("None");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void update_list_tiles()
        {
            var maps = new List<Map>();

            var points = get_points(SelectedPlot);
            var mapIds = new List<Guid>();
            foreach (var pp in points)
            {
                var enc = pp.Element as Encounter;
                if (enc != null)
                    if (enc.MapId != Guid.Empty)
                        mapIds.Add(enc.MapId);

                var me = pp.Element as MapElement;
                if (me != null) mapIds.Add(me.MapId);
            }

            foreach (var mapId in mapIds)
            {
                var map = Session.Project.FindTacticalMap(mapId);
                if (map != null)
                    maps.Add(map);
            }

            var tiles = new Dictionary<Guid, int>();

            foreach (var map in maps)
            {
                // Get the tile breakdown for this map
                var mapTiles = new Dictionary<Guid, int>();
                foreach (var td in map.Tiles)
                {
                    if (!mapTiles.ContainsKey(td.TileId))
                        mapTiles[td.TileId] = 0;

                    mapTiles[td.TileId] += 1;
                }

                // Update the running total
                foreach (var tileId in mapTiles.Keys)
                {
                    if (!tiles.ContainsKey(tileId))
                        tiles[tileId] = 0;

                    if (mapTiles[tileId] > tiles[tileId])
                        tiles[tileId] = mapTiles[tileId];
                }
            }

            var libs = new List<string>();
            foreach (var tileId in tiles.Keys)
            {
                var tile = Session.FindTile(tileId, SearchType.Global);
                var lib = Session.FindLibrary(tile);

                if (!libs.Contains(lib.Name))
                    libs.Add(lib.Name);
            }

            libs.Sort();

            TileList.Groups.Clear();
            foreach (var libName in libs)
                TileList.Groups.Add(libName, libName);
            TileList.ShowGroups = TileList.Groups.Count > 0;

            TileList.LargeImageList = new ImageList();
            TileList.LargeImageList.ImageSize = new Size(64, 64);

            TileList.Items.Clear();
            foreach (var tileId in tiles.Keys)
            {
                var t = Session.FindTile(tileId, SearchType.Global);
                var lib = Session.FindLibrary(t);

                var lvi = TileList.Items.Add("x " + tiles[tileId]);
                lvi.Tag = t;
                lvi.Group = TileList.Groups[lib.Name];

                // Get tile image
                var img = t.Image ?? t.BlankImage;

                var bmp = new Bitmap(64, 64);
                if (t.Size.Width > t.Size.Height)
                {
                    var height = t.Size.Height * 64 / t.Size.Width;
                    var rect = new Rectangle(0, (64 - height) / 2, 64, height);

                    var g = Graphics.FromImage(bmp);
                    g.DrawImage(img, rect);
                }
                else
                {
                    var width = t.Size.Width * 64 / t.Size.Height;
                    var rect = new Rectangle((64 - width) / 2, 0, width, 64);

                    var g = Graphics.FromImage(bmp);
                    g.DrawImage(img, rect);
                }

                TileList.LargeImageList.Images.Add(bmp);
                lvi.ImageIndex = TileList.LargeImageList.Images.Count - 1;
            }

            if (tiles.Keys.Count == 0)
            {
                var lvi = TileList.Items.Add("None");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private List<PlotPoint> get_points(Plot p)
        {
            var points = new List<PlotPoint>();

            points.AddRange(p.Points);

            foreach (var pp in p.Points)
                points.AddRange(get_points(pp.Subplot));

            return points;
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ItemList.Items)
                lvi.Checked = true;
        }

        private void SelectNone_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in ItemList.Items)
                lvi.Checked = false;
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            Close();

            var pages = ItemList.CheckedItems.Count / 9;
            var remainder = ItemList.CheckedItems.Count % 9;
            if (remainder > 0)
                pages += 1;

            for (var page = 0; page != pages; ++page)
            {
                var dlg = new SaveFileDialog();
                dlg.Filter = Program.HtmlFilter;
                dlg.FileName = Session.Project.Name + " Treasure";
                dlg.Title = "Export";
                if (pages != 1)
                    dlg.Title += " (page " + (page + 1) + ")";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var lines = Html.GetHead("Loot", "", Session.Preferences.TextSize);

                    lines.Add("<BODY>");
                    lines.Add("<P>");
                    lines.Add("<TABLE class=clear height=100%>");

                    for (var row = 0; row != 3; ++row)
                    {
                        lines.Add("<TR class=clear width=33% height=33%>");

                        for (var col = 0; col != 3; ++col)
                        {
                            lines.Add("<TD width=33% height=33%>");

                            var index = page * 9 + row * 3 + col;
                            if (ItemList.CheckedItems.Count > index)
                            {
                                var mi = ItemList.CheckedItems[index].Tag as MagicItem;
                                if (mi != null)
                                    lines.Add(Html.MagicItem(mi, Session.Preferences.TextSize, false, false));
                            }

                            lines.Add("</TD>");
                        }

                        lines.Add("</TR>");
                    }

                    lines.Add("</TABLE>");
                    lines.Add("</P>");
                    lines.Add("</BODY>");

                    lines.Add("</HTML>");

                    var html = Html.Concatenate(lines);
                    File.WriteAllText(dlg.FileName, html);
                }
            }
        }
    }
}
