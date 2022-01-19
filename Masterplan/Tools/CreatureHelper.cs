using System;
using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class CreatureHelper
    {
        public static void CopyFields(ICreature copyFrom, ICreature copyTo)
        {
            try
            {
                if (copyFrom != null)
                {
                    copyTo.Id = copyFrom.Id;
                    copyTo.Name = copyFrom.Name;
                    copyTo.Details = copyFrom.Details;
                    copyTo.Size = copyFrom.Size;
                    copyTo.Origin = copyFrom.Origin;
                    copyTo.Type = copyFrom.Type;
                    copyTo.Keywords = copyFrom.Keywords;
                    copyTo.Level = copyFrom.Level;
                    copyTo.Role = copyFrom.Role?.Copy();
                    copyTo.Senses = copyFrom.Senses;
                    copyTo.Movement = copyFrom.Movement;
                    copyTo.Alignment = copyFrom.Alignment;
                    copyTo.Languages = copyFrom.Languages;
                    copyTo.Skills = copyFrom.Skills;
                    copyTo.Equipment = copyFrom.Equipment;
                    copyTo.Category = copyFrom.Category;

                    copyTo.Strength = copyFrom.Strength.Copy();
                    copyTo.Constitution = copyFrom.Constitution.Copy();
                    copyTo.Dexterity = copyFrom.Dexterity.Copy();
                    copyTo.Intelligence = copyFrom.Intelligence.Copy();
                    copyTo.Wisdom = copyFrom.Wisdom.Copy();
                    copyTo.Charisma = copyFrom.Charisma.Copy();

                    copyTo.Hp = copyFrom.Hp;
                    copyTo.Initiative = copyFrom.Initiative;
                    copyTo.Ac = copyFrom.Ac;
                    copyTo.Fortitude = copyFrom.Fortitude;
                    copyTo.Reflex = copyFrom.Reflex;
                    copyTo.Will = copyFrom.Will;

                    copyTo.Regeneration = copyFrom.Regeneration?.Copy();

                    copyTo.Auras.Clear();
                    foreach (var aura in copyFrom.Auras)
                        copyTo.Auras.Add(aura.Copy());

                    copyTo.CreaturePowers.Clear();
                    foreach (var cp in copyFrom.CreaturePowers)
                        copyTo.CreaturePowers.Add(cp.Copy());

                    copyTo.DamageModifiers.Clear();
                    foreach (var dm in copyFrom.DamageModifiers)
                        copyTo.DamageModifiers.Add(dm.Copy());

                    copyTo.Resist = copyFrom.Resist;
                    copyTo.Vulnerable = copyFrom.Vulnerable;
                    copyTo.Immune = copyFrom.Immune;
                    copyTo.Tactics = copyFrom.Tactics;
                }
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);
            }
        }

        public static void UpdateRegen(ICreature c)
        {
            var regenAura = FindAura(c, "Regeneration");
            if (regenAura == null)
                regenAura = FindAura(c, "Regen");

            if (regenAura != null)
            {
                var regen = ConvertAura(regenAura.Details);
                if (regen != null)
                {
                    c.Regeneration = regen;
                    c.Auras.Remove(regenAura);
                }
            }
        }

        public static void UpdatePowerRange(ICreature c, CreaturePower power)
        {
            if (power.Range != null && power.Range != "")
                return;

            var ranges = new List<string>();
            ranges.Add("close blast");
            ranges.Add("close burst");
            ranges.Add("area burst");
            ranges.Add("melee");
            ranges.Add("ranged");

            var details = "";

            var clauses = power.Details.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var clause in clauses)
            {
                var isRangeClause = false;
                foreach (var range in ranges)
                    if (clause.ToLower().Contains(range))
                    {
                        isRangeClause = true;
                        break;
                    }

                if (isRangeClause)
                {
                    power.Range = clause;
                }
                else
                {
                    if (details != "")
                        details += "; ";

                    details += clause;
                }
            }

            power.Details = details;
        }

        public static Aura FindAura(ICreature c, string name)
        {
            foreach (var a in c.Auras)
                if (a.Name == name)
                    return a;

            return null;
        }

        public static Regeneration ConvertAura(string auraDetails)
        {
            auraDetails = auraDetails.Trim();

            var parsingValue = true;
            var valStr = "";
            var details = "";

            foreach (var ch in auraDetails)
            {
                if (!char.IsDigit(ch))
                    parsingValue = false;

                if (parsingValue)
                    valStr += ch;
                else
                    details += ch;
            }

            details = details.Trim();
            if (details.StartsWith("(") && details.EndsWith(")"))
            {
                details = details.Substring(1);
                details = details.Substring(0, details.Length - 1);

                details.Trim();
            }

            try
            {
                var value = valStr != "" ? int.Parse(valStr) : 0;

                return new Regeneration(value, details);
            }
            catch (Exception ex)
            {
                LogSystem.Trace(ex);

                return null;
            }
        }

        public static List<CreaturePower> CreaturePowersByCategory(ICreature c, CreaturePowerCategory category)
        {
            var powers = new List<CreaturePower>();

            foreach (var cp in c.CreaturePowers)
                if (cp.Category == category)
                    powers.Add(cp);

            return powers;
        }

        public static void AdjustCreatureLevel(ICreature creature, int delta)
        {
            // HP
            if (creature.Role is ComplexRole)
            {
                var role = creature.Role as ComplexRole;

                var hp = 8;
                switch (role.Type)
                {
                    case RoleType.Artillery:
                    case RoleType.Lurker:
                        hp = 6;
                        break;
                    case RoleType.Brute:
                        hp = 10;
                        break;
                }

                switch (role.Flag)
                {
                    case RoleFlag.Elite:
                        hp *= 2;
                        break;
                    case RoleFlag.Solo:
                        hp *= 5;
                        break;
                }

                creature.Hp += hp * delta;
                creature.Hp = Math.Max(creature.Hp, 1);
            }

            // Init
            var initBonus = creature.Initiative - creature.Level / 2;
            creature.Initiative = initBonus + (creature.Level + delta) / 2;

            // Defences
            creature.Ac += delta;
            creature.Fortitude += delta;
            creature.Reflex += delta;
            creature.Will += delta;

            // Powers
            foreach (var cp in creature.CreaturePowers)
                AdjustPowerLevel(cp, delta);

            // Skills
            if (creature.Skills != "")
            {
                // Parse string
                var skillList = ParseSkills(creature.Skills);

                // Sort
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
                    mod = bonus + (creature.Level + delta) / 2;

                    if (mod >= 0)
                        skillStr += skillName + " +" + mod;
                    else
                        skillStr += skillName + " " + mod;
                }

                creature.Skills = skillStr;
            }

            // Level
            creature.Level += delta;
        }

        public static void AdjustPowerLevel(CreaturePower cp, int delta)
        {
            if (cp.Attack != null)
                cp.Attack.Bonus += delta;

            // Adjust power damage
            var dmgStr = Ai.ExtractDamage(cp.Details);
            if (dmgStr != "")
            {
                var exp = DiceExpression.Parse(dmgStr);
                var expAdj = exp?.Adjust(delta);
                if (expAdj != null && exp.ToString() != expAdj.ToString())
                    cp.Details = cp.Details.Replace(dmgStr, expAdj + " damage");
            }
        }

        public static Dictionary<string, int> ParseSkills(string source)
        {
            var skillList = new Dictionary<string, int>();

            if (source != null && source != "")
            {
                var skills = source.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var skill in skills)
                {
                    var str = skill.Trim();

                    var index = str.IndexOf(" ");
                    if (index != -1)
                    {
                        var skillName = str.Substring(0, index);
                        var skillBonus = str.Substring(index + 1);

                        var bonus = 0;
                        try
                        {
                            bonus = int.Parse(skillBonus);
                        }
                        catch
                        {
                            bonus = 0;
                        }

                        skillList[skillName] = bonus;
                    }
                }
            }

            return skillList;
        }
    }
}
