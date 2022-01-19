using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class PowerStatisticsForm : Form
    {
        private readonly int _fCreatures;

        private readonly List<CreaturePower> _fPowers;

        public PowerStatisticsForm(List<CreaturePower> powers, int creatures)
        {
            InitializeComponent();

            _fPowers = powers;
            _fCreatures = creatures;

            update_table();
        }

        private void update_table()
        {
            var lines = new List<string>();

            lines.AddRange(Html.GetHead("Power Statistics", "", Session.Preferences.TextSize));
            lines.Add("<BODY>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=3>");
            lines.Add("<B>Number of Powers</B>");
            lines.Add("</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD colspan=2>");
            lines.Add("Number of powers");
            lines.Add("</TD>");
            lines.Add("<TD align=right>");
            lines.Add(_fPowers.Count.ToString());
            lines.Add("</TD>");
            lines.Add("</TR>");

            if (_fCreatures != 0)
            {
                // Powers per creature
                var ppc = (double)_fPowers.Count / _fCreatures;
                lines.Add("<TR>");
                lines.Add("<TD colspan=2>");
                lines.Add("Powers per creature");
                lines.Add("</TD>");
                lines.Add("<TD align=right>");
                lines.Add(ppc.ToString("F1"));
                lines.Add("</TD>");
                lines.Add("</TR>");
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            if (_fPowers.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                // Number of powers with each condition
                var conditionBreakdown = get_condition_breakdown();
                if (conditionBreakdown.Count != 0)
                {
                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Conditions</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    var list = sort_breakdown(conditionBreakdown);
                    foreach (var condition in list)
                    {
                        var count = condition.Second;
                        if (count == 0)
                            continue;

                        var pc = (double)count / _fPowers.Count;

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>");
                        lines.Add(condition.First);
                        lines.Add("</TD>");
                        lines.Add("<TD align=right>");
                        lines.Add(count + " (" + pc.ToString("P0") + ")");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (_fPowers.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                // Number of powers with each damage type
                var typeBreakdown = get_damage_type_breakdown();
                if (typeBreakdown.Count != 0)
                {
                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Damage Types</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    var list = sort_breakdown(typeBreakdown);
                    foreach (var type in list)
                    {
                        var count = type.Second;
                        var pc = (double)count / _fPowers.Count;

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>");
                        lines.Add(type.First);
                        lines.Add("</TD>");
                        lines.Add("<TD align=right>");
                        lines.Add(count + " (" + pc.ToString("P0") + ")");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (_fPowers.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                // Number of powers with each keyword
                var keywordBreakdown = get_keyword_breakdown();
                if (keywordBreakdown.Count != 0)
                {
                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Keywords</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    var list = sort_breakdown(keywordBreakdown);
                    foreach (var type in list)
                    {
                        var count = type.Second;
                        var pc = (double)count / _fPowers.Count;

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>");
                        lines.Add(type.First);
                        lines.Add("</TD>");
                        lines.Add("<TD align=right>");
                        lines.Add(count + " (" + pc.ToString("P0") + ")");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            if (_fPowers.Count != 0)
            {
                // Number of powers per category
                var categoryBreakdown = get_category_breakdown();
                if (categoryBreakdown.Count != 0)
                {
                    lines.Add("<P class=table>");
                    lines.Add("<TABLE>");

                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Powers Per Category</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    foreach (var category in categoryBreakdown.Keys)
                    {
                        var pc = categoryBreakdown[category];

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>");
                        lines.Add(category);
                        lines.Add("</TD>");
                        lines.Add("<TD align=right>");
                        lines.Add(pc.ToString("P0"));
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }

                    lines.Add("</TABLE>");
                    lines.Add("</P>");
                }
            }

            if (_fPowers.Count != 0)
            {
                lines.Add("<P class=table>");
                lines.Add("<TABLE>");

                // Frequency of damage expressions
                var damageBreakdown = get_damage_expression_breakdown();
                if (damageBreakdown.Count != 0)
                {
                    lines.Add("<TR class=heading>");
                    lines.Add("<TD colspan=3>");
                    lines.Add("<B>Damage</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");

                    var list = sort_breakdown(damageBreakdown);
                    foreach (var dmg in list)
                    {
                        var count = dmg.Second;
                        var pc = (double)count / _fPowers.Count;

                        var exp = DiceExpression.Parse(dmg.First);

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>");
                        lines.Add(dmg.First + " (avg " + exp.Average + ", max " + exp.Maximum + ")");
                        lines.Add("</TD>");
                        lines.Add("<TD align=right>");
                        lines.Add(count + " (" + pc.ToString("P0") + ")");
                        lines.Add("</TD>");
                        lines.Add("</TR>");
                    }
                }

                lines.Add("</TABLE>");
                lines.Add("</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            Browser.DocumentText = Html.Concatenate(lines);
        }

        private int count_powers(string text)
        {
            // Account for Americans
            if (text == "Immobilised")
                text = "Immobilized";

            var count = 0;

            foreach (var power in _fPowers)
                if (power.Details.ToLower().Contains(text.ToLower()))
                    count += 1;

            return count;
        }

        private List<Pair<string, int>> sort_breakdown(Dictionary<string, int> breakdown)
        {
            var list = new List<Pair<string, int>>();

            foreach (var key in breakdown.Keys)
                list.Add(new Pair<string, int>(key, breakdown[key]));

            list.Sort((x, y) => x.Second.CompareTo(y.Second) * -1);

            return list;
        }

        private Dictionary<string, int> get_condition_breakdown()
        {
            var breakdown = new Dictionary<string, int>();

            var conditions = Conditions.GetConditions();
            foreach (var condition in conditions)
            {
                var count = count_powers(condition);
                if (count == 0)
                    continue;

                breakdown[condition] = count;
            }

            return breakdown;
        }

        private Dictionary<string, int> get_damage_type_breakdown()
        {
            var breakdown = new Dictionary<string, int>();

            var damagetypes = Enum.GetValues(typeof(DamageType));
            foreach (DamageType type in damagetypes)
            {
                if (type == DamageType.Untyped)
                    continue;

                var damageType = type.ToString();

                var count = count_powers(damageType);
                if (count == 0)
                    continue;

                breakdown[damageType] = count;
            }

            return breakdown;
        }

        private Dictionary<string, double> get_category_breakdown()
        {
            var breakdown = new Dictionary<string, double>();

            var categories = Enum.GetValues(typeof(CreaturePowerCategory));
            foreach (CreaturePowerCategory category in categories)
            {
                var count = 0;

                foreach (var power in _fPowers)
                    if (power.Category == category)
                        count += 1;

                breakdown[category.ToString()] = (double)count / _fPowers.Count;
            }

            return breakdown;
        }

        private Dictionary<string, int> get_damage_expression_breakdown()
        {
            var breakdown = new Dictionary<string, int>();

            foreach (var power in _fPowers)
            {
                var exp = DiceExpression.Parse(power.Damage);
                if (exp == null)
                    continue;
                if (exp.Maximum == 0)
                    continue;

                var dmg = exp.ToString();

                if (!breakdown.ContainsKey(dmg))
                    breakdown[dmg] = 0;

                breakdown[dmg] += 1;
            }

            return breakdown;
        }

        private Dictionary<string, int> get_keyword_breakdown()
        {
            var breakdown = new Dictionary<string, int>();

            foreach (var power in _fPowers)
            {
                var tokens = power.Keywords.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var token in tokens)
                {
                    var str = token.Trim();

                    if (!breakdown.ContainsKey(str))
                        breakdown[str] = 0;

                    breakdown[str] += 1;
                }
            }

            return breakdown;
        }
    }
}
