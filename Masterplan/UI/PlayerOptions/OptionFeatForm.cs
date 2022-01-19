using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionFeatForm : Form
    {
        public Feat Feat { get; }

        public OptionFeatForm(Feat feat)
        {
            InitializeComponent();

            var tiers = Enum.GetValues(typeof(Tier));
            foreach (Tier tier in tiers)
                TierBox.Items.Add(tier);

            Feat = feat.Copy();

            NameBox.Text = Feat.Name;
            PrereqBox.Text = Feat.Prerequisites;
            TierBox.SelectedItem = Feat.Tier;
            BenefitBox.Text = Feat.Benefits;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Feat.Name = NameBox.Text;
            Feat.Prerequisites = PrereqBox.Text;
            Feat.Tier = (Tier)TierBox.SelectedItem;
            Feat.Benefits = BenefitBox.Text;
        }
    }
}
