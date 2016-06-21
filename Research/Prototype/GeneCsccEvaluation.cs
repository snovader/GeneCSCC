using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype
{
    internal class GeneCsccEvaluation
    {
        private static string[] namespaces =
        {
            "System",
            "System.Collections",
            "System.Collections.Generic",
            "System.Diagnostics",
            "System.Globalization",
            "System.IO",
            "System.Security",
            "System.Security.Permissions",
            "System.Text",
            "System.Threading"
        };

        public static void Evaluate()
        {
            CrossValidate();
        }

        private static Dictionary<string, List<ContextInfo>> CreateModel()
        {
            var combinedModel = new Dictionary<string, List<ContextInfo>>();

            var modelDirectory = Model.GetModelDirectory();
            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc"));

            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile);

                foreach (var type in namespaces)
                {
                    List<ContextInfo> contexts = combinedModel.TryGet(type);

                    contexts.AddRange(model.GetContextsForType(type));
                    combinedModel[type] = contexts;
                }
            }

            foreach (var type in namespaces)
            {
                combinedModel[type] = new HashSet<ContextInfo>(combinedModel[type]).ToList();
            }

            var savePath = Path.Combine(modelDirectory, "base");
            new ContextModel<ContextInfo>(combinedModel).Save(savePath);

            return combinedModel;
        }

        private static Dictionary<string, List<ContextInfo>> LoadModel()
        {
            Dictionary<string, List<ContextInfo>> model;

            var modelDirectory = Model.GetModelDirectory();
            var savePath = Path.Combine(modelDirectory, "base");

            if (File.Exists(savePath))
            {
                model = ContextModel<ContextInfo>.Load(savePath).Contexts;
            }
            else
            {
                model = CreateModel();
            }

            return model;
        }

        private static void CrossValidate()
        {
            var evaluationModel = new EvaluationModel(LoadModel());
            var result = evaluationModel.Evaluate();
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}