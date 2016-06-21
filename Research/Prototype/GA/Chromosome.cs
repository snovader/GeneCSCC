using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;
using Prototype.CrossValidation;

namespace Prototype.GA
{
    internal class Chromosome : ChromosomeBase
    {
        public float SimilarityThreshold => (float) GetGene(0).Value;
        public float RefinementThreshold => (float) GetGene(1).Value;
        public int RefinedCandidates => (int) GetGene(2).Value;

        public CrossValidationResult PredictionQuality;

        public Chromosome() : base(3)
        {
            ReplaceGene(0, new Gene((float) RandomizationProvider.Current.GetDouble()));
            ReplaceGene(1, new Gene((float) RandomizationProvider.Current.GetDouble()));
            ReplaceGene(2, new Gene(RandomizationProvider.Current.GetInt(0, 2000)));
        }

        public Chromosome(float similarityThreshold, float refinementThreshold, int refinedCandidates) : base(3)
        {
            ReplaceGene(0, new Gene(similarityThreshold));
            ReplaceGene(1, new Gene(refinementThreshold));
            ReplaceGene(2, new Gene(refinedCandidates));
        }

        public override Gene GenerateGene(int geneIndex)
        {
            Gene gene;

            switch (geneIndex)
            {
                case 0: gene = new Gene((float) RandomizationProvider.Current.GetDouble());
                    break;
                case 1: gene = new Gene((float) RandomizationProvider.Current.GetDouble());
                    break;
                case 2: gene = new Gene(RandomizationProvider.Current.GetInt(0, 2000));
                    break;
            }

            return gene;
        }

        public override IChromosome CreateNew()
        {
            return new Chromosome();
        }

        public override IChromosome Clone()
        {
            return base.Clone() as Chromosome;
        }
    }
}