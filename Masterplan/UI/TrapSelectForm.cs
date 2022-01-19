using System;
using System.Collections;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TrapSelectForm : Form
    {
        public Trap Trap
        {
            get
            {
                if (TrapList.SelectedItems.Count != 0)
                    return TrapList.SelectedItems[0].Tag as Trap;

                return null;
            }
        }

        public TrapSelectForm()
        {
            InitializeComponent();

            TrapList.ListViewItemSorter = new TrapSorter();

            Application.Idle += Application_Idle;

            if (Session.Project != null)
            {
                var min = Math.Max(1, Session.Project.Party.Level - 4);
                var max = Session.Project.Party.Level + 5;
                LevelRangePanel.SetLevelRange(min, max);
            }

            update_list();

            Browser.DocumentText = "";
            TrapList_SelectedIndexChanged(null, null);
        }

        ~TrapSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = Trap != null;
        }

        private void TrapList_DoubleClick(object sender, EventArgs e)
        {
            if (Trap != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void TrapList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.Trap(Trap, null, true, false, false, Session.Preferences.TextSize);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void TrapList_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            var sorter = TrapList.ListViewItemSorter as TrapSorter;
            sorter.Set(e.Column);

            TrapList.Sort();
        }

        private void LevelRangePanel_RangeChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void update_list()
        {
            var traps = Session.Traps;

            TrapList.BeginUpdate();
            TrapList.Items.Clear();
            foreach (var trap in traps)
            {
                // Check level
                if (trap.Level < LevelRangePanel.MinimumLevel || trap.Level > LevelRangePanel.MaximumLevel)
                    continue;

                if (!Match(trap, LevelRangePanel.NameQuery))
                    continue;

                var lvi = TrapList.Items.Add(trap.Name);
                lvi.SubItems.Add(trap.Info);
                lvi.Group = TrapList.Groups[trap.Type == TrapType.Trap ? 0 : 1];
                lvi.Tag = trap;
            }

            TrapList.EndUpdate();
        }

        private bool Match(Trap trap, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(trap, token))
                    return false;

            return true;
        }

        private bool match_token(Trap trap, string token)
        {
            if (trap.Name.ToLower().Contains(token))
                return true;

            return false;
        }

        public class TrapSorter : IComparer
        {
            private bool _fAscending = true;
            private int _fColumn;

            public void Set(int column)
            {
                if (_fColumn == column)
                    _fAscending = !_fAscending;

                _fColumn = column;
            }

            public int Compare(object x, object y)
            {
                var lviX = x as ListViewItem;
                var lviY = y as ListViewItem;

                var result = 0;

                switch (_fColumn)
                {
                    case 0:
                    {
                        var lvsiX = lviX.SubItems[_fColumn];
                        var lvsiY = lviY.SubItems[_fColumn];

                        var strX = lvsiX.Text;
                        var strY = lvsiY.Text;

                        result = strX.CompareTo(strY);
                    }
                        break;
                    case 1:
                    {
                        var trapX = lviX.Tag as Trap;
                        var trapY = lviY.Tag as Trap;

                        var levelX = trapX.Level;
                        var levelY = trapY.Level;

                        result = levelX.CompareTo(levelY);
                    }
                        break;
                }

                if (!_fAscending)
                    result *= -1;

                return result;
            }
        }
    }
}
