using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Controls
{
    internal partial class DicePanel : UserControl
    {
        private const int DieSize = 32;
        private readonly StringFormat _centered = new StringFormat();
        private readonly List<Pair<int, int>> _fDice = new List<Pair<int, int>>();
        private int _fConstant;
        private bool _fUpdating;

        public DiceExpression Expression
        {
            get => DiceExpression.Parse(ExpressionBox.Text);
            set => ExpressionBox.Text = value != null ? value.ToString() : "";
        }

        public Pair<int, int> SelectedDie
        {
            get
            {
                if (DiceSourceList.SelectedItems.Count != 0)
                    return DiceSourceList.SelectedItems[0].Tag as Pair<int, int>;

                return null;
            }
        }

        public Pair<int, int> SelectedRoll
        {
            get
            {
                if (DiceList.SelectedItems.Count != 0)
                    return DiceList.SelectedItems[0].Tag as Pair<int, int>;

                return null;
            }
        }

        public DicePanel()
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _centered.Alignment = StringAlignment.Center;
            _centered.LineAlignment = StringAlignment.Center;
        }

        ~DicePanel()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RollBtn.Enabled = _fDice.Count != 0;
            ClearBtn.Enabled = _fDice.Count != 0;
            OddsBtn.Enabled = _fDice.Count != 0;
        }

        public void UpdateView()
        {
            update_dice_source();
            update_dice_rolls();
            update_dice_result();
        }

        private void RollBtn_Click(object sender, EventArgs e)
        {
            // Roll dice
            foreach (var die in _fDice)
            {
                var roll = Session.Dice(1, die.First);
                die.Second = roll;
            }

            _fDice.Sort(new DiceSorter());

            update_dice_rolls();
            update_dice_result();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            _fDice.Clear();
            _fConstant = 0;

            update_dice_rolls();
            update_dice_result();
        }

        private void OddsBtn_Click(object sender, EventArgs e)
        {
            var sides = new List<int>();
            foreach (var die in _fDice)
                sides.Add(die.First);

            var dlg = new OddsForm(sides, _fConstant, ExpressionBox.Text);
            dlg.ShowDialog();
        }

        private void DiceSourceList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedDie != null)
            {
                var fx = DoDragDrop(SelectedDie, DragDropEffects.Move);
                if (fx == DragDropEffects.Move)
                    add_die(SelectedDie.First);
            }
        }

        private void DiceList_DragOver(object sender, DragEventArgs e)
        {
            var die = e.Data.GetData(typeof(Pair<int, int>)) as Pair<int, int>;
            if (die != null)
                e.Effect = DragDropEffects.Move;
        }

        private void DiceSourceList_DoubleClick(object sender, EventArgs e)
        {
            // Add selected die
            if (SelectedDie != null)
                add_die(SelectedDie.First);
        }

        private void DiceList_DoubleClick(object sender, EventArgs e)
        {
            // Reroll selected die
            if (SelectedRoll != null)
            {
                SelectedRoll.Second = Session.Dice(1, SelectedRoll.First);
                update_dice_rolls();
                update_dice_result();
            }
        }

        private void ExpressionBox_TextChanged(object sender, EventArgs e)
        {
            if (_fUpdating)
                return;

            var exp = DiceExpression.Parse(ExpressionBox.Text);
            if (exp != null)
            {
                _fUpdating = true;

                ClearBtn_Click(sender, e);

                _fConstant = exp.Constant;

                for (var n = 0; n != exp.Throws; ++n)
                    add_die(exp.Sides);

                _fUpdating = false;
            }
        }

        private void update_dice_source()
        {
            DiceSourceList.Items.Clear();

            var sides = new List<int>();
            sides.Add(4);
            sides.Add(6);
            sides.Add(8);
            sides.Add(10);
            sides.Add(12);
            sides.Add(20);

            DiceSourceList.LargeImageList = new ImageList();
            DiceSourceList.LargeImageList.ImageSize = new Size(DieSize, DieSize);

            foreach (var die in sides)
            {
                var str = "d" + die;

                var lvi = DiceSourceList.Items.Add("");
                lvi.Tag = new Pair<int, int>(die, -1);

                DiceSourceList.LargeImageList.Images.Add(get_image(die, str));
                lvi.ImageIndex = DiceSourceList.LargeImageList.Images.Count - 1;
            }
        }

        private void update_dice_rolls()
        {
            DiceList.Items.Clear();

            DiceList.LargeImageList = new ImageList();
            DiceList.LargeImageList.ImageSize = new Size(DieSize, DieSize);

            var sides = new List<int>();

            foreach (var die in _fDice)
            {
                var lvi = DiceList.Items.Add("");
                lvi.Tag = die;

                DiceList.LargeImageList.Images.Add(get_image(die.First, die.Second.ToString()));
                lvi.ImageIndex = DiceList.LargeImageList.Images.Count - 1;

                sides.Add(die.First);
            }

            if (!_fUpdating)
            {
                _fUpdating = true;
                ExpressionBox.Text = _fDice.Count != 0 ? DiceStatistics.Expression(sides, _fConstant) : "";
                _fUpdating = false;
            }
        }

        private void update_dice_result()
        {
            if (_fDice.Count != 0)
            {
                var result = _fConstant;

                foreach (var die in _fDice)
                    result += die.Second;

                DiceLbl.ForeColor = SystemColors.WindowText;
                DiceLbl.Text = result.ToString();
            }
            else
            {
                DiceLbl.ForeColor = SystemColors.GrayText;
                DiceLbl.Text = "-";
            }
        }

        private void add_die(int sides)
        {
            var roll = Session.Dice(1, sides);

            _fDice.Add(new Pair<int, int>(sides, roll));
            _fDice.Sort(new DiceSorter());

            update_dice_rolls();
            update_dice_result();
        }

        private Image get_image(int sides, string caption)
        {
            var bmp = new Bitmap(DieSize, DieSize);

            var g = Graphics.FromImage(bmp);
            var rect = new RectangleF(0, 0, DieSize - 1, DieSize - 1);

            switch (sides)
            {
                case 4:
                {
                    var delta = rect.Width / 6;
                    var left = new PointF(rect.Left, rect.Bottom - delta);
                    var right = new PointF(rect.Right, rect.Bottom - delta);
                    var top = new PointF(rect.Left + rect.Width / 2, rect.Top);

                    g.FillPolygon(Brushes.LightGray, new[] { left, right, top });
                    g.DrawPolygon(Pens.Gray, new[] { left, right, top });
                }
                    break;
                case 6:
                {
                    var delta = rect.Width / 8;
                    var dieRect = new RectangleF(rect.X + delta, rect.Y + delta, rect.Width - 2 * delta,
                        rect.Height - 2 * delta);

                    g.FillRectangle(Brushes.LightGray, dieRect);
                    g.DrawRectangle(Pens.Gray, dieRect.X, dieRect.Y, dieRect.Width, dieRect.Height);
                }
                    break;
                case 8:
                {
                    var delta = rect.Width / 8;
                    var left = new PointF(rect.Left + delta, rect.Top + rect.Height / 2);
                    var right = new PointF(rect.Right - delta, rect.Top + rect.Height / 2);
                    var top = new PointF(rect.Left + rect.Width / 2, rect.Top);
                    var bottom = new PointF(rect.Left + rect.Width / 2, rect.Bottom);

                    g.FillPolygon(Brushes.LightGray, new[] { left, bottom, right, top });
                    g.DrawPolygon(Pens.Gray, new[] { left, bottom, right, top });
                }
                    break;
                case 10:
                {
                    var midX = rect.Left + rect.Width / 2;
                    var midY = rect.Top + rect.Height / 2;

                    var points = new List<PointF>();
                    for (var n = 0; n != 10; ++n)
                    {
                        var radius = rect.Width / 2;
                        var theta = n * (2 * Math.PI) / 10;

                        var dx = radius * Math.Sin(theta);
                        var dy = radius * Math.Cos(theta);

                        points.Add(new PointF((float)(midX + dx), (float)(midY + dy)));
                    }

                    g.FillPolygon(Brushes.LightGray, points.ToArray());
                    g.DrawPolygon(Pens.Gray, points.ToArray());
                }
                    break;
                case 12:
                {
                    var delta = rect.Width / 3;
                    var left = new PointF(rect.Left, rect.Top + rect.Height / 2);
                    var right = new PointF(rect.Right, rect.Top + rect.Height / 2);
                    var topleft = new PointF(rect.Left + delta, rect.Top);
                    var topright = new PointF(rect.Right - delta, rect.Top);
                    var bottomleft = new PointF(rect.Left + delta, rect.Bottom);
                    var bottomright = new PointF(rect.Right - delta, rect.Bottom);

                    g.FillPolygon(Brushes.LightGray, new[] { left, topleft, topright, right, bottomright, bottomleft });
                    g.DrawPolygon(Pens.Gray, new[] { left, topleft, topright, right, bottomright, bottomleft });
                }
                    break;
                case 20:
                {
                    var delta = rect.Width / 5;
                    var lefttop = new PointF(rect.Left, rect.Top + delta);
                    var leftbottom = new PointF(rect.Left, rect.Bottom - delta);
                    var righttop = new PointF(rect.Right, rect.Top + delta);
                    var rightbottom = new PointF(rect.Right, rect.Bottom - delta);
                    var top = new PointF(rect.Left + rect.Width / 2, rect.Top);
                    var bottom = new PointF(rect.Left + rect.Width / 2, rect.Bottom);

                    g.FillPolygon(Brushes.LightGray, new[] { lefttop, leftbottom, bottom, rightbottom, righttop, top });
                    g.DrawPolygon(Pens.Gray, new[] { lefttop, leftbottom, bottom, rightbottom, righttop, top });
                }
                    break;
            }

            g.DrawString(caption, Font, SystemBrushes.WindowText, rect, _centered);

            return bmp;
        }

        private class DiceSorter : IComparer<Pair<int, int>>
        {
            public int Compare(Pair<int, int> lhs, Pair<int, int> rhs)
            {
                var result = lhs.First.CompareTo(rhs.First);

                if (result == 0)
                    result = lhs.Second.CompareTo(rhs.Second);

                return result;
            }
        }
    }
}
