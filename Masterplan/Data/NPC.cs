using System;
using System.Collections.Generic;
using System.Drawing;

namespace Masterplan.Data
{
    /// <summary>
    ///     Class representing an NPC.
    /// </summary>
    [Serializable]
    public class Npc : ICreature
    {
        private int _fAcModifier;

        private string _fAlignment = "";

        private List<Aura> _fAuras = new List<Aura>();

        private Ability _fCharisma = new Ability();

        private Ability _fConstitution = new Ability();

        private List<CreaturePower> _fCreaturePowers = new List<CreaturePower>();

        private List<DamageModifier> _fDamageModifiers = new List<DamageModifier>();

        private string _fDetails = "";

        private Ability _fDexterity = new Ability();

        private string _fEquipment = "";

        private int _fFortitudeModifier;

        private int _fHpModifier;

        private Guid _fId = Guid.NewGuid();

        private Image _fImage;

        private string _fImmune = "";

        private int _fInitiativeModifier;

        private Ability _fIntelligence = new Ability();

        private string _fKeywords = "";

        private string _fLanguages = "";

        private int _fLevel = 1;

        private string _fMovement = "";

        private string _fName = "";

        private CreatureOrigin _fOrigin = CreatureOrigin.Natural;

        private int _fReflexModifier;

        private Regeneration _fRegeneration;

        private string _fResist = "";

        private string _fSenses = "";

        private CreatureSize _fSize = CreatureSize.Medium;

        private string _fSkills = "";

        private Ability _fStrength = new Ability();

        private string _fTactics = "";

        private Guid _fTemplateId = Guid.Empty;

        private CreatureType _fType = CreatureType.MagicalBeast;

        private string _fVulnerable = "";

        private int _fWillModifier;

        private Ability _fWisdom = new Ability();

