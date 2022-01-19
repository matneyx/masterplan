using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionClassForm : Form
    {
        public Class Class { get; }

        public LevelData SelectedLevel
        {
            get
            {
                if (LevelList.SelectedItems.Count != 0)
                    return LevelList.SelectedItems[0].Tag as LevelData;

                return null;
            }
        }

        public OptionClassForm(Class c)
        {
            InitializeComponent();

            Class = c.Copy();

            NameBox.Text = Class.Name;
            RoleBox.Text = Class.Role;
            PowerSourceBox.Text = Class.PowerSource;
            KeyAbilityBox.Text = Class.KeyAbilities;
            HPFirstBox.Value = Class.HpFirst;
            HPSubsequentBox.Value = Class.HpSubsequent;
            SurgeBox.Value = Class.HealingSurges;

            ArmourBox.Text = Class.ArmourProficiencies;
            WeaponBox.Text = Class.WeaponProficiencies;
            ImplementBox.Text = Class.Implements;
            DefencesBox.Text = Class.DefenceBonuses;
            SkillBox.Text = Class.TrainedSkills;

            DescBox.Text = Class.Description;
            QuoteBox.Text = Class.Quote;

            CharacteristicsBox.Text = Class.OverviewCharacteristics;
            ReligionBox.Text = Class.OverviewReligion;
            RacesBox.Text = Class.OverviewRaces;

            update_levels();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Class.Name = NameBox.Text;
            Class.Role = RoleBox.Text;
            Class.PowerSource = PowerSourceBox.Text;
            Class.KeyAbilities = KeyAbilityBox.Text;
            Class.HpFirst = (int)HPFirstBox.Value;
            Class.HpSubsequent = (int)HPSubsequentBox.Value;
            Class.HealingSurges = (int)SurgeBox.Value;

            Class.ArmourProficiencies = ArmourBox.Text;
            Class.WeaponProficiencies = WeaponBox.Text;
            Class.Implements = ImplementBox.Text;
            Class.DefenceBonuses = DefencesBox.Text;
            Class.TrainedSkills = SkillBox.Text;

            Class.Description = DescBox.Text;
            Class.Quote = QuoteBox.Text;

            Class.OverviewCharacteristics = CharacteristicsBox.Text;
            Class.OverviewReligion = ReligionBox.Text;
            Class.OverviewRaces = RacesBox.Text;
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = Class.Levels.IndexOf(SelectedLevel);
                var classFeatures = index == -1;

                var dlg = new OptionLevelForm(SelectedLevel, classFeatures);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (classFeatures)
                        Class.FeatureData = dlg.Level;
                    else
                        Class.Levels[index] = dlg.Level;

                    update_levels();
                }
            }
        }

        private void update_levels()
        {
            LevelList.Items.Clear();

            add_level(Class.FeatureData);

            foreach (var ld in Class.Levels)
                add_level(ld);
        }

        private void add_level(LevelData ld)
        {
            var lvi = LevelList.Items.Add(ld.ToString());
            lvi.Tag = ld;

            if (ld.Count == 0)
                lvi.ForeColor = SystemColors.GrayText;

            if (ld == Class.FeatureData)
            {
                lvi.Group = LevelList.Groups[0];
            }
            else
            {
                var tier = (ld.Level - 1) / 10;
                lvi.Group = LevelList.Groups[tier + 1];
            }
        }
    }
}
