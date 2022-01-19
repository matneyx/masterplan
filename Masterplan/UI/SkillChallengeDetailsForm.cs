using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class SkillChallengeDetailsForm : Form
    {
        private readonly SkillChallenge _fChallenge;

        public SkillChallengeDetailsForm(SkillChallenge sc)
        {
            InitializeComponent();

            _fChallenge = sc.Copy() as SkillChallenge;

            Browser.DocumentText = Html.SkillChallenge(_fChallenge, false, true, Session.Preferences.TextSize);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowSkillChallenge(_fChallenge);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fChallenge.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
