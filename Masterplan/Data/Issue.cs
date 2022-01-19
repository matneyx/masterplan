using System;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Interface for project issues.
    /// </summary>
    public interface IIssue
    {
        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        string Reason { get; }
    }

    /// <summary>
    ///     Class representing an issue with a plot point's difficulty level.
    /// </summary>
    [Serializable]
    public class DifficultyIssue : IIssue
    {
        /// <summary>
        ///     Gets the plot point.
        /// </summary>
        public PlotPoint Point { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="point">The point.</param>
        public DifficultyIssue(PlotPoint point)
        {
            Point = point;
        }

        /// <summary>
        ///     [point name]: [reason]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Point.Name + ": " + Reason;
        }

        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        public string Reason
        {
            get
            {
                if (Point.State != PlotPointState.Normal)
                    return "";

                if (Point.Element == null)
                    return "";

                var name = "game element";
                if (Point.Element is Encounter)
                    name = "encounter";
                if (Point.Element is TrapElement)
                {
                    var te = Point.Element as TrapElement;
                    name = te.Trap.Type == TrapType.Trap ? "trap" : "hazard";
                }

                if (Point.Element is SkillChallenge)
                    name = "skill challenge";
                if (Point.Element is Quest)
                    name = "quest";

                var level = Workspace.GetPartyLevel(Point);

                var diff = Point.Element.GetDifficulty(level, Session.Project.Party.Size);
                switch (diff)
                {
                    case Difficulty.Trivial:
                        return "This " + name + " is too easy for a party of level " + level + ".";
                    case Difficulty.Extreme:
                        return "This " + name + " is too difficult for a party of level " + level + ".";
                }

                return "";
            }
        }
    }

    /// <summary>
    ///     Class representing an issue with a creature's difficulty level.
    /// </summary>
    [Serializable]
    public class CreatureIssue : IIssue
    {
        /// <summary>
        ///     Gets the plot point.
        /// </summary>
        public PlotPoint Point { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="point">The plot point.</param>
        public CreatureIssue(PlotPoint point)
        {
            Point = point;
        }

        /// <summary>
        ///     [point name]: [reason]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Point.Name + ": " + Reason;
        }

        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        public string Reason
        {
            get
            {
                if (Point.State != PlotPointState.Normal)
                    return "";

                var enc = Point.Element as Encounter;
                if (enc == null)
                    return "";

                var level = Workspace.GetPartyLevel(Point);

                foreach (var slot in enc.Slots)
                {
                    var diff = slot.Card.Level - level;

                    if (diff < -4)
                        return slot.Card.Title + " is more than four levels lower than the party level.";

                    if (diff > 5)
                        return slot.Card.Title + " is more than five levels higher than the party level.";
                }

                return "";
            }
        }
    }

    /// <summary>
    ///     Class representing an issue with the number of skills defined for a skill challenge.
    /// </summary>
    [Serializable]
    public class SkillIssue : IIssue
    {
        /// <summary>
        ///     Gets the plot point.
        /// </summary>
        public PlotPoint Point { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="point">The plot point.</param>
        public SkillIssue(PlotPoint point)
        {
            Point = point;
        }

        /// <summary>
        ///     [point name]: [reason]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Point.Name + ": " + Reason;
        }

        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        public string Reason
        {
            get
            {
                if (Point.State != PlotPointState.Normal)
                    return "";

                var sc = Point.Element as SkillChallenge;
                if (sc == null)
                    return "";

                if (sc.Skills.Count == 0)
                    return "No skills are defined for this skill challenge.";

                return "";
            }
        }
    }

    /// <summary>
    ///     Class representing an issue with a treasure parcel being undefined.
    /// </summary>
    [Serializable]
    public class ParcelIssue : IIssue
    {
        private Parcel _fParcel;

        /// <summary>
        ///     Gets the plot point.
        /// </summary>
        public PlotPoint Point { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="parcel">The treasure parcel.</param>
        /// <param name="pp">The plot point.</param>
        public ParcelIssue(Parcel parcel, PlotPoint pp)
        {
            _fParcel = parcel;
            Point = pp;
        }

        /// <summary>
        ///     [point name]: [reason]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Point.Name + ": " + Reason;
        }

        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        public string Reason
        {
            get
            {
                if (Point.State != PlotPointState.Normal)
                    return "";

                if (_fParcel.Name == "")
                    return "A treasure parcel in " + Point.Name + " is undefined.";

                return "";
            }
        }
    }

    /// <summary>
    ///     Class representing an issue with the number of treasure parcels in a plot.
    /// </summary>
    [Serializable]
    public class TreasureIssue : IIssue
    {
        /// <summary>
        ///     Gets the plot name.
        /// </summary>
        public string PlotName { get; } = "";

        /// <summary>
        ///     Gets the plot.
        /// </summary>
        public Plot Plot { get; }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="name">The plot name.</param>
        /// <param name="plot">The plot.</param>
        public TreasureIssue(string name, Plot plot)
        {
            PlotName = name;
            Plot = plot;
        }

        /// <summary>
        ///     [plot name]: [reason]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return PlotName + ": " + Reason;
        }

        /// <summary>
        ///     Gets the reason for the issue.
        /// </summary>
        public string Reason
        {
            get
            {
                var xpGained = 0;
                var parcelsGained = 0;

                foreach (var pp in Plot.Points)
                {
                    xpGained += pp.GetXp();

                    var points = pp.Subtree;
                    foreach (var point in points)
                        parcelsGained += point.Parcels.Count;
                }

                var totalXp = Experience.GetHeroXp(Session.Project.Party.Level);
                totalXp += xpGained / Session.Project.Party.Size;

                var finalLevel = Experience.GetHeroLevel(totalXp);
                var levelsGained = finalLevel - Session.Project.Party.Level;

                var remainder = totalXp - Experience.GetHeroXp(finalLevel);
                var requiredXp = Experience.GetHeroXp(finalLevel + 1) - Experience.GetHeroXp(finalLevel);
                if (requiredXp == 0)
                    return "";

                var parcelsPerLevel = 10 + (Session.Project.Party.Size - 5);
                var parcelsRequired = parcelsPerLevel * levelsGained;
                parcelsRequired += remainder * parcelsPerLevel / requiredXp;

                var delta = (int)(parcelsRequired * 0.3);
                var upper = parcelsRequired + delta;
                var lower = parcelsRequired - delta;

                var str = "";

                if (parcelsGained < lower)
                    str = "Too few treasure parcels are available, compared to the amount of XP given.";

                if (parcelsGained > upper)
                    str = "Too many treasure parcels are available, compared to the amount of XP given.";

                if (str != "")
                {
                    var hasSubplots = false;
                    foreach (var pp in Plot.Points)
                        if (pp.Subplot.Points.Count != 0)
                        {
                            hasSubplots = true;
                            break;
                        }

                    str += Environment.NewLine;
                    str += "This plot";
                    if (hasSubplots)
                        str += " (and its subplots)";
                    str += " should contain ";
                    if (lower == upper)
                        str += upper.ToString();
                    else
                        str += lower + " - " + upper;
                    str += " parcels; currently " + parcelsGained + " are available.";
                }

                return str;
            }
        }
    }
}
