using System.Collections.Generic;
using System.Windows.Forms;
using Masterplan.Data;
using Masterplan.Tools;

namespace Masterplan.UI
{
    internal partial class IssuesForm : Form
    {
        public IssuesForm(Plot plot)
        {
            InitializeComponent();

            var points = plot.AllPlotPoints;

            var lines = new List<string>();

            lines.AddRange(Html.GetHead("Plot Design Issues", "", Session.Preferences.TextSize));
            lines.Add("<BODY>");

            var difficultyIssues = new List<DifficultyIssue>();
            foreach (var pp in points)
            {
                var di = new DifficultyIssue(pp);
                if (di.Reason != "")
                    difficultyIssues.Add(di);
            }

            lines.Add("<H4>Difficulty Issues</H4>");
            if (difficultyIssues.Count != 0)
            {
                foreach (var issue in difficultyIssues)
                {
                    lines.Add("<P>");
                    lines.Add("<B>" + issue.Point + "</B>: " + issue.Reason);
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(none)");
                lines.Add("</P>");
            }

            lines.Add("<HR>");

            var creatureIssues = new List<CreatureIssue>();
            foreach (var pp in points)
                if (pp.Element is Encounter)
                {
                    var ci = new CreatureIssue(pp);
                    if (ci.Reason != "")
                        creatureIssues.Add(ci);
                }

            lines.Add("<H4>Creature Choice Issues</H4>");
            if (difficultyIssues.Count != 0)
            {
                foreach (var issue in creatureIssues)
                {
                    lines.Add("<P>");
                    lines.Add("<B>" + issue.Point + "</B>: " + issue.Reason);
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(none)");
                lines.Add("</P>");
            }

            lines.Add("<HR>");

            var skillIssues = new List<SkillIssue>();
            foreach (var pp in points)
                if (pp.Element is SkillChallenge)
                {
                    var si = new SkillIssue(pp);
                    if (si.Reason != "")
                        skillIssues.Add(si);
                }

            lines.Add("<H4>Undefined Skill Challenges</H4>");
            if (skillIssues.Count != 0)
            {
                foreach (var issue in skillIssues)
                {
                    lines.Add("<P>");
                    lines.Add("<B>" + issue.Point + "</B>: " + issue.Reason);
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(none)");
                lines.Add("</P>");
            }

            lines.Add("<HR>");

            var parcelIssues = new List<ParcelIssue>();
            foreach (var pp in points)
            foreach (var parcel in pp.Parcels)
                if (parcel.Name == "")
                {
                    var pi = new ParcelIssue(parcel, pp);
                    parcelIssues.Add(pi);
                }

            lines.Add("<H4>Undefined Treasure Parcels</H4>");
            if (parcelIssues.Count != 0)
            {
                foreach (var issue in parcelIssues)
                {
                    lines.Add("<P>");
                    lines.Add("<B>" + issue.Point + "</B>: " + issue.Reason);
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(none)");
                lines.Add("</P>");
            }

            lines.Add("<HR>");

            var treasureIssues = new List<TreasureIssue>();
            var parent = Session.Project.FindParent(plot);
            var plotName = parent != null ? parent.Name : Session.Project.Name;
            add_treasure_issues(plotName, plot, treasureIssues);
            lines.Add("<H4>Treasure Allocation Issues</H4>");
            if (treasureIssues.Count != 0)
            {
                foreach (var issue in treasureIssues)
                {
                    lines.Add("<P>");
                    lines.Add("<B>" + issue.PlotName + "</B>: " + issue.Reason);
                    lines.Add("</P>");
                }
            }
            else
            {
                lines.Add("<P class=instruction>");
                lines.Add("(none)");
                lines.Add("</P>");
            }

            lines.Add("</BODY>");
            lines.Add("</HTML>");

            Browser.DocumentText = Html.Concatenate(lines);
        }

        private void add_treasure_issues(string plotname, Plot plot, List<TreasureIssue> treasureIssues)
        {
            var ti = new TreasureIssue(plotname, plot);
            if (ti.Reason != "")
            {
                treasureIssues.Add(ti);

                foreach (var pp in plot.Points)
                    add_treasure_issues(pp.Name, pp.Subplot, treasureIssues);
            }
        }
    }
}
