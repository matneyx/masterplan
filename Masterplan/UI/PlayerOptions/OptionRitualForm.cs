using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionRitualForm : Form
    {
        public Ritual Ritual { get; }

        public OptionRitualForm(Ritual ritual)
        {
            InitializeComponent();

            var cats = Enum.GetValues(typeof(RitualCategory));
            foreach (RitualCategory cat in cats)
                CatBox.Items.Add(cat);

            Ritual = ritual.Copy();

            NameBox.Text = Ritual.Name;
            LevelBox.Value = Ritual.Level;
            CatBox.SelectedItem = Ritual.Category;

            TimeBox.Text = Ritual.Time;
            DurationBox.Text = Ritual.Duration;
            ComponentBox.Text = Ritual.ComponentCost;
            MarketBox.Text = Ritual.MarketPrice;
            SkillBox.Text = Ritual.KeySkill;

            DetailsBox.Text = Ritual.Details;
            ReadAloudBox.Text = Ritual.ReadAloud;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Ritual.Name = NameBox.Text;

            Ritual.Level = (int)LevelBox.Value;
            Ritual.Category = (RitualCategory)CatBox.SelectedItem;

            Ritual.Time = TimeBox.Text;
            Ritual.Duration = DurationBox.Text;
            Ritual.ComponentCost = ComponentBox.Text;
            Ritual.MarketPrice = MarketBox.Text;
            Ritual.KeySkill = SkillBox.Text;

            Ritual.Details = DetailsBox.Text;
            Ritual.ReadAloud = ReadAloudBox.Text;
        }

        private void CatBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cat = (RitualCategory)CatBox.SelectedItem;

            SkillBox.Items.Clear();
            switch (cat)
            {
                case RitualCategory.Binding:
                    SkillBox.Items.Add("Arcana");
                    SkillBox.Items.Add("Religion");
                    break;
                case RitualCategory.Creation:
                    SkillBox.Items.Add("Arcana");
                    SkillBox.Items.Add("Religion");
                    break;
                case RitualCategory.Deception:
                    SkillBox.Items.Add("Arcana");
                    break;
                case RitualCategory.Divination:
                    SkillBox.Items.Add("Arcana");
                    SkillBox.Items.Add("Nature");
                    SkillBox.Items.Add("Religion");
                    break;
                case RitualCategory.Exploration:
                    SkillBox.Items.Add("Arcana");
                    SkillBox.Items.Add("Nature");
                    SkillBox.Items.Add("Religion");
                    break;
                case RitualCategory.Restoration:
                    SkillBox.Items.Add("Heal");
                    break;
                case RitualCategory.Scrying:
                    SkillBox.Items.Add("Arcana");
                    break;
                case RitualCategory.Travel:
                    SkillBox.Items.Add("Arcana");
                    break;
                case RitualCategory.Warding:
                    SkillBox.Items.Add("Arcana");
                    break;
            }
        }
    }
}
