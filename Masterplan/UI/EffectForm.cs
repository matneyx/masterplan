using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class EffectForm : Form
    {
        private const string Encounter = "Lasts until the end of the encounter";
        private const string SaveEnds = "Save ends";
        private const string Start = "Lasts until the start of someone's next turn";
        private const string End = "Lasts until the end of someone's next turn";

        private const string BlankEffect = "(enter effect name)";

        private const string SomeoneElse = "(someone else)";

        private const string Resist = "Resist";
        private const string Vulnerable = "Vulnerable";
        private const string Immune = "Immune";

        private const int MinHeight = 25;

        private readonly Dictionary<RadioButton, int> _fHeights = new Dictionary<RadioButton, int>();

        private CombatData _fCurrentActor;
        private int _fCurrentRound = int.MinValue;

        public OngoingCondition Effect { get; private set; }

        public EffectForm(OngoingCondition condition, Encounter enc, CombatData currentActor, int currentRound)
        {
            InitializeComponent();

            foreach (var c in Conditions.GetConditions())
                ConditionBox.Items.Add(c);

            foreach (DamageType dt in Enum.GetValues(typeof(DamageType)))
            {
                DamageTypeBox.Items.Add(dt);
                DamageModTypeBox.Items.Add(dt);
            }

            DamageModDirBox.Items.Add(Resist);
            DamageModDirBox.Items.Add(Vulnerable);
            DamageModDirBox.Items.Add(Immune);

            DurationBox.Items.Add(Encounter);
            DurationBox.Items.Add(SaveEnds);
            DurationBox.Items.Add(Start);
            DurationBox.Items.Add(End);

            foreach (var slot in enc.Slots)
            foreach (var cd in slot.CombatData)
                DurationCreatureBox.Items.Add(cd);
            foreach (var hero in Session.Project.Heroes)
                DurationCreatureBox.Items.Add(hero);
            foreach (var trap in enc.Traps)
                DurationCreatureBox.Items.Add(trap);

            Application.Idle += Application_Idle;

            Init(condition, currentActor, currentRound);
        }

        public EffectForm(OngoingCondition condition, Hero hero)
        {
            InitializeComponent();

            foreach (var c in Conditions.GetConditions())
                ConditionBox.Items.Add(c);

            foreach (DamageType dt in Enum.GetValues(typeof(DamageType)))
            {
                DamageTypeBox.Items.Add(dt);
                DamageModTypeBox.Items.Add(dt);
            }

            DamageModDirBox.Items.Add(Resist);
            DamageModDirBox.Items.Add(Vulnerable);
            DamageModDirBox.Items.Add(Immune);

            DurationBox.Items.Add(Encounter);
            DurationBox.Items.Add(SaveEnds);
            DurationBox.Items.Add(Start);
            DurationBox.Items.Add(End);

            DurationCreatureBox.Items.Add(hero);
            DurationCreatureBox.Items.Add(SomeoneElse);

            Application.Idle += Application_Idle;

            Init(condition, null, -1);
        }

        ~EffectForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Init(OngoingCondition condition, CombatData currentActor, int currentRound)
        {
            _fHeights[ConditionBtn] = ConditionPanel.Height;
            _fHeights[DamageBtn] = DamagePanel.Height;
            _fHeights[DefenceBtn] = DefencePanel.Height;
            _fHeights[DamageModBtn] = DamageModPanel.Height;
            _fHeights[RegenBtn] = RegenPanel.Height;
            _fHeights[AuraBtn] = AuraPanel.Height;

            Effect = condition.Copy();
            _fCurrentActor = currentActor;
            _fCurrentRound = currentRound;

            ConditionBtn.Checked = Effect.Type == OngoingType.Condition;
            ConditionBox.Text = Effect.Data != "" ? Effect.Data : BlankEffect;

            DamageBtn.Checked = Effect.Type == OngoingType.Damage;
            DamageBox.Value = Effect.Value;
            DamageTypeBox.SelectedItem = Effect.DamageType;

            DefenceBtn.Checked = Effect.Type == OngoingType.DefenceModifier;
            DefenceModBox.Value = Effect.DefenceMod;
            if (Effect.Defences.Count == 0)
                Effect.Defences.Add(DefenceType.Ac);
            ACBox.Checked = Effect.Defences.Contains(DefenceType.Ac);
            FortBox.Checked = Effect.Defences.Contains(DefenceType.Fortitude);
            RefBox.Checked = Effect.Defences.Contains(DefenceType.Reflex);
            WillBox.Checked = Effect.Defences.Contains(DefenceType.Will);

            if (Effect.DamageModifier.Value < 0)
                DamageModDirBox.SelectedItem = Resist;
            if (Effect.DamageModifier.Value > 0)
                DamageModDirBox.SelectedItem = Vulnerable;
            if (Effect.DamageModifier.Value == 0)
                DamageModDirBox.SelectedItem = Immune;
            DamageModValueBox.Value = Math.Abs(Effect.DamageModifier.Value);
            DamageModTypeBox.SelectedItem = Effect.DamageModifier.Type;

            RegenValueBox.Value = Effect.Regeneration.Value;
            RegenConditionsBox.Text = Effect.Regeneration.Details;

            AuraRadiusBox.Value = Effect.Aura.Radius;
            AuraDetailsBox.Text = Effect.Aura.Description;

            switch (Effect.Duration)
            {
                case DurationType.Encounter:
                    DurationBox.SelectedItem = Encounter;
                    break;
                case DurationType.SaveEnds:
                    DurationBox.SelectedItem = SaveEnds;
                    ModBox.Value = Effect.SavingThrowModifier;
                    break;
                case DurationType.BeginningOfTurn:
                    DurationBox.SelectedItem = Start;
                    break;
                case DurationType.EndOfTurn:
                    DurationBox.SelectedItem = End;
                    break;
            }

            if (Effect.DurationCreatureId != Guid.Empty)
            {
                DurationCreatureBox.SelectedItem = get_item(Effect.DurationCreatureId);
            }
            else
            {
                if (_fCurrentActor != null)
                    DurationCreatureBox.SelectedItem = get_item(_fCurrentActor.Id);
                else
                    DurationCreatureBox.SelectedItem = DurationCreatureBox.Items[0];
            }
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ConditionBox.Enabled = ConditionBtn.Checked;

            DamageLbl.Enabled = DamageBtn.Checked;
            DamageBox.Enabled = DamageBtn.Checked;
            TypeLbl.Enabled = DamageBtn.Checked;
            DamageTypeBox.Enabled = DamageBtn.Checked;

            DefenceModLbl.Enabled = DefenceBtn.Checked;
            DefenceModBox.Enabled = DefenceBtn.Checked;
            ACBox.Enabled = DefenceBtn.Checked;
            FortBox.Enabled = DefenceBtn.Checked;
            RefBox.Enabled = DefenceBtn.Checked;
            WillBox.Enabled = DefenceBtn.Checked;
            AllDefencesLbl.Enabled = DefenceBtn.Checked;
            NoDefencesLbl.Enabled = DefenceBtn.Checked;

            DamageModDirBox.Enabled = DamageModBtn.Checked;
            DamageModValueLbl.Enabled = DamageModBtn.Checked && DamageModDirBox.SelectedItem.ToString() != Immune;
            DamageModValueBox.Enabled = DamageModBtn.Checked && DamageModDirBox.SelectedItem.ToString() != Immune;
            DamageModTypeLbl.Enabled = DamageModBtn.Checked;
            DamageModTypeBox.Enabled = DamageModBtn.Checked;

            RegenValueLbl.Enabled = RegenBtn.Checked;
            RegenValueBox.Enabled = RegenBtn.Checked;
            RegenConditionsLbl.Enabled = RegenBtn.Checked;
            RegenConditionsBox.Enabled = RegenBtn.Checked;

            AuraRadiusLbl.Enabled = AuraBtn.Checked;
            AuraRadiusBox.Enabled = AuraBtn.Checked;
            AuraDetailsLbl.Enabled = AuraBtn.Checked;
            AuraDetailsBox.Enabled = AuraBtn.Checked;

            var str = DurationBox.SelectedItem as string;

            DurationCreatureBox.Enabled = str == Start || str == End;
            CreatureLbl.Enabled = DurationCreatureBox.Enabled;

            ModBox.Enabled = str == SaveEnds;
            ModLbl.Enabled = ModBox.Enabled;

            if (ConditionBtn.Checked)
                OKBtn.Enabled = ConditionBox.Text != "" && ConditionBox.Text != BlankEffect;
            else
                OKBtn.Enabled = true;
        }

        private void EffectForm_Shown(object sender, EventArgs e)
        {
            if (ConditionBtn.Checked)
            {
                ConditionBox.Focus();
                ConditionBox.SelectAll();
            }

            if (DamageBtn.Checked)
            {
                DamageBox.Focus();
                DamageBox.Select(0, 1);
            }

            if (DefenceBtn.Checked)
            {
                DefenceModBox.Focus();
                DefenceModBox.Select(0, 1);
            }

            // Damage mods - do nothing

            if (RegenBtn.Checked)
            {
                RegenValueBox.Focus();
                RegenValueBox.Select(0, 1);
            }

            if (AuraBtn.Checked)
            {
                AuraRadiusBox.Focus();
                AuraRadiusBox.Select(0, 1);
            }
        }

        private void DurationBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Effect.DurationCreatureId == Guid.Empty)
            {
                if (_fCurrentActor != null)
                    DurationCreatureBox.SelectedItem = get_item(_fCurrentActor.Id);
                else
                    DurationCreatureBox.SelectedItem = DurationCreatureBox.Items[0];
            }
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (ConditionBtn.Checked)
                Effect.Type = OngoingType.Condition;
            if (DamageBtn.Checked)
                Effect.Type = OngoingType.Damage;
            if (DefenceBtn.Checked)
                Effect.Type = OngoingType.DefenceModifier;
            if (DamageModBtn.Checked)
                Effect.Type = OngoingType.DamageModifier;
            if (RegenBtn.Checked)
                Effect.Type = OngoingType.Regeneration;
            if (AuraBtn.Checked)
                Effect.Type = OngoingType.Aura;

            Effect.Data = ConditionBox.Text;

            Effect.Value = (int)DamageBox.Value;
            Effect.DamageType = (DamageType)DamageTypeBox.SelectedItem;

            Effect.DefenceMod = (int)DefenceModBox.Value;
            Effect.Defences.Clear();
            if (ACBox.Checked)
                Effect.Defences.Add(DefenceType.Ac);
            if (FortBox.Checked)
                Effect.Defences.Add(DefenceType.Fortitude);
            if (RefBox.Checked)
                Effect.Defences.Add(DefenceType.Reflex);
            if (WillBox.Checked)
                Effect.Defences.Add(DefenceType.Will);

            var damageModAmount = (int)DamageModValueBox.Value;
            switch (DamageModDirBox.SelectedIndex)
            {
                case 0:
                    // Resist
                    Effect.DamageModifier.Value = damageModAmount * -1;
                    break;
                case 1:
                    // Vulnerable
                    Effect.DamageModifier.Value = damageModAmount;
                    break;
                case 2:
                    // Immune
                    Effect.DamageModifier.Value = 0;
                    break;
            }

            Effect.DamageModifier.Type = (DamageType)DamageModTypeBox.SelectedItem;

            Effect.Regeneration.Value = (int)RegenValueBox.Value;
            Effect.Regeneration.Details = RegenConditionsBox.Text;

            var auraRadius = (int)AuraRadiusBox.Value;
            Effect.Aura.Details = auraRadius + ": " + AuraDetailsBox.Text;

            var str = DurationBox.SelectedItem as string;
            if (str == Encounter)
            {
                Effect.Duration = DurationType.Encounter;
            }
            else if (str == SaveEnds)
            {
                Effect.Duration = DurationType.SaveEnds;
                Effect.SavingThrowModifier = (int)ModBox.Value;
            }
            else if (str == Start)
            {
                Effect.Duration = DurationType.BeginningOfTurn;
                Effect.DurationCreatureId = get_id(DurationCreatureBox.SelectedItem);
            }
            else if (str == End)
            {
                Effect.Duration = DurationType.EndOfTurn;
                Effect.DurationCreatureId = get_id(DurationCreatureBox.SelectedItem);
            }

            if (str == Start || str == End)
            {
                Effect.DurationRound = _fCurrentRound;

                if (_fCurrentActor != null)
                    if (Effect.DurationCreatureId == _fCurrentActor.Id)
                        Effect.DurationRound += 1;
            }
        }

        private object get_item(Guid id)
        {
            foreach (var obj in DurationCreatureBox.Items)
            {
                if (obj is CombatData)
                {
                    var cd = obj as CombatData;
                    if (cd.Id == id)
                        return obj;
                }

                if (obj is Hero)
                {
                    var hero = obj as Hero;
                    if (hero.Id == id)
                        return obj;
                }

                if (obj is Trap)
                {
                    var trap = obj as Trap;
                    if (trap.Id == id)
                        return obj;
                }
            }

            return null;
        }

        private Guid get_id(object obj)
        {
            if (obj is CombatData)
            {
                var cd = obj as CombatData;
                return cd.Id;
            }

            if (obj is Hero)
            {
                var hero = obj as Hero;
                return hero.Id;
            }

            if (obj is Trap)
            {
                var trap = obj as Trap;
                return trap.Id;
            }

            return Guid.Empty;
        }

        private void AllDefencesLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            set_defences(true);
        }

        private void NoDefencesLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            set_defences(false);
        }

        private void set_defences(bool enabled)
        {
            ACBox.Checked = enabled;
            FortBox.Checked = enabled;
            RefBox.Checked = enabled;
            WillBox.Checked = enabled;
        }

        private void EffectTypeChanged(object sender, EventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb == null)
                return;

            if (rb.Checked == false)
                return;

            var buttons = new List<RadioButton>();
            buttons.Add(ConditionBtn);
            buttons.Add(DamageBtn);
            buttons.Add(DefenceBtn);
            buttons.Add(DamageModBtn);
            buttons.Add(RegenBtn);
            buttons.Add(AuraBtn);

            buttons.Remove(rb);
            foreach (var btn in buttons)
                btn.Checked = false;

            ConditionPanel.Height = rb == ConditionBtn ? _fHeights[rb] : MinHeight;
            DamagePanel.Height = rb == DamageBtn ? _fHeights[rb] : MinHeight;
            DefencePanel.Height = rb == DefenceBtn ? _fHeights[rb] : MinHeight;
            DamageModPanel.Height = rb == DamageModBtn ? _fHeights[rb] : MinHeight;
            RegenPanel.Height = rb == RegenBtn ? _fHeights[rb] : MinHeight;
            AuraPanel.Height = rb == AuraBtn ? _fHeights[rb] : MinHeight;

            var topmargin = PropertiesPanel.Top;
            var middlemargin = DurationGroup.Top - PropertiesPanel.Bottom;
            var bottommargin = Height - DurationGroup.Bottom;
            Height = topmargin + ConditionPanel.Height + DamagePanel.Height + DefencePanel.Height +
                     DamageModPanel.Height + RegenPanel.Height + AuraPanel.Height + middlemargin +
                     DurationGroup.Height + bottommargin;
        }
    }
}
