using System;
using System.Windows.Forms;
using Masterplan.Tools;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionInformationForm : Form
    {
        public Pair<int, string> Information { get; }

        public OptionInformationForm(Pair<int, string> info)
        {
            InitializeComponent();

            Information = info;

            DCBox.Value = Information.First;
            DetailsBox.Text = Information.Second;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Information.First = (int)DCBox.Value;
            Information.Second = DetailsBox.Text;
        }
    }
}
