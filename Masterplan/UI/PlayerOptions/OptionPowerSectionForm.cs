using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionPowerSectionForm : Form
    {
        public PlayerPowerSection Section { get; }

        public OptionPowerSectionForm(PlayerPowerSection section)
        {
            InitializeComponent();

            HeaderBox.Items.Add("Attack");
            HeaderBox.Items.Add("Trigger");
            HeaderBox.Items.Add("Effect");
            HeaderBox.Items.Add("Aftereffect");
            HeaderBox.Items.Add("Hit");
            HeaderBox.Items.Add("Miss");
            HeaderBox.Items.Add("Target");
            HeaderBox.Items.Add("Prerequisite");
            HeaderBox.Items.Add("Requirement");
            HeaderBox.Items.Add("Sustain");
            HeaderBox.Items.Add("Special");

            Section = section.Copy();

            HeaderBox.Text = Section.Header;
            DetailsBox.Text = Section.Details;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Section.Header = HeaderBox.Text;
            Section.Details = DetailsBox.Text;
        }
    }
}
