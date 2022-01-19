using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class ThemeForm : Form
    {
        public EncounterCard Card { get; }

        public ThemeForm(EncounterCard card)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Browser.DocumentText = "";

            Card = card.Copy();

            if (Card.ThemeId != Guid.Empty)
            {
                var mt = Session.FindTheme(Card.ThemeId, SearchType.Global);
                update_selected_theme(mt, false);

                var attack = mt.FindPower(Card.ThemeAttackPowerId);
                AttackBox.SelectedItem = attack;

                var utility = mt.FindPower(Card.ThemeUtilityPowerId);
                UtilityBox.SelectedItem = utility;
            }
            else
            {
                update_selected_theme(null, true);
            }
        }

        ~ThemeForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            SelectThemeBtn.Enabled = Session.Themes.Count != 0;
            ClearThemeBtn.Enabled = Card.ThemeId != Guid.Empty;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            //
        }

        private void SelectThemeBtn_Click(object sender, EventArgs e)
        {
            var dlg = new MonsterThemeSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
                update_selected_theme(dlg.MonsterTheme, true);
        }

        private void CreateThemeBtn_Click(object sender, EventArgs e)
        {
            var mt = new MonsterTheme();
            mt.Name = "New Theme";

            var dlg = new MonsterThemeForm(mt);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Session.Project.Library.Themes.Add(dlg.Theme);
                Session.Modified = true;

                update_selected_theme(dlg.Theme, true);
            }
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            update_selected_theme(null, true);
        }

        private void AttackBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tpd = AttackBox.SelectedItem as ThemePowerData;
            Card.ThemeAttackPowerId = tpd != null ? tpd.Power.Id : Guid.Empty;

            update_browser();
        }

        private void UtilityBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tpd = UtilityBox.SelectedItem as ThemePowerData;
            Card.ThemeUtilityPowerId = tpd != null ? tpd.Power.Id : Guid.Empty;

            update_browser();
        }

        private void update_selected_theme(MonsterTheme theme, bool resetPowers)
        {
            if (theme != null)
            {
                ThemeNameLbl.Text = theme.Name;
                Card.ThemeId = theme.Id;
            }
            else
            {
                ThemeNameLbl.Text = "None";
                Card.ThemeId = Guid.Empty;
            }

            AttackBox.Items.Clear();
            AttackBox.Items.Add("(no attack power)");

            UtilityBox.Items.Clear();
            UtilityBox.Items.Add("(no utility power)");

            if (theme != null)
            {
                var attacks = theme.ListPowers(Card.Roles, PowerType.Attack);
                foreach (var tpd in attacks)
                    AttackBox.Items.Add(tpd);

                var utilities = theme.ListPowers(Card.Roles, PowerType.Utility);
                foreach (var tpd in utilities)
                    UtilityBox.Items.Add(tpd);
            }

            if (resetPowers)
            {
                AttackBox.SelectedIndex = 0;
                UtilityBox.SelectedIndex = 0;
            }

            AttackBox.Enabled = AttackBox.Items.Count > 1;
            UtilityBox.Enabled = UtilityBox.Items.Count > 1;

            update_browser();
        }

        private void update_browser()
        {
            Browser.Document.OpenNew(true);
            Browser.Document.Write(Html.StatBlock(Card, null, null, true, false, true, CardMode.View,
                Session.Preferences.TextSize));
        }
    }
}
