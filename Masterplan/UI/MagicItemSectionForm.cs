using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MagicItemSectionForm : Form
    {
        public MagicItemSection Section { get; }

        public MagicItemSectionForm(MagicItemSection section)
        {
            InitializeComponent();

            HeaderBox.Items.Add("Price");
            HeaderBox.Items.Add("Enhancement");
            HeaderBox.Items.Add("Property");
            HeaderBox.Items.Add("Power");

            Section = section.Copy();

            HeaderBox.Text = Section.Header;
            DetailsBox.Text = Section.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Section.Header = HeaderBox.Text;
            Section.Details = DetailsBox.Text;
        }
    }
}
