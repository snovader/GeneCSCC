using System;
using Prototype.Models;

namespace Prototype.CSCC
{
    [Serializable]
    public sealed class CsccContextInfo : AbstractContextInfo
    {
        public static float LowerThreshold = 0.3F;

        public CsccContextInfo(string extendedContext, string localContext, string invocation)
            : base(extendedContext, localContext, invocation)
        {
        }

        public float Similarity(CsccContextInfo context)
        {
            return Math.Max(LocalSimilarity(context), ExtendedSimilarity(context));
        }

        public bool IsSimilar(CsccContextInfo context)
        {
            return LocalSimilarity(context) >= LowerThreshold
                || ExtendedSimilarity(context) >= LowerThreshold;
        }
    }
}