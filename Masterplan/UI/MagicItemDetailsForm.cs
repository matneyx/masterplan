using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class MagicItemDetailsForm : Form
    {
        private readonly MagicItem _fItem;

        public MagicItemDetailsForm(MagicItem item)
        {
            InitializeComponent();

            _fItem = item.Copy();

            Browser.DocumentText = Html.MagicItem(_fItem, Session.Preferences.TextSize, false, true);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowMagicItem(_fItem);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fItem.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
