using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.Controls
{
    internal partial class BreakdownPanel : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();
        private Dictionary<Point, int> _cells;
        private List<string> _columns;
        private Dictionary<int, int> _columnTotals;

        private List<HeroRoleType> _rows;
        private Dictionary<int, int> _rowTotals;

        public List<Hero> Heroes { get; set; }

        public BreakdownPanel()
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

            if (Heroes == null)
            {
                e.Graphics.DrawString("(no heroes)", Font, SystemBrushes.WindowText, ClientRectangle, _centered);
                return;
            }

            analyse_party();

            var header = new Font(Font, FontStyle.Bold);

            for (var row = 0; row != _rows.Count + 1; ++row)
            {
                var str = "Total";
                if (row != _rows.Count)
                {
                    var role = _rows[row];
                    str = role.ToString();
                }

                // Draw row header cell
                var rowHeaderCell = get_rect(0, row + 1);
                e.Graphics.DrawString(str, header, SystemBrushes.WindowText, rowHeaderCell, _centered);
            }

            for (var col = 0; col != _columns.Count + 1; ++col)
            {
                var str = "Total";
                if (col != _columns.Count) str = _columns[col];

                // Draw col header cell
                var columnHeaderCell = get_rect(col + 1, 0);
                e.Graphics.DrawString(str, header, SystemBrushes.WindowText, columnHeaderCell, _centered);
            }

            for (var row = 0; row != _rows.Count; ++row)
            for (var col = 0; col != _columns.Count; ++col)
            {
                var heroes = _cells[new Point(row, col)];

                var rect = get_rect(col + 1, row + 1);
                e.Graphics.DrawString(heroes.ToString(), Font, SystemBrushes.WindowText, rect, _centered);
            }

            // Row totals
            for (var row = 0; row != _rows.Count; ++row)
            {
                var count = _rowTotals[row];

                // Draw row header cell
                var rowHeaderCell = get_rect(_columns.Count + 1, row + 1);
                e.Graphics.DrawString(count.ToString(), header, SystemBrushes.WindowText, rowHeaderCell, _centered);
            }

            // Column totals
            for (var col = 0; col != _columns.Count; ++col)
            {
                var count = _columnTotals[col];

                // Draw col header cell
                var columnHeaderCell = get_rect(col + 1, _rows.Count + 1);
                e.Graphics.DrawString(count.ToString(), header, SystemBrushes.WindowText, columnHeaderCell, _centered);
            }

            // Total
            var totalRect = get_rect(_columns.Count + 1, _rows.Count + 1);
            e.Graphics.DrawString(Heroes.Count.ToString(), header, SystemBrushes.WindowText, totalRect, _centered);

            var cellWidth = (float)ClientRectangle.Width / (_columns.Count + 2);
            var cellHeight = (float)ClientRectangle.Height / (_rows.Count + 2);

            var p = new Pen(SystemColors.ControlDark);

            // Draw horizontal lines
            for (var row = 0; row != _rows.Count + 1; ++row)
            {
                var y = (row + 1) * cellHeight;
                e.Graphics.DrawLine(p, new PointF(ClientRectangle.Left, y), new PointF(ClientRectangle.Right, y));
            }

            // Draw vertical lines
            for (var col = 0; col != _columns.Count + 1; ++col)
            {
                var x = (col + 1) * cellWidth;
                e.Graphics.DrawLine(p, new PointF(x, ClientRectangle.Top), new PointF(x, ClientRectangle.Bottom));
            }
        }

        private void analyse_party()
        {
            if (Heroes == null)
                return;

            // Rows
            _rows = new List<HeroRoleType>();
            foreach (HeroRoleType role in Enum.GetValues(typeof(HeroRoleType)))
                _rows.Add(role);

            // Columns
            _columns = new List<string>();

            Heroes.ForEach(h =>
            {
                if (!_columns.Contains(h.PowerSource))
                    _columns.Add(h.PowerSource);
            });

            _columns.Sort();

            _cells = new Dictionary<Point, int>();
            _rowTotals = new Dictionary<int, int>();
            _columnTotals = new Dictionary<int, int>();

            for (var row = 0; row != _rows.Count; ++row)
            {
                var role = _rows[row];

                if (!_rowTotals.ContainsKey(row))
                    _rowTotals[row] = 0;

                for (var col = 0; col != _columns.Count; ++col)
                {
                    var source = _columns[col];

                    // Get list of heroes for this cell
                    var heroes = Heroes.Count(card => card.Role == role && card.PowerSource == source);

                    _cells[new Point(row, col)] = heroes;

                    // Add to row total
                    _rowTotals[row] += heroes;

                    // Add to column total
                    if (!_columnTotals.ContainsKey(col))
                        _columnTotals[col] = 0;
                    _columnTotals[col] += heroes;
                }
            }
        }

        private RectangleF get_rect(int x, int y)
        {
            var width = (float)ClientRectangle.Width / (_columns.Count + 2);
            var height = (float)ClientRectangle.Height / (_rows.Count + 2);

            return new RectangleF(x * width, y * height, width, height);
        }
    }
}
