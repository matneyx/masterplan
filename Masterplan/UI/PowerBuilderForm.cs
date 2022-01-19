using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class PowerBuilderForm : Form
    {
        private readonly ICreature _fCreature;

        private readonly List<string> _fExamples = new List<string>();
        private readonly bool _fFromFunctionalTemplate;

        public CreaturePower Power { get; private set; }

        public PowerBuilderForm(CreaturePower power, ICreature creature, bool functionalTemplate)
        {
            InitializeComponent();

            Power = power;
            _fCreature = creature;
            _fFromFunctionalTemplate = functionalTemplate;

            refresh_examples();
            update_statblock();
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "power")
            {
                if (e.Url.LocalPath == "info")
                {
                    e.Cancel = true;

                    var dlg = new PowerInfoForm(Power);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Name = dlg.PowerName;
                        Power.Keywords = dlg.PowerKeywords;

                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "action")
                {
                    e.Cancel = true;

                    var action = Power.Action;
                    var dlg = new PowerActionForm(action);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Action = dlg.Action;

                        refresh_examples();
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "prerequisite")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Power.Condition, "Power Prerequisite", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Condition = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "range")
                {
                    e.Cancel = true;

                    var dlg = new PowerRangeForm(Power);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Range = dlg.PowerRange;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "attack")
                {
                    e.Cancel = true;

                    var attack = Power.Attack;
                    if (attack == null)
                        attack = new PowerAttack();

                    var level = _fCreature?.Level ?? 0;
                    var role = _fCreature?.Role;
                    var dlg = new PowerAttackForm(attack, _fFromFunctionalTemplate, level, role);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Attack = dlg.Attack;

                        refresh_examples();
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "clearattack")
                {
                    e.Cancel = true;

                    Power.Attack = null;

                    refresh_examples();
                    update_statblock();
                }

                if (e.Url.LocalPath == "details")
                {
                    e.Cancel = true;

                    var dlg = new PowerDetailsForm(Power.Details, _fCreature);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Details = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "desc")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Power.Description, "Power Description", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Power.Description = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "details")
            {
                if (e.Url.LocalPath == "refresh")
                {
                    e.Cancel = true;

                    refresh_examples();
                    update_statblock();
                }

                try
                {
                    var index = int.Parse(e.Url.LocalPath);
                    e.Cancel = true;

                    Power.Details = _fExamples[index];
                    _fExamples.RemoveAt(index);

                    if (_fExamples.Count == 0)
                        refresh_examples();

                    update_statblock();
                }
                catch
                {
                    // Not a number
                }
            }
        }

        private void refresh_examples()
        {
            _fExamples.Clear();

            var allExamples = new List<string>();

            if (_fCreature != null)
            {
                var creatures = new List<ICreature>();
                foreach (var creature in Session.Creatures)
                {
                    if (creature == null)
                        continue;

                    if (creature.Level == _fCreature.Level && creature.Role.ToString() == _fCreature.Role.ToString())
                        creatures.Add(creature);
                }

                if (Session.Project != null)
                    foreach (var creature in Session.Project.CustomCreatures)
                    {
                        if (creature == null)
                            continue;

                        if (creature.Level == _fCreature.Level &&
                            creature.Role.ToString() == _fCreature.Role.ToString())
                            creatures.Add(creature);
                    }

                foreach (var creature in creatures)
                foreach (var cp in creature.CreaturePowers)
                {
                    if (Power.Category != cp.Category)
                        continue;

                    if (cp.Details == "")
                        continue;

                    allExamples.Add(cp.Details);
                }
            }

            if (allExamples.Count != 0)
                for (var n = 0; n != 5; ++n)
                {
                    if (allExamples.Count == 0)
                        break;

                    var index = Session.Random.Next(allExamples.Count);
                    var example = allExamples[index];
                    allExamples.RemoveAt(index);

                    _fExamples.Add(example);
                }
        }

        private void update_statblock()
        {
            var level = _fCreature?.Level ?? 0;
            var role = _fCreature?.Role;

            var lines = Html.GetHead(null, null, Session.Preferences.TextSize);
            lines.Add("<BODY>");

            lines.Add("<TABLE class=clear>");
            lines.Add("<TR class=clear>");
            lines.Add("<TD class=clear>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");
            lines.AddRange(Power.AsHtml(null, CardMode.Build, _fFromFunctionalTemplate));
            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</TD>");
            lines.Add("<TD class=clear>");

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Power Advice</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=shaded>");
            lines.Add("<TD colspan=2><B>Attack Bonus</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Attack vs Armour Class</TD>");
            lines.Add("<TD align=center>+" + Statistics.AttackBonus(DefenceType.Ac, level, role) + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Attack vs Other Defence</TD>");
            lines.Add("<TD align=center>+" + Statistics.AttackBonus(DefenceType.Fortitude, level, role) + "</TD>");
            lines.Add("</TR>");

            if (role != null)
            {
                lines.Add("<TR class=shaded>");
                lines.Add("<TD colspan=2><B>Damage</B></TD>");
                lines.Add("</TR>");

                if (role is Minion)
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>Minion Damage</TD>");
                    lines.Add("<TD align=center>" + Statistics.Damage(level, DamageExpressionType.Minion) + "</TD>");
                    lines.Add("</TR>");
                }
                else
                {
                    lines.Add("<TR>");
                    lines.Add("<TD>Damage vs Single Targets</TD>");
                    lines.Add("<TD align=center>" + Statistics.Damage(level, DamageExpressionType.Normal) + "</TD>");
                    lines.Add("</TR>");

                    lines.Add("<TR>");
                    lines.Add("<TD>Damage vs Multiple Targets</TD>");
                    lines.Add("<TD align=center>" + Statistics.Damage(level, DamageExpressionType.Multiple) + "</TD>");
                    lines.Add("</TR>");
                }

                if (_fExamples.Count != 0)
                {
                    lines.Add("<TR class=shaded>");
                    lines.Add("<TD><B>Example Power Details</B></TD>");
                    lines.Add("<TD align=center><A href=details:refresh>(refresh)</A></TD>");
                    lines.Add("</TR>");

                    foreach (var example in _fExamples)
                    {
                        var index = _fExamples.IndexOf(example);

                        lines.Add("<TR>");
                        lines.Add("<TD colspan=2>" + example + " <A href=details:" + index + ">(use this)</A></TD>");
                        lines.Add("</TR>");
                    }
                }
            }

            lines.Add("</TABLE>");
            lines.Add("</P>");

            lines.Add("</TD>");
            lines.Add("</TR>");
            lines.Add("</TABLE>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            StatBlockBrowser.DocumentText = Html.Concatenate(lines);
        }

        private void PowerBrowserBtn_Click(object sender, EventArgs e)
        {
            var level = _fCreature?.Level ?? 0;
            var role = _fCreature?.Role;

            var dlg = new PowerBrowserForm(null, level, role, null);
            if (dlg.ShowDialog() == DialogResult.OK)
                if (dlg.SelectedPower != null)
                {
                    Power = dlg.SelectedPower.Copy();
                    Power.Id = Guid.NewGuid();

                    update_statblock();
                }
        }
    }
}
