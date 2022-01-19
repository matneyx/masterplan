using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TrapCountermeasureForm : Form
    {
        private readonly int _fLevel = 1;

        public string Countermeasure => DetailsBox.Text != DetailsBox.DefaultText ? DetailsBox.Text : "";

        public TrapCountermeasureForm(string cm, int level)
        {
            InitializeComponent();

            DetailsBox.Text = cm;
            _fLevel = level;

            update_advice();
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

        private void TrapCountermeasureForm_Shown(object sender, EventArgs e)
        {
            DetailsBox.Focus();
            DetailsBox.SelectAll();
        }
    }
}
