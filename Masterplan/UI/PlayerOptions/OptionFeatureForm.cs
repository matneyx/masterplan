using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionFeatureForm : Form
    {
        public Feature Feature { get; }

        public OptionFeatureForm(Feature feature)
        {
            InitializeComponent();

            Feature = feature.Copy();

            NameBox.Text = Feature.Name;
            DetailsBox.Text = Feature.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Feature.Name = NameBox.Text;
            Feature.Details = DetailsBox.Text;
        }
    }
}
