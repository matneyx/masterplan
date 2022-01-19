using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a magical artifact.
    /// </summary>
    [Serializable]
    public class Artifact
    {
        private List<ArtifactConcordance> _fConcordanceLevels = new List<ArtifactConcordance>();

        private List<Pair<string, string>> _fConcordanceRules = new List<Pair<string, string>>();

        private string _fDescription = "";

        private string _fDetails = "";

        private string _fGoals = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private string _fRoleplayingTips = "";

        private List<MagicItemSection> _fSections = new List<MagicItemSection>();

        private Tier _fTier = Tier.Heroic;

        /// <summary>
        ///     Gets or sets the unique ID of the artifact.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the artifact.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     The tier for which the artifact is suitable.
        /// </summary>
        public Tier Tier
        {
            get => _fTier;
            set => _fTier = value;
        }

        /// <summary>
        ///     The artifact's description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     The artifact's details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     The artifact's goals.
        /// </summary>
        public string Goals
        {
            get => _fGoals;
            set => _fGoals = value;
        }

        /// <summary>
        ///     Roleplaying tips for the artifact.
        /// </summary>
        public string RoleplayingTips
        {
            get => _fRoleplayingTips;
            set => _fRoleplayingTips = value;
        }

        /// <summary>
        ///     The artifact's enhancement / properties
        /// </summary>
        public List<MagicItemSection> Sections
        {
            get => _fSections;
            set => _fSections = value;
        }

        /// <summary>
        ///     The artifact's concordance rules.
        /// </summary>
        public List<Pair<string, string>> ConcordanceRules
        {
            get => _fConcordanceRules;
            set => _fConcordanceRules = value;
        }

        /// <summary>
        ///     The artifact's concordance levels.
        /// </summary>
        public List<ArtifactConcordance> ConcordanceLevels
        {
            get => _fConcordanceLevels;
            set => _fConcordanceLevels = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public Artifact()
        {
            AddStandardConcordanceLevels();
        }

        /// <summary>
        ///     Adds the standard concordance levels to the artifact.
        /// </summary>
        public void AddStandardConcordanceLevels()
        {
            _fConcordanceLevels.Add(new ArtifactConcordance("Pleased", "16-20"));
            _fConcordanceLevels.Add(new ArtifactConcordance("Satisfied", "12-15"));
            _fConcordanceLevels.Add(new ArtifactConcordance("Normal", "5-11"));
            _fConcordanceLevels.Add(new ArtifactConcordance("Unsatisfied", "1-4"));
            _fConcordanceLevels.Add(new ArtifactConcordance("Angered", "0 or lower"));
            _fConcordanceLevels.Add(new ArtifactConcordance("Moving On", ""));
        }

        /// <summary>
        ///     Creates a copy of the artifact.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Artifact Copy()
        {
            var a = new Artifact();

            a.Id = _fId;
            a.Name = _fName;
            a.Tier = _fTier;
            a.Description = _fDescription;
            a.Details = _fDetails;
            a.Goals = _fGoals;
            a.RoleplayingTips = _fRoleplayingTips;

            a.Sections.Clear();
            foreach (var mis in _fSections)
                a.Sections.Add(mis.Copy());

            a.ConcordanceRules.Clear();
            foreach (var pair in _fConcordanceRules)
            {
                var rule = new Pair<string, string>(pair.First, pair.Second);
                a.ConcordanceRules.Add(rule);
            }

            a.ConcordanceLevels.Clear();
            foreach (var ac in _fConcordanceLevels)
                a.ConcordanceLevels.Add(ac.Copy());

            return a;
        }

        /// <summary>
        ///     Returns the name of the artifact.
        /// </summary>
        /// <returns>Returns the name of the artifact.</returns>
        public override string ToString()
        {
            return _fName;
        }
    }

    /// <summary>
    ///     Class representing a concordance level for an artifact.
    /// </summary>
    [Serializable]
    public class ArtifactConcordance
    {
        private string _fDescription = "";

        private string _fName = "";

        private string _fQuote = "";

        private List<MagicItemSection> _fSections = new List<MagicItemSection>();

        private string _fValueRange = "";

        /// <summary>
        ///     The concordance level's name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     The concordance level's value range.
        /// </summary>
        public string ValueRange
        {
            get => _fValueRange;
            set => _fValueRange = value;
        }

        /// <summary>
        ///     The concordance level's quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     The concordance level's description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     The concordance level's enhancements and properties.
        /// </summary>
        public List<MagicItemSection> Sections
        {
            get => _fSections;
            set => _fSections = value;
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ArtifactConcordance()
        {
        }

        /// <summary>
        ///     Constructor taking a name and a value range.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="valueRange">The value range.</param>
        public ArtifactConcordance(string name, string valueRange)
        {
            _fName = name;
            _fValueRange = valueRange;
        }

        /// <summary>
        ///     Creates a copy of the artifact concordance.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public ArtifactConcordance Copy()
        {
            var ac = new ArtifactConcordance();

            ac.Name = _fName;
            ac.ValueRange = _fValueRange;
            ac.Quote = _fQuote;
            ac.Description = _fDescription;

            ac.Sections.Clear();
            foreach (var mis in _fSections)
                ac.Sections.Add(mis.Copy());

            return ac;
        }
    }
}
