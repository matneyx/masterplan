using System;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EffectListForm : Form
    {
        private readonly CombatData _fCurrentActor;
        private readonly int _fCurrentRound = int.MinValue;

        private readonly Encounter _fEncounter;

        public Pair<CombatData, OngoingCondition> SelectedEffect
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as Pair<CombatData, OngoingCondition>;

                return null;
            }
        }

        public EffectListForm(Encounter enc, CombatData currentActor, int currentRound)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fEncounter = enc;
            _fCurrentActor = currentActor;
            _fCurrentRound = currentRound;

            update_list();
        }

        ~EffectListForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RemoveBtn.Enabled = SelectedEffect != null;
            EditBtn.Enabled = SelectedEffect != null;
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                var cd = SelectedEffect.First;
                var oc = SelectedEffect.Second;

                cd.Conditions.Remove(oc);
                update_list();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                var cd = SelectedEffect.First;
                var oc = SelectedEffect.Second;

                var index = cd.Conditions.IndexOf(oc);

                var dlg = new EffectForm(oc, _fEncounter, _fCurrentActor, _fCurrentRound);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    cd.Conditions[index] = dlg.Effect;
                    update_list();
                }
            }
        }

        private void update_list()
        {
            EffectList.Groups.Clear();
            EffectList.Items.Clear();

            foreach (var hero in Session.Project.Heroes)
            {
                var cd = hero.CombatData;

                if (cd.Conditions.Count > 0)
                    add_conditions(cd);
            }

            foreach (var slot in _fEncounter.Slots)
            foreach (var cd in slot.CombatData)
                if (cd.Conditions.Count > 0)
                    add_conditions(cd);
        }

        private void add_conditions(CombatData cd)
        {
            var lvg = EffectList.Groups.Add(cd.DisplayName, cd.DisplayName);

            foreach (var oc in cd.Conditions)
            {
                var lvi = EffectList.Items.Add(oc.ToString(_fEncounter, false));
                lvi.Tag = new Pair<CombatData, OngoingCondition>(cd, oc);
                lvi.Group = lvg;
            }
        }
    }
}
