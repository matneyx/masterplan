using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class EncyclopediaGroupForm : Form
    {
        public EncyclopediaGroup Group { get; }

        public EncyclopediaGroupForm(EncyclopediaGroup group)
        {
            InitializeComponent();

            Group = group.Copy();

            TitleBox.Text = Group.Name;

            foreach (var ee in Session.Project.Encyclopedia.Entries)
            {
                var lvi = EntryList.Items.Add(ee.Name);
                lvi.Tag = ee;
                lvi.Checked = Group.EntryIDs.Contains(ee.Id);
            }

            if (EntryList.Items.Count == 0)
            {
                var lvi = EntryList.Items.Add("(no entries)");
                lvi.ForeColor = SystemColors.GrayText;

                EntryList.CheckBoxes = false;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Group.Name = TitleBox.Text;

            Group.EntryIDs.Clear();
            foreach (ListViewItem lvi in EntryList.CheckedItems)
            {
                var ee = lvi.Tag as EncyclopediaEntry;
                Group.EntryIDs.Add(ee.Id);
            }
        }
    }
}
