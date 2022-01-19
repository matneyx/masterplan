using System.Collections.Generic;

namespace Masterplan.Tools
{
    internal class Conditions
    {
        public static List<string> GetConditions()
        {
            var conditions = new List<string>();

            conditions.Add("Blinded");
            conditions.Add("Dazed");
            conditions.Add("Deafened");
            conditions.Add("Dominated");
            conditions.Add("Dying");
            conditions.Add("Grabbed");
            conditions.Add("Helpless");
            conditions.Add("Immobilised");
            conditions.Add("Marked");
            conditions.Add("Petrified");
            conditions.Add("Prone");
            conditions.Add("Removed from play");
            conditions.Add("Restrained");
            conditions.Add("Slowed");
            conditions.Add("Stunned");
            conditions.Add("Surprised");
            conditions.Add("Unconscious");
            conditions.Add("Weakened");

            return conditions;
        }

        public static List<string> GetConditionInfo(string condition)
        {
            var conditionTokens = new List<string>(condition.ToLower().Split(null));

            var lines = new List<string>();

            if (conditionTokens.Contains("blinded"))
                lines.Add("Grant CA; targets have total concealment; -10 to Perception; can't flank");

            if (conditionTokens.Contains("dazed"))
                lines.Add(
                    "Grant CA; one std / move / minor action per turn; no immediate / opportunity actions; can't flank");

            if (conditionTokens.Contains("deafened"))
                lines.Add("Can't hear; -10 to Perception");

            if (conditionTokens.Contains("dominated"))
                lines.Add(
                    "Grant CA; can't flank; can't take actions; dominating creature chooses one action on your turn, can make you use at-will powers");

            if (conditionTokens.Contains("dying"))
                lines.Add(
                    "Grant CA; can be targeted by coup de grace; -5 to defences; can't take actions; can't flank; make death save each round");

            if (conditionTokens.Contains("grabbed"))
                lines.Add("Can't move (can teleport, can be forced to move)");

            if (conditionTokens.Contains("helpless"))
                lines.Add("Grant CA; can be targeted by coup de grace");

            if (conditionTokens.Contains("immobilised"))
                lines.Add("Can't move (can teleport, can be forced to move)");

            if (conditionTokens.Contains("marked"))
                lines.Add("-2 to attack when your attack doesn't include the marker");

            if (conditionTokens.Contains("petrified"))
                lines.Add("Can't take actions; resist 20 to all damage; unaware of surroundings; don't age");

            if (conditionTokens.Contains("prone"))
                lines.Add(
                    "Grant CA to enemies making melee attacks; can't move (can teleport, crawl or be forced to move); +2 defences vs ranged attacks from non-adjacent enemies; -2 to attacks");

            if (conditionTokens.Contains("removed"))
                lines.Add("Can't take actions; no line of sight or effect");

            if (conditionTokens.Contains("restrained"))
                lines.Add("Grant CA; can't move (can teleport); -2 to attacks");

            if (conditionTokens.Contains("slowed"))
                lines.Add("Speed is 2");

            if (conditionTokens.Contains("stunned"))
                lines.Add("Grant CA; can't take actions; can't flank");

            if (conditionTokens.Contains("surprised"))
                lines.Add("Grant CA; can't take actions; can't flank");

            if (conditionTokens.Contains("unconscious"))
                lines.Add(
                    "Grant CA; can be targeted by coup de grace; -5 to defences; can't take actions; can't flank");

            if (conditionTokens.Contains("weakened"))
                lines.Add("Attacks deal half damage");

            return lines;
        }
    }
}
