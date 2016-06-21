using System;
using System.Collections.Generic;
using Prototype.Models;

namespace Prototype.Extensions
{
    internal static class ListExtensions
    {
        public static Dictionary<string, List<TrainingInfo>> SubDict(this List<TrainingInfo> data, int[] indices)
        {
            var result = new Dictionary<string, List<TrainingInfo>>();
            for (int i = 0; i < indices.Length; i++)
            {
                result.ChainedAdd(data[indices[i]].Type, data[indices[i]].Copy(i));
            }
            return result;
        }

        public static T[] SubArray<T>(this IList<T> data, int[] indices)
        {
            var result = new T[indices.Length];
            for (int i = 0; i < indices.Length; i++)
            {
                result[i] = data[indices[i]];
            }
            return result;
        }

        public static void Shuffle<T>(this IList<T> data, Random rnd)
        {
            for (var i = 0; i < data.Count; i++)
            {
                var j = rnd.Next(i, data.Count);
                var temp = data[i];
                data[i] = data[j];
                data[j] = data[i];
            }
        }
    }
}
