namespace Prototype.CrossValidation
{
    internal class PredictionQualityValues
    {
        public readonly double Precision;
        public readonly double Recall;
        public double FMeasure => 2 * Precision * Recall / (Precision + Recall);

        public double AverageTime;

        public PredictionQualityValues(double precision, double recall, double averageTime = 0.0) : base()
        {
            Precision = precision;
            Recall = recall;
            AverageTime = averageTime;
        }
    }
}