using System.Threading;
using Prototype.Extensions;
using Prototype.Util;

namespace Prototype.CrossValidation
{
    internal static class CrossValidation
    {
        public static void Shuffle<T>(this T[] array)
        {
            var random = RandomProvider.GetThreadRandom();

            for (int i = array.Length; i > 1; i--)
            {
                var j = random.Next(i);

                var aux = array[j];
                array[j] = array[i - 1];
                array[i - 1] = aux;
            }
        }

        public static int[][] Partition(int size, int folds)
        {
            var indices = new int[size];
            for (int i = 0; i < size; i++)
            {
                indices[i] = i;
            }
            indices.Shuffle();

            var foldSize = size / folds;
            var partitions = new int[folds][];
            for (int i = 0; i < folds; i++)
            {
                partitions[i] = indices.SubArray(i * foldSize, foldSize);
            }

            return partitions;
        }

        public static void Add(ref double location, double value)
        {
            var newCurrentValue = Volatile.Read(ref location);
            while (true)
            {
                var currentValue = newCurrentValue;
                var newValue = currentValue + value;
                newCurrentValue = Interlocked.CompareExchange(ref location, newValue, currentValue);
                if (newCurrentValue == currentValue)
                {
                    return;
                }
            }
        }
    }
}
