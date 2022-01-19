using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionLevelForm : Form
    {
        public LevelData Level { get; }

        public Feature SelectedFeature
        {
            get
            {
                if (FeatureList.SelectedItems.Count != 0)
                    return FeatureList.SelectedItems[0].Tag as Feature;

                return null;
            }
        }

        public PlayerPower SelectedPower
        {
            get
            {
                if (PowerList.SelectedItems.Count != 0)
                    return PowerList.SelectedItems[0].Tag as PlayerPower;

                return null;
            }
        }

        public OptionLevelForm(LevelData level, bool showFeatures)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Level = level.Copy();
            Text = "Level " + Level.Level;

            if (!showFeatures)
                Pages.TabPages.Remove(FeaturesPage);

            update_features();
            update_powers();
        }

        ~OptionLevelForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            FeatureRemoveBtn.Enabled = SelectedFeature != null;
            FeatureEditBtn.Enabled = SelectedFeature != null;

            PowerRemoveBtn.Enabled = SelectedPower != null;
            PowerEditBtn.Enabled = SelectedPower != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
        }

        private void FeatureAddBtn_Click(object sender, EventArgs e)
        {
            var ft = new Feature();
            ft.Name = "New Feature";

            var dlg = new OptionFeatureForm(ft);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Level.Features.Add(dlg.Feature);
                update_features();
            }
        }

        private void FeatureRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedFeature != null)
            {
                Level.Features.Remove(SelectedFeature);
                update_features();
            }
        }

        private void FeatureEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedFeature != null)
            {
                var index = Level.Features.IndexOf(SelectedFeature);

                var dlg = new OptionFeatureForm(SelectedFeature);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Level.Features[index] = dlg.Feature;
                    update_features();
                }
            }
        }

        private void update_features()
        {
            FeatureList.Items.Clear();
            foreach (var ft in Level.Features)
            {
                var lvi = FeatureList.Items.Add(ft.Name);
                lvi.Tag = ft;
            }

            if (Level.Features.Count == 0)
            {
                var lvi = FeatureList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void PowerAddBtn_Click(object sender, EventArgs e)
        {
            var power = new PlayerPower();
            power.Name = "New Power";

            var dlg = new OptionPowerForm(power);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Level.Powers.Add(dlg.Power);
                update_powers();
            }
        }

        private void PowerRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                Level.Powers.Remove(SelectedPower);
                update_powers();
            }
        }

        private void PowerEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Level.Powers.IndexOf(SelectedPower);

                var dlg = new OptionPowerForm(SelectedPower);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Level.Powers[index] = dlg.Power;
                    update_powers();
                }
            }
        }

        private void update_powers()
        {
            PowerList.Items.Clear();
            foreach (var power in Level.Powers)
            {
                var lvi = PowerList.Items.Add(power.Name);
                lvi.Tag = power;
            }

            if (Level.Powers.Count == 0)
            {
                var lvi = PowerList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
