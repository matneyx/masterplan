using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CreatureTemplateProfileForm : Form
    {
        public CreatureTemplate Template { get; }

        public CreatureTemplateProfileForm(CreatureTemplate t)
        {
            InitializeComponent();

            TypeBox.Items.Add(CreatureTemplateType.Functional);
            TypeBox.Items.Add(CreatureTemplateType.Class);

            RoleBox.Items.Add(RoleType.Artillery);
            RoleBox.Items.Add(RoleType.Brute);
            RoleBox.Items.Add(RoleType.Controller);
            RoleBox.Items.Add(RoleType.Lurker);
            RoleBox.Items.Add(RoleType.Skirmisher);
            RoleBox.Items.Add(RoleType.Soldier);

            Template = t.Copy();

            NameBox.Text = Template.Name;
            TypeBox.SelectedItem = Template.Type;
            RoleBox.SelectedItem = Template.Role;
            LeaderBox.Checked = Template.Leader;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Template.Name = NameBox.Text;
            Template.Type = (CreatureTemplateType)TypeBox.SelectedItem;
            Template.Role = (RoleType)RoleBox.SelectedItem;
            Template.Leader = LeaderBox.Checked;
        }
    }
}
