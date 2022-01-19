using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class CategoryForm : Form
    {
        public string Category => CategoryBox.Text;

        public CategoryForm(List<string> categories, string selectedCategory)
        {
            InitializeComponent();

            foreach (var cat in categories)
                CategoryBox.Items.Add(cat);

            CategoryBox.Text = selectedCategory;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
