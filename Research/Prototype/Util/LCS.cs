using System;

namespace Prototype.Util
{
    internal static class LCS
    {
        public static int Compute(string[] seq1, string[] seq2)
        {
            var lcs = new int[seq1.Length + 1, seq2.Length + 1];

            for (int i = 1; i <= seq1.Length; i++)
            {
                for (int j = 1; j <= seq2.Length; j++)
                {
                    if (seq1[i - 1].Equals(seq2[j - 1]))
                    {
                        lcs[i, j] = lcs[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        lcs[i, j] = Math.Max(lcs[i - 1, j], lcs[i, j - 1]);
                    }
                }
            }

            return lcs[seq1.Length, seq2.Length];
        }
    }
}
