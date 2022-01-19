using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TrapDetailsForm : Form
    {
        private readonly Trap _fTrap;

        public TrapDetailsForm(Trap trap)
        {
            InitializeComponent();

            _fTrap = trap.Copy();

            Browser.DocumentText = Html.Trap(_fTrap, null, true, false, false, Session.Preferences.TextSize);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowTrap(_fTrap);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fTrap.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
