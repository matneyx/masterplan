using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TrapSkillForm : Form
    {
        private readonly int _fLevel = 1;

        public TrapSkillData SkillData { get; }

        public TrapSkillForm(TrapSkillData tsd, int level)
        {
            InitializeComponent();

            var skills = Skills.GetSkillNames();
            foreach (var skill in skills)
                SkillBox.Items.Add(skill);

            Application.Idle += Application_Idle;

            SkillData = tsd.Copy();
            _fLevel = level;

            SkillBox.Text = SkillData.SkillName;
            DCBtn.Checked = SkillData.Dc != 0;
            DCBox.Value = SkillData.Dc;
            DetailsBox.Text = SkillData.Details;

            update_advice();
        }

        ~TrapSkillForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            DCLbl.Enabled = DCBtn.Checked;
            DCBox.Enabled = DCBtn.Checked;

            OKBtn.Enabled = SkillBox.Text != "" && DetailsBox.Text != "";
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            SkillData.SkillName = SkillBox.Text;
            if (DCBtn.Checked)
                SkillData.Dc = (int)DCBox.Value;
            else
                SkillData.Dc = 0;
            SkillData.Details = DetailsBox.Text;
        }

        private void update_advice()
        {
            var lviEasy = AdviceList.Items.Add("Skill DC (easy)");
            lviEasy.SubItems.Add(Ai.GetSkillDc(Difficulty.Easy, _fLevel).ToString());
            lviEasy.Group = AdviceList.Groups[0];

            var lviMod = AdviceList.Items.Add("Skill DC (moderate)");
            lviMod.SubItems.Add(Ai.GetSkillDc(Difficulty.Moderate, _fLevel).ToString());
            lviMod.Group = AdviceList.Groups[0];

            var lviHard = AdviceList.Items.Add("Skill DC (hard)");
            lviHard.SubItems.Add(Ai.GetSkillDc(Difficulty.Hard, _fLevel).ToString());
            lviHard.Group = AdviceList.Groups[0];
        }
    }
}
