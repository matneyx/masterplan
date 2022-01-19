using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionParagonPathForm : Form
    {
        public ParagonPath ParagonPath { get; }

        public LevelData SelectedLevel
        {
            get
            {
                if (LevelList.SelectedItems.Count != 0)
                    return LevelList.SelectedItems[0].Tag as LevelData;

                return null;
            }
        }

        public OptionParagonPathForm(ParagonPath pp)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            ParagonPath = pp.Copy();

            NameBox.Text = ParagonPath.Name;
            PrereqBox.Text = ParagonPath.Prerequisites;
            DetailsBox.Text = ParagonPath.Details;
            QuoteBox.Text = ParagonPath.Quote;

            update_levels();
        }

        ~OptionParagonPathForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LevelEditBtn.Enabled = SelectedLevel != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            ParagonPath.Name = NameBox.Text;
            ParagonPath.Prerequisites = PrereqBox.Text;
            ParagonPath.Details = DetailsBox.Text;
            ParagonPath.Quote = QuoteBox.Text;
        }

        private void FeatureEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = ParagonPath.Levels.IndexOf(SelectedLevel);

                var dlg = new OptionLevelForm(SelectedLevel, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ParagonPath.Levels[index] = dlg.Level;
                    update_levels();
                }
            }
        }

        private void update_levels()
        {
            LevelList.Items.Clear();
            foreach (var ld in ParagonPath.Levels)
            {
                var lvi = LevelList.Items.Add(ld.ToString());
                lvi.Tag = ld;

                if (ld.Count == 0)
                    lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
