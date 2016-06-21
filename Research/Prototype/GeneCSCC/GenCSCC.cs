using System.Collections.Generic;
using System.Linq;
using Prototype.Models;

namespace Prototype.GeneCSCC
{
    public class GeneCSCC
    {
        private static int _linesOfContext = 4;

        private static int _maximumRefinedCandidates = 326;

        private static int _maximumPredictions = 3;

        private readonly ContextModel<ContextInfo> _model;

        // TODO: Load correct model here
        public GeneCSCC(ContextModel<ContextInfo> model)
        {
            _model = model;
        }

        public List<string> GetPredictions(ContextInfo currentContext, string type)
        {
            var refinedCandidates = GetRefinedCandidates(GetBaseCandidates(type), currentContext);

            var predictions = new List<string>();
            foreach (var refinedCandidate in refinedCandidates)
            {
                if (!predictions.Contains(refinedCandidate.Context.Invocation))
                {
                    predictions.Add(refinedCandidate.Context.Invocation);
                }

                if (predictions.Count == _maximumPredictions)
                {
                    break;
                }
            }

            return predictions;
        }

        private IEnumerable<ContextInfo> GetBaseCandidates(string type)
        {
            return _model.GetContextsForType(type);
        }

        private static IEnumerable<PredictionInfo> GetRefinedCandidates(IEnumerable<ContextInfo> baseCandidates,
            ContextInfo currentContext)
        {
            // Hamming distance closeness
            var similarities = new List<PredictionInfo>();
            foreach (var baseCandidate in baseCandidates)
            {
                similarities.Add(new PredictionInfo(baseCandidate, baseCandidate.Similarity(currentContext)));
            }

            // Sort by Hamming distance
            similarities.Sort();

            // Take k = 200 most similar candidate contexts
            var refinedCandidates = similarities.Take(_maximumRefinedCandidates).ToList();

            for (var i = 0; i < refinedCandidates.Count; i++)
            {
                refinedCandidates[i].ExtendedSimilarity = refinedCandidates[i].Context.NormalizedLCS(currentContext);
                refinedCandidates[i].LocalSimilarity = refinedCandidates[i].Context.LevenshteinSimilarity(currentContext);
                if (!refinedCandidates[i].IsSimilar())
                {
                    refinedCandidates.RemoveAt(i);
                }
            }

            refinedCandidates.Sort();

            return refinedCandidates;
        }
    }
}