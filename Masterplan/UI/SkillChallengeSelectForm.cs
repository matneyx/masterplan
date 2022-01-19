using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class SkillChallengeSelectForm : Form
    {
        public SkillChallenge SkillChallenge
        {
            get
            {
                if (ChallengeList.SelectedItems.Count != 0)
                    return ChallengeList.SelectedItems[0].Tag as SkillChallenge;

                return null;
            }
        }

        public SkillChallengeSelectForm()
        {
            InitializeComponent();

            var challenges = Session.SkillChallenges;

            foreach (var sc in challenges)
            {
                var lvi = ChallengeList.Items.Add(sc.Name);
                lvi.SubItems.Add(sc.Info);
                lvi.Tag = sc;
            }

            Application.Idle += Application_Idle;

            Browser.DocumentText = "";
            ChallengeList_SelectedIndexChanged(null, null);
        }

        ~SkillChallengeSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = SkillChallenge != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (SkillChallenge != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void ChallengeList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var html = Html.SkillChallenge(SkillChallenge, false, true, Session.Preferences.TextSize);

            Browser.Document.OpenNew(true);
            Browser.Document.Write(html);
        }
    }
}
