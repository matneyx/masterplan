using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class GroupInitiativeForm : Form
    {
        private readonly Encounter _fEncounter;

        public Dictionary<string, List<CombatData>> Combatants { get; }

        public List<CombatData> SelectedCombatantGroup
        {
            get
            {
                if (CombatantList.SelectedItems.Count != 0)
                    return CombatantList.SelectedItems[0].Tag as List<CombatData>;

                return null;
            }
        }

        public GroupInitiativeForm(Dictionary<string, List<CombatData>> combatants, Encounter enc)
        {
            InitializeComponent();

            Combatants = combatants;
            _fEncounter = enc;

            update_list();
        }

        private void CombatantList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedCombatantGroup != null)
            {
                var bonus = 0;

                var cd = SelectedCombatantGroup[0];
                var slot = _fEncounter.FindSlot(cd);
                if (slot != null)
                {
                    bonus = slot.Card.Initiative;
                }
                else
                {
                    // Hero or trap

                    var hero = Session.Project.FindHero(cd.Id);
                    if (hero != null)
                        bonus = hero.InitBonus;

                    var trap = _fEncounter.FindTrap(cd.Id);
                    if (trap != null)
                        bonus = trap.Initiative;
                }

                var dlg = new InitiativeForm(bonus, cd.Initiative);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var data in SelectedCombatantGroup)
                        data.Initiative = dlg.Score;

                    update_list();
                }
            }
        }

        private void update_list()
        {
            CombatantList.Items.Clear();

            foreach (var str in Combatants.Keys)
            {
                var lvi = CombatantList.Items.Add(str);

                var list = Combatants[str];
                var cd = list[0];
                if (cd.Initiative == int.MinValue)
                {
                    var lvsi = lvi.SubItems.Add("(not set)");
                    lvsi.ForeColor = SystemColors.GrayText;
                }
                else
                {
                    lvi.SubItems.Add(cd.Initiative.ToString());
                }

                lvi.UseItemStyleForSubItems = false;
                lvi.Tag = list;
            }
        }
    }
}
