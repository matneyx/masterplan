using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class HealForm : Form
    {
        private readonly List<Pair<CombatData, EncounterCard>> _fTokens;

        public HealForm(List<Pair<CombatData, EncounterCard>> tokens)
        {
            InitializeComponent();

            _fTokens = tokens;
        }

        private void DamageForm_Shown(object sender, EventArgs e)
        {
            SurgeBox.Select(0, 1);
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            var surges = (int)SurgeBox.Value;
            var hp = (int)HPBox.Value;

            foreach (var token in _fTokens)
            {
                var maxHp = 0;
                if (token.Second != null)
                {
                    // It's a creature
                    maxHp = token.Second.Hp;
                }
                else
                {
                    // It's a hero
                    var hero = Session.Project.FindHero(token.First.Id);
                    if (hero != null)
                        maxHp = hero.Hp;
                }

                var surgeValue = maxHp / 4;
                var healValue = surgeValue * surges + hp;

                if (TempHPBox.Checked)
                {
                    // Top up temp HP
                    token.First.TempHp = Math.Max(healValue, token.First.TempHp);
                }
                else
                {
                    // Start from 0 HP
                    if (token.First.Damage > maxHp)
                        token.First.Damage = maxHp;

                    // Heal
                    token.First.Damage -= healValue;

                    // Don't heal past max HP
                    if (token.First.Damage < 0)
                        token.First.Damage = 0;
                }
            }
        }
    }
}
