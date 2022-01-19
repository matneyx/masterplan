using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TileBreakdownForm : Form
    {
        public TileBreakdownForm(Map map)
        {
            InitializeComponent();

            var tiles = new Dictionary<Guid, int>();

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

            var libs = new List<string>();
            foreach (var tileId in tiles.Keys)
            {
                var tile = Session.FindTile(tileId, SearchType.Global);
                var lib = Session.FindLibrary(tile);

                if (!libs.Contains(lib.Name))
                    libs.Add(lib.Name);
            }

            libs.Sort();

            foreach (var libName in libs)
                TileList.Groups.Add(libName, libName);

            TileList.LargeImageList = new ImageList();
            TileList.LargeImageList.ImageSize = new Size(64, 64);

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
        }
    }
}
