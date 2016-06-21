using System;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using Prototype.Models;

namespace Prototype.GA
{
    internal class Fitness : IFitness
    {
        private TrainingModel<ContextInfo> _model;
        //public PredictionQualityResult CrossValidationResult;

        public Fitness(TrainingModel<ContextInfo> model)
        {
            _model = model;
        }

        public double Evaluate(IChromosome chromosome)
        {
            var c = chromosome as Chromosome;
            c.PredictionQuality = _model.Evaluate(c);
            Console.WriteLine("{0} {1} {2} {3}", c.PredictionQuality.AveragePrecision, c.SimilarityThreshold, c.RefinementThreshold, c.RefinedCandidates);

            return c.PredictionQuality.AverageFmeasure;
        }
    }
}