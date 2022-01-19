using System;
using System.Collections.Generic;
using Masterplan.Data;

namespace Masterplan.Tools.Generators
{
    internal class Treasure
    {
        private static List<Guid> _fPlaceholderIDs;

        private static readonly List<int> FValues = new List<int>(new[]
        {
            125000, 100000, 75000, 50000, 25000, 20000, 10000, 7500, 5000, 2500, 2000, 1000, 7500, 5000, 2500, 2000,
            1000, 750, 500, 250, 200, 100, 50
        });

        private static readonly List<string> FObjects = new List<string>(new[]
            { "medal", "statuette", "sculpture", "idol", "chalice", "goblet", "dish", "bowl" });

        private static readonly List<string> FJewellery = new List<string>(new[]
        {
            "ring", "necklace", "crown", "circlet", "bracelet", "anklet", "torc", "brooch", "pendant", "locket",
            "diadem", "tiara", "earring"
        });

        private static readonly List<string> FInstruments = new List<string>(new[]
        {
            "lute", "lyre", "mandolin", "violin", "drum", "flute", "clarinet", "accordion", "banjo", "bodhran",
            "ocarina", "zither", "djembe"
        });

        private static readonly List<string> FStones = new List<string>(new[]
        {
            "diamond", "ruby", "sapphire", "emerald", "amethyst", "garnet", "topaz", "pearl", "black pearl", "opal",
            "fire opal", "amber", "coral", "agate", "carnelian", "jade", "peridot", "moonstone", "alexandrite",
            "aquamarine", "jacinth", "marble"
        });

        private static readonly List<string> FMetals = new List<string>(new[]
            { "gold", "silver", "bronze", "platinum", "electrum", "mithral", "orium", "adamantine" });

