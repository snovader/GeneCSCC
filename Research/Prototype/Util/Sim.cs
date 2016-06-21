using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Prototype.Util
{
    public class Sim
    {
        private const int HashSize = 64;
        private const float HashLength = HashSize;

        public static float GetSimilarity(long needleSimHash, long hayStackSimHash)
        {
            return (HashSize - GetHammingDistance(needleSimHash, hayStackSimHash)) / HashLength;
        }

        public static float GetSimilarity(string needle, long hayStackSimHash)
        {
            var needleSimHash = CalculateSimHash(needle);
            return (HashSize - GetHammingDistance(needleSimHash, hayStackSimHash)) / HashLength;
        }

        public static float GetSimilarity(string needle, string haystack)
        {
            var needleSimHash = CalculateSimHash(needle);
            var hayStackSimHash = CalculateSimHash(haystack);
            return (HashSize - GetHammingDistance(needleSimHash, hayStackSimHash)) / HashLength;
        }

        private static IEnumerable<long> HashTokens(IEnumerable<string> tokens)
        {
            return tokens.Select(token => GetHashCodeInt64(token)).ToList();
        }

        private static int GetHammingDistance(long firstValue, long secondValue)
        {
            var hammingBits = firstValue ^ secondValue;
            var hammingValue = 0;
            for (int i = 0; i < 32; i++)
            {
                if (IsBitSet(hammingBits, i))
                {
                    hammingValue += 1;
                }
            }
            return hammingValue;
        }

        private static bool IsBitSet(long b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        public static int CalculateSimHash(string input)
        {
            var tokeniser = new Shingle();
            var hashedtokens = HashTokens(tokeniser.Tokenise(input));
            var vector = new int[HashSize];
            for (var i = 0; i < HashSize; i++)
            {
                vector[i] = 0;
            }

            foreach (var value in hashedtokens)
            {
                for (var j = 0; j < HashSize; j++)
                {
                    if (IsBitSet(value, j))
                    {
                        vector[j] += 1;
                    }
                    else
                    {
                        vector[j] -= 1;
                    }
                }
            }

            var fingerprint = 0;
            for (var i = 0; i < HashSize; i++)
            {
                if (vector[i] > 0)
                {
                    fingerprint += 1 << i;
                }
            }
            return fingerprint;
        }

        /*public static long GetHashCodeInt64(string input)
        {
            var s1 = input.Substring(0, input.Length / 2);
            var s2 = input.Substring(input.Length / 2);

            var x = ((long) s1.GetHashCode()) << 0x20 | s2.GetHashCode();

            return x;
        }*/

        public static long GetHashCodeInt64(string input)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] hash = md5.ComputeHash(Encoding.ASCII.GetBytes(input));
            return BitConverter.ToInt64(hash, 0);
        }
    }
}