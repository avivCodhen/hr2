﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HR.Searcher
{
    public static class TextExtension
    {
        public static string ReverseSearchText(this string text)
        {
            return Reverse(text);
        }

        public static bool IsHebrew(this string value)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(value, @"^[א-ת]+$");
        }

        public static string Reverse(this string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        public static IEnumerable<string> SplitFormat(this string text)
        {
            return text
                .Replace("\n", String.Empty)
                .Replace("\r", String.Empty)
                .Replace(",", String.Empty)
                .Replace(":", String.Empty)
                .Split(" ").Select(x => x.Trim()).ToList();
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
            for (var i = 0; i <= source1Length; matrix[i, 0] = i++){}
            for (var j = 0; j <= source2Length; matrix[0, j] = j++){}

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
        
        public static bool TextDistance(this IEnumerable<string> strs, string[] texts, int threshold)
        {
            foreach (var text in texts)
            {
                foreach (var str in strs)
                {
                    if (str.Distance(text) <= threshold) return true;
                }
            }

            return false;
        }
    }
}