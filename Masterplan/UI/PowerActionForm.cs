using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class PowerActionForm : Form
    {
        public PowerAction Action { get; private set; }

        public PowerActionForm(PowerAction action)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            RechargeBox.Items.Add(PowerAction.Recharge2);
            RechargeBox.Items.Add(PowerAction.Recharge3);
            RechargeBox.Items.Add(PowerAction.Recharge4);
            RechargeBox.Items.Add(PowerAction.Recharge5);
            RechargeBox.Items.Add(PowerAction.Recharge6);

            var actions = Enum.GetValues(typeof(ActionType));
            foreach (ActionType a in actions)
            {
                ActionBox.Items.Add(a);
                SustainBox.Items.Add(a);
            }

            if (action != null)
                Action = action.Copy();

            if (Action != null)
            {
                Action = action.Copy();

                TraitBox.Checked = false;

                switch (Action.Use)
                {
                    case PowerUseType.AtWill:
                        AtWillBtn.Checked = true;
                        BasicAttackBtn.Checked = false;
                        break;
                    case PowerUseType.Basic:
                        AtWillBtn.Checked = true;
                        BasicAttackBtn.Checked = true;
                        break;
                    case PowerUseType.Encounter:
                        EncounterBtn.Checked = true;
                        RechargeBox.Text = Action.Recharge;
                        break;
                    case PowerUseType.Daily:
                        DailyBtn.Checked = true;
                        break;
                }

                ActionBox.SelectedItem = Action.Action;
                TriggerBox.Text = Action.Trigger;
                SustainBox.SelectedItem = Action.SustainAction;
            }
            else
            {
                TraitBox.Checked = true;

                AtWillBtn.Checked = true;
                BasicAttackBtn.Checked = false;
                ActionBox.SelectedItem = ActionType.Standard;
                SustainBox.SelectedItem = ActionType.None;
            }
        }

        ~PowerActionForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var isTrait = TraitBox.Checked;

            UsageGroup.Enabled = !isTrait;
            BasicAttackBtn.Enabled = UsageGroup.Enabled && AtWillBtn.Checked;
            RechargeLbl.Enabled = UsageGroup.Enabled && EncounterBtn.Checked;
            RechargeBox.Enabled = UsageGroup.Enabled && EncounterBtn.Checked;
            ActionLbl.Enabled = !isTrait;
            ActionBox.Enabled = !isTrait;
            TriggerLbl.Enabled = !isTrait;
            TriggerBox.Enabled = !isTrait;
            SustainLbl.Enabled = !isTrait;
            SustainBox.Enabled = !isTrait;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (TraitBox.Checked)
            {
                Action = null;
            }
            else
            {
                if (Action == null)
                    Action = new PowerAction();

                if (AtWillBtn.Checked) Action.Use = BasicAttackBtn.Checked ? PowerUseType.Basic : PowerUseType.AtWill;

                if (EncounterBtn.Checked)
                {
                    Action.Use = PowerUseType.Encounter;
                    Action.Recharge = RechargeBox.Text;
                }

                if (DailyBtn.Checked) Action.Use = PowerUseType.Daily;

                Action.Action = (ActionType)ActionBox.SelectedItem;
                Action.Trigger = TriggerBox.Text;
                Action.SustainAction = (ActionType)SustainBox.SelectedItem;
            }
        }
    }
}
