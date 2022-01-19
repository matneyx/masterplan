using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace Masterplan.Controls
{
    internal partial class TilePanel : UserControl
    {
        private bool _fShowGridlines = true;

        private Color _fTileColour = Color.White;

        private Image _fTileImage;

        private Size _fTileSize = new Size(2, 2);

        public Image TileImage
        {
            get => _fTileImage;
            set
            {
                _fTileImage = value;
                Invalidate();
            }
        }

        public Color TileColour
        {
            get => _fTileColour;
            set
            {
                _fTileColour = value;
                Invalidate();
            }
        }

        public Size TileSize
        {
            get => _fTileSize;
            set
            {
                _fTileSize = value;
                Invalidate();
            }
        }

        public bool ShowGridlines
        {
            get => _fShowGridlines;
            set
            {
                _fShowGridlines = value;
                Invalidate();
            }
        }

        public TilePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            e.Graphics.FillRectangle(new SolidBrush(BackColor), ClientRectangle);

            var squareX = (double)ClientRectangle.Width / _fTileSize.Width;
            var squareY = (double)ClientRectangle.Height / _fTileSize.Height;
            var squareSize = (float)Math.Min(squareX, squareY);

            var imgWidth = squareSize * _fTileSize.Width;
            var imgHeight = squareSize * _fTileSize.Height;

            var dx = (ClientRectangle.Width - imgWidth) / 2;
            var dy = (ClientRectangle.Height - imgHeight) / 2;

            var imgRect = new RectangleF(dx, dy, imgWidth, imgHeight);

            if (_fTileImage != null)
            {
                e.Graphics.DrawImage(_fTileImage, imgRect);
            }
            else
            {
                using (Brush b = new SolidBrush(_fTileColour))
                {
                    e.Graphics.FillRectangle(b, imgRect);
                }

                using (var p = new Pen(Color.Black, 2))
                {
                    e.Graphics.DrawRectangle(p, imgRect.X, imgRect.Y, imgRect.Width, imgRect.Height);
                }
            }

            if (_fShowGridlines)
                using (var p = new Pen(Color.DarkGray))
                {
                    // Vertical gridlines
                    for (var n = 1; n != _fTileSize.Width; ++n)
                    {
                        var x = dx + n * squareSize;
                        e.Graphics.DrawLine(p, x, dy, x, dy + imgHeight);
                    }

                    // Horizontal gridlines
                    for (var n = 1; n != _fTileSize.Height; ++n)
                    {
                        var y = dy + n * squareSize;
                        e.Graphics.DrawLine(p, dx, y, dx + imgWidth, y);
                    }
                }
        }
    }
}
