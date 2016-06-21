using Prototype.Util;

namespace Prototype.Extensions
{
    internal static class StringExtensions
    {
        public static int GetSimHash(this string str)
        {
            return SimHash.CalculateSimHash(str);
        }

        public static string AppendWithWhitespace<T>(this string str, T append)
        {
            return str.Equals(string.Empty) || str.EndsWith(" ") ? str + append : $"{str} {append}";
        }
    }
}
