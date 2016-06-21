using System;
using Prototype.Extensions;
using Prototype.Util;

namespace Prototype.Models
{
    [Serializable]
    public abstract class AbstractContextInfo
    {
        public string ExtendedContext;
        public string LocalContext;
        public string Invocation;

        protected int _extendedContext;
        protected int _localContext;

        protected AbstractContextInfo(string extendedContext, string localContext, string invocation)
        {
            //extendedContext = string.Join(" ", new HashSet<string>(extendedContext.Split(' ')));
            //localContext = string.Join(" ", new HashSet<string>(localContext.Split(' ')));

            ExtendedContext = extendedContext;
            LocalContext = localContext;
            Invocation = invocation;

            _extendedContext = extendedContext.GetSimHash();
            _localContext = localContext.GetSimHash();
        }

        public float ExtendedSimilarity(AbstractContextInfo context)
        {
            return Sim.GetSimilarity(_extendedContext, context._extendedContext);
        }

        public float LocalSimilarity(AbstractContextInfo context)
        {
            return Sim.GetSimilarity(_localContext, context._localContext);
        }

        public float NormalizedLCS(AbstractContextInfo context)
        {
            var left = ExtendedContext.Split(' ');
            var right = LocalContext.Split(' ');

            var lcs = LCS.Compute(left, right);
            return 2.0f * lcs / (left.Length + right.Length);
        }

        public float LevenshteinSimilarity(AbstractContextInfo context)
        {
            var left = ExtendedContext.Split(' ');
            var right = LocalContext.Split(' ');

            var distance = Levenshtein.Compute(left, right);
            return 1.0f - distance / Math.Max(left.Length, right.Length);
        }

        public override bool Equals(object obj)
        {
            var that = obj as AbstractContextInfo;
            if (that == null)
            {
                return false;
            }

            return ExtendedContext.Equals(that.ExtendedContext)
                   && LocalContext.Equals(that.LocalContext)
                   && Invocation.Equals(that.Invocation);
        }

        public override int GetHashCode()
        {
            var hash = 17;

            unchecked
            {
                hash = hash * 31 + ExtendedContext.GetHashCode();
                hash = hash * 31 + LocalContext.GetHashCode();
                hash = hash * 31 + Invocation.GetHashCode();
            }

            return hash;
        }
    }
}