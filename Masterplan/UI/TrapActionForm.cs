using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TrapActionForm : Form
    {
        public TrapAttack Attack { get; }

        public TrapActionForm(TrapAttack attack)
        {
            InitializeComponent();

            var actions = Enum.GetValues(typeof(ActionType));
            foreach (ActionType action in actions)
                ActionBox.Items.Add(action);

            Application.Idle += Application_Idle;

            Attack = attack.Copy();

            NameBox.Text = Attack.Name;
            ActionBox.SelectedItem = Attack.Action;
            RangeBox.Text = Attack.Range;
            TargetBox.Text = Attack.Target;
        }

        ~TrapActionForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Attack.Name = NameBox.Text;
            Attack.Action = (ActionType)ActionBox.SelectedItem;
            Attack.Range = RangeBox.Text;
            Attack.Target = TargetBox.Text;
        }
    }
}
