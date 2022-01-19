using System;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CampaignSettingsForm : Form
    {
        private readonly CampaignSettings _fSettings;

        public CampaignSettingsForm(CampaignSettings settings)
        {
            InitializeComponent();

            _fSettings = settings;

            HPBox.Value = (int)(_fSettings.Hp * 100);
            XPBox.Value = (int)(_fSettings.Xp * 100);
            AttackBox.Value = _fSettings.AttackBonus;
            ACBox.Value = _fSettings.AcBonus;
            DefenceBox.Value = _fSettings.NadBonus;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            _fSettings.Hp = (double)HPBox.Value / 100;
            _fSettings.Xp = (double)XPBox.Value / 100;
            _fSettings.AttackBonus = (int)AttackBox.Value;
            _fSettings.AcBonus = (int)ACBox.Value;
            _fSettings.NadBonus = (int)DefenceBox.Value;
        }
    }
}
