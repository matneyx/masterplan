using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureSkillsForm : Form
    {
        private readonly ICreature _fCreature;
        private readonly List<SkillData> _fSkills = new List<SkillData>();

        private SkillData SelectedSkill
        {
            get
            {
                if (SkillList.SelectedItems.Count != 0)
                    return SkillList.SelectedItems[0].Tag as SkillData;

                return null;
            }
        }

        public CreatureSkillsForm(ICreature creature)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fCreature = creature;

            var skills = CreatureHelper.ParseSkills(_fCreature.Skills);
            foreach (var skillName in Skills.GetSkillNames())
            {
                var level = _fCreature.Level / 2;
                var ability = 0;

                var abilityName = Skills.GetKeyAbility(skillName);
                switch (abilityName)
                {
                    case "Strength":
                        ability = _fCreature.Strength.Modifier;
                        break;
                    case "Constitution":
                        ability = _fCreature.Constitution.Modifier;
                        break;
                    case "Dexterity":
                        ability = _fCreature.Dexterity.Modifier;
                        break;
                    case "Intelligence":
                        ability = _fCreature.Intelligence.Modifier;
                        break;
                    case "Wisdom":
                        ability = _fCreature.Wisdom.Modifier;
                        break;
                    case "Charisma":
                        ability = _fCreature.Charisma.Modifier;
                        break;
                }

                var sd = new SkillData();
                sd.SkillName = skillName;
                sd.Ability = ability;
                sd.Level = level;

                if (skills.ContainsKey(skillName))
                {
                    var total = skills[skillName];
                    var misc = total - (ability + level);
                    if (misc > 3)
                    {
                        sd.Trained = true;
                        misc -= 5;
                    }

                    sd.Misc = misc;
                }

                _fSkills.Add(sd);
            }

            update_list();
        }

        ~CreatureSkillsForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            TrainedBtn.Enabled = SelectedSkill != null;
            TrainedBtn.Checked = SelectedSkill != null && SelectedSkill.Trained;
            EditSkillBtn.Enabled = SelectedSkill != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var skills = "";
            foreach (var sd in _fSkills)
            {
                if (!sd.Show)
                    continue;

                if (skills != "")
                    skills += "; ";

                skills += sd.ToString();
            }

            _fCreature.Skills = skills;
        }

        private void update_list()
        {
            SkillList.BeginUpdate();

            SkillList.Items.Clear();

            var items = new List<ListViewItem>();
            foreach (var sd in _fSkills)
            {
                var lvi = new ListViewItem(sd.SkillName);
                lvi.SubItems.Add(sd.Level >= 0 ? "+" + sd.Level : sd.Level.ToString());
                lvi.SubItems.Add(sd.Ability >= 0 ? "+" + sd.Ability : sd.Ability.ToString());
                lvi.SubItems.Add(sd.Trained ? "+5" : "");
                if (sd.Misc != 0)
                    lvi.SubItems.Add(sd.Misc >= 0 ? "+" + sd.Misc : sd.Misc.ToString());
                else
                    lvi.SubItems.Add("");
                lvi.SubItems.Add(sd.Total >= 0 ? "+" + sd.Total : sd.Total.ToString());

                if (!sd.Show)
                {
                    lvi.ForeColor = SystemColors.GrayText;
                    lvi.UseItemStyleForSubItems = false;
                }

                lvi.Tag = sd;

                items.Add(lvi);
            }

            SkillList.Items.AddRange(items.ToArray());

            SkillList.EndUpdate();
        }

        private void SkillList_DoubleClick(object sender, EventArgs e)
        {
            TrainedBtn_Click(sender, e);
        }

        private void TrainedBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSkill == null)
                return;

            SelectedSkill.Trained = !SelectedSkill.Trained;
            update_list();
        }

        private void EditSkillBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSkill == null)
                return;

            var ability = Skills.GetKeyAbility(SelectedSkill.SkillName);
            var dlg = new CreatureSkillForm(SelectedSkill.SkillName, ability, SelectedSkill.Ability,
                SelectedSkill.Level, SelectedSkill.Trained, SelectedSkill.Misc);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SelectedSkill.Trained = dlg.Trained;
                SelectedSkill.Misc = dlg.Misc;

                update_list();
            }
        }

        private class SkillData
        {
            public int Ability;
            public int Level;
            public int Misc;
            public string SkillName;
            public bool Trained;

            public bool Show => Trained || Misc != 0;

            public int Total
            {
                get
                {
                    var training = Trained ? 5 : 0;
                    return training + Ability + Level + Misc;
                }
            }

            public override string ToString()
            {
                var sign = Total < 0 ? "-" : "";
                return SkillName + " " + sign + Total;
            }
        }
    }
}
