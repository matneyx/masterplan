using System.Collections.Generic;

namespace Masterplan.Tools.Generators
{
    internal class HalflingName
    {
        public static string MaleName()
        {
            return Name(true);
        }

        public static string FemaleName()
        {
            return Name(false);
        }

        private static string Name(bool male)
        {
            var firstName = "";
            var lastName = "";

            switch (Session.Random.Next(20))
            {
                case 0:
                case 1:
                case 2:
                    firstName = Simple(true);
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    firstName = Simple(true) + Simple(false);
                    break;
                case 7:
                case 8:
                case 9:
                case 10:
                    firstName = Simple(true);
                    lastName = Simple(true) + Simple(false);
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    firstName = Simple(true) + Simple(false);
                    lastName = Simple(true);
                    break;
                case 16:
                case 17:
                case 18:
                case 19:
                    firstName = Simple(true) + Simple(false);
                    lastName = Earned(true) + Earned(false);
                    break;
            }

            if (!male)
            {
                // Feminise the first name
                var last = firstName[firstName.Length - 1];
                if (!TextHelper.IsVowel(last))
                {
                    firstName += last;
                    firstName += "a";
                }
            }

            var name = firstName;
            if (lastName != "")
                name += " " + lastName;

            return TextHelper.Capitalise(name, true);
        }

        private static string Simple(bool start)
        {
            var values = new List<string>();
            values.Add("arv");
            values.Add("baris");
            values.Add("brand");
            values.Add("bren");
            values.Add("cal");
            values.Add("chen");
            values.Add("cyrr");
            values.Add("dair");
            values.Add("dal");
            values.Add("deree");
            values.Add("dric");
            values.Add("essel");
            values.Add("fur");
            values.Add("galan");
            values.Add("gen");
            values.Add("gren");
            values.Add("ien");
            values.Add("illi");
            values.Add("indy");
            values.Add("iss");
            values.Add("kal");
            values.Add("kep");
            values.Add("kin");
            values.Add("li");
            values.Add("lur");
            values.Add("mel");
            values.Add("opee");
            values.Add("ped");
            values.Add("pery");
            values.Add("penel");
            values.Add("reen");
            values.Add("rill");
            values.Add("royl");
            values.Add("sheel");
            values.Add("thea");
            values.Add("ur");
            values.Add("wort");
            values.Add("yon");

            if (!start)
            {
                values.Add("eere");
                values.Add("llalee");
            }

            var index = Session.Random.Next(values.Count);
            return values[index];
        }

        private static string Earned(bool first)
        {
            var values = new List<string>();

            if (first)
            {
                values.Add("laughing");
                values.Add("fast");
                values.Add("happy");
                values.Add("kind");
                values.Add("nimble");
                values.Add("little");
                values.Add("proud");
                values.Add("quick");
                values.Add("sly");
                values.Add("small");
                values.Add("smooth");
                values.Add("snug");
                values.Add("stout");
                values.Add("sweet");
                values.Add("swift");
                values.Add("warm");
                values.Add("wild");
                values.Add("young");
                values.Add("under");
            }
            else
            {
                values.Add("caller");
                values.Add("dancer");
                values.Add("strider");
                values.Add("weaver");
                values.Add("wanderer");
            }

            values.Add("badger");
            values.Add("burrow");
            values.Add("home");
            values.Add("rascal");
            values.Add("riddle");
            values.Add("bottom");
            values.Add("cloak");
            values.Add("earth");
            values.Add("eye");
            values.Add("fellow");
            values.Add("flower");
            values.Add("finger");
            values.Add("foot");
            values.Add("glen");
            values.Add("glitter");
            values.Add("gold");
            values.Add("hand");
            values.Add("heart");
            values.Add("hearth");
            values.Add("hill");
            values.Add("hollow");
            values.Add("leaf");
            values.Add("light");
            values.Add("love");
            values.Add("meadow");
            values.Add("moon");
            values.Add("reed");
            values.Add("silver");
            values.Add("skin");
            values.Add("sun");
            values.Add("thistle");
            values.Add("will");
            values.Add("whisper");

            var index = Session.Random.Next(values.Count);
            return values[index];
        }
    }
}
