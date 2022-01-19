using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class InitiativeForm : Form
    {
        public int Score => (int)InitBox.Value;

        public InitiativeForm(int bonus, int score)
        {
            InitializeComponent();

            if (bonus >= 0)
                BonusValueLbl.Text = "+" + bonus;
            else
                BonusValueLbl.Text = bonus.ToString();

            if (score == int.MinValue)
                score = bonus + 1;

            InitBox.Value = score;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }

        private void InitiativeForm_Shown(object sender, EventArgs e)
        {
            var length = 1;
            if (InitBox.Value >= 10)
                length = 2;
            if (InitBox.Value >= 100)
                length = 3;

            InitBox.Select(0, length);
        }
    }
}
