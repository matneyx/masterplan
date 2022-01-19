using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class AttackRollForm : Form
    {
        private readonly Encounter _fEncounter;

        private readonly CreaturePower _fPower;

        private readonly List<Pair<CombatData, int>> _fRolls = new List<Pair<CombatData, int>>();

        private bool _fAddedCombatant;

        public Pair<CombatData, int> SelectedRoll
        {
            get
            {
                if (RollList.SelectedItems.Count != 0)
                    return RollList.SelectedItems[0].Tag as Pair<CombatData, int>;

                return null;
            }
        }

        public AttackRollForm(CreaturePower power, Encounter enc)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            _fPower = power;
            _fEncounter = enc;

            add_attack_roll(null);

            update_damage();
            RollDamageBtn_Click(null, null);
        }

        ~AttackRollForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            ApplyDamageBox.Visible = _fAddedCombatant;
        }

        private void OKBtn_Click(object sender, EventArgs e)
        {
            if (ApplyDamageBox.Visible && ApplyDamageBox.Checked)
                apply_damage();
        }

        private void PowerBrowser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "opponent")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var cd = _fEncounter.FindCombatData(id);

                add_attack_roll(cd);
            }

            if (e.Url.Scheme == "hero")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var hero = Session.Project.FindHero(id);
                if (hero != null)
                {
                    var cd = hero.CombatData;

                    add_attack_roll(cd);
                }
            }

            if (e.Url.Scheme == "target")
            {
                e.Cancel = true;

                add_attack_roll(null);
            }
        }

        private void RollList_DoubleClick(object sender, EventArgs e)
        {
            if (SelectedRoll != null)
            {
                var roll = Session.Dice(1, 20);
                SelectedRoll.Second = roll;
                update_list();
            }
        }

        private void RollDamageBtn_Click(object sender, EventArgs e)
        {
            var exp = DiceExpression.Parse(DamageExpLbl.Text);
            if (exp != null)
            {
                var roll = exp.Evaluate();
                DamageBox.Value = roll;
            }
        }

        private void DamageBox_ValueChanged(object sender, EventArgs e)
        {
            var roll = (int)DamageBox.Value;
            var miss = roll / 2;
            MissValueLbl.Text = miss.ToString();
        }

        private void update_power()
        {
            var lines = new List<string>();

            lines.AddRange(Html.GetHead(_fPower.Name, "", Session.Preferences.TextSize));

            lines.Add("<BODY>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");
            lines.AddRange(_fPower.AsHtml(null, CardMode.View, false));
            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("<P class=instruction align=left>");
            lines.Add("Click to add an attack roll for:");
            var heroes = "";
            foreach (var hero in Session.Project.Heroes)
            {
                var cd = hero.CombatData;

                if (!roll_exists(hero.Id) && hero.GetState(cd.Damage) != CreatureState.Defeated)
                {
                    if (heroes != "")
                        heroes += " | ";

                    heroes += "<A href=hero:" + hero.Id + ">" + hero.Name + "</A>";
                }
            }

            if (heroes != "")
            {
                lines.Add("<BR>");
                lines.Add(heroes);
            }

            var creatures = "";
            foreach (var slot in _fEncounter.Slots)
            foreach (var cd in slot.CombatData)
                if (!roll_exists(cd.Id) && slot.GetState(cd) != CreatureState.Defeated)
                {
                    if (creatures != "")
                        creatures += " | ";

                    creatures += "<A href=opponent:" + cd.Id + ">" + cd.DisplayName + "</A>";
                }

            if (creatures != "")
            {
                lines.Add("<BR>");
                lines.Add(creatures);
            }

            lines.Add("<BR>");
            lines.Add("<A href=target:blank>An unnamed target</A>");
            lines.Add("</P>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            PowerBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void update_list()
        {
            RollList.Items.Clear();

            foreach (var roll in _fRolls)
            {
                var die = roll.Second;
                var bonus = _fPower.Attack?.Bonus ?? 0;
                var total = die + bonus;

                var lvi = RollList.Items.Add(roll.First != null ? roll.First.DisplayName : "Roll");
                lvi.SubItems.Add(die.ToString());
                lvi.SubItems.Add(bonus.ToString());
                lvi.SubItems.Add(total.ToString());

                var hit = true;
                if (roll.First != null && _fPower.Attack != null)
                {
                    // Work out whether we've hit the defence
                    var defence = 0;
                    var hero = Session.Project.FindHero(roll.First.Id);
                    if (hero != null)
                    {
                        switch (_fPower.Attack.Defence)
                        {
                            case DefenceType.Ac:
                                defence = hero.Ac;
                                break;
                            case DefenceType.Fortitude:
                                defence = hero.Fortitude;
                                break;
                            case DefenceType.Reflex:
                                defence = hero.Reflex;
                                break;
                            case DefenceType.Will:
                                defence = hero.Will;
                                break;
                        }
                    }
                    else
                    {
                        var slot = _fEncounter.FindSlot(roll.First);

                        switch (_fPower.Attack.Defence)
                        {
                            case DefenceType.Ac:
                                defence = slot.Card.Ac;
                                break;
                            case DefenceType.Fortitude:
                                defence = slot.Card.Fortitude;
                                break;
                            case DefenceType.Reflex:
                                defence = slot.Card.Reflex;
                                break;
                            case DefenceType.Will:
                                defence = slot.Card.Will;
                                break;
                        }
                    }

                    // Take account of defence-boosting conditions
                    foreach (var oc in roll.First.Conditions)
                    {
                        if (oc.Type != OngoingType.DefenceModifier)
                            continue;

                        if (oc.Defences.Contains(_fPower.Attack.Defence))
                            defence += oc.DefenceMod;
                    }

                    hit = total >= defence;
                }

                if (die == 20)
                    lvi.Font = new Font(lvi.Font, lvi.Font.Style | FontStyle.Bold);
                else if (die == 1)
                    lvi.ForeColor = Color.Red;
                else if (!hit)
                    lvi.ForeColor = SystemColors.GrayText;

                lvi.Tag = roll;
            }
        }

        private void update_damage()
        {
            var dmgStr = _fPower.Damage;
            if (dmgStr == "")
            {
                Pages.TabPages.Remove(DamagePage);
            }
            else
            {
                var exp = DiceExpression.Parse(dmgStr);

                DamageExpLbl.Text = dmgStr;
                CritValueLbl.Text = exp.Maximum.ToString();
            }
        }

        private void add_attack_roll(CombatData cd)
        {
            if (cd != null)
                if (_fRolls.Count == 1 && _fRolls[0].First == null)
                    _fRolls.Clear();

            var roll = Session.Dice(1, 20);
            _fRolls.Add(new Pair<CombatData, int>(cd, roll));

            if (cd != null)
                _fAddedCombatant = true;

            update_list();
            update_power();
        }

        private bool roll_exists(Guid id)
        {
            foreach (var roll in _fRolls)
                if (roll.First != null && roll.First.Id == id)
                    return true;

            return false;
        }

        private void apply_damage()
        {
            foreach (ListViewItem lvi in RollList.Items)
            {
                var roll = lvi.Tag as Pair<CombatData, int>;
                if (roll.First == null)
                    continue;

                var damage = 0;
                if (roll.Second == 20)
                {
                    damage = int.Parse(CritValueLbl.Text);
                }
                else if (lvi.ForeColor == SystemColors.WindowText)
                {
                    // Hit
                    damage = (int)DamageBox.Value;
                }

                if (damage == 0)
                    continue;

                // Determine damage type(s)
                var array = Enum.GetValues(typeof(DamageType));
                var types = new List<DamageType>();
                foreach (DamageType type in array)
                {
                    var details = _fPower.Details.ToLower();
                    var dmg = type.ToString().ToLower();
                    if (details.Contains(dmg))
                        types.Add(type);
                }

                var slot = _fEncounter.FindSlot(roll.First);
                var card = slot?.Card;

                DamageForm.DoDamage(roll.First, card, damage, types, false);
            }
        }
    }
}
