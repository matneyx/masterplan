using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class SavingThrowForm : Form
    {
        private readonly EncounterCard _fCard;

        private readonly CombatData _fData;
        private readonly Encounter _fEncounter;

        private readonly Dictionary<OngoingCondition, int> _fRolls = new Dictionary<OngoingCondition, int>();

        public OngoingCondition SelectedEffect
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as OngoingCondition;

                return null;
            }
        }

        public SavingThrowForm(CombatData data, EncounterCard card, Encounter enc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fData = data;
            _fCard = card;
            _fEncounter = enc;

            Text = "Saving Throws: " + _fData.DisplayName;

            foreach (var oc in _fData.Conditions)
            {
                if (oc.Duration != DurationType.SaveEnds)
                    continue;

                var roll = _fCard != null ? Session.Dice(1, 20) : 0;
                _fRolls[oc] = roll;
            }

            var saveModifier = 0;
            if (_fCard != null)
                switch (_fCard.Flag)
                {
                    case RoleFlag.Elite:
                        saveModifier = 2;
                        break;
                    case RoleFlag.Solo:
                        saveModifier = 5;
                        break;
                }

            ModBox.Value = saveModifier;

            update_list();
        }

        ~SavingThrowForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            AddBtn.Enabled = SelectedEffect != null;
            SubtractBtn.Enabled = SelectedEffect != null;
            RollBtn.Enabled = SelectedEffect != null;

            if (SelectedEffect == null)
            {
                SavedBtn.Enabled = false;
                NotSavedBtn.Enabled = false;
            }
            else
            {
                var roll = _fRolls[SelectedEffect];

                SavedBtn.Enabled = roll != int.MaxValue;
                NotSavedBtn.Enabled = roll != int.MinValue;
            }
        }

        private void ModBox_ValueChanged(object sender, EventArgs e)
        {
            update_list();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var mod = (int)ModBox.Value;

            var obsolete = new List<OngoingCondition>();
            foreach (var oc in _fData.Conditions)
            {
                if (oc.Duration != DurationType.SaveEnds)
                    continue;

                var save = _fRolls[oc];
                if (save == 0)
                    continue;

                var result = save + mod;
                if (result >= 10)
                    obsolete.Add(oc);
            }

            foreach (var oc in obsolete) _fData.Conditions.Remove(oc);
        }

        private void update_list()
        {
            var selection = SelectedEffect;

            EffectList.BeginUpdate();
            EffectList.Items.Clear();
            foreach (var oc in _fData.Conditions)
            {
                if (oc.Duration != DurationType.SaveEnds)
                    continue;

                var mod = (int)ModBox.Value;
                var roll = _fRolls[oc];

                var lvi = EffectList.Items.Add(oc.ToString(_fEncounter, false));
                lvi.Tag = oc;
                if (oc == selection)
                    lvi.Selected = true;

                if (roll == 0)
                {
                    lvi.SubItems.Add("(not rolled)");
                    lvi.SubItems.Add("(not rolled)");

                    lvi.ForeColor = SystemColors.GrayText;
                }
                else if (roll == int.MinValue)
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("Not saved");
                }
                else if (roll == int.MaxValue)
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("Saved");
                    lvi.ForeColor = SystemColors.GrayText;
                }
                else
                {
                    var result = roll + oc.SavingThrowModifier + mod;

                    if (result == roll)
                        lvi.SubItems.Add(roll.ToString());
                    else
                        lvi.SubItems.Add(roll + " => " + result);

                    if (result >= 10)
                    {
                        lvi.SubItems.Add("Saved");
                        lvi.ForeColor = SystemColors.GrayText;
                    }
                    else
                    {
                        lvi.SubItems.Add("Not saved");
                    }
                }
            }

            if (EffectList.Items.Count == 0)
            {
                var lvi = EffectList.Items.Add("(no conditions)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            EffectList.EndUpdate();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                _fRolls[SelectedEffect] += 1;
                update_list();
            }
        }

        private void SubtractBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                _fRolls[SelectedEffect] -= 1;
                _fRolls[SelectedEffect] = Math.Max(_fRolls[SelectedEffect], 0);
                update_list();
            }
        }

        private void RollBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                _fRolls[SelectedEffect] = Session.Dice(1, 20);
                update_list();
            }
        }

        private void SavedBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                _fRolls[SelectedEffect] = int.MaxValue;
                update_list();
            }
        }

        private void NotSavedBtn_Click(object sender, EventArgs e)
        {
            if (SelectedEffect != null)
            {
                _fRolls[SelectedEffect] = int.MinValue;
                update_list();
            }
        }
    }
}
