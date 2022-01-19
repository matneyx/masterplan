using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.Controls
{
    internal partial class DeckGrid : UserControl
    {
        private readonly StringFormat _centered = new StringFormat();

        private Dictionary<Point, int> _fCells;
        private List<Difficulty> _fColumns;
        private Dictionary<int, int> _fColumnTotals;

        private EncounterDeck _fDeck;

        private Point _fHoverCell = Point.Empty;

        private List<CardCategory> _fRows;
        private Dictionary<int, int> _fRowTotals;

        private Point _fSelectedCell = Point.Empty;

        public EncounterDeck Deck
        {
            get => _fDeck;
            set
            {
                _fDeck = value;
                _fSelectedCell = Point.Empty;

                Invalidate();
            }
        }

        public bool IsCellSelected => _fSelectedCell != Point.Empty;

        public DeckGrid()
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

        public bool InSelectedCell(EncounterCard card)
        {
            if (_fSelectedCell == Point.Empty)
                return false;

            var diffIndex = _fSelectedCell.X - 1;
            var diff = _fColumns[diffIndex];

            var catIndex = _fSelectedCell.Y - 1;
            var cat = _fRows[catIndex];

            return card.Category == cat && card.GetDifficulty(_fDeck.Level) == diff;
        }

        public event EventHandler SelectedCellChanged;

        protected void OnSelectedCellChanged()
        {
            SelectedCellChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler CellActivated;

        protected void OnCellActivated()
        {
            CellActivated?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, ClientRectangle);

            if (_fDeck == null)
            {
                e.Graphics.DrawString("(no deck)", Font, SystemBrushes.WindowText, ClientRectangle, _centered);
                return;
            }

            analyse_deck();

            var cellwidth = (float)ClientRectangle.Width / (_fColumns.Count + 2);
            var cellheight = (float)ClientRectangle.Height / (_fRows.Count + 2);

            using (var p = new Pen(SystemColors.ControlDark))
            {
                // Draw horizontal lines
                for (var row = 0; row != _fRows.Count + 1; ++row)
                {
                    var y = (row + 1) * cellheight;
                    e.Graphics.DrawLine(p, new PointF(ClientRectangle.Left, y), new PointF(ClientRectangle.Right, y));
                }

                // Draw vertical lines
                for (var col = 0; col != _fColumns.Count + 1; ++col)
                {
                    var x = (col + 1) * cellwidth;
                    e.Graphics.DrawLine(p, new PointF(x, ClientRectangle.Top), new PointF(x, ClientRectangle.Bottom));
                }
            }

            e.Graphics.FillRectangle(Brushes.Black, get_rect(0, 0));

            for (var index = 1; index != _fColumns.Count + 2; ++index)
            {
                var cellrect = get_rect(index, 0);
                e.Graphics.FillRectangle(Brushes.Black, cellrect);
            }

            for (var index = 1; index != _fRows.Count + 2; ++index)
            {
                var cellrect = get_rect(0, index);
                e.Graphics.FillRectangle(Brushes.Black, cellrect);
            }

            using (Brush b = new SolidBrush(Color.FromArgb(30, Color.Gray)))
            {
                for (var index = 1; index != _fColumns.Count + 1; ++index)
                {
                    var cellrect = get_rect(index, _fRows.Count + 1);
                    e.Graphics.FillRectangle(b, cellrect);
                }

                for (var index = 1; index != _fRows.Count + 1; ++index)
                {
                    var cellrect = get_rect(_fColumns.Count + 1, index);
                    e.Graphics.FillRectangle(b, cellrect);
                }
            }

            // Draw highlighted cell
            if (_fHoverCell != Point.Empty)
                if (_fHoverCell.X <= _fColumns.Count && _fHoverCell.Y <= _fRows.Count)
                {
                    var cellrect = get_rect(_fHoverCell.X, _fHoverCell.Y);
                    e.Graphics.DrawRectangle(SystemPens.Highlight, cellrect.X, cellrect.Y, cellrect.Width,
                        cellrect.Height);

                    using (Brush b = new SolidBrush(Color.FromArgb(30, SystemColors.Highlight)))
                    {
                        e.Graphics.FillRectangle(b, cellrect);
                    }
                }

            // Draw selected cell
            if (_fSelectedCell != Point.Empty)
            {
                var cellrect = get_rect(_fSelectedCell.X, _fSelectedCell.Y);
                using (Brush b = new SolidBrush(Color.FromArgb(100, SystemColors.Highlight)))
                {
                    e.Graphics.FillRectangle(b, cellrect);
                }
            }

            var header = new Font(Font, FontStyle.Bold);

            for (var row = 0; row != _fRows.Count + 1; ++row)
            {
                var str = "Total";
                if (row != _fRows.Count)
                {
                    var cat = _fRows[row];
                    str = cat.ToString();

                    if (cat == CardCategory.SoldierBrute)
                        str = "Sldr / Brute";
                }

                // Draw row header cell
                var rowhdr = get_rect(0, row + 1);
                e.Graphics.DrawString(str, header, Brushes.White, rowhdr, _centered);
            }

            for (var col = 0; col != _fColumns.Count + 1; ++col)
            {
                var str = "Total";
                if (col != _fColumns.Count)
                    switch (_fColumns[col])
                    {
                        case Difficulty.Trivial:
                            str = "Lower";
                            break;
                        case Difficulty.Easy:
                            var min = Math.Max(1, _fDeck.Level - 1);
                            str = "Lvl " + min + " to " + (_fDeck.Level + 1);
                            break;
                        case Difficulty.Moderate:
                            str = "Lvl " + (_fDeck.Level + 2) + " to " + (_fDeck.Level + 3);
                            break;
                        case Difficulty.Hard:
                            str = "Lvl " + (_fDeck.Level + 4) + " to " + (_fDeck.Level + 5);
                            break;
                        case Difficulty.Extreme:
                            str = "Higher";
                            break;
                    }

                // Draw col header cell
                var columnHeaderCell = get_rect(col + 1, 0);
                e.Graphics.DrawString(str, header, Brushes.White, columnHeaderCell, _centered);
            }

            for (var row = 0; row != _fRows.Count; ++row)
            for (var col = 0; col != _fColumns.Count; ++col)
            {
                var pt = new Point(row, col);

                var count = _fCells[pt];
                if (count == 0)
                    continue;

                var rect = get_rect(col + 1, row + 1);
                e.Graphics.DrawString(count.ToString(), Font, SystemBrushes.WindowText, rect, _centered);
            }

            // Row totals
            for (var row = 0; row != _fRows.Count; ++row)
            {
                var cat = _fRows[row];
                var count = _fRowTotals[row];

                var suggested = 0;
                switch (cat)
                {
                    case CardCategory.Artillery:
                        suggested = 5;
                        break;
                    case CardCategory.Controller:
                        suggested = 5;
                        break;
                    case CardCategory.Lurker:
                        suggested = 2;
                        break;
                    case CardCategory.Skirmisher:
                        suggested = 14;
                        break;
                    case CardCategory.SoldierBrute:
                        suggested = 18;
                        break;
                    case CardCategory.Minion:
                        suggested = 5;
                        break;
                    case CardCategory.Solo:
                        suggested = 1;
                        break;
                }

                // Draw row header cell
                var rowhdr = get_rect(_fColumns.Count + 1, row + 1);
                e.Graphics.DrawString(count + " (" + suggested + ")", header, SystemBrushes.WindowText, rowhdr,
                    _centered);
            }

            // Column totals
            for (var col = 0; col != _fColumns.Count; ++col)
            {
                var count = _fColumnTotals[col];

                // Draw col header cell
                var columnHeaderCell = get_rect(col + 1, _fRows.Count + 1);
                e.Graphics.DrawString(count.ToString(), header, SystemBrushes.WindowText, columnHeaderCell, _centered);
            }

            // Total
            var totalRect = get_rect(_fColumns.Count + 1, _fRows.Count + 1);
            e.Graphics.DrawString(_fDeck.Cards.Count + " cards", header, SystemBrushes.WindowText, totalRect,
                _centered);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_fColumns == null || _fRows == null)
                return;

            var width = (float)ClientRectangle.Width / (_fColumns.Count + 2);
            var height = (float)ClientRectangle.Height / (_fRows.Count + 2);

            var pt = PointToClient(Cursor.Position);
            var x = (int)(pt.X / width);
            var y = (int)(pt.Y / height);

            if (x == 0 || y == 0)
            {
                _fHoverCell = Point.Empty;
                Invalidate();
            }
            else if (x != _fHoverCell.X || y != _fHoverCell.Y)
            {
                _fHoverCell = new Point(x, y);
                Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            _fHoverCell = Point.Empty;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            _fSelectedCell = _fHoverCell;

            if (_fSelectedCell.X > _fColumns.Count || _fSelectedCell.Y > _fRows.Count)
                _fSelectedCell = Point.Empty;

            Invalidate();

            OnSelectedCellChanged();
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            OnCellActivated();
        }

        private void analyse_deck()
        {
            if (_fDeck == null)
                return;

            // Rows
            _fRows = new List<CardCategory>();
            var cats = Enum.GetValues(typeof(CardCategory));
            foreach (CardCategory cat in cats)
                _fRows.Add(cat);

            // Columns
            _fColumns = new List<Difficulty>();
            var diffs = Enum.GetValues(typeof(Difficulty));
            foreach (Difficulty diff in diffs)
            {
                if (diff == Difficulty.Trivial && _fDeck.Level < 3)
                    continue;

                if (diff == Difficulty.Random)
                    continue;

                _fColumns.Add(diff);
            }

            _fCells = new Dictionary<Point, int>();

            _fRowTotals = new Dictionary<int, int>();
            _fColumnTotals = new Dictionary<int, int>();

            for (var row = 0; row != _fRows.Count; ++row)
            {
                var cat = _fRows[row];

                for (var col = 0; col != _fColumns.Count; ++col)
                {
                    var diff = _fColumns[col];

                    // Get list of cards for this cell
                    var count = 0;
                    foreach (var card in _fDeck.Cards)
                        if (card.Category == cat && card.GetDifficulty(_fDeck.Level) == diff)
                            count += 1;
                    _fCells[new Point(row, col)] = count;

                    // Add to row total
                    if (!_fRowTotals.ContainsKey(row))
                        _fRowTotals[row] = 0;
                    _fRowTotals[row] += count;

                    // Add to column total
                    if (!_fColumnTotals.ContainsKey(col))
                        _fColumnTotals[col] = 0;
                    _fColumnTotals[col] += count;
                }
            }
        }

        private RectangleF get_rect(int x, int y)
        {
            var width = (float)ClientRectangle.Width / (_fColumns.Count + 2);
            var height = (float)ClientRectangle.Height / (_fRows.Count + 2);

            return new RectangleF(x * width, y * height, width, height);
        }
    }
}
