using System.Collections.Generic;

namespace Masterplan.Tools.Generators
{
    internal class ExoticName
    {
        public static string SingleName()
        {
            return TextHelper.Capitalise(get_word(), true);
        }

        public static string FullName()
        {
            var first = TextHelper.Capitalise(get_word(), true);
            var second = TextHelper.Capitalise(get_word(), true);

            return first + " " + second;
        }

        public static string Sentence()
        {
            var sentence = "";

            var words = Session.Dice(3, 6);
            for (var n = 0; n != words; ++n)
            {
                if (sentence != "")
                    sentence += " ";

                sentence += get_word();
            }

            sentence += ".";

            return TextHelper.Capitalise(sentence, false);
        }

        private static string get_word()
        {
            var vowels = new List<string>();
            vowels.Add("a");
            vowels.Add("e");
            vowels.Add("i");
            vowels.Add("o");
            vowels.Add("u");
            vowels.Add("ae");
            vowels.Add("ai");
            vowels.Add("ao");
            vowels.Add("au");
            vowels.Add("ea");
            vowels.Add("ee");
            vowels.Add("ei");
            vowels.Add("eo");
            vowels.Add("eu");
            vowels.Add("ia");
            vowels.Add("ie");
            vowels.Add("io");
            vowels.Add("iu");
            vowels.Add("oa");
            vowels.Add("oe");
            vowels.Add("oi");
            vowels.Add("oo");
            vowels.Add("ou");
            vowels.Add("ua");
            vowels.Add("ue");
            vowels.Add("ui");
            vowels.Add("uo");
            vowels.Add("y");

            var consonants = new List<string>();
            consonants.AddRange(new[] { "b" });
            consonants.AddRange(new[] { "c", "ch" });
            consonants.AddRange(new[] { "d" });
            consonants.AddRange(new[] { "f", "fl", "fr" });
            consonants.AddRange(new[] { "g", "gh", "gn", "gr" });
            consonants.AddRange(new[] { "h" });
            consonants.AddRange(new[] { "j" });
            consonants.AddRange(new[] { "k", "kh", "kr" });
            consonants.AddRange(new[] { "l", "ll" });
            consonants.AddRange(new[] { "m" });
            consonants.AddRange(new[] { "n" });
            consonants.AddRange(new[] { "p", "ph", "pr" });
            consonants.AddRange(new[] { "q" });
            consonants.AddRange(new[] { "r", "rh" });
            consonants.AddRange(new[] { "s", "sc", "sch", "sh", "sk", "sp", "st" });
            consonants.AddRange(new[] { "t", "th" });
            consonants.AddRange(new[] { "v" });
            consonants.AddRange(new[] { "w", "wr" });

            var separator = "-";
            if (Session.Random.Next(3) == 0)
                separator = "'";

            var name = "";
            var syllables = Session.Random.Next(2) + 1;
            for (var n = 0; n != syllables; ++n)
            {
                if (name != "")
                    if (Session.Random.Next(10) == 0)
                        name += separator;

                if (name == "")
                {
                    // Add a starting consonant
                    var index = Session.Random.Next(consonants.Count);
                    name += consonants[index];
                }

                // Add a vowel
                {
                    var index = Session.Random.Next(vowels.Count);
                    name += vowels[index];
                }

                // Add a consonant
                {
                    var index = Session.Random.Next(consonants.Count);
                    name += consonants[index];
                }
            }

            if (Session.Random.Next(4) == 0)
            {
                // Add a final vowel
                var index = Session.Random.Next(vowels.Count);
                var vowel = vowels[index];
                name += vowel[0];
            }

            return name;
        }
    }
}
