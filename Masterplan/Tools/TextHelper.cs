using System;
using System.Collections.Generic;

namespace Masterplan.Tools
{
    internal class TextHelper
    {
        private static readonly int LineLength = 50;

        private static List<char> _fVowels;

        public static string Wrap(string str)
        {
            var lines = new List<string>();

            while (str != "")
            {
                var line = get_first_line(ref str);
                lines.Add(line);
            }

            var wrapped = "";
            foreach (var line in lines)
            {
                if (wrapped != "")
                    wrapped += Environment.NewLine;

                wrapped += line;
            }

            return wrapped;
        }

        private static string get_first_line(ref string str)
        {
            var line = "";

            var length = Math.Min(LineLength, str.Length);
            var index = str.IndexOf(" ", length);
            if (index == -1)
            {
                line = str;
                str = "";
            }
            else
            {
                line = str.Substring(0, index);
                str = str.Substring(index + 1);
            }

            return line;
        }

        public static string Abbreviation(string title)
        {
            var abbrev = "";
            foreach (var token in title.Split(null))
            {
                if (token == "")
                    continue;

                var isNumber = false;
                try
                {
                    isNumber = true;
                }
                catch
                {
                    isNumber = false;
                }

                if (isNumber)
                {
                    abbrev += token;
                    continue;
                }

                var first = token[0];

                if (char.IsUpper(first))
                    abbrev += first;
            }

            return abbrev;
        }

        public static bool IsVowel(char ch)
        {
            if (_fVowels == null)
            {
                _fVowels = new List<char>();

                _fVowels.Add('a');
                _fVowels.Add('e');
                _fVowels.Add('i');
                _fVowels.Add('o');
                _fVowels.Add('u');
            }

            return _fVowels.Contains(ch);
        }

        public static bool StartsWithVowel(string str)
        {
            if (str.Length == 0)
                return false;

            var first = char.ToLower(str[0]);
            return IsVowel(first);
        }

        public static string Capitalise(string str, bool titleCase)
        {
            if (titleCase)
            {
                var tokens = str.Split(null);

                str = "";
                foreach (var token in tokens)
                {
                    if (str != "")
                        str += " ";

                    str += Capitalise(token, false);
                }

                return str;
            }

            var first = str[0];

            return char.ToUpper(first) + str.Substring(1);
        }
    }
}
