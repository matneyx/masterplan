using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionBackgroundForm : Form
    {
        public PlayerBackground Background { get; }

        public OptionBackgroundForm(PlayerBackground bg)
        {
            InitializeComponent();

            Background = bg.Copy();

            NameBox.Text = Background.Name;
            SkillBox.Text = Background.AssociatedSkills;
            FeatBox.Text = Background.RecommendedFeats;
            DetailsBox.Text = Background.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Background.Name = NameBox.Text;
            Background.AssociatedSkills = SkillBox.Text;
            Background.RecommendedFeats = FeatBox.Text;
            Background.Details = DetailsBox.Text;
        }
    }
}
