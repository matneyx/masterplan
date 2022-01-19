using System;
using System.Windows.Forms;

namespace Masterplan.UI
{
    internal partial class CreatureSkillForm : Form
    {
        private readonly int _fAbility;
        private readonly int _fLevel;

        public bool Trained => TrainedBox.Checked;
        public int Misc => (int)MiscBox.Value;

        public CreatureSkillForm(string skillName, string abilityName, int abilityBonus, int levelBonus, bool trained,
            int miscBonus)
        {
            InitializeComponent();

            _fAbility = abilityBonus;
            _fLevel = levelBonus;

            Text = skillName;
            AbilityNameLbl.Text = abilityName + " bonus:";
            AbilityBonusLbl.Text = _fAbility.ToString();
            LevelBonusLbl.Text = _fLevel.ToString();
            TrainedBox.Checked = trained;
            MiscBox.Value = miscBonus;

            update_total();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }

        private void update_total()
        {
            var training = TrainedBox.Checked ? 5 : 0;
            var total = _fAbility + _fLevel + training + Misc;

            TrainingBonusLbl.Text = TrainedBox.Checked ? "5" : "";
            TotalBonusLbl.Text = total.ToString();
        }

        private void TrainedBox_CheckedChanged(object sender, EventArgs e)
        {
            update_total();
        }

        private void MiscBox_ValueChanged(object sender, EventArgs e)
        {
            update_total();
        }
    }
}
