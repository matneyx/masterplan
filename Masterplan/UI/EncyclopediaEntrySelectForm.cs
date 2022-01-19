using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EncyclopediaEntrySelectForm : Form
    {
        public EncyclopediaEntry EncyclopediaEntry
        {
            get
            {
                if (EntryList.SelectedItems.Count != 0)
                    return EntryList.SelectedItems[0].Tag as EncyclopediaEntry;

                return null;
            }
        }

        public EncyclopediaEntrySelectForm(List<Guid> ignoreIds)
        {
            InitializeComponent();

            var bst = new BinarySearchTree<string>();
            foreach (var entry in Session.Project.Encyclopedia.Entries)
                if (entry.Category != null && entry.Category != "")
                    bst.Add(entry.Category);
            var categories = bst.SortedList;
            categories.Insert(0, "Miscellaneous Entries");

            foreach (var cat in categories)
                EntryList.Groups.Add(new ListViewGroup(cat, cat));

            foreach (var entry in Session.Project.Encyclopedia.Entries)
            {
                if (ignoreIds.Contains(entry.Id))
                    continue;

                var lvi = EntryList.Items.Add(entry.Name);
                lvi.Tag = entry;

                if (entry.Category != null && entry.Category != "")
                    lvi.Group = EntryList.Groups[entry.Category];
                else
                    lvi.Group = EntryList.Groups["Miscellaneous Entries"];
            }

            if (EntryList.Items.Count == 0)
            {
                var lvi = EntryList.Items.Add("(no entries)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            Application.Idle += Application_Idle;
        }

        ~EncyclopediaEntrySelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = EncyclopediaEntry != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (EncyclopediaEntry != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
