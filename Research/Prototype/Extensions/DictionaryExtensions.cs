using System.Collections.Generic;

namespace Prototype.Extensions
{
    internal static class DictionaryExtensions
    {
        public static void ChainedAdd<TK, TV>(this Dictionary<TK, List<TV>> dict, TK key, TV value)
        {
            List<TV> values;

            if (!dict.TryGetValue(key, out values))
            {
                values = new List<TV>();
                dict.Add(key, values);
            }

            values.Add(value);
        }

        public static TV TryGet<TK, TV>(this Dictionary<TK, TV> dict, TK key) where TV : new()
        {
            TV value;

            if (!dict.TryGetValue(key, out value))
            {
                value = new TV();
            }

            return value;
        }
    }
}
