using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TerrainPowerDetailsForm : Form
    {
        private readonly TerrainPower _fTerrainPower;

        public TerrainPowerDetailsForm(TerrainPower tp)
        {
            InitializeComponent();

            _fTerrainPower = tp.Copy();

            Browser.DocumentText = Html.TerrainPower(_fTerrainPower, Session.Preferences.TextSize);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowTerrainPower(_fTerrainPower);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fTerrainPower.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
