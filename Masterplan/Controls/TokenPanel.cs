using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.UI;

namespace Masterplan.Controls
{
    internal partial class TokenPanel : UserControl
    {
        private Color _fColour = Color.Blue;

        private Image _fImage;

        private Size _fTileSize = new Size(2, 2);

        public Size TileSize
        {
            get => _fTileSize;
            set => _fTileSize = value;
        }

        public Image Image
        {
            get => _fImage;
            set
            {
                _fImage = value;
                update_picture();
            }
        }

        public Color Colour
        {
            get => _fColour;
            set
            {
                _fColour = value;
                update_picture();
            }
        }

        public TokenPanel()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
        }

        ~TokenPanel()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ImageClear.Enabled = _fImage != null;
        }

        private void ImageSelectFile_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = Program.ImageFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fImage = Image.FromFile(dlg.FileName);
                update_picture();
            }
        }

        private void ImageSelectTile_Click(object sender, EventArgs e)
        {
            var dlg = new TileSelectForm(_fTileSize, TileCategory.Feature);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fImage = dlg.Tile.Image;

                if (dlg.Tile.Size.Width != _fTileSize.Width || dlg.Tile.Size.Height != _fTileSize.Height)
                {
                    // Rotate once
                    _fImage = new Bitmap(_fImage);
                    _fImage.RotateFlip(RotateFlipType.Rotate90FlipNone);
                }

                update_picture();
            }
        }

        private void ImageSelectColour_Click(object sender, EventArgs e)
        {
            var dlg = new ColorDialog();
            dlg.AllowFullOpen = true;
            dlg.Color = ImageBox.BackColor;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _fImage = null;
                _fColour = dlg.Color;
                update_picture();
            }
        }

        private void ImageClear_Click(object sender, EventArgs e)
        {
            _fImage = null;
            update_picture();
        }

        private void update_picture()
        {
            if (_fImage != null)
            {
                ImageBox.BackColor = Color.Transparent;
                ImageBox.Image = _fImage;
            }
            else
            {
                ImageBox.BackColor = _fColour;
                ImageBox.Image = null;
            }
        }
    }
}
