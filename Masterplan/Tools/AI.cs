using System;
using Masterplan.Data;

namespace Masterplan.Tools
{
    internal class Ai
    {
        public static Difficulty GetThreatDifficulty(int threatLevel, int partyLevel)
        {
            var levelDiff = threatLevel - partyLevel;

            if (levelDiff > 5)
                return Difficulty.Extreme;

            if (levelDiff < -3)
                return Difficulty.Trivial;

            return Difficulty.Easy;
        }

        public static Difficulty GetSkillDifficulty(int dc, int partyLevel)
        {
            var easy = GetSkillDc(Difficulty.Easy, partyLevel);
            var mod = GetSkillDc(Difficulty.Moderate, partyLevel);
            var hard = GetSkillDc(Difficulty.Hard, partyLevel);
            var ext = hard + (hard - easy) / 2;

            if (dc < easy)
                return Difficulty.Trivial;
            if (dc < mod)
                return Difficulty.Easy;
            if (dc < hard)
                return Difficulty.Moderate;
            if (dc < ext)
                return Difficulty.Hard;
            return Difficulty.Extreme;
        }

        public static int GetSkillDc(Difficulty diff, int level)
        {
            switch (diff)
            {
                case Difficulty.Easy:
                    switch (level)
                    {
                        case 1:
                            return 8;
                        case 2:
                            return 9;
                        case 3:
                            return 9;
                        case 4:
                            return 10;
                        case 5:
                            return 10;
                        case 6:
                            return 11;
                        case 7:
                            return 11;
                        case 8:
                            return 12;
                        case 9:
                            return 12;
                        case 10:
                            return 13;
                        case 11:
                            return 13;
                        case 12:
                            return 14;
                        case 13:
                            return 14;
                        case 14:
                            return 15;
                        case 15:
                            return 15;
                        case 16:
                            return 16;
                        case 17:
                            return 16;
                        case 18:
                            return 17;
                        case 19:
                            return 17;
                        case 20:
                            return 18;
                        case 21:
                            return 19;
                        case 22:
                            return 20;
                        case 23:
                            return 20;
                        case 24:
                            return 21;
                        case 25:
                            return 21;
                        case 26:
                            return 22;
                        case 27:
                            return 22;
                        case 28:
                            return 23;
                        case 29:
                            return 23;
                        case 30:
                            return 24;
                    }

                    break;
                case Difficulty.Moderate:
                    switch (level)
                    {
                        case 1:
                            return 12;
                        case 2:
                            return 13;
                        case 3:
                            return 13;
                        case 4:
                            return 14;
                        case 5:
                            return 15;
                        case 6:
                            return 15;
                        case 7:
                            return 16;
                        case 8:
                            return 16;
                        case 9:
                            return 17;
                        case 10:
                            return 18;
                        case 11:
                            return 19;
                        case 12:
                            return 20;
                        case 13:
                            return 20;
                        case 14:
                            return 21;
                        case 15:
                            return 22;
                        case 16:
                            return 22;
                        case 17:
                            return 23;
                        case 18:
                            return 23;
                        case 19:
                            return 24;
                        case 20:
                            return 25;
                        case 21:
                            return 26;
                        case 22:
                            return 27;
                        case 23:
                            return 27;
                        case 24:
                            return 28;
                        case 25:
                            return 29;
                        case 26:
                            return 29;
                        case 27:
                            return 30;
                        case 28:
                            return 30;
                        case 29:
                            return 31;
                        case 30:
                            return 32;
                    }

                    break;
                case Difficulty.Hard:
                    switch (level)
                    {
                        case 1:
                            return 19;
                        case 2:
                            return 20;
                        case 3:
                            return 21;
                        case 4:
                            return 21;
                        case 5:
                            return 22;
                        case 6:
                            return 23;
                        case 7:
                            return 23;
                        case 8:
                            return 24;
                        case 9:
                            return 25;
                        case 10:
                            return 26;
                        case 11:
                            return 27;
                        case 12:
                            return 28;
                        case 13:
                            return 29;
                        case 14:
                            return 29;
                        case 15:
                            return 30;
                        case 16:
                            return 31;
                        case 17:
                            return 31;
                        case 18:
                            return 32;
                        case 19:
                            return 33;
                        case 20:
                            return 34;
                        case 21:
                            return 35;
                        case 22:
                            return 36;
                        case 23:
                            return 37;
                        case 24:
                            return 37;
                        case 25:
                            return 38;
                        case 26:
                            return 39;
                        case 27:
                            return 39;
                        case 28:
                            return 40;
                        case 29:
                            return 41;
                        case 30:
                            return 42;
                    }

                    break;
            }

            return 0;
        }

        public static string ExtractDamage(string source)
        {
            string[] separators = { ",", ";", ".", ":", Environment.NewLine };
            var sections = source.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var section in sections)
            {
                var str = section.Trim().ToLower();

                if (str.Contains("damage") || str.Contains("dmg"))
                    return str;
            }

            foreach (var section in sections)
            {
                var exp = DiceExpression.Parse(section);
                if (exp != null)
                    return section;
            }

            return "";
        }
    }
}
