using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TileSelectForm : Form
    {
        private readonly TileCategory _fCategory = TileCategory.Map;

        private GroupBy _fGroupBy = GroupBy.Library;
        private bool _fMatchCategory = true;

        private Size _fTileSize = Size.Empty;

        public Tile Tile
        {
            get
            {
                if (TileList.SelectedItems.Count != 0)
                    return TileList.SelectedItems[0].Tag as Tile;

                return null;
            }
        }

        public TileSelectForm(Size tilesize, TileCategory category)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fTileSize = tilesize;
            _fCategory = category;

            MatchCatBtn.Text = "Show only tiles in category: " + _fCategory;

            update_tiles();
        }

        ~TileSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Tile != null;

            LibraryBtn.Checked = _fGroupBy == GroupBy.Library;
            CategoryBtn.Checked = _fGroupBy == GroupBy.Category;

            MatchCatBtn.Checked = _fMatchCategory;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (Tile != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void LibraryBtn_Click(object sender, EventArgs e)
        {
            _fGroupBy = GroupBy.Library;
            update_tiles();
        }

        private void CategoryBtn_Click(object sender, EventArgs e)
        {
            _fGroupBy = GroupBy.Category;
            update_tiles();
        }

        private void MatchCatBtn_Click(object sender, EventArgs e)
        {
            _fMatchCategory = !_fMatchCategory;
            update_tiles();
        }

        private void update_tiles()
        {
            var tiles = new List<Tile>();
            foreach (var lib in Session.Libraries)
            foreach (var t in lib.Tiles)
            {
                if (_fMatchCategory)
                    if (_fCategory != t.Category)
                        continue;

                var sameSize = false;

                if (_fTileSize != Size.Empty)
                {
                    if (t.Size.Width == _fTileSize.Width && t.Size.Height == _fTileSize.Height)
                        sameSize = true;

                    if (t.Size.Width == _fTileSize.Height && t.Size.Height == _fTileSize.Width)
                        sameSize = true;
                }
                else
                {
                    sameSize = true;
                }

                if (sameSize)
                    tiles.Add(t);
            }

            TileList.Groups.Clear();
            switch (_fGroupBy)
            {
                case GroupBy.Library:
                {
                    foreach (var lib in Session.Libraries)
                        TileList.Groups.Add(lib.Name, lib.Name);
                }
                    break;
                case GroupBy.Category:
                {
                    var cats = Enum.GetValues(typeof(TileCategory));
                    foreach (TileCategory cat in cats)
                        TileList.Groups.Add(cat.ToString(), cat.ToString());
                }
                    break;
            }

            TileList.BeginUpdate();

            TileList.LargeImageList = new ImageList();
            TileList.LargeImageList.ColorDepth = ColorDepth.Depth32Bit;
            TileList.LargeImageList.ImageSize = new Size(64, 64);

            var itemList = new List<ListViewItem>();

            foreach (var t in tiles)
            {
                var lvi = new ListViewItem(t.ToString());
                lvi.Tag = t;

                switch (_fGroupBy)
                {
                    case GroupBy.Library:
                    {
                        var lib = Session.FindLibrary(t);
                        lvi.Group = TileList.Groups[lib.Name];
                    }
                        break;
                    case GroupBy.Category:
                    {
                        lvi.Group = TileList.Groups[t.Category.ToString()];
                    }
                        break;
                }

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

                itemList.Add(lvi);
            }

            TileList.Items.Clear();
            TileList.Items.AddRange(itemList.ToArray());

            TileList.EndUpdate();
        }

        private enum GroupBy
        {
            Library,
            Category
        }
    }
}
