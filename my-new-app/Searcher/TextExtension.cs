using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HR.Searcher
{
    public static class TextExtension
    {
        public static bool IsHebrew(this string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value.Replace(" ", ""), @"^[א-ת]+$");
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static IEnumerable<string> SplitFormat(this string text)
        {
            return text.Replace
                    ("\r", string.Empty)
                .Split("\n")
                .Select(x => x.Replace(",", " ").Replace(":", " "))
                .SelectMany(x => x.Split(" "))
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public static int Distance(this string source1, string source2)
        {
            source1 = source1.ToLower();
            source2 = source2.ToLower();
            var source1Length = source1.Length;
            var source2Length = source2.Length;

            var matrix = new int[source1Length + 1, source2Length + 1];

            // First calculation, if one entry is empty return full length
            if (source1Length == 0)
                return source2Length;

            if (source2Length == 0)
                return source1Length;

            // Initialization of matrix with row size source1Length and columns size source2Length
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++)
            {
            }

            for (var j = 0; j <= source2Length; matrix[0, j] = j++)
            {
            }

            // Calculate rows and collumns distances
            for (var i = 1; i <= source1Length; i++)
            {
                for (var j = 1; j <= source2Length; j++)
                {
                    var cost = (source2[j - 1] == source1[i - 1]) ? 0 : 1;

                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            // return result
            return matrix[source1Length, source2Length];
        }

        public static string ReverseText(this string text)
        {
            var s = string.Join(" ", text).Split(" ").Select(x => new string(x.IsHebrew() ? x.Reverse() : x)).ToList();
            return string.Join(" ", s);
        }

        public static bool HebrewSexContained(this string value, string text)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var words = value.Split(" ");
            var restOfWords = string.Join(" ", words.Skip(1));
            var hebrewSex = new[] {"ה", "ות", "ים", "ת"}.Select(s =>
                words[0] + s + (restOfWords.Length > 0 ? " " + restOfWords : ""));
            return hebrewSex.Any(x => text.Contains(x));
        }

        public static bool HebrewConnectionContained(this string value, string text)
        {
            if (string.IsNullOrEmpty(value)) return false;
            var words = value.Split(" ");
            var restOfWords = string.Join(" ", words.Skip(1));
            var hebrewConnection = new[] {"ו", "ב", "כ", "ל"}.Select(s =>
                s + words[0] + (restOfWords.Length > 0 ? " " + restOfWords : ""));
            return hebrewConnection.Any(x => text.Contains(x));
        }
    }
}