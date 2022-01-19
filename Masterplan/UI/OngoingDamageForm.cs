using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class OngoingDamageForm : Form
    {
        private readonly EncounterCard _fCard;

        private readonly CombatData _fData;
        private readonly Encounter _fEncounter;

        private int _fTotalDamage;

        public OngoingDamageForm(CombatData data, EncounterCard card, Encounter enc)
        {
            InitializeComponent();

            _fData = data;
            _fCard = card;
            _fEncounter = enc;

            Text = "Ongoing Damage: " + _fData.DisplayName;

            update_list();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var damageRemaining = _fTotalDamage;

            if (_fData.TempHp > 0)
            {
                var delta = Math.Min(damageRemaining, _fData.TempHp);
                _fData.TempHp -= delta;
                damageRemaining -= delta;
            }

            _fData.Damage += damageRemaining;
        }

        private void update_list()
        {
            DamageList.Items.Clear();
            _fTotalDamage = 0;

            foreach (var oc in _fData.Conditions)
            {
                if (oc.Type != OngoingType.Damage)
                    continue;

                var value = oc.Value;
                var dm = find_damage_modifier(oc.DamageType);
                if (dm != null)
                {
                    if (dm.Value == 0)
                    {
                        value = 0;
                    }
                    else
                    {
                        value += dm.Value;
                        value = Math.Max(value, 0);
                    }
                }

                var lvi = DamageList.Items.Add(oc.ToString(_fEncounter, false));
                lvi.SubItems.Add(dm != null ? dm.ToString() : "");
                lvi.SubItems.Add(value.ToString());
                lvi.Tag = oc;
                lvi.Group = DamageList.Groups[0];

                _fTotalDamage += value;
            }

            var lviTotal = DamageList.Items.Add("Total");
            lviTotal.SubItems.Add("");
            lviTotal.SubItems.Add(_fTotalDamage.ToString());
            lviTotal.Group = DamageList.Groups[1];
            lviTotal.Font = new Font(Font, Font.Style | FontStyle.Bold);

            if (_fData.Conditions.Count == 0)
            {
                var lvi = DamageList.Items.Add("(no damage)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private DamageModifier find_damage_modifier(DamageType type)
        {
            if (_fCard == null)
                return null;

            var types = new List<DamageType>();
            types.Add(type);
            var value = _fCard.GetDamageModifier(types, _fData);

            if (value == 0)
                // No modifier
                return null;

            if (value == int.MinValue)
                // Immune
                value = 0;

            var mod = new DamageModifier();
            mod.Type = type;
            mod.Value = value;

            return mod;
        }
    }
}
