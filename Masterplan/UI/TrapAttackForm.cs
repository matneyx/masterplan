using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TrapAttackForm : Form
    {
        private readonly bool _fElite;

        private readonly int _fLevel = 1;

        public TrapAttack Attack { get; }

        public TrapAttackForm(TrapAttack attack, int level, bool elite)
        {
            InitializeComponent();

            var actions = Enum.GetValues(typeof(ActionType));
            foreach (ActionType action in actions)
                ActionBox.Items.Add(action);

            Application.Idle += Application_Idle;

            Attack = attack.Copy();
            _fLevel = level;
            _fElite = elite;

            TriggerBox.Text = Attack.Trigger;
            ActionBox.SelectedItem = Attack.Action;
            RangeBox.Text = Attack.Range;
            TargetBox.Text = Attack.Target;
            InitBtn.Checked = Attack.HasInitiative;
            InitBox.Value = Attack.Initiative;
            AttackBtn.Text = Attack.Attack.ToString();
            HitBox.Text = Attack.OnHit;
            MissBox.Text = Attack.OnMiss;
            EffectBox.Text = Attack.Effect;

            update_advice();
        }

        ~TrapAttackForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            InitBox.Enabled = InitBtn.Checked;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Attack.Trigger = TriggerBox.Text;
            Attack.Action = (ActionType)ActionBox.SelectedItem;
            Attack.Range = RangeBox.Text;
            Attack.Target = TargetBox.Text;
            Attack.HasInitiative = InitBtn.Checked;
            Attack.Initiative = (int)InitBox.Value;
            Attack.OnHit = HitBox.Text;
            Attack.OnMiss = MissBox.Text;
            Attack.Effect = EffectBox.Text;
        }

        private void AttackBtn_Click(object sender, EventArgs e)
        {
            var dlg = new PowerAttackForm(Attack.Attack, false, 0, null);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Attack.Attack = dlg.Attack;
                AttackBtn.Text = Attack.Attack.ToString();
            }
        }

        private void update_advice()
        {
            var init = 2;
            var attackAc = _fLevel + 5;
            var attackNad = _fLevel + 3;

            if (_fElite)
            {
                init += 2;
                attackAc += 2;
                attackNad += 2;
            }

            // Init
            var lviInit = AdviceList.Items.Add("Initiative");
            lviInit.SubItems.Add("+" + init);
            lviInit.Group = AdviceList.Groups[0];

            // Attack vs AC
            var lviAttAc = AdviceList.Items.Add("Attack vs AC");
            lviAttAc.SubItems.Add("+" + attackAc);
            lviAttAc.Group = AdviceList.Groups[1];

            // Attack vs NAD
            var lviAttNad = AdviceList.Items.Add("Attack vs other defence");
            lviAttNad.SubItems.Add("+" + attackNad);
            lviAttNad.Group = AdviceList.Groups[1];
        }
    }
}
