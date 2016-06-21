using System.Linq;

namespace Prototype.CrossValidation
{
    class CrossValidationResult
    {
        private PredictionQualityValues[] _results;

        public double AveragePrecision => _results.Sum(i => i.Precision) / _results.Length;
        public double AverageRecall => _results.Sum(i => i.Recall) / _results.Length;
        public double AverageFmeasure => _results.Sum(i => i.FMeasure) / _results.Length;

        public CrossValidationResult(PredictionQualityValues[] results)
        {
            _results = results;
        }

        public override string ToString()
        {
            return string.Format("Precision: {0} Recall: {1} FMeasure: {2}", AveragePrecision, AverageRecall, AverageFmeasure);
        }
    }
}