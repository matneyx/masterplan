using System;
using System.Windows.Forms;
using Masterplan.Controls;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class DemographicsForm : Form
    {
        public DemographicsForm(Library library, DemographicsSource source)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            BreakdownPanel.Library = library;
            BreakdownPanel.Source = source;
        }

        ~DemographicsForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RoleBtn.Enabled = BreakdownPanel.Source != DemographicsSource.MagicItems;
            StatusBtn.Enabled = BreakdownPanel.Source != DemographicsSource.MagicItems;

            LevelBtn.Checked = BreakdownPanel.Mode == DemographicsMode.Level;
            RoleBtn.Checked = BreakdownPanel.Mode == DemographicsMode.Role;
            StatusBtn.Checked = BreakdownPanel.Mode == DemographicsMode.Status;
        }

        private void LevelBtn_Click(object sender, EventArgs e)
        {
            BreakdownPanel.Mode = DemographicsMode.Level;
        }

        private void RoleBtn_Click(object sender, EventArgs e)
        {
            BreakdownPanel.Mode = DemographicsMode.Role;
        }

        private void StatusBtn_Click(object sender, EventArgs e)
        {
            BreakdownPanel.Mode = DemographicsMode.Status;
        }
    }
}
