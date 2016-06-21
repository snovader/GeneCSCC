using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Prototype.CSCC;
using Prototype.Extensions;
using Prototype.Models;
using Prototype.Util;

namespace Prototype
{
    internal class TimeEvaluation
    {
        public static void Evaluate()
        {

            Console.WriteLine("Evaluating CSCC...");
            EvaluateCscc();

            Console.WriteLine("Evaluating GeneCSCC...");
            EvaluateGeneCscc();

            Console.ReadKey();
        }

        public static void EvaluateCscc()
        {
            var model = ContextModel<CsccContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training_cscc"));

            var cscc = new CSCC.CSCC(model);

            var rand = new Random();
            var unorderedQueryData = model.Contexts.SelectMany(kvp => kvp.Value.Select(c => new Tuple<string, CsccContextInfo>(kvp.Key, c))).ToList();
            unorderedQueryData.Shuffle(RandomProvider.GetThreadRandom());

            var queryData = unorderedQueryData.Take(3000);

            foreach (var query in queryData)
            {
                cscc.GetPredictions(query.Item2, query.Item1);
            }

            var sw = new Stopwatch();
            sw.Start();

            foreach (var query in queryData)
            {
                cscc.GetPredictions(query.Item2, query.Item1);
            }

            sw.Stop();

            Console.WriteLine(string.Format("Queries: {0} Inference speed: {1}",
                model.Contexts.Sum(kvp => kvp.Value.Count), (double) sw.Elapsed.Milliseconds / 3000));
        }

        public static void EvaluateGeneCscc()
        {
            var model = ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training"));

            var cscc = new GeneCSCC.GeneCSCC(model);

            var rand = new Random();
            var unorderedQueryData = model.Contexts.SelectMany(kvp => kvp.Value.Select(c => new Tuple<string, ContextInfo>(kvp.Key, c))).ToList();
            unorderedQueryData.Shuffle(RandomProvider.GetThreadRandom());

            var queryData = unorderedQueryData.Take(3000);

            var sw = new Stopwatch();
            sw.Start();

            foreach (var query in queryData)
            {
                cscc.GetPredictions(query.Item2, query.Item1);
            }

            sw.Stop();

            Console.WriteLine(
                $"Queries: {model.Contexts.Sum(kvp => kvp.Value.Count)} Inference speed: {(double) sw.Elapsed.Milliseconds/3000}");
        }
    }
}