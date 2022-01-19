using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TileSizeForm : Form
    {
        private readonly List<Tile> _fTiles;

        public Size TileSize { get; private set; } = new Size(2, 2);

        public TileSizeForm(List<Tile> tiles)
        {
            InitializeComponent();

            _fTiles = tiles;

            var x = 0;
            var y = 0;
            foreach (var t in _fTiles)
            {
                x += t.Size.Width;
                y += t.Size.Height;
            }

            x /= _fTiles.Count;
            y /= _fTiles.Count;

            WidthBox.Value = x;
            HeightBox.Value = y;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var width = (int)WidthBox.Value;
            var height = (int)HeightBox.Value;

            TileSize = new Size(width, height);
        }
    }
}
