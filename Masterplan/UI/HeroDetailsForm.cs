using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class HeroDetailsForm : Form
    {
        private readonly Hero _fHero;

        public HeroDetailsForm(Hero hero)
        {
            InitializeComponent();

            _fHero = hero.Copy();

            Browser.DocumentText = Html.StatBlock(_fHero, null, true, false, false, Session.Preferences.TextSize);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            Session.PlayerView.ShowHero(_fHero);
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fHero.Name;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
