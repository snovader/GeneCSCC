using System;

namespace Prototype.Util
{
    internal static class Levenshtein
    {
        public static int Compute(string[] seq1, string[] seq2)
        {
            var distance = new int[seq1.Length + 1, seq2.Length + 1];

            for (int i = 0; i <= seq1.Length; i++)
            {
                distance[i, 0] = i;
            }

            for (int i = 0; i <= seq2.Length; i++)
            {
                distance[0, i] = i;
            }

            for (int i = 1; i <= seq1.Length; i++)
            {
                for (int j = 1; j <= seq2.Length; j++)
                {
                    if (seq1[i - 1].Equals(seq2[j - 1]))
                    {
                        distance[i, j] = distance[i - 1, j - 1];
                    }
                    else
                    {
                        distance[i, j] = Math.Min(Math.Min(distance[i - 1, j], distance[i, j - 1]), distance[i - 1, j - 1]) + 1;
                    }
                }
            }

            return distance[seq1.Length, seq2.Length];
        }
    }
}
