using System;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;

namespace Masterplan.UI
{
    internal partial class CombatDataForm : Form
    {
        private readonly EncounterCard _fCard;

        private readonly CombatData _fCurrentActor;
        private readonly int _fCurrentRound = int.MinValue;
        private readonly Encounter _fEncounter;

        public CombatData Data { get; }

        public OngoingCondition SelectedCondition
        {
            get
            {
                if (EffectList.SelectedItems.Count != 0)
                    return EffectList.SelectedItems[0].Tag as OngoingCondition;

                return null;
            }
        }

        public CombatDataForm(CombatData data, EncounterCard card, Encounter enc, CombatData currentActor,
            int currentRound, bool allowNameEdit)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;
            EffectList_SizeChanged(null, null);

            Data = data.Copy();
            _fCard = card;
            _fEncounter = enc;

            _fCurrentActor = currentActor;
            _fCurrentRound = currentRound;

            if (Data.Initiative == int.MinValue)
                Data.Initiative = 0;

            Text = Data.DisplayName;
            LabelBox.Text = Data.DisplayName;

            if (!allowNameEdit)
                LabelBox.Enabled = false;

            update_hp();
            InitBox.Value = Data.Initiative;
            AltitudeBox.Value = Data.Altitude;
            update_effects();
        }

        ~CombatDataForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            var damage = false;
            foreach (var oc in Data.Conditions)
                if (oc.Type == OngoingType.Damage && oc.Value > 0)
                {
                    damage = true;
                    break;
                }

            RemoveBtn.Enabled = SelectedCondition != null;
            EditBtn.Enabled = SelectedCondition != null;
            SavesBtn.Enabled = Data.Conditions.Count > 0;
            DmgBtn.Enabled = damage;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            Data.DisplayName = LabelBox.Text;
            Data.Initiative = (int)InitBox.Value;
            Data.Altitude = (int)AltitudeBox.Value;
        }

        private void DamageBox_ValueChanged(object sender, EventArgs e)
        {
            Data.Damage = (int)DamageBox.Value;

            update_hp();
        }

        private void TempHPBox_ValueChanged(object sender, EventArgs e)
        {
            Data.TempHp = (int)TempHPBox.Value;

            update_hp();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var oc = new OngoingCondition();

            var dlg = new EffectForm(oc, _fEncounter, _fCurrentActor, _fCurrentRound);
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Data.Conditions.Add(dlg.Effect);
                update_effects();
            }
        }

        private void RemoveBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCondition != null)
            {
                Data.Conditions.Remove(SelectedCondition);
                update_effects();
            }
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (SelectedCondition != null)
            {
                var index = Data.Conditions.IndexOf(SelectedCondition);

                var dlg = new EffectForm(SelectedCondition, _fEncounter, _fCurrentActor, _fCurrentRound);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Data.Conditions[index] = dlg.Effect;
                    update_effects();
                }
            }
        }

        private void DmgBtn_Click(object sender, EventArgs e)
        {
            var dlg = new OngoingDamageForm(Data, _fCard, _fEncounter);
            if (dlg.ShowDialog() == DialogResult.OK) update_hp();
        }

        private void SavesBtn_Click(object sender, EventArgs e)
        {
            var dlg = new SavingThrowForm(Data, _fCard, _fEncounter);
            if (dlg.ShowDialog() == DialogResult.OK) update_effects();
        }

        private void update_hp()
        {
            DamageBox.Value = Data.Damage;
            TempHPBox.Value = Data.TempHp;

            var maxHp = 0;
            if (_fCard != null)
                maxHp = _fCard.Hp;
            else
                // Must be a hero
                foreach (var h in Session.Project.Heroes)
                    if (Data.DisplayName == h.Name)
                        maxHp = h.Hp;
            var currentHp = maxHp - Data.Damage;

            HPBox.Text = currentHp + " HP";
            if (Data.TempHp > 0)
                HPBox.Text += "; " + Data.TempHp + " temp HP";

            if (currentHp + Data.TempHp <= 0)
                HPBox.Text += " (dead)";
            else if (currentHp <= maxHp / 2)
                HPBox.Text += " (bloodied)";

            HPGauge.FullHp = maxHp;
            HPGauge.Damage = Data.Damage;
            HPGauge.TempHp = Data.TempHp;
        }

        private void update_effects()
        {
            EffectList.Items.Clear();
            EffectList.ShowGroups = true;

            foreach (var oc in Data.Conditions)
            {
                var effect = oc.ToString();
                var duration = oc.GetDuration(_fEncounter);
                if (duration == "")
                    duration = "until the end of the encounter";

                var lvi = EffectList.Items.Add(effect);
                lvi.SubItems.Add(duration);

                lvi.Tag = oc;
                lvi.Group = EffectList.Groups[oc.Type == OngoingType.Condition ? 0 : 1];
            }

            if (Data.Conditions.Count == 0)
            {
                EffectList.ShowGroups = false;

                var lvi = EffectList.Items.Add("(no ongoing effects)");
                lvi.ForeColor = SystemColors.GrayText;
            }
        }

        private void EffectList_SizeChanged(object sender, EventArgs e)
        {
            var width = EffectList.Width - (SystemInformation.VerticalScrollBarWidth + 6);
            EffectList.TileSize = new Size(width, EffectList.TileSize.Height);
        }
    }
}
