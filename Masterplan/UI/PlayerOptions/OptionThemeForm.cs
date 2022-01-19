using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionThemeForm : Form
    {
        public Theme Theme { get; }

        public LevelData SelectedLevel
        {
            get
            {
                if (LevelList.SelectedItems.Count != 0)
                    return LevelList.SelectedItems[0].Tag as LevelData;

                return null;
            }
        }

        public OptionThemeForm(Theme theme)
        {
            InitializeComponent();

            RoleBox.Items.Add("Controller");
            RoleBox.Items.Add("Defender");
            RoleBox.Items.Add("Leader");
            RoleBox.Items.Add("Striker");

            SourceBox.Items.Add("Martial");
            SourceBox.Items.Add("Arcane");
            SourceBox.Items.Add("Divine");
            SourceBox.Items.Add("Primal");
            SourceBox.Items.Add("Psionic");
            SourceBox.Items.Add("Shadow");
            SourceBox.Items.Add("Elemental");

            Application.Idle += Application_Idle;

            Theme = theme.Copy();

            NameBox.Text = Theme.Name;
            PrereqBox.Text = Theme.Prerequisites;
            RoleBox.Text = Theme.SecondaryRole;
            SourceBox.Text = Theme.PowerSource;
            DetailsBox.Text = Theme.Details;
            QuoteBox.Text = Theme.Quote;

            update_levels();
        }

        ~OptionThemeForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LevelEditBtn.Enabled = SelectedLevel != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Theme.Name = NameBox.Text;
            Theme.Prerequisites = PrereqBox.Text;
            Theme.Details = DetailsBox.Text;
            Theme.Quote = QuoteBox.Text;
        }

        private void FeatureEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = Theme.Levels.IndexOf(SelectedLevel);

                var dlg = new OptionLevelForm(SelectedLevel, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Theme.Levels[index] = dlg.Level;
                    update_levels();
                }
            }
        }

        private void update_levels()
        {
            LevelList.Items.Clear();
            foreach (var ld in Theme.Levels)
            {
                var lvi = LevelList.Items.Add(ld.ToString());
                lvi.Tag = ld;

                if (ld.Count == 0)
                    lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void PowerBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OptionPowerForm(Theme.GrantedPower);
            if (dlg.ShowDialog() == DialogResult.OK) Theme.GrantedPower = dlg.Power;
        }
    }
}
