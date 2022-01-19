using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class DiceGraphPanel : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();

        private readonly float _fRange = 0.5F;

        private int _fConstant;

        private List<int> _fDice = new List<int>();

        private Dictionary<int, int> _fDistribution;

        private string _title = "";

        public List<int> Dice
        {
            get => _fDice;
            set
            {
                _fDice = value;

                _fDistribution = null;
                Invalidate();
            }
        }

        public int Constant
        {
            get => _fConstant;
            set
            {
                _fConstant = value;

                _fDistribution = null;
                Invalidate();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                _title = value;

                Invalidate();
            }
        }

        public DiceGraphPanel()
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            try
            {
                if (_fDistribution == null)
                    _fDistribution = DiceStatistics.Odds(_fDice, _fConstant);

                if (_fDistribution == null || _fDistribution.Keys.Count == 0)
                    return;

                var deltaX = Width / 10;
                var deltaY = Height / 10;
                var rect = new Rectangle(deltaX, 3 * deltaY, Width - 2 * deltaX, Height - 5 * deltaY);

                if (_title != null && _title != "")
                {
                    // Draw graph title
                    var titleRect = new Rectangle(rect.X, rect.Y - 2 * deltaY, rect.Width, deltaY);
                    e.Graphics.FillRectangle(Brushes.White, titleRect);
                    e.Graphics.DrawRectangle(Pens.DarkGray, titleRect);
                    e.Graphics.DrawString(_title, new Font(Font.FontFamily, deltaY / 3), Brushes.Black, titleRect,
                        _centered);
                }

                var minX = int.MaxValue;
                var maxX = int.MinValue;
                var maxY = int.MinValue;
                var sum = 0;
                foreach (var roll in _fDistribution.Keys)
                {
                    minX = Math.Min(minX, roll);
                    maxX = Math.Max(maxX, roll);

                    maxY = Math.Max(maxY, _fDistribution[roll]);
                    sum += _fDistribution[roll];
                }

                var lowerDelta = (1 - _fRange) / 2;
                var upperDelta = 1 - lowerDelta;

                var mouse = PointToClient(Cursor.Position);

                var range = maxX - minX + 1;
                var width = (float)rect.Width / range;

                var size = Math.Min(Font.Size, width / 2);
                var labelFont = new Font(Font.FontFamily, size);

                var levels = new List<PointF>();
                var integral = 0;
                foreach (var roll in _fDistribution.Keys)
                {
                    var index = roll - minX;
                    var x = width * index;
                    float height = rect.Height * (maxY - _fDistribution[roll]) / maxY;

                    var rollRect = new RectangleF(x + rect.X, rect.Y + height, width, rect.Height - height);

                    integral += _fDistribution[roll];
                    var fraction = (float)integral / sum;

                    var highlighted = rollRect.Contains(mouse);
                    var interQuartile = fraction >= lowerDelta && fraction <= upperDelta;
                    interQuartile = false;

                    var midpoint = x + rect.X + width / 2;
                    var y = rect.Y + height;
                    levels.Add(new PointF(midpoint, y));

                    var pen = Pens.Gray;
                    if (interQuartile || highlighted)
                        pen = Pens.Black;

                    e.Graphics.DrawLine(pen, midpoint, rect.Bottom, midpoint, y);

                    var labelRect = new RectangleF(rollRect.Left, rollRect.Bottom, width, deltaY);
                    e.Graphics.DrawString(roll.ToString(), labelFont, highlighted ? Brushes.Black : Brushes.DarkGray,
                        labelRect, _centered);
                }

                // Draw x-axis
                e.Graphics.DrawLine(Pens.Black, rect.Left, rect.Bottom, rect.Right, rect.Bottom);

                // Draw curve
                for (var n = 1; n < levels.Count; ++n)
                    e.Graphics.DrawLine(new Pen(Color.Red, 2F), levels[n - 1], levels[n]);
            }
            catch
            {
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Invalidate();
        }
    }
}
