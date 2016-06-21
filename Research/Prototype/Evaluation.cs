using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prototype.Models;

namespace Prototype
{
    internal class Evaluation
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

        public static void CreateModel()
        {
            var combinedModel = new Dictionary<string, List<ContextInfo>>();

            var modelDirectory = Model.GetModelDirectory();
            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc"));

            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile);

                foreach (var type in namespaces)
                {
                    List<ContextInfo> contexts;

                    if (!combinedModel.TryGetValue(type, out contexts))
                    {
                        contexts = new List<ContextInfo>();
                    }

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
        }
    }
}