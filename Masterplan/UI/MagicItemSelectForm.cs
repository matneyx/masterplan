using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MagicItemSelectForm : Form
    {
        public MagicItem MagicItem
        {
            get
            {
                if (ItemList.SelectedItems.Count != 0)
                    return ItemList.SelectedItems[0].Tag as MagicItem;

                return null;
            }
        }

        public MagicItemSelectForm(int level)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            if (level > 0)
                LevelRangePanel.SetLevelRange(level, level);

            Browser.DocumentText = "";
            ItemList_SelectedIndexChanged(null, null);

            update_list();
        }

        ~MagicItemSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = MagicItem != null;
        }

        private void ItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.MagicItem(MagicItem, Session.Preferences.TextSize, false, true);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }

        private void ItemList_DoubleClick(object sender, EventArgs e)
        {
            if (MagicItem != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void LevelRangePanel_RangeChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void update_list()
        {
            var selection = new List<MagicItem>();

            var items = Session.MagicItems;
            foreach (var item in items)
                if (item.Level >= LevelRangePanel.MinimumLevel && item.Level <= LevelRangePanel.MaximumLevel &&
                    Match(item, LevelRangePanel.NameQuery))
                    selection.Add(item);

            var bst = new BinarySearchTree<string>();
            foreach (var item in selection)
                if (item.Type != "")
                    bst.Add(item.Type);

            var cats = bst.SortedList;
            cats.Add("Miscellaneous Items");
            foreach (var cat in cats)
                ItemList.Groups.Add(cat, cat);

            var listItems = new List<ListViewItem>();
            foreach (var item in selection)
            {
                var lvi = new ListViewItem(item.Name);
                lvi.SubItems.Add(item.Info);
                lvi.Tag = item;

                if (item.Type != "")
                    lvi.Group = ItemList.Groups[item.Type];
                else
                    lvi.Group = ItemList.Groups["Miscellaneous Items"];

                listItems.Add(lvi);
            }

            ItemList.BeginUpdate();
            ItemList.Items.Clear();
            ItemList.Items.AddRange(listItems.ToArray());
            ItemList.EndUpdate();
        }

        private bool Match(MagicItem item, string query)
        {
            var tokens = query.ToLower().Split();

            foreach (var token in tokens)
                if (!match_token(item, token))
                    return false;

            return true;
        }

        private bool match_token(MagicItem item, string token)
        {
            if (item.Name.ToLower().Contains(token))
                return true;

            return false;
        }
    }
}
