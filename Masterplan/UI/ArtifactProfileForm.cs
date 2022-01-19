using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class ArtifactProfileForm : Form
    {
        public Artifact Artifact { get; }

        public ArtifactProfileForm(Artifact artifact)
        {
            InitializeComponent();

            Artifact = artifact;

            // Populate tiers
            foreach (Tier tier in Enum.GetValues(typeof(Tier)))
                TierBox.Items.Add(tier);

            NameBox.Text = Artifact.Name;
            TierBox.SelectedItem = Artifact.Tier;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Artifact.Name = NameBox.Text;
            Artifact.Tier = (Tier)TierBox.SelectedItem;
        }
    }
}
