using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class DamageForm : Form
    {
        private readonly List<Token> _fData;

        public List<DamageType> Types { get; } = new List<DamageType>();

        public DamageForm(List<Pair<CombatData, EncounterCard>> tokens, int value)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fData = new List<Token>();
            foreach (var token in tokens)
                _fData.Add(new Token(token.First, token.Second));

            DmgBox.Value = value;

            if (_fData.Count == 1 && _fData[0].Card != null)
                HalveBtn.Checked = _fData[0].Card.Resist.ToLower().Contains("insubstantial");

            update_type();
            update_modifier();
            update_value();
        }

        ~DamageForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ResetBtn.Enabled = DmgBox.Value != 0;

            AcidBtn.Checked = Types.Contains(DamageType.Acid);
            ColdBtn.Checked = Types.Contains(DamageType.Cold);
            FireBtn.Checked = Types.Contains(DamageType.Fire);
            ForceBtn.Checked = Types.Contains(DamageType.Force);
            LightningBtn.Checked = Types.Contains(DamageType.Lightning);
            NecroticBtn.Checked = Types.Contains(DamageType.Necrotic);
            PoisonBtn.Checked = Types.Contains(DamageType.Poison);
            PsychicBtn.Checked = Types.Contains(DamageType.Psychic);
            RadiantBtn.Checked = Types.Contains(DamageType.Radiant);
            ThunderBtn.Checked = Types.Contains(DamageType.Thunder);

            TypeLbl.Enabled = Types.Count != 0;
            TypeBox.Enabled = Types.Count != 0;
            ModLbl.Enabled = Types.Count != 0;
            ModBox.Enabled = Types.Count != 0;
        }

        private void DamageForm_Shown(object sender, EventArgs e)
        {
            DmgBox.Select(0, 1);
        }

        private void DmgBox_ValueChanged(object sender, EventArgs e)
        {
            update_value();
        }

        private void HalveBtn_CheckedChanged(object sender, EventArgs e)
        {
            update_value();
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            foreach (var token in _fData)
                DoDamage(token.Data, token.Card, (int)DmgBox.Value, Types, HalveBtn.Checked);
        }

        internal static void DoDamage(CombatData data, EncounterCard card, int damage, List<DamageType> types,
            bool halveDamage)
        {
            var modifier = 0;
            if (card != null)
                modifier = card.GetDamageModifier(types, data);

            var value = get_value(damage, modifier, halveDamage);

            // Take damage off temp HP first
            if (data.TempHp > 0)
            {
                var n = Math.Min(data.TempHp, value);

                data.TempHp -= n;
                value -= n;
            }

            data.Damage += value;
        }

        private void update_type()
        {
            var str = "";
            foreach (var dt in Types)
            {
                if (str != "")
                    str += ", ";

                str += dt.ToString();
            }

            if (str == "")
                str = "(untyped)";
            TypeBox.Text = str;
        }

        private void update_modifier()
        {
            foreach (var token in _fData)
                if (token.Card != null)
                    token.Modifier = token.Card.GetDamageModifier(Types, token.Data);

            if (_fData.Count == 1)
            {
                var token = _fData[0];

                if (token.Modifier == int.MinValue)
                    ModBox.Text = "Immune";
                else if (token.Modifier > 0)
                    ModBox.Text = "Vulnerable " + token.Modifier;
                else if (token.Modifier < 0)
                    ModBox.Text = "Resist " + Math.Abs(token.Modifier);
                else
                    ModBox.Text = "(none)";
            }
            else
            {
                ModBox.Text = "(multiple tokens)";
            }
        }

        private void update_value()
        {
            if (_fData.Count == 1)
            {
                var value = get_value((int)DmgBox.Value, _fData[0].Modifier, HalveBtn.Checked);
                ValBox.Text = value.ToString();
            }
            else
            {
                ValBox.Text = "(multiple tokens)";
            }
        }

        private static int get_value(int initialValue, int modifier, bool halveDamage)
        {
            var value = initialValue;

            if (modifier != 0)
            {
                if (modifier == int.MinValue)
                {
                    value = 0;
                }
                else
                {
                    value += modifier;
                    value = Math.Max(value, 0);
                }
            }

            if (halveDamage)
                value /= 2;

            return value;
        }

        private void Dmg1_Click(object sender, EventArgs e)
        {
            Damage(1);
        }

        private void Dmg2_Click(object sender, EventArgs e)
        {
            Damage(2);
        }

        private void Dmg5_Click(object sender, EventArgs e)
        {
            Damage(5);
        }

        private void Dmg10_Click(object sender, EventArgs e)
        {
            Damage(10);
        }

        private void Dmg20_Click(object sender, EventArgs e)
        {
            Damage(20);
        }

        private void Dmg50_Click(object sender, EventArgs e)
        {
            Damage(50);
        }

        private void Dmg100_Click(object sender, EventArgs e)
        {
            Damage(100);
        }

        private void Damage(int n)
        {
            DmgBox.Value += n;
        }

        private void ResetBtn_Click(object sender, EventArgs e)
        {
            DmgBox.Value = 0;
        }

        private void AcidBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Acid);
        }

        private void ColdBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Cold);
        }

        private void FireBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Fire);
        }

        private void ForceBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Force);
        }

        private void LightningBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Lightning);
        }

        private void NecroticBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Necrotic);
        }

        private void PoisonBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Poison);
        }

        private void PsychicBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Psychic);
        }

        private void RadiantBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Radiant);
        }

        private void ThunderBtn_Click(object sender, EventArgs e)
        {
            add_type(DamageType.Thunder);
        }

        private void add_type(DamageType type)
        {
            if (Types.Contains(type))
            {
                Types.Remove(type);
            }
            else
            {
                Types.Add(type);
                Types.Sort();
            }

            update_type();
            update_modifier();
            update_value();
        }

        public class Token
        {
            public EncounterCard Card;

            public CombatData Data;
            public int Modifier;

            public Token(CombatData data, EncounterCard card)
            {
                Data = data;
                Card = card;
            }
        }
    }
}
