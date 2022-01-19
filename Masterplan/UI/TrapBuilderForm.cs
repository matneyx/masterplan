using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class TrapBuilderForm : Form
    {
        public Trap Trap { get; private set; }

        public TrapBuilderForm(Trap trap)
        {
            InitializeComponent();

            Application.Idle += Application_Idle;

            Trap = trap.Copy();

            update_statblock();
        }

        ~TrapBuilderForm()
        {
            Application.Idle -= Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            LevelDownBtn.Enabled = Trap.Level > 1;
        }

        private void OptionsCopy_Click(object sender, EventArgs e)
        {
            var dlg = new TrapSelectForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Trap = dlg.Trap.Copy();
                update_statblock();
            }
        }

        private void Browser_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (e.Url.Scheme == "build")
            {
                if (e.Url.LocalPath == "profile")
                {
                    e.Cancel = true;

                    var dlg = new TrapProfileForm(Trap);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Name = dlg.Trap.Name;
                        Trap.Type = dlg.Trap.Type;
                        Trap.Level = dlg.Trap.Level;
                        Trap.Role = dlg.Trap.Role;
                        Trap.Initiative = dlg.Trap.Initiative;

                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "readaloud")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Trap.ReadAloud, "Read-Aloud Text", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.ReadAloud = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "desc")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Trap.Description, "Description", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Description = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "details")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Trap.Details, "Details", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Details = dlg.Details;
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "addskill")
                {
                    e.Cancel = true;

                    var tsd = new TrapSkillData();
                    tsd.SkillName = "Perception";
                    tsd.Dc = Ai.GetSkillDc(Difficulty.Moderate, Trap.Level);

                    var dlg = new TrapSkillForm(tsd, Trap.Level);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Skills.Add(dlg.SkillData);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "addattack")
                {
                    e.Cancel = true;

                    var ta = new TrapAttack();
                    ta.Name = "Attack";

                    Trap.Attacks.Add(ta);
                    update_statblock();
                }

                if (e.Url.LocalPath == "addcm")
                {
                    e.Cancel = true;

                    var cm = "";
                    var dlg = new TrapCountermeasureForm(cm, Trap.Level);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Countermeasures.Add(dlg.Countermeasure);
                        update_statblock();
                    }
                }

                if (e.Url.LocalPath == "trigger")
                {
                    e.Cancel = true;

                    var dlg = new DetailsForm(Trap.Trigger, "Trigger", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Trigger = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attackaction")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new TrapActionForm(attack);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.Name = dlg.Attack.Name;
                        attack.Action = dlg.Attack.Action;
                        attack.Range = dlg.Attack.Range;
                        attack.Target = dlg.Attack.Target;

                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attackremove")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    Trap.Attacks.Remove(attack);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "attackattack")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new PowerAttackForm(attack.Attack, false, Trap.Level, Trap.Role);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.Attack = dlg.Attack;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attackhit")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new DetailsForm(attack.OnHit, "On Hit", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.OnHit = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attackmiss")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new DetailsForm(attack.OnMiss, "On Miss", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.OnMiss = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attackeffect")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new DetailsForm(attack.Effect, "Effect", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.Effect = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "attacknotes")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var attack = Trap.FindAttack(id);
                if (attack != null)
                {
                    var dlg = new DetailsForm(attack.Notes, "Notes", null);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        attack.Notes = dlg.Details;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "skill")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var tsd = Trap.FindSkill(id);
                if (tsd != null)
                {
                    var index = Trap.Skills.IndexOf(tsd);

                    var dlg = new TrapSkillForm(tsd, Trap.Level);
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        Trap.Skills[index] = dlg.SkillData;
                        update_statblock();
                    }
                }
            }

            if (e.Url.Scheme == "skillremove")
            {
                e.Cancel = true;

                var id = new Guid(e.Url.LocalPath);
                var tsd = Trap.FindSkill(id);
                if (tsd != null)
                {
                    Trap.Skills.Remove(tsd);
                    update_statblock();
                }
            }

            if (e.Url.Scheme == "cm")
            {
                e.Cancel = true;

                var index = int.Parse(e.Url.LocalPath);
                var cm = Trap.Countermeasures[index];

                var dlg = new TrapCountermeasureForm(cm, Trap.Level);
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Trap.Countermeasures[index] = dlg.Countermeasure;
                    update_statblock();
                }
            }
        }

        private void update_statblock()
        {
            var lines = Html.GetHead("Trap", "", Session.Preferences.TextSize);
            lines.Add("<BODY>");

            lines.Add("<TABLE class=clear>");
            lines.Add("<TR class=clear>");
            lines.Add("<TD class=clear>");
            lines.Add("<P class=table>");
            lines.Add(Html.Trap(Trap, null, false, false, true, Session.Preferences.TextSize));
            lines.Add("</P>");
            lines.Add("</TD>");
            lines.Add("<TD class=clear>");
            lines.AddRange(get_advice());
            lines.Add("</TD>");
            lines.Add("</TR>");
            lines.Add("</TABLE>");

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            StatBlockBrowser.DocumentText = Html.Concatenate(lines);
        }

        private List<string> get_advice()
        {
            var init = 2;
            var attackAc = Trap.Level + 5;
            var attackNad = Trap.Level + 3;

            var isElite = false;
            if (Trap.Role is ComplexRole)
            {
                var cr = Trap.Role as ComplexRole;
                if (cr.Flag == RoleFlag.Elite || cr.Flag == RoleFlag.Solo)
                    isElite = true;
            }

            if (isElite)
            {
                init += 2;
                attackAc += 2;
                attackNad += 2;
            }

            var lines = new List<string>();

            lines.Add("<P class=table>");
            lines.Add("<TABLE>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Initiative Advice</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Initiative</TD>");
            lines.Add("<TD align=center>+" + init + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Attack Advice</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Attack vs Armour Class</TD>");
            lines.Add("<TD align=center>+" + attackAc + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Attack vs Other Defence</TD>");
            lines.Add("<TD align=center>+" + attackNad + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Damage Advice</B></TD>");
            lines.Add("</TR>");

            if (Trap.Role is Minion)
            {
                lines.Add("<TR>");
                lines.Add("<TD>Minion Damage</TD>");
                lines.Add("<TD align=center>" + Statistics.Damage(Trap.Level, DamageExpressionType.Minion) + "</TD>");
                lines.Add("</TR>");
            }
            else
            {
                lines.Add("<TR>");
                lines.Add("<TD>Damage vs Single Targets</TD>");
                lines.Add("<TD align=center>" + Statistics.Damage(Trap.Level, DamageExpressionType.Normal) + "</TD>");
                lines.Add("</TR>");

                lines.Add("<TR>");
                lines.Add("<TD>Damage vs Multiple Targets</TD>");
                lines.Add("<TD align=center>" + Statistics.Damage(Trap.Level, DamageExpressionType.Multiple) + "</TD>");
                lines.Add("</TR>");
            }

            lines.Add("<TR class=heading>");
            lines.Add("<TD colspan=2><B>Skill Advice</B></TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Easy DC</TD>");
            lines.Add("<TD align=center>" + Ai.GetSkillDc(Difficulty.Easy, Trap.Level) + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Moderate DC</TD>");
            lines.Add("<TD align=center>" + Ai.GetSkillDc(Difficulty.Moderate, Trap.Level) + "</TD>");
            lines.Add("</TR>");

            lines.Add("<TR>");
            lines.Add("<TD>Hard DC</TD>");
            lines.Add("<TD align=center>" + Ai.GetSkillDc(Difficulty.Hard, Trap.Level) + "</TD>");
            lines.Add("</TR>");

            lines.Add("</TABLE>");
            lines.Add("</P>");

            return lines;
        }

        private void LevelUpBtn_Click(object sender, EventArgs e)
        {
            Trap.AdjustLevel(+1);
            update_statblock();
        }

        private void LevelDownBtn_Click(object sender, EventArgs e)
        {
            Trap.AdjustLevel(-1);
            update_statblock();
        }

        private void FileExport_Click(object sender, EventArgs e)
        {
            var dlg = new SaveFileDialog();
            dlg.Title = "Export Trap";
            dlg.FileName = Trap.Name;
            dlg.Filter = Program.TrapFilter;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var ok = Serialisation<Trap>.Save(dlg.FileName, Trap, SerialisationMode.Binary);

                if (!ok)
                {
                    var error = "The trap could not be exported.";
                    MessageBox.Show(error, "Masterplan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
