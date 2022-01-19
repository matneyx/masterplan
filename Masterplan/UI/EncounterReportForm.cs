using System;
using System.IO;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EncounterReportForm : Form
    {
        private readonly Encounter _fEncounter;

        private readonly EncounterReport _fReport;
        private BreakdownType _fBreakdownType = BreakdownType.Individual;

        private ReportType _fReportType = ReportType.Time;

        public EncounterReportForm(EncounterLog log, Encounter enc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fReport = log.CreateReport(enc, true);
            _fEncounter = enc;

            if (_fEncounter.MapId == Guid.Empty)
                ReportBtn.DropDownItems.Remove(ReportMovement);

            update_report();
            update_mvp();
        }

        ~EncounterReportForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ReportTime.Checked = _fReportType == ReportType.Time;
            ReportDamageEnemies.Checked = _fReportType == ReportType.DamageToEnemies;
            ReportDamageAllies.Checked = _fReportType == ReportType.DamageToAllies;
            ReportMovement.Checked = _fReportType == ReportType.Movement;

            BreakdownIndividually.Checked = _fBreakdownType == BreakdownType.Individual;
            BreakdownByController.Checked = _fBreakdownType == BreakdownType.Controller;
            BreakdownByFaction.Checked = _fBreakdownType == BreakdownType.Faction;
        }

        private void ReportTime_Click(object sender, EventArgs e)
        {
            _fReportType = ReportType.Time;
            update_report();
        }

        private void ReportDamageEnemies_Click(object sender, EventArgs e)
        {
            _fReportType = ReportType.DamageToEnemies;
            update_report();
        }

        private void ReportDamageAllies_Click(object sender, EventArgs e)
        {
            _fReportType = ReportType.DamageToAllies;
            update_report();
        }

        private void ReportMovement_Click(object sender, EventArgs e)
        {
            _fReportType = ReportType.Movement;
            update_report();
        }

        private void BreakdownIndividually_Click(object sender, EventArgs e)
        {
            _fBreakdownType = BreakdownType.Individual;
            update_report();
        }

        private void BreakdownByController_Click(object sender, EventArgs e)
        {
            _fBreakdownType = BreakdownType.Controller;
            update_report();
        }

        private void BreakdownByFaction_Click(object sender, EventArgs e)
        {
            _fBreakdownType = BreakdownType.Faction;
            update_report();
        }

        private void ExportBtn_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.FileName = "Encounter Report";
            dlg.Filter = Program.HtmlFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
                File.WriteAllText(dlg.FileName, Browser.DocumentText);
        }

        private void update_report()
        {
            var table = _fReport.CreateTable(_fReportType, _fBreakdownType, _fEncounter);
            Browser.DocumentText = Html.EncounterReportTable(table, Session.Preferences.TextSize);

            Graph.ShowTable(table);
        }

        private void update_mvp()
        {
            var ids = _fReport.MvPs(_fEncounter);
            var mvps = "";
            foreach (var id in ids)
            {
                var hero = Session.Project.FindHero(id);
                if (hero != null)
                {
                    if (mvps != "")
                        mvps += ", ";

                    mvps += hero.Name;
                }
            }

            if (mvps != "")
            {
                MVPLbl.Text = "MVP: " + mvps;
            }
            else
            {
                MVPLbl.Text = "(no MVP for this encounter)";
                MVPLbl.Enabled = false;
            }
        }

        private void PlayerViewBtn_Click(object sender, EventArgs e)
        {
            if (Session.PlayerView == null)
                Session.PlayerView = new PlayerViewForm(this);

            var table = _fReport.CreateTable(_fReportType, _fBreakdownType, _fEncounter);
            Session.PlayerView.ShowEncounterReportTable(table);
        }
    }
}
