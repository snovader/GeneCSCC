using System;

namespace Prototype.Models
{
    internal class PredictionInfo : IComparable<PredictionInfo>
    {
        public static float LowerThreshold = 0.3f;

        public AbstractContextInfo Context;
        public float ExtendedSimilarity;
        public float LocalSimilarity;

        public PredictionInfo(AbstractContextInfo context, float extendedSimilarity = 0.0f, float localSimilarity = 0.0f)
        {
            Context = context;
            ExtendedSimilarity = extendedSimilarity;
            LocalSimilarity = localSimilarity;
        }

        public bool IsSimilar()
        {
            return ExtendedSimilarity >= LowerThreshold || LocalSimilarity >= LowerThreshold;
        }

        public int CompareTo(PredictionInfo that)
        {
            var extendedSimilarity = ExtendedSimilarity.CompareTo(that.ExtendedSimilarity);
            return -1 * (extendedSimilarity != 0 ? extendedSimilarity : LocalSimilarity.CompareTo(that.LocalSimilarity));
        }
    }
}