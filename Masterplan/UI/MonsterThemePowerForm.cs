using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class MonsterThemePowerForm : Form
    {
        public ThemePowerData Power { get; }

        public MonsterThemePowerForm(ThemePowerData power)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            var types = Enum.GetValues(typeof(PowerType));
            foreach (PowerType type in types)
                TypeBox.Items.Add(type);

            Power = power.Copy();

            TypeBox.SelectedItem = Power.Type;

            ArtilleryBox.Checked = Power.Roles.Contains(RoleType.Artillery);
            BruteBox.Checked = Power.Roles.Contains(RoleType.Brute);
            ControllerBox.Checked = Power.Roles.Contains(RoleType.Controller);
            LurkerBox.Checked = Power.Roles.Contains(RoleType.Lurker);
            SkirmisherBox.Checked = Power.Roles.Contains(RoleType.Skirmisher);
            SoldierBox.Checked = Power.Roles.Contains(RoleType.Soldier);
        }

        ~MonsterThemePowerForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var roles = get_roles();
            OKBtn.Enabled = roles.Count != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Power.Type = (PowerType)TypeBox.SelectedItem;
            Power.Roles = get_roles();
        }

        private List<RoleType> get_roles()
        {
            var roles = new List<RoleType>();

            if (ArtilleryBox.Checked)
                roles.Add(RoleType.Artillery);

            if (BruteBox.Checked)
                roles.Add(RoleType.Brute);

            if (ControllerBox.Checked)
                roles.Add(RoleType.Controller);

            if (LurkerBox.Checked)
                roles.Add(RoleType.Lurker);

            if (SkirmisherBox.Checked)
                roles.Add(RoleType.Skirmisher);

            if (SoldierBox.Checked)
                roles.Add(RoleType.Soldier);

            return roles;
        }
    }
}
