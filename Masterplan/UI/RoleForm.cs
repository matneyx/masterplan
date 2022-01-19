using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RoleForm : Form
    {
        public IRole Role { get; private set; }

        public RoleForm(IRole r, ThreatType type)
        {
            InitializeComponent();

            var roles = new List<RoleType>();
            switch (type)
            {
                case ThreatType.Creature:
                    roles.Add(RoleType.Artillery);
                    roles.Add(RoleType.Brute);
                    roles.Add(RoleType.Controller);
                    roles.Add(RoleType.Lurker);
                    roles.Add(RoleType.Skirmisher);
                    roles.Add(RoleType.Soldier);
                    break;
                case ThreatType.Trap:
                    roles.Add(RoleType.Blaster);
                    roles.Add(RoleType.Lurker);
                    roles.Add(RoleType.Obstacle);
                    roles.Add(RoleType.Warder);
                    LeaderBox.Text = "This trap is a leader";
                    break;
            }

            foreach (var role in roles)
            {
                RoleBox.Items.Add(role);
                MinionRoleBox.Items.Add(role);
            }

            foreach (RoleFlag flag in Enum.GetValues(typeof(RoleFlag)))
                ModBox.Items.Add(flag);

            Application.Idle += Application_Idle;

            Role = r.Copy();

            if (Role is ComplexRole)
            {
                StandardBtn.Checked = true;

                var cr = Role as ComplexRole;

                RoleBox.SelectedItem = cr.Type;
                MinionRoleBox.SelectedItem = cr.Type;

                ModBox.SelectedItem = cr.Flag;
                LeaderBox.Checked = cr.Leader;
                HasRoleBox.Checked = false;
            }

            if (Role is Minion)
            {
                MinionBtn.Checked = true;

                var m = Role as Minion;

                RoleBox.SelectedItem = m.Type;
                MinionRoleBox.SelectedItem = m.Type;

                ModBox.SelectedItem = RoleFlag.Standard;
                LeaderBox.Checked = false;
                HasRoleBox.Checked = m.HasRole;
            }
        }

        ~RoleForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RoleLbl.Enabled = StandardBtn.Checked;
            RoleBox.Enabled = StandardBtn.Checked;
            ModLbl.Enabled = StandardBtn.Checked;
            ModBox.Enabled = StandardBtn.Checked;
            LeaderBox.Enabled = StandardBtn.Checked;

            HasRoleBox.Enabled = MinionBtn.Checked;
            MinionRoleLbl.Enabled = MinionBtn.Checked && HasRoleBox.Checked;
            MinionRoleBox.Enabled = MinionBtn.Checked && HasRoleBox.Checked;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (StandardBtn.Checked)
            {
                var cr = new ComplexRole();

                cr.Type = (RoleType)RoleBox.SelectedItem;
                cr.Flag = (RoleFlag)ModBox.SelectedItem;
                cr.Leader = LeaderBox.Checked;

                Role = cr;
            }

            if (MinionBtn.Checked)
            {
                var m = new Minion();

                m.HasRole = HasRoleBox.Checked;
                m.Type = (RoleType)MinionRoleBox.SelectedItem;

                Role = m;
            }
        }
    }
}
