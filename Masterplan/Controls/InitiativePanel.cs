using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class InitiativePanel : UserControl
    {
        private const int Border = 8;

        private readonly StringFormat _centered = new StringFormat();

        private readonly Pen _fTickPen = new Pen(Color.Gray, 0.5f);

        private int _fCurrent;

        private int _fHoveredInit = int.MinValue;

        private List<int> _fInitiatives = new List<int>();

        /// <summary>
        ///     Gets or sets the list of initiative scores.
        /// </summary>
        public List<int> InitiativeScores
        {
            get => _fInitiatives;
            set
            {
                _fInitiatives = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets or sets the current initiative score.
        /// </summary>
        public int CurrentInitiative
        {
            get => _fCurrent;
            set
            {
                _fCurrent = value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Gets the maximum initiative score.
        /// </summary>
        public int Minimum
        {
            get
            {
                var range = get_range();
                return range.First;
            }
        }

        /// <summary>
        ///     Gets the mimimum initiative score.
        /// </summary>
        public int Maximum
        {
            get
            {
                var range = get_range();
                return range.Second;
            }
        }

        public InitiativePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            _centered.Alignment = StringAlignment.Center;
            _centered.LineAlignment = StringAlignment.Center;
            _centered.Trimming = StringTrimming.EllipsisWord;
        }

        public event EventHandler InitiativeChanged;

        protected void OnInitiativeChanged()
        {
            InitiativeChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);

            float xLine = ClientRectangle.Right - Border;

            // Initiative line
            var top = new PointF(xLine, Border);
            var bottom = new PointF(xLine, ClientRectangle.Bottom - Border);
            e.Graphics.DrawLine(Pens.Black, top, bottom);

            // Ticks
            var range = get_range();
            for (var n = range.First; n <= range.Second; ++n)
                if (n % 5 == 0)
                {
                    // Draw a tick
                    var y = get_y(n);
                    var left = new PointF(xLine - 5, y);
                    var right = new PointF(xLine, y);
                    e.Graphics.DrawLine(_fTickPen, left, right);
                }

            // Combatants
            foreach (var score in _fInitiatives)
            {
                // Draw a marker
                var y = get_y(score);
                var pt1 = new PointF(xLine, y);
                var pt2 = new PointF(xLine - 10, y - 5);
                var pt3 = new PointF(xLine - 10, y + 5);
                e.Graphics.FillPolygon(Brushes.White, new[] { pt1, pt2, pt3 });
                e.Graphics.DrawPolygon(Pens.Gray, new[] { pt1, pt2, pt3 });
            }

            // Current init
            if (_fCurrent != int.MinValue)
            {
                var yCurrent = get_y(_fCurrent);
                var currentRect = new RectangleF(Border, yCurrent - Border, ClientRectangle.Width - Border * 2,
                    Border * 2);
                using (Brush b = new LinearGradientBrush(currentRect, Color.Blue, Color.DarkBlue,
                           LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(b, currentRect);
                    e.Graphics.DrawRectangle(Pens.Black, currentRect.X, currentRect.Y, currentRect.Width,
                        currentRect.Height);
                    e.Graphics.DrawString(_fCurrent.ToString(), Font, Brushes.White, currentRect, _centered);
                }
            }

            // Hovered init
            if (_fHoveredInit != int.MinValue && _fHoveredInit != _fCurrent)
            {
                var yHover = get_y(_fHoveredInit);
                var hoverRect = new RectangleF(Border, yHover - Border, ClientRectangle.Width - Border * 2, Border * 2);
                e.Graphics.FillRectangle(Brushes.White, hoverRect);
                e.Graphics.DrawRectangle(Pens.Gray, hoverRect.X, hoverRect.Y, hoverRect.Width, hoverRect.Height);
                e.Graphics.DrawString(_fHoveredInit.ToString(), Font, Brushes.Gray, hoverRect, _centered);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var mouse = PointToClient(Cursor.Position);
            _fCurrent = get_score(mouse.Y);

            OnInitiativeChanged();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var mouse = PointToClient(Cursor.Position);
            _fHoveredInit = get_score(mouse.Y);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _fHoveredInit = int.MinValue;
            Invalidate();
        }

        private Pair<int, int> get_range()
        {
            var min = int.MaxValue;
            var max = int.MinValue;

            foreach (var score in _fInitiatives)
            {
                min = Math.Min(min, score);
                max = Math.Max(max, score);
            }

            if (_fCurrent != int.MinValue)
            {
                min = Math.Min(min, _fCurrent);
                max = Math.Max(max, _fCurrent);
            }

            if (min == int.MaxValue)
                min = 0;

            if (max == int.MinValue)
                max = 20;

            if (min == max)
            {
                min -= 5;
                max += 5;
            }

            return new Pair<int, int>(min, max);
        }

        private float get_y(int score)
        {
            var rect = get_rect(score);
            return rect.Top + rect.Height / 2;
        }

        private RectangleF get_rect(int score)
        {
            var range = get_range();

            var count = range.Second - range.First + 1;

            var totalHeight = ClientRectangle.Height - 2 * Border;
            float height = totalHeight / count;

            var n = score - range.First;
            float y = ClientRectangle.Height - Border;
            y -= n * height;
            y -= height;

            return new RectangleF(0, y, ClientRectangle.Width, height);
        }

        private int get_score(int y)
        {
            var range = get_range();
            for (var score = range.First; score <= range.Second; ++score)
            {
                var rect = get_rect(score);
                if (rect.Top <= y && rect.Bottom >= y)
                    return score;
            }

            return int.MinValue;
        }
    }
}
