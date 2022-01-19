using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RandomCreatureForm : Form
    {
        public int Level => (int)LevelBox.Value;

        public IRole Role { get; private set; }

        public RandomCreatureForm(int level, IRole role)
        {
            InitializeComponent();

            Role = role;

            LevelBox.Value = level;
            RoleBtn.Text = Role.ToString();
        }

        private void RoleBtn_Click(object sender, EventArgs e)
        {
            var dlg = new RoleForm(Role, ThreatType.Creature);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Role = dlg.Role;
                RoleBtn.Text = Role.ToString();
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }
    }
}
