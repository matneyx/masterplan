using System.Collections.Generic;

namespace Masterplan.Tools
{
    internal class Skills
    {
        public static List<string> GetAbilityNames()
        {
            var abilities = new List<string>();

            abilities.Add("Strength");
            abilities.Add("Constitution");
            abilities.Add("Dexterity");
            abilities.Add("Intelligence");
            abilities.Add("Wisdom");
            abilities.Add("Charisma");

            return abilities;
        }

        public static List<string> GetSkillNames()
        {
            var skills = new List<string>();

            skills.Add("Acrobatics");
            skills.Add("Arcana");
            skills.Add("Athletics");
            skills.Add("Bluff");
            skills.Add("Diplomacy");
            skills.Add("Dungeoneering");
            skills.Add("Endurance");
            skills.Add("Heal");
            skills.Add("History");
            skills.Add("Insight");
            skills.Add("Intimidate");
            skills.Add("Nature");
            skills.Add("Perception");
            skills.Add("Religion");
            skills.Add("Stealth");
            skills.Add("Streetwise");
            skills.Add("Thievery");

            return skills;
        }

        public static string GetKeyAbility(string skillName)
        {
            if (skillName == "Athletics")
                return "Strength";

            if (skillName == "Endurance")
                return "Constitution";

            if (skillName == "Acrobatics" || skillName == "Stealth" || skillName == "Thievery")
                return "Dexterity";

            if (skillName == "Arcana" || skillName == "History" || skillName == "Religion")
                return "Intelligence";

            if (skillName == "Dungeoneering" || skillName == "Heal" || skillName == "Insight" ||
                skillName == "Nature" || skillName == "Perception")
                return "Wisdom";

            if (skillName == "Bluff" || skillName == "Diplomacy" || skillName == "Intimidate" ||
                skillName == "Streetwise")
                return "Charisma";

            return "";
        }
    }
}
