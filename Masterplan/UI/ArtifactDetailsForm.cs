using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ArtifactDetailsForm : Form
    {
        private readonly Artifact _fArtifact;

        public ArtifactDetailsForm(Artifact artifact)
        {
            InitializeComponent();

            _fArtifact = artifact.Copy();

            Browser.DocumentText = Html.Artifact(_fArtifact, Session.Preferences.TextSize, false, true);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowArtifact(_fArtifact);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fArtifact.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
