using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RegenerationForm : Form
    {
        public Regeneration Regeneration { get; }

        public RegenerationForm(Regeneration regen)
        {
            InitializeComponent();

            Regeneration = regen.Copy();

            ValueBox.Value = Regeneration.Value;
            DetailsBox.Text = Regeneration.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Regeneration.Value = (int)ValueBox.Value;
            Regeneration.Details = DetailsBox.Text;
        }
    }
}
