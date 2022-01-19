using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class PowerInfoForm : Form
    {
        public string PowerName => NameBox.Text;

        public string PowerKeywords => KeywordBox.Text;

        public PowerInfoForm(CreaturePower power)
        {
            InitializeComponent();

            NameBox.Text = power.Name;
            KeywordBox.Text = power.Keywords;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
