using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class AuraForm : Form
    {
        public Aura Aura { get; }

        public AuraForm(Aura aura)
        {
            InitializeComponent();

            Aura = aura.Copy();

            NameBox.Text = Aura.Name;
            KeywordBox.Text = Aura.Keywords;
            SizeBox.Value = Aura.Radius;
            DetailsBox.Text = Aura.Description;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Aura.Name = NameBox.Text;
            Aura.Keywords = KeywordBox.Text;
            Aura.Details = SizeBox.Value + ": " + DetailsBox.Text;
        }
    }
}
