using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class PartyForm : Form
    {
        public Party Party { get; }

        public PartyForm(Party p)
        {
            InitializeComponent();

            Party = p;

            SizeBox.Value = Party.Size;
            LevelBox.Value = Party.Level;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Party.Size = (int)SizeBox.Value;
            Party.Level = (int)LevelBox.Value;
        }
    }
}