        /// <summary>
        ///     Gets or sets the ID of the template.
        /// </summary>
        public Guid TemplateId
        {
            get => _fTemplateId;
            set => _fTemplateId = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the initiative bonus.
        /// </summary>
        public int InitiativeModifier
        {
            get => _fInitiativeModifier;
            set => _fInitiativeModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the HP total.
        /// </summary>
        public int HpModifier
        {
            get => _fHpModifier;
            set => _fHpModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the AC defence.
        /// </summary>
        public int AcModifier
        {
            get => _fAcModifier;
            set => _fAcModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Fortitude defence.
        /// </summary>
        public int FortitudeModifier
        {
            get => _fFortitudeModifier;
            set => _fFortitudeModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Reflex defence.
        /// </summary>
        public int ReflexModifier
        {
            get => _fReflexModifier;
            set => _fReflexModifier = value;
        }

        /// <summary>
        ///     Gets or sets the modifier for the Will defence.
        /// </summary>
        public int WillModifier
        {
            get => _fWillModifier;
            set => _fWillModifier = value;
        }

        /// <summary>
        ///     Gets the point-buy cost of the NPC's ability scores.
        /// </summary>
        public int AbilityCost
        {
            get
            {
                var total = 0;
                var countBelow10 = 0;
                var min = 10;

                total += _fStrength.Cost;
                if (_fStrength.Score < 10)
                    countBelow10 += 1;
                if (_fStrength.Score < min)
                    min = _fStrength.Score;

                total += _fConstitution.Cost;
                if (_fConstitution.Score < 10)
                    countBelow10 += 1;
                if (_fConstitution.Score < min)
                    min = _fConstitution.Score;

                total += _fDexterity.Cost;
                if (_fDexterity.Score < 10)
                    countBelow10 += 1;
                if (_fDexterity.Score < min)
                    min = _fDexterity.Score;

                total += _fIntelligence.Cost;
                if (_fIntelligence.Score < 10)
                    countBelow10 += 1;
                if (_fIntelligence.Score < min)
                    min = _fIntelligence.Score;

                total += _fWisdom.Cost;
                if (_fWisdom.Score < 10)
                    countBelow10 += 1;
                if (_fWisdom.Score < min)
                    min = _fWisdom.Score;

                total += _fCharisma.Cost;
                if (_fCharisma.Score < 10)
                    countBelow10 += 1;
                if (_fCharisma.Score < min)
                    min = _fCharisma.Score;

                if (countBelow10 > 1)
                    return -1;

                if (min < 8)
                    return -1;

                if (min == 9)
                    total += 1;

                if (min > 9)
                    total += 2;

                return total;
            }
        }

        /// <summary>
        ///     Gets a string representation of the NPC.
        /// </summary>
        /// <returns>Returns the name of the NPC.</returns>
        public override string ToString()
        {
            return _fName;
        }

        /// <summary>
        ///     Creates a copy of the NPC.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public Npc Copy()
        {
            var npc = new Npc();

            npc.Id = _fId;
            npc.Name = _fName;
            npc.Details = _fDetails;
            npc.Size = _fSize;
            npc.Origin = _fOrigin;
            npc.Type = _fType;
            npc.Keywords = _fKeywords;
            npc.Level = _fLevel;
            npc.TemplateId = _fTemplateId;
            npc.Senses = _fSenses;
            npc.Movement = _fMovement;
            npc.Alignment = _fAlignment;
            npc.Languages = _fLanguages;
            npc.Skills = _fSkills;
            npc.Equipment = _fEquipment;

            npc.Strength = _fStrength.Copy();
            npc.Constitution = _fConstitution.Copy();
            npc.Dexterity = _fDexterity.Copy();
            npc.Intelligence = _fIntelligence.Copy();
            npc.Wisdom = _fWisdom.Copy();
            npc.Charisma = _fCharisma.Copy();

            npc.InitiativeModifier = _fInitiativeModifier;
            npc.HpModifier = _fHpModifier;
            npc.AcModifier = _fAcModifier;
            npc.FortitudeModifier = _fFortitudeModifier;
            npc.ReflexModifier = _fReflexModifier;
            npc.WillModifier = _fWillModifier;

            npc.Regeneration = _fRegeneration?.Copy();

            foreach (var aura in _fAuras)
                npc.Auras.Add(aura.Copy());

            foreach (var cp in _fCreaturePowers)
                npc.CreaturePowers.Add(cp.Copy());

            foreach (var dm in _fDamageModifiers)
                npc.DamageModifiers.Add(dm.Copy());

            npc.Resist = _fResist;
            npc.Vulnerable = _fVulnerable;
            npc.Immune = _fImmune;
            npc.Tactics = _fTactics;

            npc.Image = _fImage;

            return npc;
        }

        /// <summary>
        ///     Gets or sets the unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets or sets the size.
        /// </summary>
        public CreatureSize Size
        {
            get => _fSize;
            set => _fSize = value;
        }

        /// <summary>
        ///     Gets or sets the creature origin.
        /// </summary>
        public CreatureOrigin Origin
        {
            get => _fOrigin;
            set => _fOrigin = value;
        }

        /// <summary>
        ///     Gets or sets the creature type.
        /// </summary>
        public CreatureType Type
        {
            get => _fType;
            set => _fType = value;
        }

        /// <summary>
        ///     Gets or sets the creature keywords.
        /// </summary>
        public string Keywords
        {
            get => _fKeywords;
            set => _fKeywords = value;
        }

        /// <summary>
        ///     Gets or sets the level.
        /// </summary>
        public int Level
        {
            get => _fLevel;
            set => _fLevel = value;
        }

        /// <summary>
        ///     Gets the role.
        /// </summary>
        public IRole Role
        {
            get
            {
                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                {
                    var cr = new ComplexRole();
                    cr.Type = ct.Role;
                    cr.Leader = ct.Leader;

                    return cr;
                }

                return null;
            }
            set { }
        }

        /// <summary>
        ///     Gets or sets the senses.
        /// </summary>
        public string Senses
        {
            get => _fSenses;
            set => _fSenses = value;
        }

        /// <summary>
        ///     Gets or sets the movement.
        /// </summary>
        public string Movement
        {
            get
            {
                if (_fMovement == null || _fMovement == "")
                    return Creature.GetSpeed(_fSize) + " squares";
                return _fMovement;
            }
            set => _fMovement = value;
        }

        /// <summary>
        ///     Gets or sets the alignment.
        /// </summary>
        public string Alignment
        {
            get => _fAlignment;
            set => _fAlignment = value;
        }

        /// <summary>
        ///     Gets or sets the languages.
        /// </summary>
        public string Languages
        {
            get => _fLanguages;
            set => _fLanguages = value;
        }

        /// <summary>
        ///     Gets or sets the skills.
        /// </summary>
        public string Skills
        {
            get => _fSkills;
            set => _fSkills = value;
        }

        /// <summary>
        ///     Gets or sets the equipment.
        /// </summary>
        public string Equipment
        {
            get => _fEquipment;
            set => _fEquipment = value;
        }

        /// <summary>
        ///     Gets the category.
        /// </summary>
        public string Category
        {
            get => "NPCs";
            set { }
        }

        /// <summary>
        ///     Gets or sets the strength ability.
        /// </summary>
        public Ability Strength
        {
            get => _fStrength;
            set => _fStrength = value;
        }

        /// <summary>
        ///     Gets or sets the constitution ability.
        /// </summary>
        public Ability Constitution
        {
            get => _fConstitution;
            set => _fConstitution = value;
        }

        /// <summary>
        ///     Gets or sets the dexterity ability.
        /// </summary>
        public Ability Dexterity
        {
            get => _fDexterity;
            set => _fDexterity = value;
        }

        /// <summary>
        ///     Gets or sets the intelligence ability.
        /// </summary>
        public Ability Intelligence
        {
            get => _fIntelligence;
            set => _fIntelligence = value;
        }

        /// <summary>
        ///     Gets or sets the wisdom ability.
        /// </summary>
        public Ability Wisdom
        {
            get => _fWisdom;
            set => _fWisdom = value;
        }

        /// <summary>
        ///     Gets or sets the charisma ability.
        /// </summary>
        public Ability Charisma
        {
            get => _fCharisma;
            set => _fCharisma = value;
        }

        /// <summary>
        ///     Gets the initiative bonus.
        /// </summary>
        public int Initiative
        {
            get
            {
                var score = _fLevel / 2 + _fDexterity.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    score += ct.Initiative;

                return score + _fInitiativeModifier;
            }

            set
            {
                var score = _fLevel / 2 + _fDexterity.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    score += ct.Initiative;

                _fInitiativeModifier = value - score;
            }
        }

        /// <summary>
        ///     Gets the HP total.
        /// </summary>
        public int Hp
        {
            get
            {
                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                {
                    var hp = _fConstitution.Score;
                    hp += (_fLevel + 1) * ct.Hp;

                    return hp + _fHpModifier;
                }

                return 0;
            }
            set
            {
                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                {
                    var hp = _fConstitution.Score;
                    hp += (_fLevel + 1) * ct.Hp;

                    _fHpModifier = value - hp;
                }
            }
        }

        /// <summary>
        ///     Gets the AC defence.
        /// </summary>
        public int Ac
        {
            get
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Ac;

                return defence + _fAcModifier;
            }
            set
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Ac;

                _fAcModifier = value - defence;
            }
        }

        /// <summary>
        ///     Gets the Fortitude defence.
        /// </summary>
        public int Fortitude
        {
            get
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fStrength.Score, _fConstitution.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Fortitude;

                return defence + _fFortitudeModifier;
            }
            set
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fStrength.Score, _fConstitution.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Fortitude;

                _fFortitudeModifier = value - defence;
            }
        }

        /// <summary>
        ///     Gets the Reflex defence.
        /// </summary>
        public int Reflex
        {
            get
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Reflex;

                return defence + _fReflexModifier;
            }
            set
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fDexterity.Score, _fIntelligence.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Reflex;

                _fReflexModifier = value - defence;
            }
        }

