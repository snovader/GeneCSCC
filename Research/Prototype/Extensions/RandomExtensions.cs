using System;

namespace Prototype.Extensions
{
    internal static class RandomExtensions
    {
        public static double NextDouble(this Random random, int minValue, int maxValue)
        {
            return minValue + maxValue * random.NextDouble();
        }
    }
}
