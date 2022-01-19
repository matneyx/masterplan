using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing a skill challenge.
    /// </summary>
    [Serializable]
    public class SkillChallenge : IElement
    {
        private int _fComplexity = 1;

        private string _fFailure = "";

        private Guid _fId = Guid.NewGuid();

        private int _fLevel = -1;

        private Guid _fMapAreaId = Guid.Empty;

        private Guid _fMapId = Guid.Empty;

        private string _fName = "";

        private string _fNotes = "";

        private List<SkillChallengeData> _fSkills = new List<SkillChallengeData>();

        private string _fSuccess = "";

        /// <summary>
        ///     Gets or sets the unique ID of the challenge.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the challenge.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the level of the challenge.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the challenge complexity.
        /// </summary>
        public int Complexity
        {
            get => _fComplexity;
            set => _fComplexity = value;
        }

        /// <summary>
        ///     Gets or sets the list of skills used in this challenge.
        /// </summary>
        public List<SkillChallengeData> Skills
        {
            get => _fSkills;
            set => _fSkills = value;
        }

        /// <summary>
        ///     Gets or sets the victory information.
        /// </summary>
        public string Success
        {
            get => _fSuccess;
            set => _fSuccess = value;
        }

        /// <summary>
        ///     Gets or sets the defeat information.
        /// </summary>
        public string Failure
        {
            get => _fFailure;
            set => _fFailure = value;
        }

        /// <summary>
        ///     Gets or sets custom notes.
        /// </summary>
        public string Notes
        {
            get => _fNotes;
            set => _fNotes = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map where the challenge takes place.
        /// </summary>
        public Guid MapId
        {
            get => _fMapId;
            set => _fMapId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the map area where the challenge takes place.
        /// </summary>
        public Guid MapAreaId
        {
            get => _fMapAreaId;
            set => _fMapAreaId = value;
        }

        /// <summary>
        ///     Gets the number of successes required by the challenge.
        /// </summary>
        public int Successes => GetSuccesses(_fComplexity);

        /// <summary>
        ///     Gets the number of successes and failures so far.
        /// </summary>
        public SkillChallengeResult Results
        {
            get
            {
                var result = new SkillChallengeResult();

                foreach (var scd in _fSkills)
                    if (scd.Results != null)
                    {
                        result.Successes += scd.Results.Successes;
                        result.Fails += scd.Results.Fails;
                    }

                return result;
            }
        }

        /// <summary>
        ///     Level N, N successes before 3 failures
        /// </summary>
        public string Info
        {
            get
            {
                if (_fLevel != -1)
                    return "Level " + _fLevel + ", " + Successes + " successes before 3 failures";
                return Successes + " successes before 3 failures";
            }
        }

        /// <summary>
        ///     Calculates the number of successes required for a challenge of the given complexity.
        /// </summary>
        /// <param name="complexity">The complexity.</param>
        /// <returns>Returns the number of successes required.</returns>
        public static int GetSuccesses(int complexity)
        {
            return (complexity + 1) * 2;
        }

        /// <summary>
        ///     Calculates the XP value of a challenge of the given level and complexity.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="complexity">The complexity.</param>
        /// <returns>Returns the XP value.</returns>
        public static int GetXp(int level, int complexity)
        {
            var xp = Experience.GetCreatureXp(level) * complexity;

            if (Session.Project != null)
                xp = (int)(xp * Session.Project.CampaignSettings.Xp);

            return xp;
        }

        /// <summary>
        ///     Find the skill data for the given skill.
        /// </summary>
        /// <param name="skill_name">The name of the skill to look for.</param>
        /// <returns>The SkillChallengeData for the skill if found; null otherwise.</returns>
        public SkillChallengeData FindSkill(string skillName)
        {
            foreach (var scd in _fSkills)
                if (scd.SkillName == skillName)
                    return scd;

            return null;
        }

        /// <summary>
        ///     Calculates the XP value of the challenge.
        /// </summary>
        /// <returns>Returns the XP value.</returns>
        public int GetXp()
        {
            return GetXp(_fLevel, _fComplexity);
        }

        /// <summary>
        ///     Calculates the difficulty of the challenge.
        /// </summary>
        /// <param name="party_level">The party level.</param>
        /// <param name="party_size">The party size.</param>
        /// <returns>Returns the difficulty.</returns>
        public Difficulty GetDifficulty(int partyLevel, int partySize)
        {
            if (_fSkills.Count == 0)
                return Difficulty.Trivial;

            var diffs = new List<Difficulty>();
            diffs.Add(Ai.GetThreatDifficulty(_fLevel, partyLevel));

            foreach (var scd in _fSkills)
                diffs.Add(scd.Difficulty);

            if (diffs.Contains(Difficulty.Extreme))
                return Difficulty.Extreme;

            if (diffs.Contains(Difficulty.Hard))
                return Difficulty.Hard;

            if (diffs.Contains(Difficulty.Moderate))
                return Difficulty.Moderate;

            if (diffs.Contains(Difficulty.Easy))
                return Difficulty.Easy;

            return Difficulty.Trivial;
        }

        /// <summary>
        ///     Creates a copy of the challenge.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public IElement Copy()
        {
            var sc = new SkillChallenge();

            sc.Id = _fId;
            sc.Name = _fName;
            sc.Level = _fLevel;
            sc.Complexity = _fComplexity;

            foreach (var scd in _fSkills)
                sc.Skills.Add(scd.Copy());

            sc.Success = _fSuccess;
            sc.Failure = _fFailure;
            sc.Notes = _fNotes;
            sc.MapId = _fMapId;
            sc.MapAreaId = _fMapAreaId;

            return sc;
        }
    }

    /// <summary>
    ///     Primary or secondary skill.
    /// </summary>
    public enum SkillType
    {
        /// <summary>
        ///     Primary skill.
        /// </summary>
        Primary,

        /// <summary>
        ///     Secondary skill.
        /// </summary>
        Secondary,

        /// <summary>
        ///     Skill which results in automatic failure.
        /// </summary>
        AutoFail
    }

    /// <summary>
    ///     Class representing a skill in a skill challenge.
    /// </summary>
    [Serializable]
    public class SkillChallengeData : IComparable<SkillChallengeData>
    {
        private int _fDcModifier;

        private string _fDetails = "";

        private Difficulty _fDifficulty = Difficulty.Moderate;

        private string _fFailure = "";

        private SkillChallengeResult _fResults = new SkillChallengeResult();

        private string _fSkillName = "";

        private string _fSuccess = "";

        private SkillType _fType = SkillType.Primary;

        /// <summary>
        ///     Gets or sets the name of the skill.
        /// </summary>
        public string SkillName
        {
            get => _fSkillName;
            set => _fSkillName = value;
        }

        /// <summary>
        ///     Gets or sets the difficulty of the skill.
        /// </summary>
        public Difficulty Difficulty
        {
            get => _fDifficulty;
            set => _fDifficulty = value;
        }

        /// <summary>
        ///     Gets or sets the skill's DC modifier.
        /// </summary>
        public int DcModifier
        {
            get => _fDcModifier;
            set => _fDcModifier = value;
        }

        /// <summary>
        ///     Gets or sets the skill type (primary / secondary).
        /// </summary>
        public SkillType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the skill details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the success information.
        /// </summary>
        public string Success
        {
            get => _fSuccess;
            set => _fSuccess = value;
        }

        /// <summary>
        ///     Gets or sets the failure information.
        /// </summary>
        public string Failure
        {
            get => _fFailure;
            set => _fFailure = value;
        }

        /// <summary>
        ///     Gets or sets the results for this skill.
        /// </summary>
        public SkillChallengeResult Results
        {
            get => _fResults;
            set => _fResults = value;
        }

        /// <summary>
        ///     Creates a copy of the skill data.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public SkillChallengeData Copy()
        {
            var scd = new SkillChallengeData();

            scd.SkillName = _fSkillName;
            scd.Difficulty = _fDifficulty;
            scd.DcModifier = _fDcModifier;
            scd.Type = _fType;
            scd.Details = _fDetails;
            scd.Success = _fSuccess;
            scd.Failure = _fFailure;
            scd.Results = _fResults.Copy();

            return scd;
        }

        /// <summary>
        ///     Compares this skill with another.
        /// </summary>
        /// <param name="rhs">The other skill.</param>
        /// <returns>
        ///     Returns -1 if this skill should be sorted before the other, +1 if the other should be sorted before this
        ///     skill, 0 otherwise.
        /// </returns>
        public int CompareTo(SkillChallengeData rhs)
        {
            var result = _fSkillName.CompareTo(rhs.SkillName);

            if (result == 0)
                result = _fDifficulty.CompareTo(rhs.Difficulty);

            if (result == 0)
                result = _fDcModifier.CompareTo(rhs.DcModifier);

            return result;
        }
    }

    /// <summary>
    ///     Class representing successes or fails for a skill challenge.
    /// </summary>
    [Serializable]
    public class SkillChallengeResult
    {
        private int _fFails;

        private int _fSuccesses;

        /// <summary>
        ///     Gets or sets the number of successes.
        /// </summary>
        public int Successes
        {
            get => _fSuccesses;
            set => _fSuccesses = value;
        }

        /// <summary>
        ///     Gets or sets the number of fails.
        /// </summary>
        public int Fails
        {
            get => _fFails;
            set => _fFails = value;
        }

        /// <summary>
        ///     Creates a copy of this SkillChallengeResult.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public SkillChallengeResult Copy()
        {
            var scr = new SkillChallengeResult();

            scr.Successes = _fSuccesses;
            scr.Fails = _fFails;

            return scr;
        }
    }
}