        /// <summary>
        ///     Gets the Will defence.
        /// </summary>
        public int Will
        {
            get
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fWisdom.Score, _fCharisma.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Will;

                return defence + _fWillModifier;
            }
            set
            {
                var defence = 10 + _fLevel / 2;

                var ab = new Ability();
                ab.Score = Math.Max(_fWisdom.Score, _fCharisma.Score);
                defence += ab.Modifier;

                var ct = Session.FindTemplate(_fTemplateId, SearchType.Global);
                if (ct != null)
                    defence += ct.Will;

                _fWillModifier = value - defence;
            }
        }

        /// <summary>
        ///     Gets or sets the creature's regeneration.
        /// </summary>
        public Regeneration Regeneration
        {
            get => _fRegeneration;
            set => _fRegeneration = value;
        }

        /// <summary>
        ///     Gets or sets the list of auras.
        /// </summary>
        public List<Aura> Auras
        {
            get => _fAuras;
            set => _fAuras = value;
        }

        /// <summary>
        ///     Gets or sets the list of powers.
        /// </summary>
        public List<CreaturePower> CreaturePowers
        {
            get => _fCreaturePowers;
            set => _fCreaturePowers = value;
        }

        /// <summary>
        ///     Gets or sets the list of damage modifiers.
        /// </summary>
        public List<DamageModifier> DamageModifiers
        {
            get => _fDamageModifiers;
            set => _fDamageModifiers = value;
        }

        /// <summary>
        ///     Gets or sets the resistances.
        /// </summary>
        public string Resist
        {
            get => _fResist;
            set => _fResist = value;
        }

        /// <summary>
        ///     Gets or sets the vulnerabilities.
        /// </summary>
        public string Vulnerable
        {
            get => _fVulnerable;
            set => _fVulnerable = value;
        }

        /// <summary>
        ///     Gets or sets the immunities.
        /// </summary>
        public string Immune
        {
            get => _fImmune;
            set => _fImmune = value;
        }

        /// <summary>
        ///     Gets or sets the tactics.
        /// </summary>
        public string Tactics
        {
            get => _fTactics;
            set => _fTactics = value;
        }

        /// <summary>
        ///     Gets or sets the picture to display on the map.
        /// </summary>
        public Image Image
        {
            get => _fImage;
            set => _fImage = value;
        }

        /// <summary>
        ///     Level N [role]
        /// </summary>
        public string Info => "Level " + _fLevel + " " + Role;

        /// <summary>
        ///     [origin] [type] [keywords]
        /// </summary>
        public string Phenotype
        {
            get
            {
                var str = _fSize + " " + _fOrigin.ToString().ToLower();

                if (_fType == CreatureType.MagicalBeast)
                    str += " magical beast";
                else
                    str += " " + _fType.ToString().ToLower();

                if (_fKeywords != null && _fKeywords != "")
                    str += " (" + _fKeywords.ToLower() + ")";

                return str;
            }
        }

        /// <summary>
        ///     Compares this creature to another.
        /// </summary>
        /// <param name="rhs">The other creature.</param>
        /// <returns>
        ///     Returns -1 if this creature should be sorted before the other, +1 if the other should be sorted before this; 0
        ///     otherwise.
        /// </returns>
        public int CompareTo(ICreature rhs)
        {
            return _fName.CompareTo(rhs.Name);
        }
    }
}
