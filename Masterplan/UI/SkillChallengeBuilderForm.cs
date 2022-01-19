using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class SkillChallengeBuilderForm : Form
    {
        public SkillChallenge SkillChallenge { get; }

        public SkillChallengeData SelectedSkill
        {
            get
            {
                if (SkillList.SelectedItems.Count != 0)
                    return SkillList.SelectedItems[0].Tag as SkillChallengeData;

                return null;
            }
        }

        public string SelectedSourceSkill
        {
            get
            {
                if (SkillSourceList.SelectedItems.Count != 0)
                    return SkillSourceList.SelectedItems[0].Text;

                return "";
            }
        }

        public SkillChallengeBuilderForm(SkillChallenge sc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            SkillChallenge = sc.Copy() as SkillChallenge;

            update_all();

            var skillNames = Skills.GetSkillNames();
            foreach (var skill in skillNames)
            {
                var ability = Skills.GetKeyAbility(skill).Substring(0, 3);
                var abbr = ability.Substring(0, 3);

                var lvi = SkillSourceList.Items.Add(skill);
                var lvsi = lvi.SubItems.Add(abbr);
                lvi.UseItemStyleForSubItems = false;
                lvsi.ForeColor = SystemColors.GrayText;
                lvi.Group = SkillSourceList.Groups[0];
            }

            var abilityNames = Skills.GetAbilityNames();
            foreach (var ability in abilityNames)
            {
                var abbr = ability.Substring(0, 3);

                var lvi = SkillSourceList.Items.Add(ability);
                var lvsi = lvi.SubItems.Add(abbr);
                lvi.UseItemStyleForSubItems = false;
                lvsi.ForeColor = SystemColors.GrayText;
                lvi.Group = SkillSourceList.Groups[1];
            }

            // Custom
            var lviCustom = SkillSourceList.Items.Add("(custom skill)");
            var lvsiCustom = lviCustom.SubItems.Add("");
            lviCustom.UseItemStyleForSubItems = false;
            lvsiCustom.ForeColor = SystemColors.GrayText;
            lviCustom.Group = SkillSourceList.Groups[2];
        }

        ~SkillChallengeBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedSkill != null;
            EditBtn.Enabled = SelectedSkill != null;

            BreakdownBtn.Enabled = SkillChallenge.Skills.Count != 0;

            var results = SkillChallenge.Results;
            ResetProgressBtn.Enabled = results.Successes + results.Fails != 0;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SkillChallenge.Name = NameBox.Text;
            SkillChallenge.Complexity = (int)CompBox.Value;

            if (LevelBox.Enabled)
                SkillChallenge.Level = (int)LevelBox.Value;

            SkillChallenge.Success = VictoryBox.Text != VictoryBox.DefaultText ? VictoryBox.Text : "";
            SkillChallenge.Failure = DefeatBox.Text != DefeatBox.DefaultText ? DefeatBox.Text : "";
            SkillChallenge.Notes = NotesBox.Text != NotesBox.DefaultText ? NotesBox.Text : "";
        }

        public void update_all()
        {
            NameBox.Text = SkillChallenge.Name;
            CompBox.Value = SkillChallenge.Complexity;
            VictoryBox.Text = SkillChallenge.Success;
            DefeatBox.Text = SkillChallenge.Failure;
            NotesBox.Text = SkillChallenge.Notes;

            if (SkillChallenge.Level != -1)
                LevelBox.Value = SkillChallenge.Level;
            else
                LevelBox.Enabled = false;

            update_view();
            update_skills();
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            update_view();
        }

        private void CompBox_ValueChanged(object sender, EventArgs e)
        {
            update_view();
        }

        private void update_view()
        {
            var level = (int)LevelBox.Value;
            var complexity = (int)CompBox.Value;

            LengthLbl.Text = SkillChallenge.GetSuccesses(complexity) + " successes before 3 failures";

            InfoList.Items.Clear();
            if (SkillChallenge.Level != -1)
            {
                var lviEasy = InfoList.Items.Add("Easy");
                lviEasy.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Easy, level));
                lviEasy.Group = InfoList.Groups[0];

                var lviMod = InfoList.Items.Add("Moderate");
                lviMod.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Moderate, level));
                lviMod.Group = InfoList.Groups[0];

                var lviHard = InfoList.Items.Add("Hard");
                lviHard.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Hard, level));
                lviHard.Group = InfoList.Groups[0];

                XPLbl.Text = SkillChallenge.GetXp(level, complexity) + " XP";
            }
            else
            {
                var lvi = InfoList.Items.Add("DCs");
                var lvsi = lvi.SubItems.Add("(varies by level)");
                lvi.UseItemStyleForSubItems = false;
                lvsi.ForeColor = SystemColors.GrayText;
                lvi.Group = InfoList.Groups[0];

                XPLbl.Text = "(XP varies by level)";
            }

            var results = SkillChallenge.Results;
            SuccessCountLbl.Text = "Successes: " + results.Successes;
            FailureCountLbl.Text = "Failures: " + results.Fails;
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSkill != null)
            {
                SkillChallenge.Skills.Remove(SelectedSkill);

                update_view();
                update_skills();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedSkill != null)
            {
                var index = SkillChallenge.Skills.IndexOf(SelectedSkill);

                var dlg = new SkillChallengeSkillForm(SelectedSkill);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    SkillChallenge.Skills[index] = dlg.SkillData;
                    SkillChallenge.Skills.Sort();

                    update_view();
                    update_skills();
                }
            }
        }

        private void update_skills()
        {
            SkillList.Items.Clear();

            foreach (var scd in SkillChallenge.Skills)
            {
                var diff = scd.Difficulty + " DCs";
                if (scd.DcModifier != 0)
                {
                    if (scd.DcModifier > 0)
                        diff += " +" + scd.DcModifier;
                    else
                        diff += " " + scd.DcModifier;
                }

                var lvi = SkillList.Items.Add(scd.SkillName);
                lvi.SubItems.Add(diff);
                lvi.Tag = scd;

                switch (scd.Type)
                {
                    case SkillType.Primary:
                        lvi.Group = SkillList.Groups[0];
                        break;
                    case SkillType.Secondary:
                        lvi.Group = SkillList.Groups[1];
                        break;
                    case SkillType.AutoFail:
                        lvi.Group = SkillList.Groups[2];
                        break;
                }

                if (scd.Details == "" && scd.Success == "" && scd.Failure == "")
                    lvi.ForeColor = SystemColors.GrayText;

                if (scd.Difficulty == Difficulty.Trivial || scd.Difficulty == Difficulty.Extreme)
                {
                    lvi.UseItemStyleForSubItems = false;
                    lvi.SubItems[1].ForeColor = Color.Red;
                }
            }

            if (SkillList.Groups[0].Items.Count == 0)
            {
                var lvi = SkillList.Items.Add("(none)");
                lvi.Group = SkillList.Groups[0];
                lvi.ForeColor = SystemColors.GrayText;
            }

            if (SkillList.Groups[1].Items.Count == 0)
            {
                var lvi = SkillList.Items.Add("(none)");
                lvi.Group = SkillList.Groups[1];
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void SkillSourceList_ItemDrag(object sender, ItemDragEventArgs e)
        {
            var lvi = e.Item as ListViewItem;
            if (lvi != null)
            {
                var skillName = lvi.Text;

                var fx = DoDragDrop(skillName, DragDropEffects.Copy);

                if (fx == DragDropEffects.Copy)
                    add_skill(skillName);
            }
        }

        private void SkillList_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;

            var skill = e.Data.GetData(typeof(string)) as string;
            if (skill != null && skill != "")
                e.Effect = DragDropEffects.Copy;
        }

        private void SkillSourceList_DoubleClick(object sender, EventArgs e)
        {
            var skillName = SelectedSourceSkill;

            if (skillName != "")
                add_skill(skillName);
        }

        private void add_skill(string skillName)
        {
            var scd = new SkillChallengeData();
            scd.SkillName = skillName;

            var dlg = new SkillChallengeSkillForm(scd);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                SkillChallenge.Skills.Add(dlg.SkillData);
                SkillChallenge.Skills.Sort();

                update_view();
                update_skills();
            }
        }

        private void BreakdownBtn_Click(object sender, EventArgs e)
        {
            // Show breakdown
            var dlg = new SkillChallengeBreakdownForm(SkillChallenge);
            dlg.ShowDialog();
        }

        private void ResetProgressBtn_Click(object sender, EventArgs e)
        {
            foreach (var scd in SkillChallenge.Skills)
            {
                scd.Results.Successes = 0;
                scd.Results.Fails = 0;
            }

            update_view();
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Skill Challenge";
            dlg.FileName = SkillChallenge.Name;
            dlg.Filter = Program.SkillChallengeFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ok = Serialisation<SkillChallenge>.Save(dlg.FileName, SkillChallenge, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The skill challenge could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
