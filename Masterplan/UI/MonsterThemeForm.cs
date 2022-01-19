using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MonsterThemeForm : Form
    {
        public MonsterTheme Theme { get; }

        public ThemePowerData SelectedPower
        {
            get
            {
                if (PowerList.SelectedItems.Count != 0)
                    return PowerList.SelectedItems[0].Tag as ThemePowerData;

                return null;
            }
        }

        public MonsterThemeForm(MonsterTheme theme)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Theme = theme.Copy();

            foreach (var skillName in Skills.GetSkillNames())
            {
                var lvi = SkillList.Items.Add(skillName);

                var present = false;
                foreach (var pair in Theme.SkillBonuses)
                    if (pair.First == skillName)
                        present = true;
                lvi.Checked = present;
            }

            NameBox.Text = Theme.Name;

            update_powers();
        }

        ~MonsterThemeForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            PowerRemoveBtn.Enabled = SelectedPower != null;
            PowerEditBtn.Enabled = SelectedPower != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Theme.Name = NameBox.Text;

            Theme.SkillBonuses.Clear();
            foreach (ListViewItem lvi in SkillList.CheckedItems)
                Theme.SkillBonuses.Add(new Pair<string, int>(lvi.Text, 2));
        }

        private void PowerAddBtn_Click(object sender, EventArgs e)
        {
            var power = new CreaturePower();
            power.Name = "New Power";

            var dlg = new PowerBuilderForm(power, null, false);
            if (dlg.ShowDialog() == DialogResult.OK)
                add_power(dlg.Power);
        }

        private void PowerBrowse_Click(object sender, EventArgs e)
        {
            var dlg = new PowerBrowserForm(NameBox.Text, 0, null, add_power);
            dlg.ShowDialog();
        }

        private void add_power(CreaturePower power)
        {
            var tpd = new ThemePowerData();
            tpd.Power = power;

            Theme.Powers.Add(tpd);
            update_powers();
        }

        private void PowerRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                Theme.Powers.Remove(SelectedPower);
                update_powers();
            }
        }

        private void PowerEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Theme.Powers.IndexOf(SelectedPower);

                var dlg = new PowerBuilderForm(SelectedPower.Power, null, false);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Theme.Powers[index].Power = dlg.Power;
                    update_powers();
                }
            }
        }

        private void EditClassification_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Theme.Powers.IndexOf(SelectedPower);

                var dlg = new MonsterThemePowerForm(SelectedPower);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Theme.Powers[index] = dlg.Power;
                    update_powers();
                }
            }
        }

        private void update_powers()
        {
            PowerList.ShowGroups = true;

            PowerList.Items.Clear();
            foreach (var p in Theme.Powers)
            {
                var roleStr = "";
                if (p.Roles.Count == 6)
                    roleStr = "(any)";
                else
                    foreach (var rt in p.Roles)
                    {
                        if (roleStr != "")
                            roleStr += ", ";

                        roleStr += rt.ToString();
                    }

                var lvi = PowerList.Items.Add(p.Power.Name);
                lvi.SubItems.Add(roleStr);
                lvi.Tag = p;

                switch (p.Type)
                {
                    case PowerType.Attack:
                        lvi.Group = PowerList.Groups[0];
                        break;
                    case PowerType.Utility:
                        lvi.Group = PowerList.Groups[1];
                        break;
                }
            }

            if (PowerList.Items.Count == 0)
            {
                PowerList.ShowGroups = false;

                var lvi = PowerList.Items.Add("(no powers)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
