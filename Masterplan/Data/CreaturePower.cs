using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Masterplan.Properties;
using Masterplan.Tools;

namespace Masterplan.Data
{
    /// <summary>
    ///     Enumeration containing the possible power categories for stat block grouping.
    /// </summary>
    public enum CreaturePowerCategory
    {
        /// <summary>
        ///     Trait.
        /// </summary>
        Trait,

        /// <summary>
        ///     Standard action.
        /// </summary>
        Standard,

        /// <summary>
        ///     Move action.
        /// </summary>
        Move,

        /// <summary>
        ///     Minor action.
        /// </summary>
        Minor,

        /// <summary>
        ///     Free action.
        /// </summary>
        Free,

        /// <summary>
        ///     Triggered action.
        /// </summary>
        Triggered,

        /// <summary>
        ///     Other powers.
        /// </summary>
        Other
    }

    /// <summary>
    ///     Enumeration containing the possible defences.
    /// </summary>
    public enum DefenceType
    {
        /// <summary>
        ///     Armour Class.
        /// </summary>
        Ac,

        /// <summary>
        ///     Fortitude.
        /// </summary>
        Fortitude,

        /// <summary>
        ///     Reflex.
        /// </summary>
        Reflex,

        /// <summary>
        ///     Will.
        /// </summary>
        Will
    }

    /// <summary>
    ///     Enumeration containing the possible action types.
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        ///     No action.
        /// </summary>
        None,

        /// <summary>
        ///     Standard action.
        /// </summary>
        Standard,

        /// <summary>
        ///     Move action.
        /// </summary>
        Move,

        /// <summary>
        ///     Minor action.
        /// </summary>
        Minor,

        /// <summary>
        ///     Immediate reaction.
        /// </summary>
        Reaction,

        /// <summary>
        ///     Immediate interrupt.
        /// </summary>
        Interrupt,

        /// <summary>
        ///     Opportunity action.
        /// </summary>
        Opportunity,

