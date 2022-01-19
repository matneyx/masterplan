using System.Collections.Generic;
using System.Text;

namespace Masterplan.Tools
{
    /// <summary>
    ///     Class containing string manipulation methods.
    /// </summary>
    public class StringHelper
    {
        /// <summary>
        ///     Finds the longest common substring from each token of each string.
        /// </summary>
        /// <param name="str1">The first string.</param>
        /// <param name="str2">The second string.</param>
        /// <returns>Returns the longest common token subtring.</returns>
        public static string LongestCommonToken(string str1, string str2)
        {
            var tokens1 = str1.Split(null);
            var tokens2 = str2.Split(null);

            var list = new List<string>();
            foreach (var token1 in tokens1)
            foreach (var token2 in tokens2)
            {
                var str = LongestCommonSubstring(token1, token2);
                if (str != "")
                    list.Add(str);
            }

            var longest = "";
            foreach (var str in list)
                if (str.Length > longest.Length)
                    longest = str;

            return longest;
        }

        /// <summary>
        ///     Finds the longest common substring.
        /// </summary>
        /// <param name="str1">The first string.</param>
        /// <param name="str2">The second string.</param>
        /// <returns>Returns the longest common substring.</returns>
        public static string LongestCommonSubstring(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return "";

            var num = new int[str1.Length, str2.Length];

            var maxLength = 0;
            var lastStartPos = 0;

            var builder = new StringBuilder();

            for (var i = 0; i < str1.Length; i++)
            for (var j = 0; j < str2.Length; j++)
                if (str1[i] != str2[j])
                {
                    num[i, j] = 0;
                }
                else
                {
                    if (i == 0 || j == 0)
                        num[i, j] = 1;
                    else
                        num[i, j] = 1 + num[i - 1, j - 1];

                    if (num[i, j] > maxLength)
                    {
                        maxLength = num[i, j];
                        var thisStartPos = i - num[i, j] + 1;
                        if (lastStartPos == thisStartPos)
                        {
                            builder.Append(str1[i]);
                        }
                        else
                        {
                            lastStartPos = thisStartPos;
                            builder.Remove(0, builder.Length);
                            builder.Append(str1.Substring(lastStartPos, i + 1 - lastStartPos));
                        }
                    }
                }

            return builder.ToString();
        }
    }
}
