using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Masterplan.Controls
{
    internal partial class HitPointGauge : UserControl
    {
        private int _fDamage;

        private int _fFullHp;

        private int _fTempHp;

        public int FullHp
        {
            get => _fFullHp;
            set
            {
                _fFullHp = value;
                Invalidate();
            }
        }

        public int Damage
        {
            get => _fDamage;
            set
            {
                _fDamage = value;
                Invalidate();
            }
        }

        public int TempHp
        {
            get => _fTempHp;
            set
            {
                _fTempHp = value;
                Invalidate();
            }
        }

        public HitPointGauge()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_fFullHp == 0)
            {
                e.Graphics.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
                return;
            }

            var currentHp = _fFullHp - _fDamage;
            var bloodiedHp = _fFullHp / 2;

            var midpoint = (int)(Width * 0.8);

            var level0 = get_level(0);
            var levelBloodied = get_level(bloodiedHp);
            var levelFull = get_level(_fFullHp);
            var levelCurrent = get_level(currentHp);

            // Normal range bars
            if (_fFullHp != 0)
            {
                var normalRect = new Rectangle(midpoint, levelFull, Width - midpoint, level0 - levelFull);
                Brush normalBrush = new LinearGradientBrush(normalRect, Color.Black, Color.LightGray,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(normalBrush, normalRect);
            }

            // HP bar
            if (currentHp != 0)
            {
                var minHp = Math.Min(level0, levelCurrent);
                var maxHp = Math.Max(level0, levelCurrent);
                var currentRect = new Rectangle(0, minHp, midpoint, maxHp - minHp);
                Brush hpBrush = null;
                if (currentHp > bloodiedHp)
                    hpBrush = new LinearGradientBrush(currentRect, Color.Green, Color.DarkGreen,
                        LinearGradientMode.Vertical);
                else
                    hpBrush = new LinearGradientBrush(currentRect, Color.Red, Color.DarkRed,
                        LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(hpBrush, currentRect);
                e.Graphics.DrawRectangle(Pens.DarkGray, currentRect);
            }

            if (_fTempHp != 0)
            {
                var top = Math.Max(0, currentHp + _fTempHp);
                var levelTop = get_level(top);

                var bottom = top - _fTempHp;
                var levelBottom = get_level(bottom);

                // Temp HP bar
                var tempRect = new Rectangle(0, levelTop, midpoint, levelBottom - levelTop);
                Brush tempBrush =
                    new LinearGradientBrush(tempRect, Color.Blue, Color.Navy, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(tempBrush, tempRect);
                e.Graphics.DrawRectangle(Pens.DarkGray, tempRect);
            }

            if (_fFullHp != 0)
            {
                // Markers
                e.Graphics.DrawLine(Pens.DarkGray, 0, level0, midpoint, level0);
                e.Graphics.DrawLine(Pens.DarkGray, 0, levelBloodied, midpoint, levelBloodied);
                e.Graphics.DrawLine(Pens.DarkGray, 0, levelFull, midpoint, levelFull);
            }
        }

        private int get_level(int value)
        {
            var min = Math.Min(0, _fFullHp - _fDamage);
            var max = Math.Max(_fFullHp + _fTempHp - _fDamage, _fFullHp);

            var hpRange = max - min;
            if (hpRange == 0)
                return 0;

            var delta = max - value;

            return delta * Height / hpRange;
        }
    }
}