        public static List<Guid> PlaceholderIDs
        {
            get
            {
                if (_fPlaceholderIDs == null)
                {
                    _fPlaceholderIDs = new List<Guid>();

                    for (var level = 1; level <= 30; ++level)
                    {
                        var id = new Guid(level, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                        _fPlaceholderIDs.Add(id);
                    }
                }

                return _fPlaceholderIDs;
            }
        }

        private static MagicItem get_placeholder_item(int level)
        {
            var lvl = Math.Min(level, 30);

            var item = new MagicItem();
            item.Name = "Magic Item (level " + lvl + ")";
            item.Level = lvl;
            item.Id = PlaceholderIDs[lvl - 1];

            return item;
        }

        public static int GetItemValue(int level)
        {
            switch (level)
            {
                case 1:
                    return 360;
                case 2:
                    return 520;
                case 3:
                    return 680;
                case 4:
                    return 840;
                case 5:
                    return 1000;
                case 6:
                    return 1800;
                case 7:
                    return 2600;
                case 8:
                    return 3400;
                case 9:
                    return 4200;
                case 10:
                    return 5000;
                case 11:
                    return 9000;
                case 12:
                    return 13000;
                case 13:
                    return 17000;
                case 14:
                    return 21000;
                case 15:
                    return 25000;
                case 16:
                    return 45000;
                case 17:
                    return 65000;
                case 18:
                    return 85000;
                case 19:
                    return 105000;
                case 20:
                    return 125000;
                case 21:
                    return 225000;
                case 22:
                    return 325000;
                case 23:
                    return 425000;
                case 24:
                    return 525000;
                case 25:
                    return 625000;
                case 26:
                    return 1125000;
                case 27:
                    return 1625000;
                case 28:
                    return 2125000;
                case 29:
                    return 2625000;
                case 30:
                    return 3125000;
            }

            return 0;
        }

        public static List<Parcel> CreateParcelSet(int level, int size, bool placeholderItems)
        {
            var parcels = new List<Parcel>();

            switch (size)
            {
                case 1:
                {
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                }
                    break;
                case 2:
                {
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                }
                    break;
                case 3:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                }
                    break;
                case 4:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                }
                    break;
                case 5:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                }
                    break;
                case 6:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                }
                    break;
                case 7:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                }
                    break;
                case 8:
                {
                    parcels.Add(get_magic_item(level + 4, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 3, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 2, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                    parcels.Add(get_magic_item(level + 1, placeholderItems));
                }
                    break;
            }

            var gpValues = get_gp_values(level);
            if (size == 1)
                gpValues.RemoveAt(0);

            foreach (var value in gpValues)
            {
                var p = CreateParcel(value, placeholderItems);
                parcels.Add(p);
            }

            return parcels;
        }

        public static Parcel CreateParcel(int level, int size, bool placeholder)
        {
            var parcels = CreateParcelSet(level, size, placeholder);
            var index = Session.Random.Next() % parcels.Count;
            return parcels[index];
        }

        public static Parcel CreateParcel(int value, bool placeholder)
        {
            var p = new Parcel();
            p.Name = "Items worth " + value + " GP";
            p.Value = value;

            if (!placeholder) p.Details = RandomMundaneItem(value);

            return p;
        }

        public static MagicItem RandomMagicItem(int level)
        {
            var itemLevel = Math.Min(30, level);

            var candidates = new List<MagicItem>();
            foreach (var item in Session.MagicItems)
                if (item.Level == itemLevel)
                    candidates.Add(item);

            if (candidates.Count != 0)
            {
                var index = Session.Random.Next() % candidates.Count;
                var item = candidates[index];

                return item;
            }

            return null;
        }

        public static Artifact RandomArtifact(Tier tier)
        {
            var candidates = new List<Artifact>();
            foreach (var item in Session.Artifacts)
                if (item.Tier == tier)
                    candidates.Add(item);

            if (candidates.Count != 0)
            {
                var index = Session.Random.Next() % candidates.Count;
                var item = candidates[index];

                return item;
            }

            return null;
        }

        public static string RandomMundaneItem(int value)
        {
            var items = create_from_gp(value);

            var details = "";
            foreach (var item in items)
            {
                if (details != "")
                    details += "; ";

                details += item;
            }

            return details;
        }

        public static string ArtObject()
        {
            var item = random_item_type(false, false);

            item = (TextHelper.StartsWithVowel(item) ? "An" : "A") + " " + item;

            return item;
        }

        private static Parcel get_magic_item(int level, bool placeholder)
        {
            var itemLevel = Math.Min(30, level);

            if (placeholder)
            {
                return new Parcel(get_placeholder_item(level));
            }

            var mi = RandomMagicItem(itemLevel);
            if (mi != null)
                return new Parcel(mi);

            var p = new Parcel();
            p.Details = "Random magic item (level " + itemLevel + ")";
            return p;
        }

        private static List<int> get_gp_values(int level)
        {
            switch (level)
            {
                case 1:
                    return new List<int>(new[] { 200, 180, 120, 120, 60, 40 });
                case 2:
                    return new List<int>(new[] { 290, 260, 170, 170, 90, 60 });
                case 3:
                    return new List<int>(new[] { 380, 340, 225, 225, 110, 75 });
                case 4:
                    return new List<int>(new[] { 470, 420, 280, 280, 140, 90 });
                case 5:
                    return new List<int>(new[] { 550, 500, 340, 340, 160, 110 });
                case 6:
                    return new List<int>(new[] { 1000, 900, 600, 600, 300, 200 });
                case 7:
                    return new List<int>(new[] { 1500, 1300, 850, 850, 400, 300 });
                case 8:
                    return new List<int>(new[] { 1900, 1700, 1100, 1100, 600, 400 });
                case 9:
                    return new List<int>(new[] { 2400, 2100, 1400, 1400, 700, 400 });
                case 10:
                    return new List<int>(new[] { 2800, 2500, 1700, 1700, 800, 500 });
                case 11:
                    return new List<int>(new[] { 5000, 4000, 3000, 3000, 2000, 1000 });
                case 12:
                    return new List<int>(new[] { 7200, 7000, 4400, 4400, 2000, 1000 });
                case 13:
                    return new List<int>(new[] { 9500, 8500, 5700, 5700, 2800, 1800 });
                case 14:
                    return new List<int>(new[] { 12000, 10000, 7000, 7000, 4000, 2000 });
                case 15:
                    return new List<int>(new[] { 14000, 12000, 8500, 8500, 5000, 2000 });
                case 16:
                    return new List<int>(new[] { 25000, 22000, 15000, 15000, 8000, 5000 });
                case 17:
                    return new List<int>(new[] { 36000, 33000, 22000, 22000, 11000, 6000 });
                case 18:
                    return new List<int>(new[] { 48000, 42000, 29000, 29000, 15000, 7000 });
                case 19:
                    return new List<int>(new[] { 60000, 52000, 35000, 35000, 18000, 10000 });
                case 20:
                    return new List<int>(new[] { 70000, 61000, 42000, 42000, 21000, 14000 });
                case 21:
                    return new List<int>(new[] { 125000, 112000, 75000, 75000, 38000, 25000 });
                case 22:
                    return new List<int>(new[] { 180000, 160000, 110000, 110000, 55000, 35000 });
                case 23:
                    return new List<int>(new[] { 240000, 210000, 140000, 140000, 70000, 50000 });
                case 24:
                    return new List<int>(new[] { 300000, 250000, 175000, 175000, 90000, 60000 });
                case 25:
                    return new List<int>(new[] { 350000, 320000, 200000, 200000, 100000, 80000 });
                case 26:
                    return new List<int>(new[] { 625000, 560000, 375000, 375000, 190000, 125000 });
                case 27:
                    return new List<int>(new[] { 900000, 800000, 550000, 550000, 280000, 170000 });
                case 28:
                    return new List<int>(new[] { 1200000, 1000000, 720000, 720000, 360000, 250000 });
                case 29:
                    return new List<int>(new[] { 1500000, 1300000, 875000, 875000, 450000, 250000 });
                case 30:
                    return new List<int>(new[] { 1750000, 1500000, 1000000, 1000000, 600000, 400000 });
            }

            return null;
        }

        private static List<string> create_from_gp(int gp)
        {
            var items = new List<string>();

            if (Session.Random.Next() % 4 == 0)
            {
                // Just coins
                items.Add(Coins(gp));
            }
            else
            {
                // Split into [gems, art objects, potions] and leftover coins
                var current = gp;
                while (current != 0)
                {
                    var value = get_value(current);
                    if (value == 0)
                        break;

                    // How many of these will fit?
                    var count = current / value;

                    // What sort of item is it?
                    var type = random_item_type(count != 1, true);

                    if (count == 1)
                    {
                        var start = TextHelper.StartsWithVowel(type) ? "an" : "a";
                        items.Add(start + " " + type + " (worth " + value + " GP)");
                    }
                    else
                    {
                        items.Add(count + " " + type + " (worth " + value + " GP each)");
                    }

                    current -= value * count;
                }

                if (current != 0)
                    // Add leftover as coins
                    items.Add(Coins(current));
            }

            for (var n = 0; n != items.Count; ++n) items[n] = TextHelper.Capitalise(items[n], false);

            return items;
        }

        private static int get_value(int total)
        {
            var candidates = new List<int>();

            foreach (var value in FValues)
            {
                var count = total / value;

                if (count >= 1 && count <= 10)
                    candidates.Add(value);
            }

            if (candidates.Count == 0)
                return 0;

            var index = Session.Random.Next() % candidates.Count;
            return candidates[index];
        }

        private static string random_item_type(bool plural, bool allowPotion)
        {
            var result = "";

            if (allowPotion)
                if (Session.Random.Next() % 4 == 0)
                {
                    result = "potion";

                    if (plural)
                        result += "s";

                    return result;
                }

            switch (Session.Random.Next() % 12)
            {
                case 0:
                case 1:
                case 2:
                    // Gemstone
                {
                    var index = Session.Random.Next() % FStones.Count;
                    var stone = FStones[index];

                    switch (Session.Random.Next() % 2)
                    {
                        case 0:
                            stone = stone + " gemstone";
                            break;
                        case 1:
                            stone = "piece of " + stone;
                            break;
                    }

                    switch (Session.Random.Next() % 12)
                    {
                        case 0:
                            stone = "well cut " + stone;
                            break;
                        case 1:
                            stone = "rough-cut " + stone;
                            break;
                        case 2:
                            stone = "poorly cut " + stone;
                            break;
                        case 3:
                            stone = "small " + stone;
                            break;
                        case 4:
                            stone = "large " + stone;
                            break;
                        case 5:
                            stone = "oddly shaped " + stone;
                            break;
                        case 6:
                            stone = "highly polished " + stone;
                            break;
                    }

                    result = stone;
                }
                    break;
                case 3:
                case 4:
                case 5:
                    // Object
                {
                    var index = Session.Random.Next() % FObjects.Count;
                    var artobject = FObjects[index];

                    var adjectives = new List<string>();
                    adjectives.Add("small");
                    adjectives.Add("large");
                    adjectives.Add("light");
                    adjectives.Add("heavy");
                    adjectives.Add("delicate");
                    adjectives.Add("fragile");
                    adjectives.Add("masterwork");
                    adjectives.Add("elegant");

                    var adjIndex = Session.Random.Next() % adjectives.Count;
                    var adjective = adjectives[adjIndex];

                    result = adjective + " " + artobject;
                }
                    break;
                case 6:
                case 7:
                case 8:
                    // Jewellery
                {
                    var itemIndex = Session.Random.Next() % FJewellery.Count;
                    var item = FJewellery[itemIndex];

                    var metalIndex = Session.Random.Next() % FMetals.Count;
                    var metal = FMetals[metalIndex];

                    result = metal + " " + item;

                    switch (Session.Random.Next(5))
                    {
                        case 0:
                            // Enamelled or laquered
                        {
                            var deco = Session.Random.Next(2) == 0 ? "enamelled" : "laquered";
                            result = deco + " " + result;
                        }
                            break;
                        case 1:
                            // Filigree or plating
                        {
                            metalIndex = Session.Random.Next() % FMetals.Count;
                            metal = FMetals[metalIndex];

                            var deco = Session.Random.Next(2) == 0 ? "plated" : "filigreed";
                            result = metal + "-" + deco + " " + result;
                        }
                            break;
                    }

                    switch (Session.Random.Next() % 10)
                    {
                        case 0:
                            result = "delicate " + result;
                            break;
                        case 1:
                            result = "intricate " + result;
                            break;
                        case 2:
                            result = "elegant " + result;
                            break;
                        case 3:
                            result = "simple " + result;
                            break;
                        case 4:
                            result = "plain " + result;
                            break;
                    }
                }
                    break;
                case 9:
                case 10:
                    // Artwork
                {
                    var artwork = "";
                    switch (Session.Random.Next(2))
                    {
                        case 0:
                            // Painting
                        {
                            artwork = "painting";

                            switch (Session.Random.Next(2))
                            {
                                case 0:
                                    artwork = "oil " + artwork;
                                    break;
                                case 1:
                                    artwork = "watercolour " + artwork;
                                    break;
                            }
                        }
                            break;
                        case 1:
                            // Drawing
                        {
                            artwork = "drawing";

                            switch (Session.Random.Next(2))
                            {
                                case 0:
                                    artwork = "pencil " + artwork;
                                    break;
                                case 1:
                                    artwork = "charcoal " + artwork;
                                    break;
                            }
                        }
                            break;
                    }

                    var adjectives = new List<string>();
                    adjectives.Add("small");
                    adjectives.Add("large");
                    adjectives.Add("delicate");
                    adjectives.Add("fragile");
                    adjectives.Add("elegant");
                    adjectives.Add("detailed");

                    var media = new List<string>();
                    media.Add("canvas");
                    media.Add("paper");
                    media.Add("parchment");
                    media.Add("wood panels");
                    media.Add("fabric");

                    var adjIndex = Session.Random.Next() % adjectives.Count;
                    var adjective = adjectives[adjIndex];

                    var mediumIndex = Session.Random.Next() % media.Count;
                    var medium = media[mediumIndex];

                    result = adjective + " " + artwork + " on " + medium;

                    // TODO: Subject
                }
                    break;
                case 11:
                    // Musical instrument
                {
                    var index = Session.Random.Next() % FInstruments.Count;
                    var artobject = FInstruments[index];

                    var adjectives = new List<string>();
                    adjectives.Add("small");
                    adjectives.Add("large");
                    adjectives.Add("light");
                    adjectives.Add("heavy");
                    adjectives.Add("delicate");
                    adjectives.Add("fragile");
                    adjectives.Add("masterwork");
                    adjectives.Add("elegant");

                    var adjIndex = Session.Random.Next() % adjectives.Count;
                    var adjective = adjectives[adjIndex];

                    result = adjective + " " + artobject;
                }
                    break;
            }

            if (plural)
                result += "s";

            switch (Session.Random.Next() % 5)
            {
                case 0:
                {
                    var sources = new List<string>();
                    sources.Add("feywild");
                    sources.Add("shadowfell");
                    sources.Add("elemental chaos");
                    sources.Add("astral plane");
                    sources.Add("abyss");
                    sources.Add("distant north");
                    sources.Add("distant east");
                    sources.Add("distant west");
                    sources.Add("distant south");

                    var sourceIndex = Session.Random.Next() % sources.Count;
                    var source = sources[sourceIndex];

                    result += " from the " + source;
                }
                    break;
                case 1:
                {
                    var gerunds = new List<string>();
                    gerunds.Add("decorated with");
                    gerunds.Add("inscribed with");
                    gerunds.Add("engraved with");
                    gerunds.Add("embossed with");
                    gerunds.Add("carved with");

                    var adjectives = new List<string>();
                    adjectives.Add("indecipherable");
                    adjectives.Add("ancient");
                    adjectives.Add("curious");
                    adjectives.Add("unusual");
                    adjectives.Add("dwarven");
                    adjectives.Add("eladrin");
                    adjectives.Add("elven");
                    adjectives.Add("draconic");
                    adjectives.Add("gith");

                    var designs = new List<string>();
                    designs.Add("script");
                    designs.Add("designs");
                    designs.Add("sigils");
                    designs.Add("runes");
                    designs.Add("glyphs");
                    designs.Add("patterns");

                    var gerundIndex = Session.Random.Next() % gerunds.Count;
                    var gerund = gerunds[gerundIndex];

                    var adjectiveIndex = Session.Random.Next() % adjectives.Count;
                    var adjective = adjectives[adjectiveIndex];

                    var designIndex = Session.Random.Next() % designs.Count;
                    var design = designs[designIndex];

                    result += " " + gerund + " " + adjective + " " + design;
                }
                    break;
                case 2:
                {
                    var gerunds = new List<string>();
                    gerunds.Add("glowing with");
                    gerunds.Add("suffused with");
                    gerunds.Add("infused with");
                    gerunds.Add("humming with");
                    gerunds.Add("pulsing with");

                    var sources = new List<string>();
                    sources.Add("arcane");
                    sources.Add("divine");
                    sources.Add("primal");
                    sources.Add("psionic");
                    sources.Add("shadow");
                    sources.Add("elemental");
                    sources.Add("unknown");

                    var powers = new List<string>();
                    powers.Add("energy");
                    powers.Add("power");
                    powers.Add("magic");

                    var gerundIndex = Session.Random.Next() % gerunds.Count;
                    var gerund = gerunds[gerundIndex];

                    var sourceIndex = Session.Random.Next() % sources.Count;
                    var source = sources[sourceIndex];

                    var powerIndex = Session.Random.Next() % powers.Count;
                    var power = powers[powerIndex];

                    result += " " + gerund + " " + source + " " + power;
                }
                    break;
                case 4:
                {
                    var gerunds = new List<string>();
                    gerunds.Add("set with");
                    gerunds.Add("inlaid with");
                    gerunds.Add("studded with");
                    gerunds.Add("with shards of");

                    var stoneIndex = Session.Random.Next() % FStones.Count;
                    var stone = FStones[stoneIndex];

                    if (Session.Random.Next() % 2 == 0)
                        stone += "s";
                    else
                        stone = "a single " + stone;

                    var gerundIndex = Session.Random.Next() % gerunds.Count;
                    var gerund = gerunds[gerundIndex];

                    result += " " + gerund + " " + stone;
                }
                    break;
            }

            return result;
        }

        private static string Coins(int gp)
        {
            // 1 AD = 10000 GP
            var ad = gp / 10000;
            var adRem = gp % 10000;
            if (ad > 0 && adRem == 0)
            {
                var str = "astral diamond";
                if (ad > 1)
                    str += "s";

                return ad + " " + str;
            }

            // 1 PP = 100 GP
            var pp = gp / 100;
            var ppRem = gp % 100;
            if (pp >= 100 && ppRem == 0)
                return pp + " PP";

            // 10 SP = 1 GP
            var sp = gp * 10;
            if (sp <= 100)
                return sp + " SP";

            return gp + " GP";
        }
    }
}
