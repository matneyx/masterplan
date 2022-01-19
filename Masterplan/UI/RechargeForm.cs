using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class RechargeForm : Form
    {
        private readonly EncounterCard _fCard;

        private readonly CombatData _fData;

        private readonly Dictionary<Guid, int> _fRolls = new Dictionary<Guid, int>();

        public Guid SelectedPowerId
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return (Guid)EffectList.SelectedItems[0].Tag;

                return Guid.Empty;
            }
        }

        public RechargeForm(CombatData data, EncounterCard card)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fData = data;
            _fCard = card;

            Text = "Power Recharging: " + _fData.DisplayName;

            foreach (var powerId in _fData.UsedPowers)
            {
                var power = get_power(powerId);
                if (power?.Action == null || power.Action.Recharge == "")
                    continue;

                _fRolls[powerId] = Session.Dice(1, 6);
            }

            update_list();
        }

        ~RechargeForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            RollBtn.Enabled = SelectedPowerId != Guid.Empty;

            if (SelectedPowerId == Guid.Empty)
            {
                SavedBtn.Enabled = false;
                NotSavedBtn.Enabled = false;
            }
            else
            {
                var roll = _fRolls[SelectedPowerId];

                SavedBtn.Enabled = roll != int.MaxValue;
                NotSavedBtn.Enabled = roll != int.MinValue;
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var obsolete = new List<Guid>();
            foreach (var powerId in _fRolls.Keys)
            {
                if (!_fRolls.ContainsKey(powerId))
                    continue;

                var roll = _fRolls[powerId];

                var power = get_power(powerId);
                if (power?.Action == null || power.Action.Recharge == "")
                    continue;

                var min = get_minimum(power.Action.Recharge);
                if (min != 0 && roll >= min)
                    obsolete.Add(powerId);
            }

            foreach (var powerId in obsolete) _fData.UsedPowers.Remove(powerId);
        }

        private void RollBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPowerId != Guid.Empty)
            {
                _fRolls[SelectedPowerId] = Session.Dice(1, 6);
                update_list();
            }
        }

        private void SavedBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPowerId != Guid.Empty)
            {
                _fRolls[SelectedPowerId] = int.MaxValue;
                update_list();
            }
        }

        private void NotSavedBtn_Click(object sender, EventArgs e)
        {
            if (SelectedPowerId != Guid.Empty)
            {
                _fRolls[SelectedPowerId] = int.MinValue;
                update_list();
            }
        }

        private void update_list()
        {
            var selection = SelectedPowerId;

            EffectList.BeginUpdate();
            EffectList.Items.Clear();
            foreach (var powerId in _fData.UsedPowers)
            {
                if (!_fRolls.ContainsKey(powerId))
                    continue;

                var power = get_power(powerId);
                if (power == null)
                    continue;

                var roll = _fRolls[powerId];

                var lvi = EffectList.Items.Add(power.Name);
                lvi.SubItems.Add(power.Action.Recharge);
                lvi.Tag = power.Id;
                if (powerId == selection)
                    lvi.Selected = true;

                if (roll == int.MinValue)
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("Not recharged");
                }
                else if (roll == int.MaxValue)
                {
                    lvi.SubItems.Add("-");
                    lvi.SubItems.Add("Recharged");
                    lvi.ForeColor = SystemColors.GrayText;
                }
                else
                {
                    var min = get_minimum(power.Action.Recharge);
                    if (min == int.MaxValue)
                    {
                        lvi.SubItems.Add("Not rolled");
                        lvi.SubItems.Add("Not rolled");
                    }
                    else
                    {
                        lvi.SubItems.Add(roll.ToString());

                        if (roll >= min)
                        {
                            lvi.SubItems.Add("Recharged");
                            lvi.ForeColor = SystemColors.GrayText;
                        }
                        else
                        {
                            lvi.SubItems.Add("Not recharged");
                        }
                    }
                }
            }

            if (EffectList.Items.Count == 0)
            {
                var lvi = EffectList.Items.Add("(no rechargable powers)");
                lvi.ForeColor = SystemColors.GrayText;
            }

            EffectList.EndUpdate();
        }

        private CreaturePower get_power(Guid powerId)
        {
            var powers = _fCard.CreaturePowers;
            foreach (var power in powers)
                if (power.Id == powerId)
                    return power;

            return null;
        }

        private int get_minimum(string rechargeStr)
        {
            var min = int.MaxValue;

            if (rechargeStr.Contains("6"))
                min = 6;

            if (rechargeStr.Contains("5"))
                min = 5;

            if (rechargeStr.Contains("4"))
                min = 4;

            if (rechargeStr.Contains("3"))
                min = 3;

            if (rechargeStr.Contains("2"))
                min = 2;

            return min;
        }
    }
}
