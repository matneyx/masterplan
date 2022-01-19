using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CategoryListForm : Form
    {
        public List<string> Categories
        {
            get
            {
                if (CatList.CheckedItems.Count == CatList.Items.Count)
                    return null;

                var cats = new List<string>();

                foreach (ListViewItem lvi in CatList.CheckedItems)
                    cats.Add(lvi.Text);

                return cats;
            }
        }

        public CategoryListForm(List<string> categories)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            var bst = new BinarySearchTree<string>();
            foreach (var c in Session.Creatures)
                if (c.Category != "")
                    bst.Add(c.Category);

            var allCategories = bst.SortedList;

            var letters = new List<string>();
            foreach (var cat in allCategories)
            {
                var letter = cat.Substring(0, 1);
                if (!letters.Contains(letter))
                    letters.Add(letter);
            }

            foreach (var letter in letters)
                CatList.Groups.Add(letter, letter);

            foreach (var cat in allCategories)
            {
                var letter = cat.Substring(0, 1);

                var lvi = CatList.Items.Add(cat);
                lvi.Checked = categories == null || categories.Contains(cat);
                lvi.Group = CatList.Groups[letter];
            }
        }

        ~CategoryListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = CatList.CheckedItems.Count != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }

        private void SelectBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CatList.Items)
                lvi.Checked = true;
        }

        private void DeselectBtn_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvi in CatList.Items)
                lvi.Checked = false;
        }
    }
}
