using System;
using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ArtifactConcordanceForm : Form
    {
        public Pair<string, string> Concordance { get; }

        public ArtifactConcordanceForm(Pair<string, string> concordance)
        {
            InitializeComponent();

            Concordance = concordance;

            RuleBox.Text = Concordance.First;
            ValueBox.Text = Concordance.Second;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Concordance.First = RuleBox.Text;
            Concordance.Second = ValueBox.Text;
        }
    }
}
