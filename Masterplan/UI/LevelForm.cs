using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class LevelForm : Form
    {
        public int Level => (int)LevelBox.Value;

        public LevelForm(int score)
        {
            InitializeComponent();

            if (score == int.MinValue)
                score = 0;

            LevelBox.Value = score;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
