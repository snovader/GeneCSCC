using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using Prototype.GA;
using Prototype.Models;

namespace Prototype
{
    internal class GeneticAlgorithmTraining
    {
        private Dictionary<string, List<ContextInfo>>[] trainingModels;

        private int _folds;

        public GeneticAlgorithmTraining(int folds = 10)
        {
            _folds = folds;
        }

        public void InitializeTrainingModels()
        {
            var trainingModel = ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training"));
            var namespaces = trainingModel.Contexts.ToList();
            namespaces.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));

            trainingModels = new Dictionary<string, List<ContextInfo>>[_folds];

            for (int i = 0; i < _folds; i++)
            {
                trainingModels[i] = new Dictionary<string, List<ContextInfo>>();
            }

            for (int i = 0; i < namespaces.Count / 2; i++)
            {
                trainingModels[i % _folds].Add(namespaces[i].Key, namespaces[i].Value);
                trainingModels[i % _folds].Add(namespaces[namespaces.Count - i - 1].Key, namespaces[namespaces.Count - i - 1].Value);
            }
        }

        public void Train()
        {
            IChromosome previousChromosome = new Chromosome(1.0f, 0.3f, 200);
            IChromosome currentBest = new Chromosome(1.0f, 0.3f, 200);
            previousChromosome.Fitness = 0.0;
            currentBest.Fitness = 0.0;

            for (int i = 0; i < trainingModels.Length; i++)
            {
                var bestChromosome = Compute(new TrainingModel<ContextInfo>(trainingModels[i]), currentBest);
                currentBest = bestChromosome.Fitness > currentBest.Fitness ? bestChromosome : currentBest;

                if (i == (_folds - 1) && currentBest.Fitness > previousChromosome.Fitness)
                {
                    previousChromosome = currentBest;
                    i = 0;
                }
            }

            Console.ReadKey();
        }

        public IChromosome Compute(TrainingModel<ContextInfo> model, IChromosome baseChromosome)
        {
            model.InitializePartitions(_folds);

            var sw = new Stopwatch();
            sw.Start();

            var selection = new EliteSelection();
            var crossover = new TwoPointCrossover(); // Or OnePoint
            var mutation = new UniformMutation(true);
            var fitness = new Fitness(model);
            var chromosome = baseChromosome == null ? new Chromosome() : baseChromosome;
            var population = new Population(2, 5, chromosome);

            var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
            {
                Termination = new OrTermination(
                    new GenerationNumberTermination(5),
                    new FitnessStagnationTermination(),
                    new FitnessThresholdTermination(baseChromosome.Fitness.HasValue ? baseChromosome.Fitness.Value : 0.0))
            };

            Console.WriteLine("GA running...");
            ga.Start();

            sw.Stop();
            Console.WriteLine("Best solution found has {0} fitness. Time elapsed {1}", ga.BestChromosome.Fitness, sw.Elapsed);
            foreach (var gene in ga.BestChromosome.GetGenes())
            {
                Console.WriteLine(gene);
            }

            return ga.BestChromosome;
        }
    }
}