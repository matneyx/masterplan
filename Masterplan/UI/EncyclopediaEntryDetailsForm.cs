using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EncyclopediaEntryDetailsForm : Form
    {
        private readonly EncyclopediaEntry _fEntry;
        private bool _fShowDmInfo;

        public EncyclopediaEntryDetailsForm(EncyclopediaEntry entry)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fEntry = entry;

            update_entry();
        }

        ~EncyclopediaEntryDetailsForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            DMBtn.Checked = _fShowDmInfo;
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (_fEntry != null)
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowEncyclopediaItem(_fEntry);
            }
        }

        private void DMBtn_Click(object sender, EventArgs e)
        {
            _fShowDmInfo = !_fShowDmInfo;
            update_entry();
        }

        private void update_entry()
        {
            Browser.DocumentText = Html.EncyclopediaEntry(_fEntry, Session.Project.Encyclopedia,
                Session.Preferences.TextSize, _fShowDmInfo, false, false, true);
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "picture")
            {
                e.Cancel = true;
                var id = new Guid(e.Url.LocalPath);

                var img = _fEntry.FindImage(id);
                if (img != null)
                {
                    var dlg = new EncyclopediaImageForm(img);
                    dlg.ShowDialog();
                }
            }
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fEntry.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
