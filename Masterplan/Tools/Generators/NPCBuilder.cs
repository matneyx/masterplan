namespace Masterplan.Tools.Generators
{
    internal class NpcBuilder
    {
        private static readonly string[] FProfession =
        {
            "apothecary", "architect", "armourer", "arrowsmith", "astrologer", "baker", "barber", "lawyer", "beggar",
            "bellfounder", "blacksmith", "bookbinder", "brewer", "bricklayer", "butcher", "carpenter", "carter",
            "cartwright", "chandler", "peddler", "clerk", "clockmaker", "cobbler", "cook", "cooper", "merchant",
            "embroiderer", "engraver", "fisherman", "fishmonger", "forester", "furrier", "gardener", "gemcutter",
            "glassblower", "goldsmith", "grocer", "haberdasher", "stableman", "courtier", "herbalist", "innkeeper",
            "ironmonger", "labourer", "painter", "locksmith", "mason", "messenger", "miller", "miner", "minstrel",
            "ploughman", "farmer", "porter", "sailor", "scribe", "seamstress", "shepherd", "shipwright", "soapmaker",
            "tailor", "tinker", "vintner", "weaver"
        };

        private static readonly string[] FAge = { "elderly", "middle-aged", "teenage", "youthful", "young", "old" };

        private static readonly string[] FHeight =
            { "gangly", "gigantic", "hulking", "lanky", "short", "small", "stumpy", "tall", "tiny", "willowy" };

        private static readonly string[] FWeight =
        {
            "broad-shouldered", "fat", "gaunt", "obese", "plump", "pot-bellied", "rotund", "skinny", "slender", "slim",
            "statuesque", "stout", "thin"
        };

        private static readonly string[] FHairColour =
        {
            "black", "brown", "dark brown", "light brown", "red", "ginger", "strawberry blonde", "blonde", "ash blonde",
            "graying", "silver", "white", "gray", "auburn"
        };

        private static readonly string[] FHairStyle =
        {
            "short", "cropped", "long", "braided", "dreadlocked", "shoulder-length", "wiry", "balding", "receeding",
            "curly", "tightly-curled", "straight", "greasy", "limp", "sparse", "thinning", "wavy"
        };

        private static readonly string[] FPhysical =
        {
            "bearded", "buck-toothed", "chiselled", "doe-eyed", "fine-featured", "florid", "gap-toothed", "goggle-eyed",
            "grizzled", "jowly", "jug-eared", "pock-marked", "broken nose", "red-cheeked", "scarred", "squinting",
            "thin-lipped", "toothless", "weather-beaten", "wrinkled"
        };

        private static readonly string[] FMental =
        {
            "hot-tempered", "overbearing", "antagonistic", "haughty", "elitist", "proud", "rude", "aloof",
            "mischievous", "impulsive", "lusty", "irreverent", "madcap", "thoughtless", "absent-minded", "insensitive",
            "brave", "craven", "shy", "fearless", "obsequious", "inquisitive", "prying", "intellectual", "perceptive",
            "keen", "perfectionist", "stern", "harsh", "punctual", "driven", "trusting", "kind-hearted", "forgiving",
            "easy-going", "compassionate", "miserly", "hard-hearted", "covetous", "avaricious", "thrifty", "wastrel",
            "spendthrift", "extravagant", "kind", "charitable", "gloomy", "morose", "compulsive", "irritable",
            "vengeful", "honest", "truthful", "innocent", "gullible", "bigoted", "biased", "narrow-minded", "cheerful",
            "happy", "diplomatic", "pleasant", "foolhardy", "affable", "fatalistic", "depressing", "cynical",
            "sarcastic", "realistic", "secretive", "retiring", "practical", "level-headed", "dull", "reverent",
            "scheming", "paranoid", "cautious", "deceitful", "nervous", "uncultured", "boorish", "barbaric",
            "graceless", "crude", "cruel", "sadistic", "immoral", "jealous", "belligerent", "argumentative", "arrogant",
            "careless", "curious", "exacting", "friendly", "greedy", "generous", "moody", "naive", "opinionated",
            "optimistic", "pessimistic", "quiet", "sober", "suspicious", "uncivilised", "violent", "peaceful"
        };

        private static readonly string[] FSpeech =
        {
            "accented", "articulate", "garrulous", "breathless", "crisp", "gutteral", "high-pitched", "lisping", "loud",
            "nasal", "slow", "fast", "squeaky", "stuttering", "wheezy", "whiny", "whispery", "soft-spoken", "laconic",
            "blustering"
        };

        public static string Description()
        {
            var age = FAge[Session.Random.Next(FAge.Length)];
            var profession = FProfession[Session.Random.Next(FProfession.Length)];
            var main = "";
            switch (Session.Random.Next(3))
            {
                case 0:
                case 1:
                    main = profession;
                    break;
                case 2:
                    main = age + " " + profession;
                    break;
            }

            main = (TextHelper.StartsWithVowel(main) ? "An" : "A") + " " + main;

            var height = FHeight[Session.Random.Next(FHeight.Length)];
            var weight = FWeight[Session.Random.Next(FWeight.Length)];
            var stature = "";
            switch (Session.Random.Next(4))
            {
                case 0:
                case 1:
                    stature = height + " and " + weight;
                    break;
                case 2:
                    stature = height;
                    break;
                case 3:
                    stature = weight;
                    break;
            }

            var colour = FHairColour[Session.Random.Next(FHairColour.Length)];
            var style = FHairStyle[Session.Random.Next(FHairStyle.Length)];
            var hair = style + " " + colour;

            var desc = "";
            switch (Session.Random.Next(4))
            {
                case 0:
                case 1:
                    desc = main + ", " + stature + " with " + hair + " hair.";
                    break;
                case 2:
                    desc = main + " with " + hair + " hair.";
                    break;
                case 3:
                    desc = main + ", " + stature + ".";
                    break;
            }

            return desc;
        }

        public static string Physical()
        {
            return get_values(FPhysical);
        }

        public static string Personality()
        {
            return get_values(FMental);
        }

        public static string Speech()
        {
            return get_values(FSpeech);
        }

        private static string get_values(string[] array)
        {
            var count = get_number();

            var bst = new BinarySearchTree<string>();
            while (bst.Count != count)
            {
                var value = array[Session.Random.Next(array.Length)];
                bst.Add(value);
            }

            var values = bst.SortedList;

            var result = "";
            foreach (var value in values)
            {
                if (result != "")
                {
                    if (value == values[values.Count - 1])
                        result += " and ";
                    else
                        result += ", ";
                }

                result += value;
            }

            if (result != "")
                result = TextHelper.Capitalise(result, false);

            return result;
        }

        private static int get_number()
        {
            switch (Session.Random.Next(10))
            {
                case 0:
                    return 0;
                case 1:
                case 2:
                case 3:
                    return 1;
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                    return 2;
                case 9:
                    return 3;
            }

            return 1;
        }
    }
}
