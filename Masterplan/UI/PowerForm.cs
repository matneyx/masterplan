using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class PowerForm : Form
    {
        private readonly bool _fFunctionalTemplate;

        private int _fLevel;
        private IRole _fRole;

        public CreaturePower Power { get; }

        public PowerForm(CreaturePower power, bool functionalTemplate, bool unused)
        {
            InitializeComponent();

            Pages.TabPages.Remove(AdvicePage);

            RangeBox.Items.Add("Melee");
            RangeBox.Items.Add("Melee Touch");
            RangeBox.Items.Add("Melee Weapon");
            RangeBox.Items.Add("Melee N");
            RangeBox.Items.Add("Reach N");
            RangeBox.Items.Add("Ranged N");
            RangeBox.Items.Add("Close Blast N");
            RangeBox.Items.Add("Close Burst N");
            RangeBox.Items.Add("Area Burst N within N");
            RangeBox.Items.Add("Personal");

            Power = power.Copy();
            _fFunctionalTemplate = functionalTemplate;

            NameBox.Text = Power.Name;
            KeywordBox.Text = Power.Keywords;
            ConditionBox.Text = Power.Condition;
            update_action();
            update_attack();
            RangeBox.Text = Power.Range;
            DetailsBox.Text = Power.Details;
            DescBox.Text = Power.Description;
        }

        public void ShowAdvicePage(int level, IRole role)
        {
            _fLevel = level;
            _fRole = role;

            Pages.TabPages.Add(AdvicePage);
            update_advice();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Power.Name = NameBox.Text;
            Power.Keywords = KeywordBox.Text;
            Power.Condition = ConditionBox.Text;
            Power.Range = RangeBox.Text;
            Power.Details = DetailsBox.Text;
            Power.Description = DescBox.Text;
        }

        private void ActionBtn_Click(object sender, EventArgs e)
        {
            var pa = Power.Action;
            if (pa == null)
                pa = new PowerAction();

            var dlg = new PowerActionForm(pa);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Power.Action = dlg.Action;
                update_action();
                update_advice();
            }
        }

        private void AttackBtn_Click(object sender, EventArgs e)
        {
            var pa = Power.Attack;
            if (pa == null)
                pa = new PowerAttack();

            var dlg = new PowerAttackForm(pa, _fFunctionalTemplate, 0, null);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Power.Attack = dlg.Attack;
                update_attack();
                update_advice();
            }
        }

        private void ActionClearLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Power.Action = null;

            update_action();
            update_advice();
        }

        private void AttackClearLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Power.Attack = null;

            update_attack();
            update_advice();
        }

        private void update_action()
        {
            ActionBtn.Text = Power.Action != null ? Power.Action.ToString() : "(not defined)";
            ActionClearLbl.Enabled = Power.Action != null;
        }

        private void update_attack()
        {
            AttackBtn.Text = Power.Attack != null ? Power.Attack.ToString() : "(not defined)";
            AttackClearLbl.Enabled = Power.Attack != null;
        }

        private void update_advice()
        {
            if (!Pages.TabPages.Contains(AdvicePage))
                return;

            AdviceList.Items.Clear();

            if (Power.Attack != null && Power.Action != null)
            {
                AdviceList.ShowGroups = true;

                // Attack advice

                var defence = Power.Attack.Defence == DefenceType.Ac ? "AC" : "non-AC defence";
                var lviAttack = new ListViewItem("Attack vs " + defence + ": ");
                lviAttack.SubItems.Add("+" + Statistics.AttackBonus(Power.Attack.Defence, _fLevel, _fRole));
                lviAttack.Group = AdviceList.Groups[0];
                AdviceList.Items.Add(lviAttack);

                // Damage advice

                if (_fRole is ComplexRole)
                {
                    var lviDmg = new ListViewItem("Damage:");
                    lviDmg.SubItems.Add(Statistics.NormalDamage(_fLevel));
                    lviDmg.Group = AdviceList.Groups[1];
                    AdviceList.Items.Add(lviDmg);
                }
                else if (_fRole is Minion)
                {
                    var lviMinion = new ListViewItem("Minion damage:");
                    lviMinion.SubItems.Add(Statistics.MinionDamage(_fLevel).ToString());
                    lviMinion.Group = AdviceList.Groups[1];
                    AdviceList.Items.Add(lviMinion);
                }
            }

            if (AdviceList.Items.Count == 0)
            {
                AdviceList.ShowGroups = false;

                var lvi = new ListViewItem("(no advice)");
                lvi.ForeColor = SystemColors.GrayText;
                AdviceList.Items.Add(lvi);
            }
        }
    }
}
