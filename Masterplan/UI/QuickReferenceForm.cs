using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class QuickReferenceForm : Form
    {
        public QuickReferenceForm()
        {
            InitializeComponent();

            foreach (var addin in Session.AddIns)
            foreach (var page in addin.QuickReferencePages)
            {
                var tabpage = new TabPage();
                tabpage.Text = page.Name;

                tabpage.Controls.Add(page.Control);
                page.Control.Dock = DockStyle.Fill;

                Pages.TabPages.Add(tabpage);
            }

            UpdateView();
        }

        private void QuickReferenceForm_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        public void UpdateView()
        {
            if (Session.Project != null)
                LevelBox.Value = Session.Project.Party.Level;
            update_skills();

            foreach (var addin in Session.AddIns)
            foreach (var page in addin.QuickReferencePages)
                page.UpdateView();
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            update_skills();
        }

        private void update_skills()
        {
            var level = (int)LevelBox.Value;

            SkillList.BeginUpdate();
            SkillList.Items.Clear();

            var lviEasy = SkillList.Items.Add("Easy");
            lviEasy.SubItems.Add(Ai.GetSkillDc(Difficulty.Easy, level).ToString());

            var lviModerate = SkillList.Items.Add("Moderate");
            lviModerate.SubItems.Add(Ai.GetSkillDc(Difficulty.Moderate, level).ToString());

            var lviHard = SkillList.Items.Add("Hard");
            lviHard.SubItems.Add(Ai.GetSkillDc(Difficulty.Hard, level).ToString());

            SkillList.EndUpdate();

            DamageList.BeginUpdate();
            DamageList.Items.Clear();

            DamageList.ShowGroups = false;

            var lviDamage = DamageList.Items.Add("Against a single target");
            lviDamage.SubItems.Add(Statistics.NormalDamage(level));
            lviDamage.Group = DamageList.Groups[0];

            var lviMultiple = DamageList.Items.Add("Against multiple targets");
            lviMultiple.SubItems.Add(Statistics.MultipleDamage(level));
            lviMultiple.Group = DamageList.Groups[0];

            DamageList.EndUpdate();
        }
    }
}
