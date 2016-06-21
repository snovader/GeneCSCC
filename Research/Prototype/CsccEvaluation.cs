using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prototype.CSCC;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype
{
    internal class CsccEvaluation
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

        private static Dictionary<string, List<CsccContextInfo>> CreateModel()
        {
            var combinedModel = new Dictionary<string, List<CsccContextInfo>>();

            var modelDirectory = Model.GetModelDirectory();
            var modelFiles = Directory.GetFiles(modelDirectory, "*_cscc");

            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<CsccContextInfo>.Load(modelFile);

                foreach (var type in namespaces)
                {
                    List<CsccContextInfo> contexts = combinedModel.TryGet(type);

                    contexts.AddRange(model.GetContextsForType(type));
                    combinedModel[type] = contexts;
                }
            }

            foreach (var type in namespaces)
            {
                combinedModel[type] = new HashSet<CsccContextInfo>(combinedModel[type]).ToList();
            }

            var savePath = Path.Combine(modelDirectory, "base_cscc");
            new ContextModel<CsccContextInfo>(combinedModel).Save(savePath);

            return combinedModel;
        }

        private static Dictionary<string, List<CsccContextInfo>> LoadModel()
        {
            Dictionary<string, List<CsccContextInfo>> model;

            var modelDirectory = Model.GetModelDirectory();
            var savePath = Path.Combine(modelDirectory, "base_cscc");

            if (File.Exists(savePath))
            {
                model = ContextModel<CsccContextInfo>.Load(savePath).Contexts;
            }
            else
            {
                model = CreateModel();
            }

            return model;
        }

        private static void CrossValidate()
        {
            var evaluationModel = new CsccEvaluationModel(LoadModel());
            var result = evaluationModel.Evaluate();
            Console.WriteLine(result);
            Console.ReadKey();
        }
    }
}
