using System;
using System.Collections.Generic;
using System.Linq;
using Prototype.CrossValidation;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype.CSCC
{
    internal sealed class CsccEvaluationModel : ContextModel<CsccContextInfo>
    {
        private int _folds;

        public int Folds => _folds;

        private readonly List<Tuple<string, CsccContextInfo>> _inputs;
        public List<Tuple<string, CsccContextInfo>> Inputs => _inputs ?? new List<Tuple<string, CsccContextInfo>>();

        private readonly List<ValidationInfo> _outputs;
        public List<ValidationInfo> Outputs => _outputs ?? new List<ValidationInfo>();

        public CsccEvaluationModel(Dictionary<string, List<CsccContextInfo>> contexts) : base(contexts)
        {
            _inputs = Contexts.SelectMany(kvp => kvp.Value.Select(ci =>
                new Tuple<string, CsccContextInfo>(kvp.Key, ci))).ToList();

            _outputs = Contexts.SelectMany(kvp => kvp.Value.Select(ci =>
                new ValidationInfo(kvp.Key, ci.Invocation))).ToList();
        }

        public CrossValidationResult Evaluate()
        {
            var crossvalidation = new KFoldCrossValidation(_inputs.Count, 10);

            crossvalidation.Evaluation = delegate (int k, int[] indicesTrain, int[] indicesValidation)
            {
                var trainingInputs = _inputs.SubArray(indicesTrain);
                var trainingModel = new Dictionary<string, List<CsccContextInfo>>();
                foreach (var trainingInput in trainingInputs)
                {
                    trainingModel.ChainedAdd(trainingInput.Item1, trainingInput.Item2);
                }

                var validationInputs = _inputs.SubArray(indicesValidation);
                var validationOutputs = _outputs.SubArray(indicesValidation);

                var cscc = new CSCC(new ContextModel<CsccContextInfo>(trainingModel));

                var top1Matches = 0.0;
                var top3Matches = 0.0;
                var predictionsMade = 0.0;

                /*var sw = new Stopwatch();
                sw.Start();*/

                for (var i = 0; i < validationInputs.Length; i++)
                {
                    var predictions = cscc.GetPredictions(validationInputs[i].Item2, validationInputs[i].Item1);

                    if (predictions.Count == 0)
                    {
                        continue;
                    }

                    predictionsMade++;

                    if (validationOutputs[i].Validate(predictions[0]) == ValidationInfo.Result.Match)
                    {
                        top1Matches++;
                        top3Matches++;
                    }
                    else if (predictions.Count > 1 && validationOutputs[i].Validate(predictions[1]) == ValidationInfo.Result.Match)
                    {
                        top3Matches++;
                    }
                    else if (predictions.Count > 2 && validationOutputs[i].Validate(predictions[2]) == ValidationInfo.Result.Match)
                    {
                        top3Matches++;
                    }
                }
                //sw.Stop();
                //Console.WriteLine("Elapsed time: " + sw.ElapsedMilliseconds / validationInputs.Length);

                return new PredictionQualityValues(top3Matches / validationInputs.Length, predictionsMade / validationInputs.Length);
            };

            var predictionQualities = crossvalidation.Compute();

            return predictionQualities;
        }
    }
}