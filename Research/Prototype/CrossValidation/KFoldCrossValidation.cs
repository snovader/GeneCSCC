using System;
using System.Linq;
using System.Threading.Tasks;

namespace Prototype.CrossValidation
{
    internal class KFoldCrossValidation
    {
        private readonly int[][] _folds;

        private int _samples;

        public readonly int K;

        public readonly bool RunInParallel;

        public delegate PredictionQualityValues CrossValidationFittingFunction(int k, int[] trainingSamples, int[] validationSamples);

        public CrossValidationFittingFunction Evaluation;

        public KFoldCrossValidation(int size, int folds = 10, bool runInParallel = true)
        {
            _samples = size;
            K = folds;
            _folds = Prototype.CrossValidation.CrossValidation.Partition(size, folds);
            RunInParallel = runInParallel;
        }

        private void CreatePartitions(int validationFoldIndex, out int[] trainingSet, out int[] validationSet)
        {
            var indices = _folds.Where((foldIndices, i) => validationFoldIndex != i).ToList();
            trainingSet = indices.SelectMany(foldIndices => foldIndices).ToArray();

            validationSet = _folds[validationFoldIndex];
        }

        public CrossValidationResult Compute()
        {
            if (Evaluation == null)
            {
                throw new InvalidOperationException("Evaluation function must have been previously defined.");
            }

            var results = new PredictionQualityValues[K];

            if (RunInParallel)
            {
                Parallel.For(0, K, i =>
                {
                    int[] trainingSet, validationSet;
                    CreatePartitions(i, out trainingSet, out validationSet);

                    results[i] = Evaluation(i, trainingSet, validationSet);
                });
            }
            else
            {
                for (int i = 0; i < K; i++)
                {
                    int[] trainingSet, validationSet;
                    CreatePartitions(i, out trainingSet, out validationSet);

                    results[i] = Evaluation(i, trainingSet, validationSet);
                }
            }

            return new CrossValidationResult(results);
        }
    }
}