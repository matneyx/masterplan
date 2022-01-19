using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI.PlayerOptions
{
    internal partial class OptionEpicDestinyForm : Form
    {
        public EpicDestiny EpicDestiny { get; }

        public LevelData SelectedLevel
        {
            get
            {
                if (LevelList.SelectedItems.Count != 0)
                    return LevelList.SelectedItems[0].Tag as LevelData;

                return null;
            }
        }

        public OptionEpicDestinyForm(EpicDestiny pp)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            EpicDestiny = pp.Copy();

            NameBox.Text = EpicDestiny.Name;
            PrereqBox.Text = EpicDestiny.Prerequisites;
            DetailsBox.Text = EpicDestiny.Details;
            QuoteBox.Text = EpicDestiny.Quote;
            ImmortalityBox.Text = EpicDestiny.Immortality;

            update_levels();
        }

        ~OptionEpicDestinyForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LevelEditBtn.Enabled = SelectedLevel != null;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            EpicDestiny.Name = NameBox.Text;
            EpicDestiny.Prerequisites = PrereqBox.Text;
            EpicDestiny.Details = DetailsBox.Text;
            QuoteBox.Text = EpicDestiny.Quote;
            EpicDestiny.Immortality = ImmortalityBox.Text;
        }

        private void LevelEditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedLevel != null)
            {
                var index = EpicDestiny.Levels.IndexOf(SelectedLevel);

                var dlg = new OptionLevelForm(SelectedLevel, true);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    EpicDestiny.Levels[index] = dlg.Level;
                    update_levels();
                }
            }
        }

        private void update_levels()
        {
            LevelList.Items.Clear();
            foreach (var ld in EpicDestiny.Levels)
            {
                var lvi = LevelList.Items.Add(ld.ToString());
                lvi.Tag = ld;

                if (ld.Count == 0)
                    lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
