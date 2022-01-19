using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class EncounterGauge : UserControl
    {
        private const int ControlHeight = 20;

        private Party _fParty;

        private int _fXp;

        public Party Party
        {
            get => _fParty;
            set
            {
                _fParty = value;
                Invalidate();
            }
        }

        public int Xp
        {
            get => _fXp;
            set
            {
                _fXp = value;
                Invalidate();
            }
        }

        public EncounterGauge()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint
                     | ControlStyles.OptimizedDoubleBuffer
                     | ControlStyles.ResizeRedraw
                     | ControlStyles.UserPaint, true);

            Height = ControlHeight;
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);
            Height = ControlHeight;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Height = ControlHeight;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_fParty == null)
                return;

            var f = new Font(Font.FontFamily, 7);

            // Draw XP gauge
            const int deltaY = 4;
            var rect = new Rectangle(0, deltaY, get_x(_fXp), Height - 2 * deltaY);
            if (rect.Width > 0)
            {
                Brush b = new LinearGradientBrush(rect, SystemColors.Control, SystemColors.ControlDark,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(b, rect);
            }

            var minLvl = Math.Max(get_min_level(), 1);
            var maxLvl = get_max_level();

            for (var level = minLvl; level != maxLvl; ++level)
            {
                var xp = Experience.GetCreatureXp(level) * _fParty.Size;

                var x = get_x(xp);
                e.Graphics.DrawLine(Pens.Black, new Point(x, 1), new Point(x, Height - 3));
                e.Graphics.DrawString(level.ToString(), f, SystemBrushes.WindowText, new PointF(x, 1));
            }
        }

        private int get_min_level()
        {
            var currentLevel = Experience.GetCreatureLevel(_fXp / _fParty.Size);
            var min = Math.Min(_fParty.Level - 3, currentLevel);

            return Math.Max(min, 0);
        }

        private int get_max_level()
        {
            var currentLevel = Experience.GetCreatureLevel(_fXp / _fParty.Size);
            return Math.Max(_fParty.Level + 5, currentLevel + 1);
        }

        private int get_x(int xp)
        {
            var trivial = Experience.GetCreatureXp(get_min_level()) * _fParty.Size;
            var extreme = Experience.GetCreatureXp(get_max_level()) * _fParty.Size;

            var min = Math.Min(_fXp, trivial);
            var max = Math.Max(_fXp, extreme);
            var range = max - min;

            var delta = xp - min;
            return delta * Width / range;
        }
    }
}
