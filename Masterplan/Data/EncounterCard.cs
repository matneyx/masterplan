using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using Masterplan.Properties;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enum containing the ways an encounter card can be shown.
    /// </summary>
    public enum CardMode
    {
        /// <summary>
        ///     The card will be shown as plaintext.
        /// </summary>
        Text,

        /// <summary>
        ///     The card will be non-interactive HTML.
        /// </summary>
        View,

        /// <summary>
        ///     The card will show links for use in combat.
        /// </summary>
        Combat,

        /// <summary>
        ///     The card will show links for use in creature building.
        /// </summary>
        Build
    }

    /// <summary>
    ///     Class representing a creature plus optional templates.
    /// </summary>
    [Serializable]
    public class EncounterCard
    {
        private ICreature _fCreature;

        private Guid _fCreatureId = Guid.Empty;

        private bool _fDrawn;

        private int _fLevelAdjustment;

        private List<Guid> _fTemplateIDs = new List<Guid>();

        private Guid _fThemeAttackPowerId = Guid.Empty;

        private Guid _fThemeId = Guid.Empty;

        private Guid _fThemeUtilityPowerId = Guid.Empty;

        /// <summary>
        ///     Gets or sets the ID of the creature.
        /// </summary>
        public Guid CreatureId
        {
            get => _fCreatureId;
            set
            {
                _fCreatureId = value;

                // This will save us some time later
                if (_fCreatureId != Guid.Empty)
                    _fCreature = Session.FindCreature(_fCreatureId, SearchType.Global);
            }
        }

        /// <summary>
        ///     Gets or sets the list of template IDs.
        /// </summary>
        public List<Guid> TemplateIDs
        {
            get => _fTemplateIDs;
            set => _fTemplateIDs = value;
        }

        /// <summary>
        ///     Gets or sets the card's level adjustment.
        /// </summary>
        public int LevelAdjustment
        {
            get => _fLevelAdjustment;
            set => _fLevelAdjustment = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the monster theme to be used, or Guid.Empty to use no theme.
        /// </summary>
        public Guid ThemeId
        {
            get => _fThemeId;
            set => _fThemeId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the card's theme attack power.
        /// </summary>
        public Guid ThemeAttackPowerId
        {
            get => _fThemeAttackPowerId;
            set => _fThemeAttackPowerId = value;
        }

        /// <summary>
        ///     Gets or sets the ID of the card's theme utility power.
        /// </summary>
        public Guid ThemeUtilityPowerId
        {
            get => _fThemeUtilityPowerId;
            set => _fThemeUtilityPowerId = value;
        }

        /// <summary>
        ///     Gets or sets whether the card has been drawn from its deck.
        /// </summary>
        public bool Drawn
        {
            get => _fDrawn;
            set => _fDrawn = value;
        }

        /// <summary>
        ///     Gets the XP value of this card.
        /// </summary>
        public int Xp
        {
            get
            {
                var xp = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                {
                    if (creature.Role is Minion)
                    {
                        var experience = (float)Experience.GetCreatureXp(creature.Level + _fLevelAdjustment) / 4;
                        xp = (int)Math.Round(experience, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        xp = Experience.GetCreatureXp(creature.Level + _fLevelAdjustment);
                        switch (Flag)
                        {
                            case RoleFlag.Elite:
                                xp *= 2;
                                break;
                            case RoleFlag.Solo:
                                xp *= 5;
                                break;
                        }
                    }
                }

                if (Session.Project != null)
                    // Apply campaign settings
                    xp = (int)(xp * Session.Project.CampaignSettings.Xp);

                return xp;
            }
        }

        /// <summary>
        ///     Gets the title of the card.
        /// </summary>
        public string Title
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                var name = creature != null ? creature.Name : "(unknown creature)";

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        name = template.Name + " " + name;
                }

                if (_fThemeId != Guid.Empty)
                {
                    var mt = Session.FindTheme(_fThemeId, SearchType.Global);
                    if (mt != null)
                        name += " (" + mt.Name + ")";
                }

                return name;
            }
        }

        /// <summary>
        ///     Level N [Elite / Solo] Role
        /// </summary>
        public string Info
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                {
                    var level = creature.Level + _fLevelAdjustment;

                    if (creature.Role is Minion) return "Level " + level + " " + creature.Role;

                    var prefix = "";
                    switch (Flag)
                    {
                        case RoleFlag.Elite:
                            prefix = "Elite ";
                            break;
                        case RoleFlag.Solo:
                            prefix = "Solo ";
                            break;
                    }

                    var str = "";
                    foreach (var r in Roles)
                    {
                        if (str != "")
                            str += " / ";

                        str += r.ToString();
                    }

                    if (Leader)
                        str += " (L)";

                    return "Level " + level + " " + prefix + str;
                }

                return "";
            }
        }

        /// <summary>
        ///     Gets the level of the card.
        /// </summary>
        public int Level
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null)
                    return _fLevelAdjustment;

                return creature.Level + _fLevelAdjustment;
            }
        }

        /// <summary>
        ///     Gets the roles filled by this card.
        /// </summary>
        public List<RoleType> Roles
        {
            get
            {
                var result = new List<RoleType>();

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null || creature.Role is Minion)
                    return result;

                var role = creature.Role as ComplexRole;
                if (role != null)
                    result.Add(role.Type);

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null && !result.Contains(template.Role))
                        result.Add(template.Role);
                }

                return result;
            }
        }

        /// <summary>
        ///     Gets the standard / elite / solo flag for this card.
        /// </summary>
        public RoleFlag Flag
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null || creature.Role is Minion)
                    return RoleFlag.Standard;

                var steps = _fTemplateIDs.Count;

                var role = creature.Role as ComplexRole;
                if (role == null)
                    return RoleFlag.Standard;

                switch (role.Flag)
                {
                    case RoleFlag.Elite:
                        steps += 1;
                        break;
                    case RoleFlag.Solo:
                        steps += 2;
                        break;
                }

                if (steps == 0)
                    return RoleFlag.Standard;
                if (steps == 1)
                    return RoleFlag.Elite;
                return RoleFlag.Solo;
            }
        }

        /// <summary>
        ///     Gets whether this card represents a leader.
        /// </summary>
        public bool Leader
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null || creature.Role is Minion)
                    return false;

                var cr = creature.Role as ComplexRole;
                if (cr != null && cr.Leader)
                    return true;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null && template.Leader)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        ///     Gets the card's regeneration
        /// </summary>
        public Regeneration Regeneration
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null)
                    return null;

                var r = creature.Regeneration;

                foreach (var id in _fTemplateIDs)
                {
                    var ct = Session.FindTemplate(id, SearchType.Global);

                    if (ct?.Regeneration != null)
                    {
                        if (r != null)
                        {
                            if (ct.Regeneration.Value > r.Value)
                                r = ct.Regeneration;
                        }
                        else
                        {
                            r = ct.Regeneration;
                        }
                    }
                }

                return r?.Copy();
            }
        }

        /// <summary>
        ///     Gets the list of auras for this card.
        /// </summary>
        public List<Aura> Auras
        {
            get
            {
                var auras = new List<Aura>();

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    auras.AddRange(creature.Auras);

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        auras.AddRange(template.Auras);
                }

                return auras;
            }
        }

        /// <summary>
        ///     Gets the senses for this card.
        /// </summary>
        public string Senses
        {
            get
            {
                var senses = new List<string>();

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature.Senses != "")
                    senses.Add(creature.Senses);

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null && template.Senses != "")
                        if (!senses.Contains(template.Senses))
                            senses.Add(template.Senses);
                }

                var senseStr = "";
                foreach (var sense in senses)
                {
                    if (senseStr != "")
                        senseStr += "; ";

                    senseStr += sense;
                }

                return senseStr;
            }
        }

        /// <summary>
        ///     Gets the movement for this card.
        /// </summary>
        public string Movement
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                var speed = creature.Movement;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null && template.Movement != "")
                    {
                        if (speed != "")
                            speed += "; ";

                        speed += template.Movement;
                    }
                }

                return speed;
            }
        }

        /// <summary>
        ///     Gets the equipment for this card.
        /// </summary>
        public string Equipment
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                return creature.Equipment ?? "";
            }
        }

        /// <summary>
        ///     Gets the category of this card.
        /// </summary>
        public CardCategory Category
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);

                if (creature.Role is Minion)
                    return CardCategory.Minion;

                if (Flag == RoleFlag.Solo)
                    return CardCategory.Solo;

                var roles = Roles;
                if (roles.Contains(RoleType.Soldier) || roles.Contains(RoleType.Brute))
                    return CardCategory.SoldierBrute;
                if (roles.Contains(RoleType.Skirmisher))
                    return CardCategory.Skirmisher;
                if (roles.Contains(RoleType.Artillery))
                    return CardCategory.Artillery;
                if (roles.Contains(RoleType.Controller))
                    return CardCategory.Controller;
                if (roles.Contains(RoleType.Lurker))
                    return CardCategory.Lurker;

                // Should never reach here
                throw new Exception();
            }
        }

        /// <summary>
        ///     Gets the HP total for this card.
        /// </summary>
        public int Hp
        {
            get
            {
                var hp = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    hp += creature.Hp;

                if (_fTemplateIDs.Count != 0)
                {
                    // Add only the highest HP bonus
                    var maxHpBonus = 0;
                    foreach (var templateId in _fTemplateIDs)
                    {
                        var template = Session.FindTemplate(templateId, SearchType.Global);
                        if (template != null && template.Hp > maxHpBonus)
                            maxHpBonus = template.Hp;
                    }

                    hp += maxHpBonus * Level;
                    hp += creature.Constitution.Score;

                    // If we're using templates to create a solo, multiply HP by 2
                    if (Flag == RoleFlag.Solo)
                        hp *= 2;
                }

                // Handle level adjustment
                if (_fLevelAdjustment != 0 && creature?.Role is ComplexRole)
                {
                    var cr = creature.Role as ComplexRole;

                    var factor = 1;
                    switch (cr.Flag)
                    {
                        case RoleFlag.Elite:
                            factor = 2;
                            break;
                        case RoleFlag.Solo:
                            factor = 5;
                            break;
                    }

                    switch (cr.Type)
                    {
                        case RoleType.Artillery:
                            hp += 6 * _fLevelAdjustment * factor;
                            break;
                        case RoleType.Brute:
                            hp += 10 * _fLevelAdjustment * factor;
                            break;
                        case RoleType.Controller:
                            hp += 8 * _fLevelAdjustment * factor;
                            break;
                        case RoleType.Lurker:
                            hp += 6 * _fLevelAdjustment * factor;
                            break;
                        case RoleType.Skirmisher:
                            hp += 8 * _fLevelAdjustment * factor;
                            break;
                        case RoleType.Soldier:
                            hp += 8 * _fLevelAdjustment * factor;
                            break;
                    }
                }

                if (Session.Project != null)
                    // Campaign settings
                    if (creature?.Role is ComplexRole)
                        hp = (int)(hp * Session.Project.CampaignSettings.Hp);

                return hp;
            }
        }

        /// <summary>
        ///     Gets the initiative bonus for this card.
        /// </summary>
        public int Initiative
        {
            get
            {
                var init = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    init += creature.Initiative;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        init += template.Initiative;
                }

                if (_fLevelAdjustment != 0)
                    init += _fLevelAdjustment / 2;

                return init;
            }
        }

        /// <summary>
        ///     Gets the AC defence for this card.
        /// </summary>
        public int Ac
        {
            get
            {
                var ac = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    ac += creature.Ac;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        ac += template.Ac;
                }

                // Handle level adjustment
                ac += _fLevelAdjustment;

                if (Session.Project != null)
                    // Campaign settings
                    ac += Session.Project.CampaignSettings.AcBonus;

                return ac;
            }
        }

        /// <summary>
        ///     Gets the Fortitude defence for this card.
        /// </summary>
        public int Fortitude
        {
            get
            {
                var fort = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    fort += creature.Fortitude;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        fort += template.Fortitude;
                }

                // Handle level adjustment
                fort += _fLevelAdjustment;

                if (Session.Project != null)
                    // Campaign settings
                    fort += Session.Project.CampaignSettings.NadBonus;

                return fort;
            }
        }

        /// <summary>
        ///     Gets the Reflex defence for this card.
        /// </summary>
        public int Reflex
        {
            get
            {
                var reflex = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    reflex += creature.Reflex;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        reflex += template.Reflex;
                }

                // Handle level adjustment
                reflex += _fLevelAdjustment;

                if (Session.Project != null)
                    // Campaign settings
                    reflex += Session.Project.CampaignSettings.NadBonus;

                return reflex;
            }
        }

        /// <summary>
        ///     Gets the Will defence for this card.
        /// </summary>
        public int Will
        {
            get
            {
                var will = 0;

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    will += creature.Will;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        will += template.Will;
                }

                // Handle level adjustment
                will += _fLevelAdjustment;

                if (Session.Project != null)
                    // Campaign settings
                    will += Session.Project.CampaignSettings.NadBonus;

                return will;
            }
        }

        /// <summary>
        ///     Gets the list of powers for this card.
        /// </summary>
        public List<CreaturePower> CreaturePowers
        {
            get
            {
                var powers = new List<CreaturePower>();

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    foreach (var cp in creature.CreaturePowers)
                        powers.Add(cp.Copy());

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template != null)
                        foreach (var cp in template.CreaturePowers)
                        {
                            var power = cp.Copy();

                            // Need to add level to attack powers from functional templates
                            if (template.Type == CreatureTemplateType.Functional && power.Attack != null)
                                power.Attack.Bonus += Level;

                            powers.Add(power);
                        }
                }

                if (_fThemeId != Guid.Empty)
                {
                    var mt = Session.FindTheme(_fThemeId, SearchType.Global);
                    if (mt != null)
                    {
                        if (_fThemeAttackPowerId != null)
                        {
                            var power = mt.FindPower(_fThemeAttackPowerId);
                            if (power != null)
                                powers.Add(power.Power.Copy());
                        }

                        if (_fThemeUtilityPowerId != null)
                        {
                            var power = mt.FindPower(_fThemeUtilityPowerId);
                            if (power != null)
                                powers.Add(power.Power.Copy());
                        }
                    }
                }

                // Handle level adjustment
                if (_fLevelAdjustment != 0)
                    foreach (var cp in powers)
                    {
                        if (cp.Attack != null)
                        {
                            cp.Attack.Bonus += _fLevelAdjustment;

                            if (Session.Project != null)
                                // Campaign settings
                                cp.Attack.Bonus += Session.Project.CampaignSettings.AttackBonus;
                        }

                        // Adjust power damage
                        var dmgStr = Ai.ExtractDamage(cp.Details);
                        if (dmgStr != "")
                        {
                            var exp = DiceExpression.Parse(dmgStr);
                            var expAdj = exp?.Adjust(_fLevelAdjustment);
                            if (expAdj != null && exp.ToString() != expAdj.ToString())
                                cp.Details = cp.Details.Replace(dmgStr,
                                    expAdj + " damage (adjusted from " + dmgStr + ")");
                        }
                    }

                return powers;
            }
        }

        /// <summary>
        ///     Gets the list of damage modifiers for this card.
        /// </summary>
        public List<DamageModifier> DamageModifiers
        {
            get
            {
                var mods = new List<DamageModifier>();

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    foreach (var dm in creature.DamageModifiers)
                        mods.Add(dm.Copy());

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template == null)
                        continue;

                    foreach (var dmt in template.DamageModifierTemplates)
                    {
                        // Do we already have this?
                        DamageModifier current = null;
                        foreach (var mod in mods)
                            if (mod.Type == dmt.Type)
                            {
                                current = mod;
                                break;
                            }

                        // If it's an immunity, ignore the new one
                        if (current != null && current.Value == 0)
                            continue;

                        if (current == null)
                        {
                            current = new DamageModifier();
                            current.Type = dmt.Type;
                            current.Value = 0;

                            mods.Add(current);
                        }

                        // Set the new value
                        var totalMod = dmt.HeroicValue + dmt.ParagonValue + dmt.EpicValue;
                        if (totalMod == 0)
                        {
                            // Set immunity
                            current.Value = 0;
                        }
                        else
                        {
                            var value = dmt.HeroicValue;
                            if (creature.Level >= 10)
                                value = dmt.ParagonValue;
                            if (creature.Level >= 20)
                                value = dmt.EpicValue;

                            current.Value += value;

                            // If new value is 0, remove mod
                            if (current.Value == 0)
                                mods.Remove(current);
                        }
                    }
                }

                return mods;
            }
        }

        /// <summary>
        ///     Gets the resistances for this card.
        /// </summary>
        public string Resist
        {
            get
            {
                var str = "";

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    str += creature.Resist;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template == null || template.Resist == "")
                        continue;

                    if (str != "")
                        str += ", ";

                    str += template.Resist;
                }

                return str;
            }
        }

        /// <summary>
        ///     Gets the vulnerabilities for this card.
        /// </summary>
        public string Vulnerable
        {
            get
            {
                var str = "";

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    str += creature.Vulnerable;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template == null || template.Vulnerable == "")
                        continue;

                    if (str != "")
                        str += ", ";

                    str += template.Vulnerable;
                }

                return str;
            }
        }

        /// <summary>
        ///     Gets the immunities for this card.
        /// </summary>
        public string Immune
        {
            get
            {
                var str = "";

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    str += creature.Immune;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template == null || template.Immune == "")
                        continue;

                    if (str != "")
                        str += ", ";

                    str += template.Immune;
                }

                return str;
            }
        }

        /// <summary>
        ///     Gets the tactics for this card.
        /// </summary>
        public string Tactics
        {
            get
            {
                var str = "";

                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature != null)
                    str += creature.Tactics;

                foreach (var templateId in _fTemplateIDs)
                {
                    var template = Session.FindTemplate(templateId, SearchType.Global);
                    if (template == null && template.Tactics == "")
                        continue;

                    if (str != "")
                        str += ", ";

                    str += template.Tactics;
                }

                return str;
            }
        }

        /// <summary>
        ///     Gets the skills for this card.
        /// </summary>
        public string Skills
        {
            get
            {
                var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);
                if (creature == null)
                    return "";

                var skillList = CreatureHelper.ParseSkills(creature.Skills);

                var mt = _fThemeId != Guid.Empty ? Session.FindTheme(_fThemeId, SearchType.Global) : null;
                if (mt != null)
                    foreach (var themeBonus in mt.SkillBonuses)
                        if (skillList.ContainsKey(themeBonus.First))
                        {
                            skillList[themeBonus.First] += themeBonus.Second;
                        }
                        else
                        {
                            var mod = Level / 2;

                            var ab = Tools.Skills.GetKeyAbility(themeBonus.First);
                            if (ab == "Strength")
                                mod += creature.Strength.Modifier;
                            if (ab == "Constitution")
                                mod += creature.Constitution.Modifier;
                            if (ab == "Dexterity")
                                mod += creature.Dexterity.Modifier;
                            if (ab == "Intelligence")
                                mod += creature.Intelligence.Modifier;
                            if (ab == "Wisdom")
                                mod += creature.Wisdom.Modifier;
                            if (ab == "Charisma")
                                mod += creature.Charisma.Modifier;

                            skillList[themeBonus.First] = themeBonus.Second + mod;
                        }

                var bst = new BinarySearchTree<string>();
                foreach (var skillName in skillList.Keys)
                    bst.Add(skillName);

                var skillStr = "";
                foreach (var skillName in bst.SortedList)
                {
                    if (skillStr != "")
                        skillStr += ", ";

                    var mod = skillList[skillName];

                    // Apply level adjustment
                    var bonus = mod - creature.Level / 2;
                    mod = bonus + (creature.Level + _fLevelAdjustment) / 2;

                    if (mod >= 0)
                        skillStr += skillName + " +" + mod;
                    else
                        skillStr += skillName + " " + mod;
                }

                return skillStr;
            }
        }

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public EncounterCard()
        {
        }

        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="creature_id">The ID of the creature.</param>
        public EncounterCard(Guid creatureId)
        {
            _fCreatureId = creatureId;

            // This will save us some time later
            if (_fCreatureId != Guid.Empty)
                _fCreature = Session.FindCreature(_fCreatureId, SearchType.Global);
        }

        /// <summary>
        ///     Constructor.
        ///     Use this constructor when you need to use a creature which is not contained in a library.
        /// </summary>
        /// <param name="creature">The creature.</param>
        public EncounterCard(ICreature creature)
        {
            _fCreature = creature;
            _fCreatureId = creature.Id;
        }

        /// <summary>
        ///     Finds the power with the given ID.
        /// </summary>
        /// <param name="power_id">The ID to search for.</param>
        /// <returns>Returns the power if it exists; null otherwise.</returns>
        public CreaturePower FindPower(Guid powerId)
        {
            var powers = CreaturePowers;
            foreach (var cp in powers)
                if (cp.Id == powerId)
                    return cp;

            return null;
        }

        /// <summary>
        ///     Calculates the difficulty of the card.
        /// </summary>
        /// <param name="party_level">The level of the party.</param>
        /// <returns>Returns the difficulty level.</returns>
        public Difficulty GetDifficulty(int partyLevel)
        {
            var delta = Level - partyLevel;

            if (delta < -1)
                return Difficulty.Trivial;

            var diff = Difficulty.Extreme;
            switch (delta)
            {
                case -1:
                case 0:
                case 1:
                    diff = Difficulty.Easy;
                    break;
                case 2:
                case 3:
                    diff = Difficulty.Moderate;
                    break;
                case 4:
                case 5:
                    diff = Difficulty.Hard;
                    break;
            }

            return diff;
        }

        /// <summary>
        ///     Calculates the damage modifier for the given damage type.
        /// </summary>
        /// <param name="type">The damage type.</param>
        /// <param name="data">The current combat data.</param>
        /// <returns>Returns the damage modifier.</returns>
        public int GetDamageModifier(DamageType type, CombatData data)
        {
            var mods = new List<DamageModifier>();
            mods.AddRange(DamageModifiers);
            if (data != null)
                foreach (var oc in data.Conditions)
                {
                    if (oc.Type != OngoingType.DamageModifier)
                        continue;

                    mods.Add(oc.DamageModifier);
                }

            if (mods.Count == 0)
                return 0;

            // Look for all modifiers for this damage type
            var values = new List<int>();
            foreach (var mod in mods)
            {
                if (mod.Type != type)
                    continue;

                if (mod.Value == 0)
                    values.Add(int.MinValue);
                else
                    values.Add(mod.Value);
            }

            var total = 0;
            if (values.Contains(int.MinValue))
            {
                total = int.MinValue;
            }
            else
            {
                var maxPos = 0;
                var minNeg = 0;

                foreach (var value in values)
                {
                    if (value > 0 && value > maxPos)
                        maxPos = value;
                    if (value < 0 && value < minNeg)
                        minNeg = value;
                }

                total = maxPos + minNeg;
            }

            return total;
        }

        /// <summary>
        ///     Calculates the damage modifier for the given damage types.
        /// </summary>
        /// <param name="types">The damage types.</param>
        /// <param name="data">The current combat data.</param>
        /// <returns>Returns the damage modifier.</returns>
        public int GetDamageModifier(List<DamageType> types, CombatData data)
        {
            if (types == null || types.Count == 0)
                return 0;

            var modifiers = new Dictionary<DamageType, int>();
            foreach (var dt in types)
                modifiers[dt] = GetDamageModifier(dt, data);

            // Collate immunities, vulnerabilities, resistances
            var immune = new List<int>();
            var resist = new List<int>();
            var vulnerable = new List<int>();
            foreach (var dt in types)
            {
                var value = modifiers[dt];

                if (value == int.MinValue)
                    immune.Add(value);

                if (value < 0)
                    resist.Add(value);

                if (value > 0)
                    vulnerable.Add(value);
            }

            // If we're immune to all, we're immune
            if (immune.Count == types.Count)
                return int.MinValue;

            // If we resist all, we resist the smallest
            if (resist.Count == types.Count)
            {
                resist.Sort();
                resist.Reverse();
                return resist[0];
            }

            // If we're vulnerable all, we vuln the smallest
            if (vulnerable.Count == types.Count)
            {
                vulnerable.Sort();
                return vulnerable[0];
            }

            return 0;
        }

        /// <summary>
        ///     Creates a copy of this card.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public EncounterCard Copy()
        {
            var card = new EncounterCard();

            card.CreatureId = _fCreatureId;

            foreach (var templateId in _fTemplateIDs)
                card.TemplateIDs.Add(templateId);

            card.LevelAdjustment = _fLevelAdjustment;
            card.ThemeId = _fThemeId;
            card.ThemeAttackPowerId = _fThemeAttackPowerId;
            card.ThemeUtilityPowerId = _fThemeUtilityPowerId;

            card.Drawn = _fDrawn;

            return card;
        }

        /// <summary>
        ///     Returns the text of the card.
        /// </summary>
        /// <param name="combat_data">The CombatData object for this card; null to show the card out of combat.</param>
        /// <param name="mode">How the card should be shown.</param>
        /// <param name="full">If false, only shows combat stats.</param>
        /// <returns>Returns the text of the card.</returns>
        public List<string> AsText(CombatData combatData, CardMode mode, bool full)
        {
            var creature = _fCreature ?? Session.FindCreature(_fCreatureId, SearchType.Global);

            if (creature == null)
            {
                if (mode == CardMode.Text)
                {
                    var lines = new List<string>();
                    lines.Add("(unknown creature)");
                    return lines;
                }
                else
                {
                    var lines = new List<string>();

                    lines.Add("<TABLE>");
                    lines.Add("<TR class=creature>");
                    lines.Add("<TD>");
                    lines.Add("<B>(unknown creature)</B>");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                    lines.Add("<TR>");
                    lines.Add("<TD>");
                    lines.Add("No details");
                    lines.Add("</TD>");
                    lines.Add("</TR>");
                    lines.Add("</TABLE>");

                    return lines;
                }
            }

            var content = new List<string>();

            if (mode != CardMode.Text)
            {
                var title = combatData == null ? Title : combatData.DisplayName;

                content.Add("<TABLE>");

                if (mode == CardMode.Build)
                {
                    var hasBasicAttack = false;
                    foreach (var power in CreaturePowers)
                        if (power.Action != null && power.Action.Use == PowerUseType.Basic && power.Attack != null)
                            hasBasicAttack = true;

                    if (!hasBasicAttack)
                    {
                        content.Add("<TR class=warning>");
                        content.Add("<TD colspan=3 align=center>");
                        content.Add("<B>Warning</B>: This creature has no basic attack");
                        content.Add("</TD>");
                        content.Add("</TR>");
                    }

                    if (CreaturePowers.Count > 10)
                    {
                        content.Add("<TR class=warning>");
                        content.Add("<TD colspan=3 align=center>");
                        content.Add("<B>Warning</B>: This many powers might be slow in play");
                        content.Add("</TD>");
                        content.Add("</TR>");
                    }
                }

                content.Add("<TR class=creature>");

                content.Add("<TD colspan=2>");
                content.Add("<B>" + Html.Process(title, true) + "</B>");
                content.Add("<BR>");
                content.Add(creature.Phenotype);
                content.Add("</TD>");

                content.Add("<TD>");
                content.Add("<B>" + Html.Process(Info, true) + "</B>");
                content.Add("<BR>");
                content.Add(Xp + " XP");
                content.Add("</TD>");

                content.Add("</TR>");

                if (mode == CardMode.Build)
                {
                    content.Add("<TR class=creature>");
                    content.Add("<TD colspan=3 align=center>");
                    content.Add(
                        "<A href=build:profile style=\"color:white\">(click here to edit this top section)</A>");
                    content.Add("</TD>");
                    content.Add("</TR>");
                }
            }

            if (mode != CardMode.Text)
                content.Add("<TR>");

            var hp = Hp.ToString();
            if (combatData != null && combatData.Damage != 0)
            {
                var health = Hp - combatData.Damage;
                if (creature.Role is Minion)
                    hp = health.ToString();
                else
                    hp = health + " of " + Hp;
            }

            var hpStr = mode != CardMode.Text ? "<B>HP</B>" : "HP";
            hpStr += " " + hp;
            if (combatData != null && mode == CardMode.Combat)
            {
                if (creature.Role is Minion)
                {
                    if (combatData.Damage == 0)
                        hpStr = hpStr + " (<A href=kill:" + combatData.Id + ">kill</A>)";
                    else
                        hpStr = hpStr + " (<A href=revive:" + combatData.Id + ">revive</A>)";
                }
                else
                {
                    hpStr = hpStr + " (<A href=dmg:" + combatData.Id + ">dmg</A> | <A href=heal:" + combatData.Id +
                            ">heal</A>)";
                }
            }

            if (!(creature.Role is Minion))
            {
                var bloodiedStr = mode != CardMode.Text ? "<B>Bloodied</B>" : "Bloodied";
                hpStr += "; " + bloodiedStr + " " + Hp / 2;
            }

            if (combatData != null && combatData.TempHp > 0)
                hpStr += "; " + (mode != CardMode.Text ? "<B>Temp HP</B>" : "Temp HP") + " " + combatData.TempHp;

            if (mode == CardMode.Build)
                hpStr = " <A href=build:combat>" + hpStr + "</A>";

            if (mode != CardMode.Text)
            {
                content.Add("<TD colspan=2>");
                content.Add(hpStr);
                content.Add("</TD>");
            }
            else
            {
                content.Add(hpStr);
            }

            var initBonus = Initiative;
            var initStr = initBonus.ToString();

            if (initBonus >= 0)
                initStr = "+" + initStr;

            if (combatData != null && combatData.Initiative != int.MinValue)
                initStr = combatData.Initiative + " (" + initStr + ")";

            switch (mode)
            {
                case CardMode.Text:
                    content.Add("Initiative " + initStr);
                    break;
                case CardMode.View:
                    content.Add("<TD>");
                    content.Add("<B>Initiative</B> " + initStr);
                    content.Add("</TD>");
                    break;
                case CardMode.Combat:
                    content.Add("<TD>");
                    content.Add("<B>Initiative</B> <A href=init:" + combatData.Id + ">" + initStr + "</A>");
                    content.Add("</TD>");
                    break;
                case CardMode.Build:
                    content.Add("<TD>");
                    content.Add("<A href=build:combat><B>Initiative</B> " + initStr + "</A>");
                    content.Add("</TD>");
                    break;
            }

            if (mode != CardMode.Text)
            {
                content.Add("</TR>");
                content.Add("<TR>");
            }

            var acStr = mode != CardMode.Text ? "<B>AC</B>" : "AC";
            var fortStr = mode != CardMode.Text ? "<B>Fort</B>" : "Fort";
            var refStr = mode != CardMode.Text ? "<B>Ref</B>" : "Ref";
            var willStr = mode != CardMode.Text ? "<B>Will</B>" : "Will";

            var ac = Ac;
            var fort = Fortitude;
            var reflex = Reflex;
            var will = Will;

            if (combatData != null)
                foreach (var oc in combatData.Conditions)
                {
                    if (oc.Type != OngoingType.DefenceModifier)
                        continue;

                    if (oc.Defences.Contains(DefenceType.Ac))
                        ac += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Fortitude))
                        fort += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Reflex))
                        reflex += oc.DefenceMod;
                    if (oc.Defences.Contains(DefenceType.Will))
                        will += oc.DefenceMod;
                }

            if (ac == Ac || mode == CardMode.Text)
                acStr += " " + ac;
            else
                acStr += " <B><I>" + ac + "</I></B>";
            if (fort == Fortitude || mode == CardMode.Text)
                fortStr += " " + fort;
            else
                fortStr += " <B><I>" + fort + "</I></B>";
            if (reflex == Reflex || mode == CardMode.Text)
                refStr += " " + reflex;
            else
                refStr += " <B><I>" + reflex + "</I></B>";
            if (will == Will || mode == CardMode.Text)
                willStr += " " + will;
            else
                willStr += " <B><I>" + will + "</I></B>";
            var defences = acStr + "; " + fortStr + "; " + refStr + "; " + willStr;

            if (mode != CardMode.Text)
                content.Add("<TD colspan=2>");

            if (mode == CardMode.Build)
                defences = "<A href=build:combat>" + defences + "</A>";

            content.Add(defences);

            if (mode != CardMode.Text)
                content.Add("</TD>");

            if (mode != CardMode.Text)
            {
                var perc = "";

                if (creature.Skills != null && creature.Skills != "")
                {
                    var skills = creature.Skills.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var skill in skills)
                    {
                        var sk = skill.Trim();
                        if (sk.ToLower().Contains("perc"))
                            perc = sk;
                    }
                }

                if (perc == "")
                {
                    // Get the wisdom mod + 1/2 level
                    var bonus = creature.Wisdom.Modifier + Level / 2;
                    perc = "Perception ";
                    if (bonus >= 0)
                        perc += "+";
                    perc += bonus.ToString();
                }

                if (perc != "")
                {
                    content.Add("<TD>");

                    if (mode == CardMode.Build)
                        perc = "<A href=build:skills>" + perc + "</A>";

                    content.Add(perc);

                    content.Add("</TD>");
                }
            }

            if (mode != CardMode.Text)
            {
                content.Add("</TR>");
                content.Add("<TR>");
            }

            if (mode != CardMode.Text)
            {
                var movement = Html.Process(Movement, true);
                if (movement != "")
                    movement = "<B>Speed</B> " + movement;

                if (mode == CardMode.Build)
                    if (movement == "")
                        movement = "(specify movement)";

                if (movement != "")
                {
                    content.Add("<TD colspan=2>");

                    if (mode == CardMode.Build)
                        movement = "<A href=build:movement>" + movement + "</A>";

                    content.Add(movement);

                    content.Add("</TD>");
                }
            }

            if (mode != CardMode.Text)
            {
                var senses = Senses;
                if (senses == null)
                    senses = "";
                senses = Html.Process(senses, true);

                if (senses.ToLower().Contains("perception"))
                {
                    // Remove the Perception clause

                    var clauses = senses.Split(new[] { ";", "," }, StringSplitOptions.RemoveEmptyEntries);

                    senses = "";
                    foreach (var clause in clauses)
                    {
                        if (clause.ToLower().Contains("perception"))
                            continue;

                        if (senses != "")
                            senses += "; ";

                        senses += clause;
                    }
                }

                var rows = Flag == RoleFlag.Standard ? 1 : 2;

                // Add 1 if we have damage mods
                var damageMods = DamageModifiers.Count;
                if (combatData != null)
                    foreach (var oc in combatData.Conditions)
                        if (oc.Type == OngoingType.DamageModifier)
                            damageMods += 1;
                if (Resist != "" || Vulnerable != "" || Immune != "" || damageMods != 0 || mode == CardMode.Build)
                    rows += 1;

                if (mode == CardMode.Build)
                {
                    if (senses == "")
                        senses = "(specify senses)";

                    senses = "<A href=build:senses>" + senses + "</A>";
                }

                content.Add("<TD rowspan=" + rows + ">" + senses + "</TD>");
            }

            if (mode != CardMode.Text) content.Add("</TR>");

            if (mode != CardMode.Text)
            {
                var resist = Html.Process(Resist, true);
                var vuln = Html.Process(Vulnerable, true);
                var immune = Html.Process(Immune, true);
                if (resist == null)
                    resist = "";
                if (vuln == null)
                    vuln = "";
                if (immune == null)
                    immune = "";

                var dmgModList = new List<DamageModifier>();
                dmgModList.AddRange(DamageModifiers);
                if (combatData != null)
                    foreach (var oc in combatData.Conditions)
                    {
                        if (oc.Type != OngoingType.DamageModifier)
                            continue;

                        dmgModList.Add(oc.DamageModifier);
                    }

                foreach (var dm in dmgModList)
                {
                    if (dm.Value == 0)
                    {
                        if (immune != "")
                            immune += ", ";

                        immune += dm.Type.ToString().ToLower();
                    }

                    if (dm.Value > 0)
                    {
                        if (vuln != "")
                            vuln += ", ";

                        vuln += dm.Value + " " + dm.Type.ToString().ToLower();
                    }

                    if (dm.Value < 0)
                    {
                        if (resist != "")
                            resist += ", ";

                        var val = Math.Abs(dm.Value);
                        resist += val + " " + dm.Type.ToString().ToLower();
                    }
                }

                var damageMods = "";
                if (immune != "") damageMods += "<B>Immune</B> " + immune;
                if (resist != "")
                {
                    if (damageMods != "")
                        damageMods += "; ";

                    damageMods += "<B>Resist</B> " + resist;
                }

                if (vuln != "")
                {
                    if (damageMods != "")
                        damageMods += "; ";

                    damageMods += "<B>Vulnerable</B> " + vuln;
                }

                if (damageMods != "")
                {
                    if (mode == CardMode.Build)
                        damageMods = "<A href=build:damage>" + damageMods + "</A>";

                    content.Add("<TR>");
                    content.Add("<TD colspan=2>");
                    content.Add(damageMods);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }
                else
                {
                    if (mode == CardMode.Build)
                    {
                        content.Add("<TR>");
                        content.Add("<TD colspan=2>");
                        content.Add("<A href=build:damage>No resistances / vulnerabilities / immunities</A>");
                        content.Add("</TD>");
                        content.Add("</TR>");
                    }
                }
            }

            var addedAp = false;

            if (mode != CardMode.Text)
            {
                var saveMod = 0;
                var ap = 0;

                switch (Flag)
                {
                    case RoleFlag.Elite:
                        saveMod = 2;
                        ap = 1;
                        break;
                    case RoleFlag.Solo:
                        saveMod = 5;
                        ap = 2;
                        break;
                }

                if (ap != 0)
                {
                    content.Add("<TD colspan=2>");
                    content.Add("<B>Saving Throws</B>" + " +" + saveMod + " <B>Action Points</B> " + ap);
                    content.Add("</TD>");

                    addedAp = true;
                }
            }

            if (addedAp && mode != CardMode.Text) content.Add("</TR>");

            if (mode == CardMode.Build)
            {
                content.Add("<TR>");

                content.Add("<TD colspan=3 align=center>");
                content.Add("(click on any value in this section to edit it)");
                content.Add("</TD>");

                content.Add("</TR>");
            }

            if (mode != CardMode.Text && full)
            {
                if (mode == CardMode.Build)
                {
                    content.Add("<TR class=creature>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Powers and Traits</B>");
                    content.Add("</TD>");
                    content.Add("</TR>");

                    content.Add("<TR>");
                    content.Add("<TD colspan=3 align=center>");
                    content.Add("<A href=power:addtrait>add a trait</A>");
                    content.Add("|");
                    content.Add("<A href=power:addpower>add a power</A>");
                    content.Add("|");
                    content.Add("<A href=power:addaura>add an aura</A>");
                    if (Regeneration == null)
                    {
                        content.Add("|");
                        content.Add("<A href=power:regenedit>add regeneration</A>");
                    }

                    content.Add("<BR>");
                    content.Add("<A href=power:browse>browse for an existing power or trait</A>");
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                var powers = new Dictionary<CreaturePowerCategory, List<CreaturePower>>();
                var powerCategories = Enum.GetValues(typeof(CreaturePowerCategory));
                foreach (CreaturePowerCategory cat in powerCategories)
                    powers[cat] = new List<CreaturePower>();
                foreach (var cp in CreaturePowers)
                    powers[cp.Category].Add(cp);
                foreach (CreaturePowerCategory cat in powerCategories)
                    powers[cat].Sort();

                foreach (CreaturePowerCategory cat in powerCategories)
                {
                    var count = powers[cat].Count;
                    if (cat == CreaturePowerCategory.Trait)
                    {
                        // Add auras
                        count += Auras.Count;
                        if (combatData != null)
                            foreach (var oc in combatData.Conditions)
                                if (oc.Type == OngoingType.Aura)
                                    count += 1;

                        // Add regeneration
                        var hasRegen = Regeneration != null;
                        if (combatData != null)
                            foreach (var oc in combatData.Conditions)
                                if (oc.Type == OngoingType.Regeneration)
                                    hasRegen = true;
                        if (hasRegen)
                            count += 1;
                    }

                    if (count == 0)
                        continue;

                    var name = "";
                    switch (cat)
                    {
                        case CreaturePowerCategory.Trait:
                            name = "Traits";
                            break;
                        case CreaturePowerCategory.Standard:
                        case CreaturePowerCategory.Move:
                        case CreaturePowerCategory.Minor:
                        case CreaturePowerCategory.Free:
                            name = cat + " Actions";
                            break;
                        case CreaturePowerCategory.Triggered:
                            name = "Triggered Actions";
                            break;
                        case CreaturePowerCategory.Other:
                            name = "Other Actions";
                            break;
                    }

                    content.Add("<TR class=creature>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>" + name + "</B>");
                    content.Add("</TD>");
                    content.Add("</TR>");

                    if (cat == CreaturePowerCategory.Trait)
                    {
                        // Auras
                        var auraList = new List<Aura>();
                        auraList.AddRange(Auras);
                        if (combatData != null)
                            foreach (var oc in combatData.Conditions)
                                if (oc.Type == OngoingType.Aura)
                                    auraList.Add(oc.Aura);

                        foreach (var aura in auraList)
                        {
                            var auraDetails = Html.Process(aura.Description.Trim(), true);
                            if (auraDetails.StartsWith("aura", StringComparison.OrdinalIgnoreCase))
                                auraDetails = "A" + auraDetails.Substring(1);

                            var ms = new MemoryStream();
                            Resources.Aura.Save(ms, ImageFormat.Png);
                            var byteImage = ms.ToArray();
                            var data = Convert.ToBase64String(byteImage);

                            content.Add("<TR class=shaded>");
                            content.Add("<TD colspan=3>");
                            content.Add("<img src=data:image/png;base64," + data + ">");
                            content.Add("<B>" + Html.Process(aura.Name, true) + "</B>");
                            if (aura.Keywords != "")
                                content.Add("(" + aura.Keywords + ")");
                            if (aura.Radius > 0)
                                content.Add(" &diams; Aura " + aura.Radius);
                            content.Add("</TD>");
                            content.Add("</TR>");

                            content.Add("<TR>");
                            content.Add("<TD colspan=3>");
                            content.Add(auraDetails);
                            content.Add("</TD>");
                            content.Add("</TR>");

                            if (mode == CardMode.Build)
                            {
                                content.Add("<TR>");
                                content.Add("<TD colspan=3 align=center>");
                                content.Add("<A href=auraedit:" + aura.Id + ">edit</A>");
                                content.Add("|");
                                content.Add("<A href=auraremove:" + aura.Id + ">remove</A>");
                                content.Add("this aura");
                                content.Add("</TD>");
                                content.Add("</TR>");
                            }
                        }

                        // Regeneration
                        var regenList = new List<Regeneration>();
                        if (Regeneration != null)
                            regenList.Add(Regeneration);
                        if (combatData != null)
                            foreach (var oc in combatData.Conditions)
                                if (oc.Type == OngoingType.Regeneration)
                                    regenList.Add(oc.Regeneration);

                        foreach (var regen in regenList)
                        {
                            content.Add("<TR class=shaded>");
                            content.Add("<TD colspan=3>");
                            content.Add("<B>Regeneration</B>");
                            content.Add("</TD>");
                            content.Add("</TR>");

                            content.Add("<TR>");
                            content.Add("<TD colspan=3>");
                            content.Add("Regeneration " + Html.Process(regen.ToString(), true));
                            content.Add("</TD>");
                            content.Add("</TR>");

                            if (mode == CardMode.Build)
                            {
                                content.Add("<TR>");
                                content.Add("<TD colspan=3 align=center>");
                                content.Add("<A href=power:regenedit>edit</A>");
                                content.Add("|");
                                content.Add("<A href=power:regenremove>remove</A>");
                                content.Add("this trait");
                                content.Add("</TD>");
                                content.Add("</TR>");
                            }
                        }
                    }

                    foreach (var cp in powers[cat])
                    {
                        var powerMode = mode;
                        if (mode == CardMode.Build)
                            powerMode = CardMode.View;

                        content.AddRange(cp.AsHtml(combatData, powerMode, false));

                        if (mode == CardMode.Build)
                        {
                            content.Add("<TR>");
                            content.Add("<TD colspan=3 align=center>");
                            content.Add("<A href=\"poweredit:" + cp.Id + "\">edit</A>");
                            content.Add("|");
                            content.Add("<A href=\"powerremove:" + cp.Id + "\">remove</A>");
                            content.Add("|");
                            content.Add("<A href=\"powerduplicate:" + cp.Id + "\">duplicate</A>");
                            if (cat == CreaturePowerCategory.Trait)
                                content.Add("this trait");
                            else
                                content.Add("this power");
                            content.Add("</TD>");
                            content.Add("</TR>");
                        }
                    }
                }

                var skills = Skills;
                if (skills != null && skills.ToLower().Contains("perception"))
                {
                    // Remove the Perception skill
                    var str = "";
                    var tokens = skills.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var token in tokens)
                    {
                        if (token.ToLower().Contains("perception"))
                            continue;

                        if (str != "")
                            str += "; ";

                        str += token;
                    }

                    skills = str;
                }

                if (skills == null)
                    skills = "";
                if (skills == "" && mode == CardMode.Build)
                    skills = "(none)";
                if (skills != "")
                {
                    skills = Html.Process(skills, true);
                    if (mode == CardMode.Build)
                        skills = "<A href=build:skills>" + skills + "</A>";

                    content.Add("<TR class=shaded>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Skills</B> " + skills);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                content.Add("<TR class=shaded>");

                content.Add("<TD>");
                content.Add("<B>Str</B>: " + Ability(creature.Strength, mode));
                content.Add("<BR>");
                content.Add("<B>Con</B>: " + Ability(creature.Constitution, mode));
                content.Add("</TD>");

                content.Add("<TD>");
                content.Add("<B>Dex</B>: " + Ability(creature.Dexterity, mode));
                content.Add("<BR>");
                content.Add("<B>Int</B>: " + Ability(creature.Intelligence, mode));
                content.Add("</TD>");

                content.Add("<TD>");
                content.Add("<B>Wis</B>: " + Ability(creature.Wisdom, mode));
                content.Add("<BR>");
                content.Add("<B>Cha</B>: " + Ability(creature.Charisma, mode));
                content.Add("</TD>");

                content.Add("</TR>");

                var alignment = creature.Alignment;
                if (alignment == null)
                    alignment = "";
                if (alignment == "")
                {
                    if (mode == CardMode.Build)
                        alignment = "(not set)";
                    else
                        alignment = "Unaligned";
                }

                if (alignment != "")
                {
                    alignment = Html.Process(alignment, true);
                    if (mode == CardMode.Build)
                        alignment = "<A href=build:alignment>" + alignment + "</A>";

                    content.Add("<TR>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Alignment</B> " + alignment);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                var langs = creature.Languages;
                if (langs == null)
                    langs = "";
                if (langs == "" && mode == CardMode.Build)
                    langs = "(none)";
                if (langs != "")
                {
                    langs = Html.Process(langs, true);
                    if (mode == CardMode.Build)
                        langs = "<A href=build:languages>" + langs + "</A>";

                    content.Add("<TR>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Languages</B> " + langs);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                var equip = Equipment;
                if (equip == null)
                    equip = "";
                if (equip == "" && mode == CardMode.Build)
                    equip = "(none)";
                if (equip != "")
                {
                    equip = Html.Process(equip, true);
                    if (mode == CardMode.Build)
                        equip = "<A href=build:equipment>" + equip + "</A>";

                    content.Add("<TR>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Equipment</B> " + equip);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                var tactics = Tactics;
                if (tactics == null)
                    tactics = "";
                if (tactics == "" && mode == CardMode.Build)
                    tactics = "(none specified)";
                if (tactics != "")
                {
                    tactics = Html.Process(tactics, true);
                    if (mode == CardMode.Build)
                        tactics = "<A href=build:tactics>" + tactics + "</A>";

                    content.Add("<TR>");
                    content.Add("<TD colspan=3>");
                    content.Add("<B>Tactics</B> " + tactics);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }

                var c = creature as Creature;

                var references = new List<string>();

                if (c != null)
                {
                    var lib = Session.FindLibrary(c);
                    if (lib != null && lib.Name != "")
                        if (Session.Project == null || lib != Session.Project.Library)
                        {
                            var reference = Html.Process(lib.Name, true);
                            references.Add(reference);
                        }
                }

                foreach (var templateId in _fTemplateIDs)
                {
                    var ct = Session.FindTemplate(templateId, SearchType.Global);
                    var ctLib = Session.FindLibrary(ct);

                    if (ctLib != null && ctLib != Session.Project.Library)
                    {
                        if (references.Count != 0)
                            references.Add("<BR>");

                        var reference = Html.Process(ctLib.Name, true);
                        references.Add(ct.Name + " template: " + reference);
                    }
                }

                if (references.Count != 0)
                {
                    content.Add("<TR class=shaded>");
                    content.Add("<TD colspan=3>");
                    foreach (var reference in references) content.Add(reference);
                    content.Add("</TD>");
                    content.Add("</TR>");
                }
            }

            if (mode != CardMode.Text) content.Add("</TABLE>");

            return content;
        }

        private string Ability(Ability ab, CardMode mode)
        {
            if (ab == null)
                return "-";

            var mod = ab.Modifier + Level / 2;

            var str = "";

            switch (mode)
            {
                case CardMode.Combat:
                    str += "<A href=\"ability:" + mod + "\">";
                    break;
                case CardMode.Build:
                    str += "<A href=build:ability>";
                    break;
            }

            str += ab.Score.ToString();
            str += " ";

            var modStr = mod.ToString();
            if (mod >= 0)
                modStr = "+" + modStr;
            str += "(" + modStr + ")";

            switch (mode)
            {
                case CardMode.Combat:
                    str += "</A>";
                    break;
                case CardMode.Build:
                    str += "</A>";
                    break;
            }

            return str;
        }
    }
}
