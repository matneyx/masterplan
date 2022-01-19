using System;
using System.Collections.Generic;

namespace Masterplan.Tools
{
    internal class DiceStatistics
    {
        public static Dictionary<int, int> Odds(List<int> dice, int constant)
        {
            var odds = new Dictionary<int, int>();

            if (dice.Count > 0)
            {
                var combinations = 1;
                foreach (var die in dice)
                    combinations *= die;

                // Work out how quickly each die rolls over
                var frequencies = new int[dice.Count];
                frequencies[dice.Count - 1] = 1;
                for (var n = dice.Count - 2; n >= 0; --n)
                    frequencies[n] = frequencies[n + 1] * dice[n + 1];

                for (var n = 0; n != combinations; ++n)
                {
                    // Work out the number for each die
                    var rolls = new List<int>();
                    for (var index = 0; index != dice.Count; ++index)
                    {
                        var die = dice[index];
                        var roll = n / frequencies[index] % die + 1;

                        rolls.Add(roll);
                    }

                    // Work out the sum
                    var sum = constant;
                    foreach (var roll in rolls)
                        sum += roll;

                    if (!odds.ContainsKey(sum))
                        odds[sum] = 0;

                    odds[sum] += 1;
                }
            }

            return odds;
        }

        public static string Expression(List<int> dice, int constant)
        {
            var d4 = 0;
            var d6 = 0;
            var d8 = 0;
            var d10 = 0;
            var d12 = 0;
            var d20 = 0;

            foreach (var die in dice)
                switch (die)
                {
                    case 4:
                        d4 += 1;
                        break;
                    case 6:
                        d6 += 1;
                        break;
                    case 8:
                        d8 += 1;
                        break;
                    case 10:
                        d10 += 1;
                        break;
                    case 12:
                        d12 += 1;
                        break;
                    case 20:
                        d20 += 1;
                        break;
                }

            var exp = "";
            if (d4 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d4 + "d4";
            }

            if (d6 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d6 + "d6";
            }

            if (d8 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d8 + "d8";
            }

            if (d10 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d10 + "d10";
            }

            if (d12 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d12 + "d12";
            }

            if (d20 != 0)
            {
                if (exp != "")
                    exp += " + ";

                exp += d20 + "d20";
            }

            if (constant != 0)
            {
                exp += " ";

                if (constant > 0)
                    exp += "+";

                exp += constant.ToString();
            }

            return exp;
        }
    }

    internal class DiceExpression
    {
        public int Throws { get; set; }

        public int Sides { get; set; }

        public int Constant { get; set; }

        public int Maximum => Throws * Sides + Constant;

        public double Average
        {
            get
            {
                var mean = (double)(Sides + 1) / 2;
                return Throws * mean + Constant;
            }
        }

        public DiceExpression()
        {
            Throws = 0;
            Sides = 0;
            Constant = 0;
        }

        public DiceExpression(int throws, int sides)
        {
            Throws = throws;
            Sides = sides;
            Constant = 0;
        }

        public DiceExpression(int throws, int sides, int constant)
        {
            Throws = throws;
            Sides = sides;
            Constant = constant;
        }

        public static DiceExpression Parse(string str)
        {
            var exp = new DiceExpression();

            try
            {
                var started = false;
                var minus = false;
                char[] digits = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };

                str = str.ToLower();
                str = str.Replace("+", " + ");
                str = str.Replace("-", " - ");

                var tokens = str.Split(null);
                foreach (var token in tokens)
                {
                    if (token == "damage" || token == "dmg")
                        break;

                    if (token == "-" && started)
                    {
                        minus = true;
                        continue;
                    }

                    if (token.IndexOfAny(digits) == -1)
                        continue;

                    // Has a 'd'
                    var dIndex = token.IndexOf("d");
                    if (dIndex != -1)
                    {
                        var throws = token.Substring(0, dIndex);
                        var sides = token.Substring(dIndex + 1);

                        if (throws != "")
                            exp.Throws = int.Parse(throws);

                        exp.Sides = int.Parse(sides);
                    }
                    else
                    {
                        if (exp.Constant == 0)
                        {
                            exp.Constant = int.Parse(token);

                            if (minus)
                                exp.Constant = -exp.Constant;
                        }
                    }

                    started = true;
                }
            }
            catch
            {
                // Parse error?
                exp = null;
            }

            if (exp != null && exp.Throws == 0 && exp.Constant == 0)
                exp = null;

            return exp;
        }

        public int Evaluate()
        {
            return Session.Dice(Throws, Sides) + Constant;
        }

        public override string ToString()
        {
            var str = "";

            if (Throws != 0)
                str = Throws + "d" + Sides;

            if (Constant != 0)
            {
                if (str != "")
                {
                    str += " ";

                    if (Constant > 0)
                        str += "+";
                }

                str += Constant.ToString();
            }

            if (str == "")
                str = "0";

            return str;
        }

        public DiceExpression Adjust(int levelAdjustment)
        {
            var dmgs = Enum.GetValues(typeof(DamageExpressionType));

            // Choose the closest level and work out the differences (in throws / sides / constant)
            var minDifference = int.MaxValue;
            var bestLevel = 0;
            var bestDet = DamageExpressionType.Normal;
            DiceExpression bestExp = null;
            for (var level = 1; level <= 30; ++level)
                foreach (DamageExpressionType det in dmgs)
                {
                    var exp = Parse(Statistics.Damage(level, det));

                    var diffThrows = Math.Abs(Throws - exp.Throws);
                    var diffSides = Math.Abs(Sides - exp.Sides) / 2;
                    var diffConst = Math.Abs(Constant - exp.Constant);

                    var difference = diffThrows * 10 + diffSides * 100 + diffConst;
                    if (difference < minDifference)
                    {
                        minDifference = difference;
                        bestLevel = level;
                        bestDet = det;
                        bestExp = exp;
                    }
                }

            if (bestExp == null)
                return this;

            var throwDiff = Throws - bestExp.Throws;
            var sidesDiff = Sides - bestExp.Sides;
            var constDiff = Constant - bestExp.Constant;

            // Adjust the new expression
            var adjLevel = Math.Max(bestLevel + levelAdjustment, 1);
            var adjusted = Parse(Statistics.Damage(adjLevel, bestDet));
            adjusted.Throws += throwDiff;
            adjusted.Sides += sidesDiff;
            adjusted.Constant += constDiff;

            if (Throws == 0)
                adjusted.Throws = 0;
            else
                adjusted.Throws = Math.Max(adjusted.Throws, 1);

            // Make sure we have a valid dice type
            switch (adjusted.Sides)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    adjusted.Sides = 4;
                    break;
                case 5:
                case 6:
                    adjusted.Sides = 6;
                    break;
                case 7:
                case 8:
                    adjusted.Sides = 8;
                    break;
                case 9:
                case 10:
                    adjusted.Sides = 10;
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                    adjusted.Sides = 12;
                    break;
                default:
                    adjusted.Sides = 20;
                    break;
            }

            return adjusted;
        }
    }
}
