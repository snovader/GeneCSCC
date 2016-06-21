using System;
using System.Collections.Generic;
using System.Linq;
using Prototype.CrossValidation;
using Prototype.Extensions;
using Prototype.GA;

namespace Prototype.Models
{
    internal sealed class TrainingModel<T> where T : AbstractContextInfo
    {
        private int _folds;

        public int Folds => _folds;

        private readonly List<Tuple<string, T>> _inputs;
        public List<Tuple<string, T>> Inputs => _inputs ?? new List<Tuple<string, T>>();

        private readonly List<ValidationInfo> _outputs;
        public List<ValidationInfo> Outputs => _outputs ?? new List<ValidationInfo>();

        private Dictionary<string, List<T>>[] _trainingFolds;

        private Tuple<List<TrainingInfo>, ValidationInfo[]>[] _validationFolds;

        public TrainingModel(Dictionary<string, List<T>> contexts)
        {
            _inputs = contexts.SelectMany(kvp => kvp.Value.Select(ci =>
                new Tuple<string, T>(kvp.Key, ci))).ToList();

            _outputs = contexts.SelectMany(kvp => kvp.Value.Select(ci =>
                new ValidationInfo(kvp.Key, ci.Invocation))).ToList();
        }

        public void InitializePartitions(int folds)
        {
            _folds = folds;

            _trainingFolds = new Dictionary<string, List<T>>[folds];
            _validationFolds = new Tuple<List<TrainingInfo>, ValidationInfo[]>[folds];

            var indices = Prototype.CrossValidation.CrossValidation.Partition(_inputs.Count, folds);

            for (int i = 0; i < _folds; i++)
            {
                var trainingIndices = indices.Where((foldIndices, index) => i != index).SelectMany(index => index).ToArray();

                var trainingInputs = _inputs.SubArray(trainingIndices);
                _trainingFolds[i] = new Dictionary<string, List<T>>();

                foreach (var trainingInput in trainingInputs)
                {
                    _trainingFolds[i].ChainedAdd(trainingInput.Item1, trainingInput.Item2);
                }

                var validationInputs = _inputs.SubArray(indices[i]);
                var validationFold = new List<TrainingInfo>();

                foreach (var validationInput in validationInputs)
                {
                    validationFold.Add(new TrainingInfo(validationInput.Item1, validationInput.Item2));
                }

                _validationFolds[i] = new Tuple<List<TrainingInfo>, ValidationInfo[]>(validationFold, _outputs.SubArray(indices[i]));
            }
        }

        public CrossValidationResult Evaluate(Chromosome chromosome)
        {
            var similarityThreshold = chromosome.SimilarityThreshold;
            var refinementThreshold = chromosome.RefinementThreshold;
            var maximumRefinedCandidates = chromosome.RefinedCandidates;

            var crossvalidation = new KFoldCrossValidation(0, _folds);

            crossvalidation.Evaluation = delegate (int k, int[] indicesTrain, int[] indicesValidation)
            {
                var trainingModel = _trainingFolds[k];

                var validationInputs = _validationFolds[k].Item1;
                var validationOutputs = _validationFolds[k].Item2;

                var predictionMatches = 0.0;
                var predictionsReturned = 0;

                for (var i = 0; i < validationInputs.Count; i++)
                {
                    var baseCandidates = trainingModel.TryGet(validationInputs[i].Type);

                    // Hamming distance closeness
                    var similarities = new List<PredictionInfo>();
                    foreach (var baseCandidate in baseCandidates)
                    {
                        var extendedSimilarity = baseCandidate.ExtendedSimilarity(validationInputs[i].ContextInfo);
                        var localSimilarity = baseCandidate.ExtendedSimilarity(validationInputs[i].ContextInfo);
                        var similarity = localSimilarity / extendedSimilarity > similarityThreshold ? localSimilarity : extendedSimilarity;

                        similarities.Add(new PredictionInfo(baseCandidate, similarity));
                    }

                    // Sort by Hamming distance
                    similarities.Sort();

                    // Take k = 200 most similar candidate contexts
                    var refinedCandidates = similarities.Take(maximumRefinedCandidates).ToList();

                    for (var j = 0; j < refinedCandidates.Count; j++)
                    {
                        refinedCandidates[j].ExtendedSimilarity = refinedCandidates[j].Context.NormalizedLCS(validationInputs[i].ContextInfo);
                        refinedCandidates[j].LocalSimilarity = refinedCandidates[j].Context.LevenshteinSimilarity(validationInputs[i].ContextInfo);

                        if (refinedCandidates[j].ExtendedSimilarity < refinementThreshold
                            && refinedCandidates[j].LocalSimilarity < refinementThreshold)
                        {
                            refinedCandidates.RemoveAt(j);
                        }
                    }

                    refinedCandidates.Sort();

                    if (refinedCandidates.Count == 0)
                    {
                        continue;
                    }

                    if (validationOutputs[i].Validate(refinedCandidates[0].Context.Invocation) == ValidationInfo.Result.Match)
                    {
                        predictionMatches++;
                    }
                }

                // TODO: Not all validationInputs may be processed
                return new PredictionQualityValues(predictionMatches / validationInputs.Count, 1.0);
            };

            var predictionQualities = crossvalidation.Compute();

            return predictionQualities;
        }
    }
}