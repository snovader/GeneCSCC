using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prototype.CSCC;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype
{
    internal class Model
    {
        public static string GetModelDirectory()
        {
            var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (directoryInfo == null)
            {
                return string.Empty;
            }

            var applicationDirectory = directoryInfo.FullName;
            var modelDirectory = Path.Combine(applicationDirectory, "ModelStorage");

            return modelDirectory;
        }

        public static Dictionary<string, List<CsccContextInfo>>[] MostCommon(Dictionary<string, List<CsccContextInfo>> usageContexts)
        {
            var flatModel = usageContexts.ToList();
            flatModel.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));
            var contexts = flatModel.Take(100).ToList();

            var models = new Dictionary<string, List<CsccContextInfo>>[10];

            for (int i = 0; i < 10; i++)
            {
                models[i] = new Dictionary<string, List<CsccContextInfo>>();
            }

            for (int i = 0; i < 50; i++)
            {
                models[i % 10].Add(contexts[i].Key, contexts[i].Value);
                models[i % 10].Add(contexts[contexts.Count - i - 1].Key, contexts[contexts.Count - i - 1].Value);
            }

            return models;
        }

        public static Dictionary<string, List<CsccContextInfo>> CombineCscc()
        {
            var modelDirectory = GetModelDirectory();

            var combinedModel = new Dictionary<string, List<CsccContextInfo>>();

            var modelFiles = Directory.GetFiles(modelDirectory, "*_cscc").Where(file => !file.Equals("base_cscc") && !file.Equals("training_cscc"));
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<CsccContextInfo>.Load(modelFile).Contexts;
                foreach (var type in model.Keys)
                {
                    var contexts = combinedModel.TryGet(type);

                    contexts.AddRange(new HashSet<CsccContextInfo>(model[type]));
                    combinedModel[type] = contexts;
                }
            }

            return combinedModel;
        }

        public static void EvalCscc()
        {
            //var usageContexts = CombineCscc();
            //var models = MostCommon(usageContexts);
            var models = MostCommon(LoadCsccTrainingModel());

            foreach (var model in models)
            {
                var evaluationModel = new CsccEvaluationModel(model);
                Console.WriteLine(evaluationModel.Evaluate());
            }
            Console.ReadKey();
        }

        public static Dictionary<string, List<CsccContextInfo>> LoadCsccTrainingModel()
        {
            return ContextModel<CsccContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training_cscc")).Contexts;
        }

        public static Dictionary<string, List<ContextInfo>>[] MostCommonGene(Dictionary<string, List<ContextInfo>> usageContexts)
        {
            var flatModel = usageContexts.ToList();
            flatModel.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));
            var contexts = flatModel.Take(100).ToList();

            var models = new Dictionary<string, List<ContextInfo>>[10];

            for (int i = 0; i < 10; i++)
            {
                models[i] = new Dictionary<string, List<ContextInfo>>();
            }

            for (int i = 0; i < 50; i++)
            {
                models[i % 10].Add(contexts[i].Key, contexts[i].Value);
                models[i % 10].Add(contexts[contexts.Count - i - 1].Key, contexts[contexts.Count - i - 1].Value);
            }

            return models;
        }

        public static void EvalGeneCscc()
        {
            var models = MostCommonGene(LoadGeneCsccTrainingModel());

            foreach (var model in models)
            {
                var evaluationModel = new EvaluationModel(model);
                Console.WriteLine(evaluationModel.Evaluate());
            }
            Console.ReadKey();
        }

        public static Dictionary<string, List<ContextInfo>> LoadGeneCsccTrainingModel()
        {
            return ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training")).Contexts;
        }
    }
}