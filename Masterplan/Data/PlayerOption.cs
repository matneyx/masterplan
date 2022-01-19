using System;
using System.Collections.Generic;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Interface implemented by player option classes.
    /// </summary>
    public interface IPlayerOption
    {
        /// <summary>
        ///     Gets or sets the unique ID of the option.
        /// </summary>
        Guid Id { get; set; }

        /// <summary>
        ///     Gets or sets the name of the option.
        /// </summary>
        string Name { get; set; }
    }

    /// <summary>
    ///     At-will / encounter / daily.
    /// </summary>
    public enum PlayerPowerType
    {
        /// <summary>
        ///     At-will power.
        /// </summary>
        AtWill,

        /// <summary>
        ///     Encounter power.
        /// </summary>
        Encounter,

        /// <summary>
        ///     Daily power.
        /// </summary>
        Daily
    }

    /// <summary>
    ///     Class representing a player power.
    /// </summary>
    [Serializable]
    public class PlayerPower : IPlayerOption
    {
        private ActionType _fAction = ActionType.Standard;

        private Guid _fId = Guid.NewGuid();

        private string _fKeywords = "";

        private string _fName = "";

        private string _fRange = "Melee weapon";

        private string _fReadAloud = "";

        private List<PlayerPowerSection> _fSections = new List<PlayerPowerSection>();

        private PlayerPowerType _fType = PlayerPowerType.Encounter;

        /// <summary>
        ///     Gets or sets the power's usage type.
        /// </summary>
        public PlayerPowerType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the power's read-aloud text.
        /// </summary>
        public string ReadAloud
        {
            get => _fReadAloud;
            set => _fReadAloud = value;
        }

        /// <summary>
        ///     Gets or sets the keywords for the power.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets or sets the action required to use the power.
        /// </summary>
        public ActionType Action
        {
            get => _fAction;
            set => _fAction = value;
        }

        /// <summary>
        ///     Gets or sets the power's range.
        /// </summary>
        public string Range
        {
            get => _fRange;
            set => _fRange = value;
        }

        /// <summary>
        ///     Gets or sets the power sections.
        /// </summary>
        public List<PlayerPowerSection> Sections
        {
            get => _fSections;
            set => _fSections = value;
        }

        /// <summary>
        ///     Creates a copy of the power.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PlayerPower Copy()
        {
            var power = new PlayerPower();

            power.Id = _fId;
            power.Name = _fName;
            power.Type = _fType;
            power.ReadAloud = _fReadAloud;
            power.Keywords = _fKeywords;
            power.Action = _fAction;
            power.Range = _fRange;

            foreach (var section in _fSections)
                power.Sections.Add(section.Copy());

            return power;
        }

        /// <summary>
        ///     Returns the name of the power.
        /// </summary>
        /// <returns>Returns the name of the power.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the power.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the power.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a section in a player power.
    /// </summary>
    [Serializable]
    public class PlayerPowerSection
    {
        private string _fDetails = "";

        private string _fHeader = "Effect";

        private Guid _fId = Guid.NewGuid();

        private int _fIndent;

        /// <summary>
        ///     Gets or sets the unique ID of the power.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the section header.
        /// </summary>
        public string Header
        {
            get => _fHeader;
            set => _fHeader = value;
        }

        /// <summary>
        ///     Gets or sets the section details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the degree of indent for the section.
        /// </summary>
        public int Indent
        {
            get => _fIndent;
            set => _fIndent = value;
        }

        /// <summary>
        ///     Creates a copy of the power section.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PlayerPowerSection Copy()
        {
            var s = new PlayerPowerSection();

            s.Id = _fId;
            s.Header = _fHeader;
            s.Details = _fDetails;
            s.Indent = _fIndent;

            return s;
        }
    }

    /// <summary>
    ///     Enumeration for game tiers.
    /// </summary>
    public enum Tier
    {
        /// <summary>
        ///     Heroic tier.
        /// </summary>
        Heroic,

        /// <summary>
        ///     Paragon tier.
        /// </summary>
        Paragon,

        /// <summary>
        ///     Epic tier.
        /// </summary>
        Epic
    }

    /// <summary>
    ///     Class representing a feat.
    /// </summary>
    [Serializable]
    public class Feat : IPlayerOption
    {
        private string _fBenefits = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private string _fPrerequisites = "";

        private Tier _fTier = Tier.Heroic;

        /// <summary>
        ///     Gets or sets the feat's tier.
        /// </summary>
        public Tier Tier
        {
            get => _fTier;
            set => _fTier = value;
        }

        /// <summary>
        ///     Gets or sets the prerequisites for the feat.
        /// </summary>
        public string Prerequisites
        {
            get => _fPrerequisites;
            set => _fPrerequisites = value;
        }

        /// <summary>
        ///     Gets or sets the feat benefits.
        /// </summary>
        public string Benefits
        {
            get => _fBenefits;
            set => _fBenefits = value;
        }

        /// <summary>
        ///     Creates a copy of the feat.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Feat Copy()
        {
            var ft = new Feat();

            ft.Id = _fId;
            ft.Name = _fName;
            ft.Tier = _fTier;
            ft.Prerequisites = _fPrerequisites;
            ft.Benefits = _fBenefits;

            return ft;
        }

        /// <summary>
        ///     Returns the name of the feat.
        /// </summary>
        /// <returns>Returns the name of the feat.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the feat.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the feat.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a player background.
    /// </summary>
    [Serializable]
    public class PlayerBackground : IPlayerOption
    {
        private string _fAssociatedSkills = "";

        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private string _fRecommendedFeats = "";

        /// <summary>
        ///     Gets or sets the background details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the associated skills for the background.
        /// </summary>
        public string AssociatedSkills
        {
            get => _fAssociatedSkills;
            set => _fAssociatedSkills = value;
        }

        /// <summary>
        ///     Gets or sets the recommended feats for the background.
        /// </summary>
        public string RecommendedFeats
        {
            get => _fRecommendedFeats;
            set => _fRecommendedFeats = value;
        }

        /// <summary>
        ///     Creates a copy of the background.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PlayerBackground Copy()
        {
            var bg = new PlayerBackground();

            bg.Id = _fId;
            bg.Name = _fName;
            bg.Details = _fDetails;
            bg.AssociatedSkills = _fAssociatedSkills;
            bg.RecommendedFeats = _fRecommendedFeats;

            return bg;
        }

        /// <summary>
        ///     Returns the name of the background.
        /// </summary>
        /// <returns>Returns the name of the background.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the background.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the background.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a race.
    /// </summary>
    [Serializable]
    public class Race : IPlayerOption
    {
        private string _fAbilityScores = "";

        private string _fDetails = "";

        private List<Feature> _fFeatures = new List<Feature>();

        private string _fHeightRange = "";

        private Guid _fId = Guid.NewGuid();

        private string _fLanguages = "Common";

        private string _fName = "";

        private List<PlayerPower> _fPowers = new List<PlayerPower>();

        private string _fQuote = "";

        private CreatureSize _fSize = CreatureSize.Medium;

        private string _fSkillBonuses = "";

        private string _fSpeed = "6 squares";

        private string _fVision = "Normal";

        private string _fWeightRange = "";

        /// <summary>
        ///     Gets or sets the defining quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     Gets or sets the height range of the race.
        /// </summary>
        public string HeightRange
        {
            get => _fHeightRange;
            set => _fHeightRange = value;
        }

        /// <summary>
        ///     Gets or sets the weight range of the race.
        /// </summary>
        public string WeightRange
        {
            get => _fWeightRange;
            set => _fWeightRange = value;
        }

        /// <summary>
        ///     Gets or sets the ability score modifiers for the race.
        /// </summary>
        public string AbilityScores
        {
            get => _fAbilityScores;
            set => _fAbilityScores = value;
        }

        /// <summary>
        ///     Gets or sets the size of the race.
        /// </summary>
        public CreatureSize Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the speed of the race.
        /// </summary>
        public string Speed
        {
            get => _fSpeed;
            set => _fSpeed = value;
        }

        /// <summary>
        ///     Gets or sets the race's vision.
        /// </summary>
        public string Vision
        {
            get => _fVision;
            set => _fVision = value;
        }

        /// <summary>
        ///     Gets or sets the race's starting languages.
        /// </summary>
        public string Languages
        {
            get => _fLanguages;
            set => _fLanguages = value;
        }

        /// <summary>
        ///     Gets or sets the race's skill bonuses.
        /// </summary>
        public string SkillBonuses
        {
            get => _fSkillBonuses;
            set => _fSkillBonuses = value;
        }

        /// <summary>
        ///     Gets or sets the racial features.
        /// </summary>
        public List<Feature> Features
        {
            get => _fFeatures;
            set => _fFeatures = value;
        }

        /// <summary>
        ///     Gets or sets the racial powers.
        /// </summary>
        public List<PlayerPower> Powers
        {
            get => _fPowers;
            set => _fPowers = value;
        }

        /// <summary>
        ///     Gets or sets the race details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Creates a copy of the race.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Race Copy()
        {
            var race = new Race();

            race.Id = _fId;
            race.Name = _fName;
            race.Quote = _fQuote;
            race.HeightRange = _fHeightRange;
            race.WeightRange = _fWeightRange;
            race.AbilityScores = _fAbilityScores;
            race.Size = _fSize;
            race.Speed = _fSpeed;
            race.Vision = _fVision;
            race.Languages = _fLanguages;
            race.SkillBonuses = _fSkillBonuses;
            race.Details = _fDetails;

            foreach (var ft in _fFeatures)
                race.Features.Add(ft.Copy());

            foreach (var power in _fPowers)
                race.Powers.Add(power.Copy());

            return race;
        }

        /// <summary>
        ///     Returns the name of the race.
        /// </summary>
        /// <returns>Returns the name of the race.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the race.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the race.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a playable class.
    /// </summary>
    [Serializable]
    public class Class : IPlayerOption
    {
        private string _fArmourProficiencies = "";

        private string _fDefenceBonuses = "";

        private string _fDescription = "";

        private LevelData _fFeatureData = new LevelData();

        private int _fHealingSurges;

        private int _fHpFirst;

        private int _fHpSubsequent;

        private Guid _fId = Guid.NewGuid();

        private string _fImplements = "";

        private string _fKeyAbilities = "";

        private List<LevelData> _fLevels = new List<LevelData>();

        private string _fName = "";

        private string _fOverviewCharacteristics = "";

        private string _fOverviewRaces = "";

        private string _fOverviewReligion = "";

        private string _fPowerSource = "";

        private string _fQuote = "";

        private string _fRole = "";

        private string _fTrainedSkills = "";

        private string _fWeaponProficiencies = "";

        /// <summary>
        ///     Gets or sets the defining quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     Gets or sets the class role.
        /// </summary>
        public string Role
        {
            get => _fRole;
            set => _fRole = value;
        }

        /// <summary>
        ///     Gets or sets the class's power source.
        /// </summary>
        public string PowerSource
        {
            get => _fPowerSource;
            set => _fPowerSource = value;
        }

        /// <summary>
        ///     Gets or sets the class's key abilities.
        /// </summary>
        public string KeyAbilities
        {
            get => _fKeyAbilities;
            set => _fKeyAbilities = value;
        }

        /// <summary>
        ///     Gets or sets the class's armour proficiencies.
        /// </summary>
        public string ArmourProficiencies
        {
            get => _fArmourProficiencies;
            set => _fArmourProficiencies = value;
        }

        /// <summary>
        ///     Gets or sets the class's weapon proficiencies.
        /// </summary>
        public string WeaponProficiencies
        {
            get => _fWeaponProficiencies;
            set => _fWeaponProficiencies = value;
        }

        /// <summary>
        ///     Gets or sets the class's implement proficiencies.
        /// </summary>
        public string Implements
        {
            get => _fImplements;
            set => _fImplements = value;
        }

        /// <summary>
        ///     Gets or sets the class's defence bonuses.
        /// </summary>
        public string DefenceBonuses
        {
            get => _fDefenceBonuses;
            set => _fDefenceBonuses = value;
        }

        /// <summary>
        ///     Gets or sets the class's first level HP.
        /// </summary>
        public int HpFirst
        {
            get => _fHpFirst;
            set => _fHpFirst = value;
        }

        /// <summary>
        ///     Gets or sets the class's HP per level.
        /// </summary>
        public int HpSubsequent
        {
            get => _fHpSubsequent;
            set => _fHpSubsequent = value;
        }

        /// <summary>
        ///     Gets or sets the class's healing surges.
        /// </summary>
        public int HealingSurges
        {
            get => _fHealingSurges;
            set => _fHealingSurges = value;
        }

        /// <summary>
        ///     Gets or sets the class's trained skills.
        /// </summary>
        public string TrainedSkills
        {
            get => _fTrainedSkills;
            set => _fTrainedSkills = value;
        }

        /// <summary>
        ///     Gets or sets the description for the class.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     Gets or sets the class's overview characteristics.
        /// </summary>
        public string OverviewCharacteristics
        {
            get => _fOverviewCharacteristics;
            set => _fOverviewCharacteristics = value;
        }

        /// <summary>
        ///     Gets or sets the class's religion information.
        /// </summary>
        public string OverviewReligion
        {
            get => _fOverviewReligion;
            set => _fOverviewReligion = value;
        }

        /// <summary>
        ///     Gets or sets the class's race information.
        /// </summary>
        public string OverviewRaces
        {
            get => _fOverviewRaces;
            set => _fOverviewRaces = value;
        }

        /// <summary>
        ///     Gets or sets the class's feature powers.
        /// </summary>
        public LevelData FeatureData
        {
            get => _fFeatureData;
            set => _fFeatureData = value;
        }

        /// <summary>
        ///     Gets or sets the class's powers.
        /// </summary>
        public List<LevelData> Levels
        {
            get => _fLevels;
            set => _fLevels = value;
        }

        // TODO: Builds

        /// <summary>
        ///     Creates a copy of the class.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Class Copy()
        {
            var c = new Class();

            c.Id = _fId;
            c.Name = _fName;
            c.Quote = _fQuote;

            c.Role = _fRole;
            c.PowerSource = _fPowerSource;
            c.KeyAbilities = _fKeyAbilities;

            c.ArmourProficiencies = _fArmourProficiencies;
            c.WeaponProficiencies = _fWeaponProficiencies;
            c.Implements = _fImplements;
            c.DefenceBonuses = _fDefenceBonuses;

            c.HpFirst = _fHpFirst;
            c.HpSubsequent = _fHpSubsequent;
            c.HealingSurges = _fHealingSurges;

            c.TrainedSkills = _fTrainedSkills;

            c.Description = _fDescription;

            c.OverviewCharacteristics = _fOverviewCharacteristics;
            c.OverviewReligion = _fOverviewReligion;
            c.OverviewRaces = _fOverviewRaces;

            c.FeatureData = _fFeatureData.Copy();

            foreach (var ld in _fLevels)
                c.Levels.Add(ld.Copy());

            // Builds

            return c;
        }

        /// <summary>
        ///     Returns the name of the class.
        /// </summary>
        /// <returns>Returns the name of the class.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the class.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the class.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a character theme.
    /// </summary>
    [Serializable]
    public class Theme : IPlayerOption
    {
        private string _fDetails = "";

        private PlayerPower _fGrantedPower = new PlayerPower();

        private Guid _fId = Guid.NewGuid();

        private List<LevelData> _fLevels = new List<LevelData>();

        private string _fName = "";

        private string _fPowerSource = "";

        private string _fPrerequisites = "";

        private string _fQuote = "";

        private string _fSecondaryRole = "";

        /// <summary>
        ///     Gets or sets the defining quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     Gets or sets the prerequisites for the theme.
        /// </summary>
        public string Prerequisites
        {
            get => _fPrerequisites;
            set => _fPrerequisites = value;
        }

        /// <summary>
        ///     Gets or sets the theme's secondary role.
        /// </summary>
        public string SecondaryRole
        {
            get => _fSecondaryRole;
            set => _fSecondaryRole = value;
        }

        /// <summary>
        ///     Gets or sets the theme's power source.
        /// </summary>
        public string PowerSource
        {
            get => _fPowerSource;
            set => _fPowerSource = value;
        }

        /// <summary>
        ///     Gets or sets the theme's granted power.
        /// </summary>
        public PlayerPower GrantedPower
        {
            get => _fGrantedPower;
            set => _fGrantedPower = value;
        }

        /// <summary>
        ///     Gets or sets the theme details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the level data.
        /// </summary>
        public List<LevelData> Levels
        {
            get => _fLevels;
            set => _fLevels = value;
        }

        /// <summary>
        ///     Creates a copy of the theme.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Theme Copy()
        {
            var theme = new Theme();

            theme.Id = _fId;
            theme.Name = _fName;
            theme.Quote = _fQuote;
            theme.Prerequisites = _fPrerequisites;
            theme.SecondaryRole = _fSecondaryRole;
            theme.PowerSource = _fPowerSource;
            theme.GrantedPower = _fGrantedPower.Copy();
            theme.Details = _fDetails;

            foreach (var ld in _fLevels)
                theme.Levels.Add(ld.Copy());

            return theme;
        }

        /// <summary>
        ///     Returns the name of the theme.
        /// </summary>
        /// <returns>Returns the name of the theme.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the theme.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the theme.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a paragon path.
    /// </summary>
    [Serializable]
    public class ParagonPath : IPlayerOption
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private List<LevelData> _fLevels = new List<LevelData>();

        private string _fName = "";

        private string _fPrerequisites = "11th level";

        private string _fQuote = "";

        /// <summary>
        ///     Gets or sets the defining quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     Gets or sets the prerequisites for the paragon path.
        /// </summary>
        public string Prerequisites
        {
            get => _fPrerequisites;
            set => _fPrerequisites = value;
        }

        /// <summary>
        ///     Gets or sets the paragon path details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the level data.
        /// </summary>
        public List<LevelData> Levels
        {
            get => _fLevels;
            set => _fLevels = value;
        }

        /// <summary>
        ///     Creates a copy of the paragon path.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public ParagonPath Copy()
        {
            var pp = new ParagonPath();

            pp.Id = _fId;
            pp.Name = _fName;
            pp.Quote = _fQuote;
            pp.Prerequisites = _fPrerequisites;
            pp.Details = _fDetails;

            foreach (var ld in _fLevels)
                pp.Levels.Add(ld.Copy());

            return pp;
        }

        /// <summary>
        ///     Returns the name of the paragon path.
        /// </summary>
        /// <returns>Returns the name of the paragon path.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the paragon path.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the paragon path.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing an epic destiny.
    /// </summary>
    [Serializable]
    public class EpicDestiny : IPlayerOption
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fImmortality = "";

        private List<LevelData> _fLevels = new List<LevelData>();

        private string _fName = "";

        private string _fPrerequisites = "21st level";

        private string _fQuote = "";

        /// <summary>
        ///     Gets or sets the defining quote.
        /// </summary>
        public string Quote
        {
            get => _fQuote;
            set => _fQuote = value;
        }

        /// <summary>
        ///     Gets or sets the prerequisites for the epic destiny.
        /// </summary>
        public string Prerequisites
        {
            get => _fPrerequisites;
            set => _fPrerequisites = value;
        }

        /// <summary>
        ///     Gets or sets the epic destiny details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the immortality details for the epic destiny.
        /// </summary>
        public string Immortality
        {
            get => _fImmortality;
            set => _fImmortality = value;
        }

        /// <summary>
        ///     Gets or sets the level data.
        /// </summary>
        public List<LevelData> Levels
        {
            get => _fLevels;
            set => _fLevels = value;
        }

        /// <summary>
        ///     Creates a copy of the epic destiny.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EpicDestiny Copy()
        {
            var ed = new EpicDestiny();

            ed.Id = _fId;
            ed.Name = _fName;
            ed.Quote = _fQuote;
            ed.Prerequisites = _fPrerequisites;
            ed.Details = _fDetails;
            ed.Immortality = _fImmortality;

            foreach (var ld in _fLevels)
                ed.Levels.Add(ld.Copy());

            return ed;
        }

        /// <summary>
        ///     Returns the name of the epic destiny.
        /// </summary>
        /// <returns>Returns the name of the epic destiny.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the epic destiny.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the epic destiny.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class holding feature and power information for a level.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        private List<Feature> _fFeatures = new List<Feature>();

        private int _fLevel;

        private List<PlayerPower> _fPowers = new List<PlayerPower>();

        /// <summary>
        ///     Gets or sets the level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the list of features.
        /// </summary>
        public List<Feature> Features
        {
            get => _fFeatures;
            set => _fFeatures = value;
        }

        /// <summary>
        ///     Gets or sets the list of powers.
        /// </summary>
        public List<PlayerPower> Powers
        {
            get => _fPowers;
            set => _fPowers = value;
        }

        /// <summary>
        ///     Returns the total number of items in the level.
        /// </summary>
        public int Count => _fFeatures.Count + _fPowers.Count;

        /// <summary>
        ///     Creates a copy of the level data.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public LevelData Copy()
        {
            var ld = new LevelData();

            ld.Level = _fLevel;

            foreach (var ft in _fFeatures)
                ld.Features.Add(ft.Copy());

            foreach (var pp in _fPowers)
                ld.Powers.Add(pp.Copy());

            return ld;
        }

        /// <summary>
        ///     Returns a string representation of the level data.
        /// </summary>
        /// <returns>Returns a string representation of the level data.</returns>
        public override string ToString()
        {
            var str = "";

            foreach (var ft in _fFeatures)
            {
                if (str != "")
                    str += "; ";

                str += ft.Name;
            }

            foreach (var power in _fPowers)
            {
                if (str != "")
                    str += "; ";

                str += power.Name;
            }

            if (str == "")
                str = "(empty)";

            if (_fLevel >= 1)
                return "Level " + _fLevel + ": " + str;
            return str;
        }
    }

    /// <summary>
    ///     Class representing a race / paragon path / epic destiny feature.
    /// </summary>
    [Serializable]
    public class Feature
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the unique ID of the feature.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the featue.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the feature details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Creates a copy of the feature.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Feature Copy()
        {
            var f = new Feature();

            f.Id = _fId;
            f.Name = _fName;
            f.Details = _fDetails;

            return f;
        }

        /// <summary>
        ///     Returns the name of the feature.
        /// </summary>
        /// <returns>Returns the name of the feature.</returns>
        public override string ToString()
        {
            return _fName;
        }
    }

    /// <summary>
    ///     Enumeration containing weapon categories.
    /// </summary>
    public enum WeaponCategory
    {
        /// <summary>
        ///     Simple weapons.
        /// </summary>
        Simple,

        /// <summary>
        ///     Military weapons.
        /// </summary>
        Military,

        /// <summary>
        ///     Superior weapons.
        /// </summary>
        Superior
    }

    /// <summary>
    ///     Enumeration containing weapon types.
    /// </summary>
    public enum WeaponType
    {
        /// <summary>
        ///     Melee weapons.
        /// </summary>
        Melee,

        /// <summary>
        ///     Ranged weapons.
        /// </summary>
        Ranged
    }

    /// <summary>
    ///     Class representing a weapon.
    /// </summary>
    [Serializable]
    public class Weapon : IPlayerOption
    {
        private WeaponCategory _fCategory = WeaponCategory.Simple;

        private string _fDamage = "";

        private string _fDescription = "";

        private string _fGroup = "";

        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private string _fPrice = "";

        private int _fProficiency = 2;

        private string _fProperties = "";

        private string _fRange = "";

        private bool _fTwoHanded;

        private WeaponType _fType = WeaponType.Melee;

        private string _fWeight = "";

        /// <summary>
        ///     Gets or sets the weapon's category.
        /// </summary>
        public WeaponCategory Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the weapon's type.
        /// </summary>
        public WeaponType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets whether the weapon must be used two-handed.
        /// </summary>
        public bool TwoHanded
        {
            get => _fTwoHanded;
            set => _fTwoHanded = value;
        }

        /// <summary>
        ///     Gets or sets the weapon's proficiency bonus.
        /// </summary>
        public int Proficiency
        {
            get => _fProficiency;
            set => _fProficiency = value;
        }

        /// <summary>
        ///     Gets or sets the weapon's damage.
        /// </summary>
        public string Damage
        {
            get => _fDamage;
            set => _fDamage = value;
        }

        /// <summary>
        ///     Gets or sets the range of the weapon.
        /// </summary>
        public string Range
        {
            get => _fRange;
            set => _fRange = value;
        }

        /// <summary>
        ///     Gets or sets the price of the weapon.
        /// </summary>
        public string Price
        {
            get => _fPrice;
            set => _fPrice = value;
        }

        /// <summary>
        ///     Gets or sets the weight of the weapon.
        /// </summary>
        public string Weight
        {
            get => _fWeight;
            set => _fWeight = value;
        }

        /// <summary>
        ///     Gets or sets the weapon group(s).
        /// </summary>
        public string Group
        {
            get => _fGroup;
            set => _fGroup = value;
        }

        /// <summary>
        ///     Gets or sets the weapon properties.
        /// </summary>
        public string Properties
        {
            get => _fProperties;
            set => _fProperties = value;
        }

        /// <summary>
        ///     Gets or sets the weapon description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     Creates a copy of the weapon.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Weapon Copy()
        {
            var w = new Weapon();

            w.Id = _fId;
            w.Name = _fName;
            w.Category = _fCategory;
            w.Type = _fType;
            w.TwoHanded = _fTwoHanded;
            w.Proficiency = _fProficiency;
            w.Damage = _fDamage;
            w.Range = _fRange;
            w.Price = _fPrice;
            w.Weight = _fWeight;
            w.Group = _fGroup;
            w.Properties = _fProperties;
            w.Description = _fDescription;

            return w;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the weapon.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the weapon.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Ritual categories.
    /// </summary>
    public enum RitualCategory
    {
        /// <summary>
        ///     Binding ritual.
        /// </summary>
        Binding,

        /// <summary>
        ///     Creation ritual.
        /// </summary>
        Creation,

        /// <summary>
        ///     Deception ritual.
        /// </summary>
        Deception,

        /// <summary>
        ///     Divination ritual.
        /// </summary>
        Divination,

        /// <summary>
        ///     Exploration ritual.
        /// </summary>
        Exploration,

        /// <summary>
        ///     Restoration ritual.
        /// </summary>
        Restoration,

        /// <summary>
        ///     Scrying ritual.
        /// </summary>
        Scrying,

        /// <summary>
        ///     Travel ritual.
        /// </summary>
        Travel,

        /// <summary>
        ///     Warding ritual.
        /// </summary>
        Warding
    }

    /// <summary>
    ///     Class representing a ritual.
    /// </summary>
    [Serializable]
    public class Ritual : IPlayerOption
    {
        private RitualCategory _fCategory = RitualCategory.Binding;

        private string _fComponentCost = "";

        private string _fDetails = "";

        private string _fDuration = "";

        private Guid _fId = Guid.NewGuid();

        private string _fKeySkill = "";

        private int _fLevel = 1;

        private string _fMarketPrice = "";

        private string _fName = "";

        private string _fReadAloud = "";

        private string _fTime = "";

        /// <summary>
        ///     Gets or sets the ritual's read-aloud text.
        /// </summary>
        public string ReadAloud
        {
            get => _fReadAloud;
            set => _fReadAloud = value;
        }

        /// <summary>
        ///     Gets or sets the level of the ritual.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the ritual's category.
        /// </summary>
        public RitualCategory Category
        {
            get => _fCategory;
            set => _fCategory = value;
        }

        /// <summary>
        ///     Gets or sets the time required for the ritual.
        /// </summary>
        public string Time
        {
            get => _fTime;
            set => _fTime = value;
        }

        /// <summary>
        ///     Gets or sets the duration of the ritual.
        /// </summary>
        public string Duration
        {
            get => _fDuration;
            set => _fDuration = value;
        }

        /// <summary>
        ///     Gets or sets the component cost for the ritual.
        /// </summary>
        public string ComponentCost
        {
            get => _fComponentCost;
            set => _fComponentCost = value;
        }

        /// <summary>
        ///     Gets or sets the market price for the ritual.
        /// </summary>
        public string MarketPrice
        {
            get => _fMarketPrice;
            set => _fMarketPrice = value;
        }

        /// <summary>
        ///     Gets or sets the ritual's key skill.
        /// </summary>
        public string KeySkill
        {
            get => _fKeySkill;
            set => _fKeySkill = value;
        }

        /// <summary>
        ///     Gets or sets the ritual's details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Creates a copy of he ritual.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Ritual Copy()
        {
            var r = new Ritual();

            r.Id = _fId;
            r.Name = _fName;
            r.ReadAloud = _fReadAloud;
            r.Level = _fLevel;
            r.Category = _fCategory;
            r.Time = _fTime;
            r.Duration = _fDuration;
            r.ComponentCost = _fComponentCost;
            r.MarketPrice = _fMarketPrice;
            r.KeySkill = _fKeySkill;
            r.Details = _fDetails;

            return r;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the ritual.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the ritual.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a set of information about a creature type.
    /// </summary>
    [Serializable]
    public class CreatureLore : IPlayerOption
    {
        private Guid _fId = Guid.NewGuid();

        private string _fName = "";

        private string _fSkillName = "";

        private List<Pair<int, string>> _information = new List<Pair<int, string>>();

        /// <summary>
        ///     Gets or sets the name of the skill to be used.
        /// </summary>
        public string SkillName
        {
            get => _fSkillName;
            set => _fSkillName = value;
        }

        /// <summary>
        ///     Gets or sets the creature information.
        /// </summary>
        public List<Pair<int, string>> Information
        {
            get => _information;
            set => _information = value;
        }

        /// <summary>
        ///     Creates a copy of the creature lore.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public CreatureLore Copy()
        {
            var cl = new CreatureLore();

            cl.Id = _fId;
            cl.Name = _fName;
            cl.SkillName = _fSkillName;

            foreach (var info in _information)
                cl.Information.Add(new Pair<int, string>(info.First, info.Second));

            return cl;
        }

        /// <summary>
        ///     Returns the name of the creature type.
        /// </summary>
        /// <returns>Returns the name of the creature type.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the creature.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the creature.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a disease.
    /// </summary>
    [Serializable]
    public class Disease : IPlayerOption
    {
        private string _fAttack = "";

        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fImproveDc = "";

        private string _fLevel = "";

        private List<string> _fLevels = new List<string>();

        private string _fMaintainDc = "";

        private string _fName = "";

        /// <summary>
        ///     Gets or sets the level of the disease.
        /// </summary>
        public string Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the disease details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the disease attack details.
        /// </summary>
        public string Attack
        {
            get => _fAttack;
            set => _fAttack = value;
        }

        /// <summary>
        ///     Gets or sets the endurance DC to improve.
        /// </summary>
        public string ImproveDc
        {
            get => _fImproveDc;
            set => _fImproveDc = value;
        }

        /// <summary>
        ///     Gets or sets the endurance DC to maintain.
        /// </summary>
        public string MaintainDc
        {
            get => _fMaintainDc;
            set => _fMaintainDc = value;
        }

        /// <summary>
        ///     Gets or sets the disease levels.
        /// </summary>
        public List<string> Levels
        {
            get => _fLevels;
            set => _fLevels = value;
        }

        /// <summary>
        ///     Creates a copy of the disease.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Disease Copy()
        {
            var d = new Disease();

            d.Id = _fId;
            d.Name = _fName;
            d.Level = _fLevel;
            d.Details = _fDetails;
            d.Attack = _fAttack;
            d.ImproveDc = _fImproveDc;
            d.MaintainDc = _fMaintainDc;
            d.Levels.AddRange(_fLevels);

            return d;
        }

        /// <summary>
        ///     Returns the name of the disease.
        /// </summary>
        /// <returns>Returns the name of the disease.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the disease.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the disease.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }

    /// <summary>
    ///     Class representing a poison.
    /// </summary>
    [Serializable]
    public class Poison : IPlayerOption
    {
        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private int _fLevel = 1;

        private string _fName = "";

        private List<PlayerPowerSection> _fSections = new List<PlayerPowerSection>();

        /// <summary>
        ///     Gets or sets the level of the poison.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets or sets the poison details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the poison sections.
        /// </summary>
        public List<PlayerPowerSection> Sections
        {
            get => _fSections;
            set => _fSections = value;
        }

        /// <summary>
        ///     Creates a copy of the poison.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Poison Copy()
        {
            var poison = new Poison();

            poison.Id = _fId;
            poison.Name = _fName;
            poison.Level = _fLevel;
            poison.Details = _fDetails;

            foreach (var section in _fSections)
                poison.Sections.Add(section.Copy());

            return poison;
        }

        /// <summary>
        ///     Returns the name of the poison.
        /// </summary>
        /// <returns>Returns the name of the poison.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Gets or sets the unique ID of the poison.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name of the poison.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }
    }
}
