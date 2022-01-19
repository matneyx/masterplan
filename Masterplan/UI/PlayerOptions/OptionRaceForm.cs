using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionRaceForm : Form
    {
        public Race Race { get; }

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

        public OptionRaceForm(Race race)
        {
            InitializeComponent();

            var sizes = Enum.GetValues(typeof(CreatureSize));
            foreach (CreatureSize size in sizes)
                SizeBox.Items.Add(size);

            Application.Idle += Application_Idle;

            Race = race.Copy();

            NameBox.Text = Race.Name;

            HeightBox.Text = Race.HeightRange;
            WeightBox.Text = Race.WeightRange;
            AbilityScoreBox.Text = Race.AbilityScores;
            SizeBox.SelectedItem = Race.Size;
            SpeedBox.Text = Race.Speed;
            VisionBox.Text = Race.Vision;
            LanguageBox.Text = Race.Languages;
            SkillBonusBox.Text = Race.SkillBonuses;

            DetailsBox.Text = Race.Details;
            QuoteBox.Text = Race.Quote;

            update_features();
            update_powers();
        }

        ~OptionRaceForm()
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
            Race.Name = NameBox.Text;
            Race.HeightRange = HeightBox.Text;
            Race.WeightRange = WeightBox.Text;
            Race.AbilityScores = AbilityScoreBox.Text;
            Race.Size = (CreatureSize)SizeBox.SelectedItem;
            Race.Speed = SpeedBox.Text;
            Race.Vision = VisionBox.Text;
            Race.Languages = LanguageBox.Text;
            Race.SkillBonuses = SkillBonusBox.Text;
            Race.Details = DetailsBox.Text;
            Race.Quote = QuoteBox.Text;
        }

        private void FeatureAddBtn_Click(object sender, EventArgs e)
        {
            var ft = new Feature();
            ft.Name = "New Feature";

            var dlg = new OptionFeatureForm(ft);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Race.Features.Add(dlg.Feature);
                update_features();
            }
        }

        private void FeatureRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedFeature != null)
            {
                Race.Features.Remove(SelectedFeature);
                update_features();
            }
        }

        private void FeatureEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedFeature != null)
            {
                var index = Race.Features.IndexOf(SelectedFeature);

                var dlg = new OptionFeatureForm(SelectedFeature);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Race.Features[index] = dlg.Feature;
                    update_features();
                }
            }
        }

        private void update_features()
        {
            FeatureList.Items.Clear();
            foreach (var ft in Race.Features)
            {
                var lvi = FeatureList.Items.Add(ft.Name);
                lvi.Tag = ft;
            }

            if (Race.Features.Count == 0)
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
                Race.Powers.Add(dlg.Power);
                update_powers();
            }
        }

        private void PowerRemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                Race.Powers.Remove(SelectedPower);
                update_powers();
            }
        }

        private void PowerEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPower != null)
            {
                var index = Race.Powers.IndexOf(SelectedPower);

                var dlg = new OptionPowerForm(SelectedPower);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Race.Powers[index] = dlg.Power;
                    update_powers();
                }
            }
        }

        private void update_powers()
        {
            PowerList.Items.Clear();
            foreach (var power in Race.Powers)
            {
                var lvi = PowerList.Items.Add(power.Name);
                lvi.Tag = power;
            }

            if (Race.Powers.Count == 0)
            {
                var lvi = PowerList.Items.Add("(none)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
