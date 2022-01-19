using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EndedEffectsForm : Form
    {
        private readonly Encounter _fEncounter;

        public Pair<CombatData, OngoingCondition> SelectedCondition
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as Pair<CombatData, OngoingCondition>;

                return null;
            }
        }

        public List<Pair<CombatData, OngoingCondition>> EndedConditions { get; set; }

        public List<Pair<CombatData, OngoingCondition>> ExtendedConditions { get; set; }

        public EndedEffectsForm(List<Pair<CombatData, OngoingCondition>> conditions, Encounter enc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            EndedConditions = conditions;
            ExtendedConditions = new List<Pair<CombatData, OngoingCondition>>();
            _fEncounter = enc;

            update_list();
        }

        ~EndedEffectsForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ExtendBtn.Enabled = SelectedCondition != null;

            if (SelectedCondition != null)
            {
                if (EndedConditions.Contains(SelectedCondition))
                    ExtendBtn.Text = "Extend this effect";
                else
                    ExtendBtn.Text = "End this effect";
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            foreach (var pair in EndedConditions)
                pair.First.Conditions.Remove(pair.Second);
        }

        private void ExtendBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCondition == null)
                return;

            if (EndedConditions.Contains(SelectedCondition))
            {
                EndedConditions.Remove(SelectedCondition);
                ExtendedConditions.Add(SelectedCondition);
            }
            else if (ExtendedConditions.Contains(SelectedCondition))
            {
                ExtendedConditions.Remove(SelectedCondition);
                EndedConditions.Add(SelectedCondition);
            }

            update_list();
        }

        private void update_list()
        {
            EffectList.Items.Clear();

            foreach (var condition in EndedConditions)
            {
                var lvi = EffectList.Items.Add(condition.First.ToString());
                lvi.SubItems.Add(condition.Second.ToString(_fEncounter, false));
                lvi.Group = EffectList.Groups[0];
                lvi.Tag = condition;
            }

            if (EndedConditions.Count == 0)
            {
                var lvi = EffectList.Items.Add("(none)");
                lvi.Group = EffectList.Groups[0];
                lvi.ForeColor = SystemColors.GrayText;
            }

            foreach (var condition in ExtendedConditions)
            {
                var lvi = EffectList.Items.Add(condition.First.ToString());
                lvi.SubItems.Add(condition.Second.ToString(_fEncounter, false));
                lvi.Group = EffectList.Groups[1];
                lvi.Tag = condition;
            }

            if (ExtendedConditions.Count == 0)
            {
                var lvi = EffectList.Items.Add("(none)");
                lvi.Group = EffectList.Groups[1];
                lvi.ForeColor = SystemColors.GrayText;
            }
        }
    }
}
