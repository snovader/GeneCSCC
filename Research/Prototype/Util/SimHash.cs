using System.Collections.Generic;
using System.Linq;

namespace Prototype.Util
{
    public class SimHash
    {
        private const int HashSize = 32;
        private const float HashLength = HashSize;

        public static float GetSimilarity(int needleSimHash, int hayStackSimHash)
        {
            return (HashSize - GetHammingDistance(needleSimHash, hayStackSimHash)) / HashLength;
        }

        public static float GetSimilarity(string needle, int hayStackSimHash)
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

        private static IEnumerable<int> HashTokens(IEnumerable<string> tokens)
        {
            return tokens.Select(token => token.GetHashCode()).ToList();
        }

        private static int GetHammingDistance(int firstValue, int secondValue)
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

        private static bool IsBitSet(int b, int pos)
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
    }
}