using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;
using Masterplan.UI;

namespace Masterplan.Controls
{
    internal partial class InfoPanel : UserControl
    {
        public int Level
        {
            get => (int)LevelBox.Value;
            set => LevelBox.Value = value;
        }

        public DiceExpression SelectedDamageExpression
        {
            get
            {
                if (SkillList.SelectedItems.Count != 0)
                    return SkillList.SelectedItems[0].Tag as DiceExpression;

                return null;
            }
        }

        public InfoPanel()
        {
            InitializeComponent();

            update_list();
        }

        private void LevelBox_ValueChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void update_list()
        {
            var level = (int)LevelBox.Value;

            var aidAnother = 10 + level / 2;

            var normal = Statistics.NormalDamage(level);
            var multiple = Statistics.MultipleDamage(level);
            var minion = Statistics.MinionDamage(level).ToString();

            SkillList.BeginUpdate();
            SkillList.Items.Clear();

            var lviEasy = SkillList.Items.Add("Easy");
            lviEasy.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Easy, level));
            lviEasy.Group = SkillList.Groups[0];

            var lviModerate = SkillList.Items.Add("Moderate");
            lviModerate.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Moderate, level));
            lviModerate.Group = SkillList.Groups[0];

            var lviHard = SkillList.Items.Add("Hard");
            lviHard.SubItems.Add("DC " + Ai.GetSkillDc(Difficulty.Hard, level));
            lviHard.Group = SkillList.Groups[0];

            var lviAid = SkillList.Items.Add("Aid Another");
            lviAid.SubItems.Add("DC " + aidAnother);
            lviAid.Group = SkillList.Groups[1];

            var lviDamage = SkillList.Items.Add("Against a single target");
            lviDamage.SubItems.Add(normal);
            lviDamage.Tag = DiceExpression.Parse(normal);
            lviDamage.Group = SkillList.Groups[2];

            var lviMultiple = SkillList.Items.Add("Against multiple targets");
            lviMultiple.SubItems.Add(multiple);
            lviMultiple.Tag = DiceExpression.Parse(multiple);
            lviMultiple.Group = SkillList.Groups[2];

            var lviMinion = SkillList.Items.Add("From a minion");
            lviMinion.SubItems.Add(minion);
            lviMinion.Tag = DiceExpression.Parse(minion);
            lviMinion.Group = SkillList.Groups[2];

            var lviAberrant = SkillList.Items.Add("Aberrant");
            lviAberrant.SubItems.Add("Dungeoneering");
            lviAberrant.Group = SkillList.Groups[3];

            var lviElemental = SkillList.Items.Add("Elemental");
            lviElemental.SubItems.Add("Arcana");
            lviElemental.Group = SkillList.Groups[3];

            var lviFey = SkillList.Items.Add("Fey");
            lviFey.SubItems.Add("Arcana");
            lviFey.Group = SkillList.Groups[3];

            var lviImmortal = SkillList.Items.Add("Immortal");
            lviImmortal.SubItems.Add("Religion");
            lviImmortal.Group = SkillList.Groups[3];

            var lviNatural = SkillList.Items.Add("Natural");
            lviNatural.SubItems.Add("Nature");
            lviNatural.Group = SkillList.Groups[3];

            var lviShadow = SkillList.Items.Add("Shadow");
            lviShadow.SubItems.Add("Arcana");
            lviShadow.Group = SkillList.Groups[3];

            var lviUndead = SkillList.Items.Add("Undead keyword");
            lviUndead.SubItems.Add("Religion");
            lviUndead.Group = SkillList.Groups[3];

            SkillList.EndUpdate();
        }

        private void DamageList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedDamageExpression != null)
            {
                var dlg = new DieRollerForm();
                dlg.Expression = SelectedDamageExpression;
                dlg.ShowDialog();
            }
        }
    }
}
