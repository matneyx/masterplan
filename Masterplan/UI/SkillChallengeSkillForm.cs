using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class SkillChallengeSkillForm : Form
    {
        private const string Primary = "This is a primary skill for this challenge";
        private const string Secondary = "This is a secondary skill for this challenge";
        private const string Autofail = "This skill incurs an automatic failure";

        public SkillChallengeData SkillData { get; }

        public SkillChallengeSkillForm(SkillChallengeData scd)
        {
            InitializeComponent();

            var skills = Skills.GetSkillNames();
            foreach (var skill in skills)
                SkillBox.Items.Add(skill);

            DiffBox.Items.Add(Difficulty.Easy);
            DiffBox.Items.Add(Difficulty.Moderate);
            DiffBox.Items.Add(Difficulty.Hard);

            TypeBox.Items.Add(Primary);
            TypeBox.Items.Add(Secondary);
            TypeBox.Items.Add(Autofail);

            SkillData = scd.Copy();

            SkillBox.Text = SkillData.SkillName;

            switch (SkillData.Type)
            {
                case SkillType.Primary:
                    TypeBox.SelectedIndex = 0;
                    break;
                case SkillType.Secondary:
                    TypeBox.SelectedIndex = 1;
                    break;
                case SkillType.AutoFail:
                    TypeBox.SelectedIndex = 2;
                    break;
            }

            DiffBox.SelectedItem = SkillData.Difficulty;
            ModBox.Value = SkillData.DcModifier;

            DetailsBox.Text = SkillData.Details;
            SuccessBox.Text = SkillData.Success;
            FailureBox.Text = SkillData.Failure;

            SuccessCountBox.Value = SkillData.Results.Successes;
            FailureCountBox.Value = SkillData.Results.Fails;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SkillData.SkillName = SkillBox.Text;

            switch (TypeBox.SelectedIndex)
            {
                case 0:
                    SkillData.Type = SkillType.Primary;
                    break;
                case 1:
                    SkillData.Type = SkillType.Secondary;
                    break;
                case 2:
                    SkillData.Type = SkillType.AutoFail;
                    break;
            }

            SkillData.Difficulty = (Difficulty)DiffBox.SelectedItem;
            SkillData.DcModifier = (int)ModBox.Value;

            SkillData.Details = DetailsBox.Text;
            SkillData.Success = SuccessBox.Text;
            SkillData.Failure = FailureBox.Text;

            SkillData.Results.Successes = (int)SuccessCountBox.Value;
            SkillData.Results.Fails = (int)FailureCountBox.Value;
        }

        private void TypeBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var autofail = TypeBox.Text == Autofail;

            DiffLbl.Enabled = !autofail;
            DiffBox.Enabled = !autofail;
            ModLbl.Enabled = !autofail;
            ModBox.Enabled = !autofail;

            if (autofail)
            {
                Pages.TabPages.Remove(SuccessPage);
                Pages.TabPages.Remove(FailurePage);
            }
            else
            {
                if (!Pages.TabPages.Contains(SuccessPage))
                    Pages.TabPages.Add(SuccessPage);

                if (!Pages.TabPages.Contains(FailurePage))
                    Pages.TabPages.Add(FailurePage);
            }
        }
    }
}
