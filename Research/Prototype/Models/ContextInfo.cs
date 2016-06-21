using System;

namespace Prototype.Models
{
    [Serializable]
    public sealed class ContextInfo : AbstractContextInfo
    {
        public static float LowerThreshold = 0.77F;
        public static float SimilarityThreshold = 0.65F;

        public ContextInfo(string extendedContext, string localContext, string invocation)
            : base(extendedContext, localContext, invocation)
        {
        }

        public float Similarity(ContextInfo context)
        {
            var extendedSimilarity = ExtendedSimilarity(context);
            var localSimilarity = LocalSimilarity(context);

            return localSimilarity >= SimilarityThreshold * extendedSimilarity ? localSimilarity : extendedSimilarity;
        }

        public bool IsSimilar(ContextInfo context)
        {
            return LocalSimilarity(context) >= LowerThreshold
                || ExtendedSimilarity(context) >= LowerThreshold;
        }
    }
}