using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureTemplateDetailsForm : Form
    {
        private readonly CreatureTemplate _fTemplate;

        public CreatureTemplateDetailsForm(CreatureTemplate ct)
        {
            InitializeComponent();

            _fTemplate = ct.Copy();

            Browser.DocumentText = Html.CreatureTemplate(_fTemplate, Session.Preferences.TextSize, false);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowCreatureTemplate(_fTemplate);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fTemplate.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
