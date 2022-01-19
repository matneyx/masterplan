using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TerrainPowerSelectForm : Form
    {
        public TerrainPower TerrainPower
        {
            get
            {
                if (ChallengeList.SelectedItems.Count != 0)
                    return ChallengeList.SelectedItems[0].Tag as TerrainPower;

                return null;
            }
        }

        public TerrainPowerSelectForm()
        {
            InitializeComponent();

            var challenges = Session.TerrainPowers;

            foreach (var sc in challenges)
            {
                var lvi = ChallengeList.Items.Add(sc.Name);
                lvi.SubItems.Add(sc.Name);
                lvi.Tag = sc;
            }

            Application.Idle += Application_Idle;

            Browser.DocumentText = "";
            ChallengeList_SelectedIndexChanged(null, null);
        }

        ~TerrainPowerSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = TerrainPower != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (TerrainPower != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ChallengeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.TerrainPower(TerrainPower, Session.Preferences.TextSize);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }
    }
}
