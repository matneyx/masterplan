using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CombatantSelectForm : Form
    {
        public CombatData SelectedCombatant
        {
            get
            {
                if (CombatantList.SelectedItems.Count != 0)
                    return CombatantList.SelectedItems[0].Tag as CombatData;

                return null;
            }
        }

        public CombatantSelectForm(Encounter enc, Dictionary<Guid, CombatData> traps)
        {
            InitializeComponent();

            foreach (var slot in enc.Slots)
            foreach (var cd in slot.CombatData)
            {
                var lvi = CombatantList.Items.Add(cd.DisplayName);
                lvi.Tag = cd;
                lvi.Group = CombatantList.Groups[1];
            }

            foreach (var hero in Session.Project.Heroes)
            {
                var lvi = CombatantList.Items.Add(hero.Name);
                lvi.Tag = hero.CombatData;
                lvi.Group = CombatantList.Groups[0];
            }

            foreach (var trap in traps.Values)
            {
                var lvi = CombatantList.Items.Add(trap.DisplayName);
                lvi.Tag = trap;
                lvi.Group = CombatantList.Groups[2];
            }

            Application.Idle += Application_Idle;
        }

        ~CombatantSelectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            OKBtn.Enabled = SelectedCombatant != null;
        }

        private void TileList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCombatant != null)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}