        /// <summary>
        ///     Free action.
        /// </summary>
        Free
    }

    /// <summary>
    ///     Normal / limited damage.
    /// </summary>
    public enum DamageCategory
    {
        /// <summary>
        ///     Normal damage.
        /// </summary>
        Normal,

        /// <summary>
        ///     Limited damage.
        /// </summary>
        Limited
    }

    /// <summary>
    ///     Low / medium / high damage.
    /// </summary>
    public enum DamageDegree
    {
        /// <summary>
        ///     Low damage.
        /// </summary>
        Low,

        /// <summary>
        ///     Medium damage.
        /// </summary>
        Medium,

        /// <summary>
        ///     High damage.
        /// </summary>
        High
    }

    /// <summary>
    ///     The usage type for a power.
    /// </summary>
    public enum PowerUseType
    {
        /// <summary>
        ///     Per encounter usage.
        /// </summary>
        Encounter,

        /// <summary>
        ///     At will usage.
        /// </summary>
        AtWill,

        /// <summary>
        ///     Basic attack.
        /// </summary>
        Basic,

        /// <summary>
        ///     Daily usage.
        /// </summary>
        Daily
    }

    /// <summary>
    ///     Class representing a power.
    ///     This class should be used in preference to the Power class.
    /// </summary>
    [Serializable]
    public class CreaturePower : IComparable<CreaturePower>
    {
        private PowerAction _fAction;

        private PowerAttack _fAttack;

        private string _fCondition = "";

        private string _fDescription = "";

        private string _fDetails = "";

        private Guid _fId = Guid.NewGuid();

        private string _fKeywords = "";

        private string _fName = "";

        private string _fRange = "";

        /// <summary>
        ///     Gets or sets the power's unique ID.
        /// </summary>
        public Guid Id
        {
            get => _fId;
            set => _fId = value;
        }

        /// <summary>
        ///     Gets or sets the power's name.
        /// </summary>
        public string Name
        {
            get => _fName;
            set => _fName = value;
        }

        /// <summary>
        ///     Gets or sets the action the power requires.
        /// </summary>
        public PowerAction Action
        {
            get => _fAction;
            set => _fAction = value;
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
        ///     Gets or sets the condition required for the power.
        /// </summary>
        public string Condition
        {
            get => _fCondition;
            set => _fCondition = value;
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
        ///     Gets or sets the attack bonus and defence targeted by the power.
        /// </summary>
        public PowerAttack Attack
        {
            get => _fAttack;
            set => _fAttack = value;
        }

        /// <summary>
        ///     Gets or sets the power's read-aloud description.
        /// </summary>
        public string Description
        {
            get => _fDescription;
            set => _fDescription = value;
        }

        /// <summary>
        ///     Gets or sets the power details.
        /// </summary>
        public string Details
        {
            get => _fDetails;
            set => _fDetails = value;
        }

        /// <summary>
        ///     Gets the power's damage expression.
        /// </summary>
        public string Damage => Ai.ExtractDamage(_fDetails);

        /// <summary>
        ///     Gets the category of power for use in grouping actions in the stat block.
        /// </summary>
        public CreaturePowerCategory Category
        {
            get
            {
                if (_fAction == null)
                    return CreaturePowerCategory.Trait;

                if (_fAction.Trigger != null && _fAction.Trigger != "")
                    return CreaturePowerCategory.Triggered;

                switch (_fAction.Action)
                {
                    case ActionType.Interrupt:
                    case ActionType.Opportunity:
                    case ActionType.Reaction:
                        return CreaturePowerCategory.Triggered;
                    case ActionType.Free:
                        return CreaturePowerCategory.Free;
                    case ActionType.Minor:
                        return CreaturePowerCategory.Minor;
                    case ActionType.Move:
                        return CreaturePowerCategory.Move;
                    case ActionType.Standard:
                        return CreaturePowerCategory.Standard;
                }

                return CreaturePowerCategory.Other;
            }
        }

        /// <summary>
        ///     Creates a copy of the power.
        /// </summary>
        /// <returns></returns>
        public CreaturePower Copy()
        {
            var cp = new CreaturePower();

            cp.Id = _fId;
            cp.Name = _fName;
            cp.Action = _fAction?.Copy();
            cp.Keywords = _fKeywords;
            cp.Condition = _fCondition;
            cp.Range = _fRange;
            cp.Attack = _fAttack?.Copy();
            cp.Description = _fDescription;
            cp.Details = _fDetails;

            return cp;
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
        ///     Returns the HTML representation of the power.
        /// </summary>
        /// <param name="cd">The CombatData to use.</param>
        /// <param name="mode">The type of HTML to generate</param>
        /// <param name="functionalTemplate">True if this power is from a functional template; false otherwise</param>
        /// <returns>Returns the HTML source code.</returns>
        public List<string> AsHtml(CombatData cd, CardMode mode, bool functionalTemplate)
        {
            var used = mode == CardMode.Combat && cd != null && cd.UsedPowers.Contains(_fId);

            var cat = "Actions";
            switch (Category)
            {
                case CreaturePowerCategory.Trait:
                    cat = "Traits";
                    break;
                case CreaturePowerCategory.Standard:
                    cat = "Standard Actions";
                    break;
                case CreaturePowerCategory.Move:
                    cat = "Move Actions";
                    break;
                case CreaturePowerCategory.Minor:
                    cat = "Minor Actions";
                    break;
                case CreaturePowerCategory.Free:
                    cat = "Free Actions";
                    break;
                case CreaturePowerCategory.Triggered:
                    cat = "Triggered Actions";
                    break;
                case CreaturePowerCategory.Other:
                    cat = "Other Actions";
                    break;
            }

            var content = new List<string>();

            if (mode == CardMode.Build)
            {
                content.Add("<TR class=creature>");
                content.Add("<TD colspan=3>");
                content.Add("<A href=power:action style=\"color:white\"><B>" + cat +
                            "</B> (click here to change the action)</A>");
                content.Add("</TD>");
                content.Add("</TR>");
            }

            if (!used)
                content.Add("<TR class=shaded>");
            else
                content.Add("<TR class=shaded_dimmed>");
            content.Add("<TD colspan=3>");
            content.Add(power_topline(cd, mode));
            content.Add("</TD>");
            content.Add("</TR>");

            if (!used)
                content.Add("<TR>");
            else
                content.Add("<TR class=dimmed>");
            content.Add("<TD colspan=3>");
            content.Add(power_content(mode));
            content.Add("</TD>");
            content.Add("</TR>");

            if (mode == CardMode.Combat)
            {
                if (used)
                {
                    content.Add("<TR>");
                    content.Add("<TD class=indent colspan=3>");
                    content.Add("<A href=\"refresh:" + cd.Id + ";" + _fId + "\">(recharge this power)</A>");
                    content.Add("</TD>");
                    content.Add("</TR>");
                }
                else
                {
                    if (_fAction != null)
                        if (_fAction.Use == PowerUseType.Encounter || _fAction.Use == PowerUseType.Daily)
                        {
                            content.Add("<TR>");
                            content.Add("<TD class=indent colspan=3>");
                            content.Add("<A href=\"refresh:" + cd.Id + ";" + _fId + "\">(use this power)</A>");
                            content.Add("</TD>");
                            content.Add("</TR>");
                        }
                }
            }

            if (functionalTemplate)
            {
                content.Add("<TR class=shaded>");
                content.Add("<TD colspan=3>");
                content.Add(
                    "<B>Note</B>: This power is part of a functional template, and so its attack bonus will be increased by the level of the creature it is applied to.");
                content.Add("</TD>");
                content.Add("</TR>");
            }

            return content;
        }

        private string power_topline(CombatData cd, CardMode mode)
        {
            var str = "";

            Image icon = null;
            var rng = _fRange.ToLower();
            if (rng.Contains("melee"))
            {
                if (_fAction != null && _fAction.Use == PowerUseType.Basic)
                    icon = Resources.MeleeBasic;
                else
                    icon = Resources.Melee;
            }

            if (rng.Contains("ranged"))
            {
                if (_fAction != null && _fAction.Use == PowerUseType.Basic)
                    icon = Resources.RangedBasic;
                else
                    icon = Resources.Ranged;
            }

            if (rng.Contains("area")) icon = Resources.Area;
            if (rng.Contains("close")) icon = Resources.Close;
            if (icon == null && _fAttack != null && _fAction != null)
            {
                if (_fAction.Use == PowerUseType.Basic)
                    icon = Resources.MeleeBasic;
                else
                    icon = Resources.Melee;
            }

            str += "<B>" + Html.Process(_fName, true) + "</B>";
            if (mode == CardMode.Combat && cd != null)
            {
                var createLink = false;

                if (!cd.UsedPowers.Contains(_fId))
                {
                    if (_fAttack != null)
                        createLink = true;

                    if (_fAction != null && _fAction.Use == PowerUseType.Encounter)
                        createLink = true;
                }

                if (createLink)
                    str = "<A href=\"power:" + cd.Id + ";" + _fId + "\">" + str + "</A>";
            }

            if (mode == CardMode.Build) str = "<A href=power:info>" + str + "</A>";

            if (icon != null)
            {
                var ms = new MemoryStream();
                icon.Save(ms, ImageFormat.Png);
                var byteImage = ms.ToArray();
                var data = Convert.ToBase64String(byteImage);
                if (data != null && data != "")
                    str = "<img src=data:image/png;base64," + data + ">" + str;
            }

            if (_fKeywords != "")
            {
                var keywords = Html.Process(_fKeywords, true);
                if (mode == CardMode.Build)
                    keywords = "<A href=power:info>" + keywords + "</A>";

                str += " (" + keywords + ")";
            }

            var info = power_parenthesis(mode);
            if (info != "")
                str += " &diams; " + info;

            return str;
        }

        private string power_parenthesis(CardMode mode)
        {
            if (_fCondition == "" && _fAction == null)
                return "";

            var info = "";
            if (_fAction != null)
            {
                var action = _fAction.ToString();
                if (mode == CardMode.Build)
                    action = "<A href=power:action>" + action + "</A>";

                info += action;
            }

            return info;
        }

        private string power_content(CardMode mode)
        {
            var lines = new List<string>();

            var desc = "";
            if (_fDescription != null)
                desc = Html.Process(_fDescription, true);
            if (desc == null)
                desc = "";
            if (mode == CardMode.Build)
            {
                if (desc == "")
                    desc = "Set read-aloud description (optional)";

                desc = "<A href=power:desc>" + desc + "</A>";
            }

            if (desc != "")
                lines.Add("<I>" + desc + "</I>");

            if (mode == CardMode.Build)
                lines.Add("");

            if (_fAction != null && _fAction.Trigger != "")
            {
                string action;
                switch (_fAction.Action)
                {
                    case ActionType.Interrupt:
                        action = "immediate interrupt";
                        break;
                    case ActionType.None:
                        action = "no action";
                        break;
                    case ActionType.Reaction:
                        action = "immediate reaction";
                        break;
                    default:
                        action = _fAction.Action.ToString().ToLower() + " action";
                        break;
                }

                if (mode != CardMode.Build)
                    lines.Add("Trigger (" + action + "): " + _fAction.Trigger);
                else
                    lines.Add("Trigger (<A href=power:action>" + action + "</A>): <A href=power:action>" +
                              _fAction.Trigger + "</A>");
            }

            var condition = Html.Process(_fCondition, true);
            if (condition == "" && mode == CardMode.Build)
                condition = "No prerequisite";
            if (condition != "")
            {
                if (mode == CardMode.Build)
                    condition = "<A href=power:prerequisite>" + condition + "</A>";

                condition = "Prerequisite: " + condition;

                lines.Add(condition);
            }

            var range = _fRange ?? "";
            var attack = _fAttack != null ? _fAttack.ToString() : "";
            if (mode == CardMode.Build)
            {
                if (range == "")
                    range = "<A href=power:range>" + "The power's range and its target(s) are not set" + "</A>";
                else
                    range = "<A href=power:range>" + range + "</A>";

                if (attack == "")
                    attack = "<A href=power:attack>Click here to make this an attack power</A>";
                else
                    attack = "<A href=power:attack>" + attack + "</A> <A href=power:clearattack>(clear attack)</A>";
            }

            if (range != "")
                lines.Add("Range: " + range);
            if (attack != "")
                lines.Add("Attack: " + attack);

            if (mode == CardMode.Build)
                lines.Add("");

            var details = Html.Process(_fDetails, true);
            if (details == null)
                details = "";
            if (mode == CardMode.Build)
            {
                if (details == "")
                    details = "Specify the power's effects";

                details = "<A href=power:details>" + details + "</A>";
            }

            if (details != "")
                lines.Add(details);

            if (mode == CardMode.Build)
                lines.Add("");

            if (_fAction != null && _fAction.SustainAction != ActionType.None)
            {
                var sustain = _fAction.SustainAction.ToString();

                if (mode == CardMode.Build)
                    sustain = "<A href=power:action>" + sustain + "</A>";

                lines.Add("Sustain: " + sustain);
            }

            var str = "";
            foreach (var line in lines)
            {
                if (str != "")
                    str += "<BR>";

                str += line;
            }

            if (str == "")
                str = "(no details)";

            return str;
        }

        /// <summary>
        ///     Parses the power details field to find the attack data (+N vs Defence).
        /// </summary>
        public void ExtractAttackDetails()
        {
            if (_fAttack != null)
                return;

            if (!_fDetails.Contains("vs"))
                return;

            var sections = _fDetails.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            _fDetails = "";

            foreach (var section in sections)
            {
                var str = section.Trim();

                var addedAttack = false;

                var index = str.IndexOf("vs");
                if (index != -1 && _fAttack == null)
                {
                    var prefix = str.Substring(0, index);
                    var suffix = str.Substring(index);

                    var digits = "1234567890";
                    var start = prefix.LastIndexOfAny(digits.ToCharArray());
                    if (start != -1)
                    {
                        var bonus = 0;
                        var defence = DefenceType.Ac;
                        var foundBonus = false;
                        var foundDefence = false;

                        if (suffix.Contains("AC"))
                        {
                            defence = DefenceType.Ac;
                            foundDefence = true;
                        }

                        if (suffix.Contains("Fort"))
                        {
                            defence = DefenceType.Fortitude;
                            foundDefence = true;
                        }

                        if (suffix.Contains("Ref"))
                        {
                            defence = DefenceType.Reflex;
                            foundDefence = true;
                        }

                        if (suffix.Contains("Will"))
                        {
                            defence = DefenceType.Will;
                            foundDefence = true;
                        }

                        if (foundDefence)
                            try
                            {
                                start = Math.Max(0, start - 2);
                                var bonusStr = prefix.Substring(start);
                                bonus = int.Parse(bonusStr);
                                foundBonus = true;
                            }
                            catch
                            {
                                foundBonus = false;
                            }

                        if (foundBonus && foundDefence)
                        {
                            _fAttack = new PowerAttack();
                            _fAttack.Bonus = bonus;
                            _fAttack.Defence = defence;

                            addedAttack = true;
                        }
                    }
                }

                if (!addedAttack)
                {
                    if (_fDetails != "")
                        _fDetails += "; ";

                    _fDetails += str;
                }
            }
        }

        /// <summary>
        ///     Used for sorting.
        /// </summary>
        /// <param name="rhs">The CreaturePower to compare to.</param>
        /// <returns>Returns -1 if this object should be sorted before rhs, +1 if rhs should be sorted before this, 0 otherwise.</returns>
        public int CompareTo(CreaturePower rhs)
        {
            var lhsBasic = false;
            var rhsBasic = false;

            if (_fAction != null && _fAction.Use == PowerUseType.Basic)
                lhsBasic = true;

            if (rhs.Action != null && rhs.Action.Use == PowerUseType.Basic)
                rhsBasic = true;

            if (lhsBasic != rhsBasic)
            {
                // Sort basic attack power before other powers

                if (lhsBasic)
                    return -1;

                if (rhsBasic)
                    return 1;
            }

            if (lhsBasic && rhsBasic)
            {
                var lhsMelee = _fRange.ToLower().Contains("melee");
                var rhsMelee = rhs.Range.ToLower().Contains("melee");

                if (lhsMelee != rhsMelee)
                {
                    // Sort melee basic before ranged basic

                    if (lhsMelee)
                        return -1;

                    if (rhsMelee)
                        return 1;
                }
            }

            if (!lhsBasic && !rhsBasic)
            {
                var lhsDouble = _fRange.ToLower().Contains("double");
                var rhsDouble = rhs.Range.ToLower().Contains("double");

                if (lhsDouble != rhsDouble)
                {
                    // Sort X before Double X

                    if (lhsDouble)
                        return -1;

                    if (rhsDouble)
                        return 1;
                }
            }

            return _fName.CompareTo(rhs.Name);
        }
    }

    /// <summary>
    ///     Class containing action / usage data for a CreaturePower.
    /// </summary>
    [Serializable]
    public class PowerAction
    {
        /// <summary>
        ///     Recharge 2-6.
        /// </summary>
        public const string Recharge2 = "Recharges on 2-6";

        /// <summary>
        ///     Recharge 3-6.
        /// </summary>
        public const string Recharge3 = "Recharges on 3-6";

        /// <summary>
        ///     Recharge 4-6.
        /// </summary>
        public const string Recharge4 = "Recharges on 4-6";

        /// <summary>
        ///     Recharge 5-6.
        /// </summary>
        public const string Recharge5 = "Recharges on 5-6";

        /// <summary>
        ///     Recharge 6.
        /// </summary>
        public const string Recharge6 = "Recharges on 6";

        private ActionType _fAction = ActionType.Standard;

        private string _fRecharge = "";

        private ActionType _fSustainAction = ActionType.None;

        private string _fTrigger = "";

        private PowerUseType _fUse = PowerUseType.AtWill;

        /// <summary>
        ///     Gets or sets the action required to use the power.
        /// </summary>
        public ActionType Action
        {
            get => _fAction;
            set => _fAction = value;
        }

        /// <summary>
        ///     Gets or sets the trigger for an immediate reaction or interrupt.
        /// </summary>
        public string Trigger
        {
            get => _fTrigger;
            set => _fTrigger = value;
        }

        /// <summary>
        ///     Gets or sets the action required to sustain the power.
        /// </summary>
        public ActionType SustainAction
        {
            get => _fSustainAction;
            set => _fSustainAction = value;
        }

        /// <summary>
        ///     Gets or sets the power's type (basic attack, at will, or encounter)
        /// </summary>
        public PowerUseType Use
        {
            get => _fUse;
            set => _fUse = value;
        }

        /// <summary>
        ///     Gets or sets the recharge condition.
        /// </summary>
        public string Recharge
        {
            get => _fRecharge;
            set => _fRecharge = value;
        }

        /// <summary>
        ///     Creates a copy of the PowerAttack.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PowerAction Copy()
        {
            var pa = new PowerAction();

            pa.Action = _fAction;
            pa.Trigger = _fTrigger;
            pa.SustainAction = _fSustainAction;
            pa.Use = _fUse;
            pa.Recharge = _fRecharge;

            return pa;
        }

        /// <summary>
        ///     Gets a string representation of the PowerAttack.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var str = "";

            if (_fUse == PowerUseType.AtWill || _fUse == PowerUseType.Basic)
            {
                str = "At-Will";

                if (_fUse == PowerUseType.Basic)
                    str += " (basic attack)";
            }

            if (_fUse == PowerUseType.Encounter && _fRecharge == "")
                str = "Encounter";

            if (_fUse == PowerUseType.Daily)
                str = "Daily";

            if (_fRecharge != "")
            {
                if (str != "")
                    str += "; ";

                str += _fRecharge;
            }

            return str;
        }
    }

    /// <summary>
    ///     Class containing attack data for a power.
    /// </summary>
    [Serializable]
    public class PowerAttack
    {
        private int _fBonus;

        private DefenceType _fDefence = DefenceType.Ac;

        /// <summary>
        ///     Gets or sets the attack bonus.
        /// </summary>
        public int Bonus
        {
            get => _fBonus;
            set => _fBonus = value;
        }

        /// <summary>
        ///     Gets or sets the targeted defence.
        /// </summary>
        public DefenceType Defence
        {
            get => _fDefence;
            set => _fDefence = value;
        }

        /// <summary>
        ///     Creates a copy of the PowerAttack.
        /// </summary>
        /// <returns>Returns the copy.</returns>
        public PowerAttack Copy()
        {
            var pa = new PowerAttack();

            pa.Bonus = _fBonus;
            pa.Defence = _fDefence;

            return pa;
        }

        /// <summary>
        ///     [bonus] vs [defence]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sign = _fBonus >= 0 ? "+" : "";
            return sign + _fBonus + " vs " + _fDefence;
        }
    }
}
