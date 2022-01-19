using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class CreatureDetailsForm : Form
    {
        private readonly EncounterCard _fCard;

        public CreatureDetailsForm(EncounterCard card)
        {
            InitializeComponent();

            _fCard = card;

            Browser.DocumentText = Html.StatBlock(_fCard, null, null, true, false, true, CardMode.View,
                Session.Preferences.TextSize);
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (_fCard != null)
            {
                if (Session.PlayerView == null)
                    Session.PlayerView = new PlayerViewForm(this);

                Session.PlayerView.ShowEncounterCard(_fCard);
            }
        }

        private void ExportHTML_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = _fCard.Title;
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }
    }
}
