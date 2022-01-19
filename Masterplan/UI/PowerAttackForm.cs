using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class PowerAttackForm : Form
    {
        private readonly bool _fFunctionalTemplate;
        private readonly int _fLevel;
        private readonly IRole _fRole;

        public PowerAttack Attack { get; }

        public PowerAttackForm(PowerAttack attack, bool functionalTemplate, int level, IRole role)
        {
            InitializeComponent();

            var defences = Enum.GetValues(typeof(DefenceType));
            foreach (DefenceType defence in defences)
                DefenceBox.Items.Add(defence);

            Application.Idle += Application_Idle;

            Attack = attack.Copy();
            _fFunctionalTemplate = functionalTemplate;
            _fLevel = level;
            _fRole = role;

            BonusBox.Value = Attack.Bonus;
            DefenceBox.SelectedItem = Attack.Defence;

            set_suggestion();

            if (!_fFunctionalTemplate)
            {
                InfoLbl.Visible = false;
                Height -= InfoLbl.Height;
            }
        }

        ~PowerAttackForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var dt = (DefenceType)DefenceBox.SelectedItem;
            var suggested = Statistics.AttackBonus(dt, _fLevel, _fRole);
            var current = (int)BonusBox.Value;

            SuggestBtn.Enabled = current != suggested;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Attack.Bonus = (int)BonusBox.Value;
            Attack.Defence = (DefenceType)DefenceBox.SelectedItem;
        }

        private void SuggestBtn_Click(object sender, EventArgs e)
        {
            var dt = (DefenceType)DefenceBox.SelectedItem;
            BonusBox.Value = Statistics.AttackBonus(dt, _fLevel, _fRole);
        }

        private void DefenceBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            set_suggestion();
        }

        private void set_suggestion()
        {
            var dt = (DefenceType)DefenceBox.SelectedItem;
            var bonus = Statistics.AttackBonus(dt, _fLevel, _fRole);
            SuggestBtn.Text = bonus >= 0 ? "+" + bonus : bonus.ToString();

            if (_fFunctionalTemplate)
                SuggestBtn.Text = "Level " + SuggestBtn.Text;
        }
    }
}
