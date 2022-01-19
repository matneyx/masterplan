using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.Controls
{
    internal partial class KeyAbilitiesPanel : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();
        private Dictionary<string, int> _fBreakdown;

        public KeyAbilitiesPanel()
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

        public void Analyse(SkillChallenge sc)
        {
            _fBreakdown = new Dictionary<string, int>();

            _fBreakdown["Strength"] = 0;
            _fBreakdown["Constitution"] = 0;
            _fBreakdown["Dexterity"] = 0;
            _fBreakdown["Intelligence"] = 0;
            _fBreakdown["Wisdom"] = 0;
            _fBreakdown["Charisma"] = 0;

            foreach (var scd in sc.Skills)
            {
                if (scd.Type == SkillType.AutoFail)
                    continue;

                var ability = "";

                if (Skills.GetAbilityNames().Contains(scd.SkillName))
                    ability = scd.SkillName;
                else
                    ability = Skills.GetKeyAbility(scd.SkillName);

                if (!_fBreakdown.ContainsKey(ability))
                    continue;

                _fBreakdown[ability] += 1;
            }

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);

            if (_fBreakdown == null)
                return;

            var maxCount = 0;
            foreach (var ability in _fBreakdown.Keys)
            {
                var count = _fBreakdown[ability];
                maxCount = Math.Max(count, maxCount);
            }

            var border = 20;
            var rect = new Rectangle(border, border, ClientRectangle.Width - 2 * border,
                ClientRectangle.Height - 3 * border);
            var barWidth = (float)rect.Width / 6;

            for (var columnIndex = 0; columnIndex != 6; ++columnIndex)
            {
                var label = get_label(columnIndex);
                if (label == "")
                    continue;

                var x = barWidth * columnIndex;
                var labelRect = new RectangleF(rect.Left + x, rect.Bottom, barWidth, border);
                e.Graphics.DrawString(label, Font, Brushes.Black, labelRect, _centered);

                var count = get_count(columnIndex);
                if (count == 0)
                    continue;

                var height = (rect.Height - border) * count / maxCount;

                var bar = new RectangleF(rect.Left + x, rect.Bottom - height, barWidth, height);
                using (Brush barFill = new LinearGradientBrush(ClientRectangle, Color.LightGray, Color.White,
                           LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(barFill, bar);
                }

                e.Graphics.DrawRectangle(Pens.Gray, bar.X, bar.Y, bar.Width, bar.Height);

                var countRect = new RectangleF(rect.Left + x, rect.Top, barWidth, border);
                e.Graphics.DrawString(count.ToString(), Font, Brushes.Gray, countRect, _centered);
            }

            e.Graphics.DrawLine(Pens.Black, rect.Left, rect.Bottom, rect.Left, rect.Top);
            e.Graphics.DrawLine(Pens.Black, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
        }

        private string get_label(int columnIndex)
        {
            switch (columnIndex)
            {
                case 0:
                    return "Strength";
                case 1:
                    return "Constitution";
                case 2:
                    return "Dexterity";
                case 3:
                    return "Intelligence";
                case 4:
                    return "Wisdom";
                case 5:
                    return "Charisma";
            }

            return "";
        }

        private int get_count(int columnIndex)
        {
            var column = get_label(columnIndex);
            return _fBreakdown[column];
        }
    }
}
