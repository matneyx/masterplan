using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TokenLinkForm : Form
    {
        public TokenLink Link { get; }

        public TokenLinkForm(TokenLink link)
        {
            InitializeComponent();

            LinkTextBox.Items.Add("Marked");
            LinkTextBox.Items.Add("Oath");
            LinkTextBox.Items.Add("Quarry");
            LinkTextBox.Items.Add("Curse");
            LinkTextBox.Items.Add("Shroud");
            LinkTextBox.Items.Add("Dominated");
            LinkTextBox.Items.Add("Sanctioned");

            Link = link.Copy();

            LinkTextBox.Text = Link.Text;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Link.Text = LinkTextBox.Text;
        }
    }
}
