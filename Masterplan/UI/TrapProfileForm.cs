using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class TrapProfileForm : Form
    {
        public Trap Trap { get; }

        public TrapProfileForm(Trap trap)
        {
            InitializeComponent();

            var types = Enum.GetValues(typeof(TrapType));
            foreach (TrapType type in types)
                TypeBox.Items.Add(type);

            Application.Idle += Application_Idle;

            Trap = trap.Copy();

            NameBox.Text = Trap.Name;
            TypeBox.SelectedItem = Trap.Type;
            LevelBox.Value = Trap.Level;
            RoleBtn.Text = Trap.Role.ToString();

            if (Trap.Initiative == int.MinValue)
            {
                HasInitBox.Checked = false;
                InitBox.Value = 0;
            }
            else
            {
                HasInitBox.Checked = true;
                InitBox.Value = Trap.Initiative;
            }
        }

        ~TrapProfileForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            InitBox.Enabled = HasInitBox.Checked;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Trap.Name = NameBox.Text;
            Trap.Type = (TrapType)TypeBox.SelectedItem;
            Trap.Level = (int)LevelBox.Value;
            Trap.Initiative = HasInitBox.Checked ? (int)InitBox.Value : int.MinValue;
        }

        private void RoleBtn_Click(object sender, EventArgs e)
        {
            var dlg = new RoleForm(Trap.Role, ThreatType.Trap);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Trap.Role = dlg.Role;
                RoleBtn.Text = Trap.Role.ToString();
            }
        }
    }
}
