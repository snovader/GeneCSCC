using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Prototype.CSCC;
using Prototype.DataMiners;
using Prototype.Extensions;
using Prototype.Models;

namespace Prototype
{
    class Program
    {
        static void Main(string[] args)
        {
            //var model = GetStatistics("C:\\Users\\M\\Desktop\\CSharp_Source\\NLog-master\\NLog-master") as CachedTrainingModel;
            //GeneticAlgorithmTraining.Compute(model);

            //ModelSizeEvaluation(@"C:\Users\M\Desktop\CSharp_Source\CSharp_Source");

            //GetUsageContexts(@"C:\Users\M\Desktop\CSharp_Source\CSharp_Source\Source");

            //CombineAllModels();

            //Test();

            //CalculateCsccModels(@"C:\Users\M\Desktop\CSharp_Source\CSharp_Source");

            //TestCombined();

            //Size();

            /*var modelDirectory = Model.GetModelDirectory();
            var baseFile = Path.Combine(modelDirectory, "base");
            var model = ContextModel<ContextInfo>.Load(baseFile);
            GeneticAlgorithmTraining.Compute(new TrainingModel<ContextInfo>(model.Contexts));*/

            //CsccEvaluation.Evaluate();

            //GenCsccEvaluation.Evaluate();

            //new ContextModel<ContextInfo>(Combine()).Save(Path.Combine(Model.GetModelDirectory(), "training"));

            //new ContextModel<CsccContextInfo>(CombineCscc()).Save(Path.Combine(Model.GetModelDirectory(), "training_cscc"));

            //Evaluation();

            /*var ga = new GeneticAlgorithmTraining();
            ga.InitializeTrainingModels();
            ga.Train();*/

            //Eval();

            //Model.EvalCscc();

            //Model.EvalGeneCscc();

            //CombineAllModels();

            //SizeEvaluation.GenCsccSize();

            //Test();

            //TestGene();

            //PerformanceEvaluation();

            TimeEvaluation.Evaluate();
        }

        private static void PerformanceEvaluation()
        {
            var modelDirectory = Model.GetModelDirectory();
            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc") && !file.Equals("training"));

            var keys = ContextModel<ContextInfo>.Load(Path.Combine(modelDirectory, "training")).GetAllTypes();

            var models = new List<ContextModel<ContextInfo>>();
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile);
                model.KeepTypes(keys);
                model.RemoveDuplicates();
                models.Add(model);
            }

            Console.WriteLine("Models loaded...");

            var precision = 0.0;
            var recall = 0.0;

            for (int i = 0; i < models.Count; i++)
            {
                var trainingFolds = models.Where((foldIndices, foldIndex) => foldIndex != i).ToArray();
                var trainingModel = ContextModel<ContextInfo>.Combine(trainingFolds);
                trainingModel.RemoveDuplicates();

                Console.WriteLine("Training model created...");

                var validationFold = models[i].Contexts.SelectMany(kvp => kvp.Value.Select(ci => new Tuple<string, ContextInfo>(kvp.Key, ci))).Take(1000);

                var cscc = new GeneCSCC.GeneCSCC(trainingModel);

                var validationHits = 0.0;
                var recallHits = 0.0;
                var validations = 0;

                foreach (var validation in validationFold)
                {
                    if (!trainingModel.Contexts.ContainsKey(validation.Item1))
                    {
                        continue;
                    }

                    var predictions = cscc.GetPredictions(validation.Item2, validation.Item1);

                    validations++;

                    if (predictions.Count == 0)
                    {
                        continue;
                    }

                    recallHits++;

                    if (validation.Item2.Invocation.Equals(predictions[0]))
                    {
                        validationHits++;
                    }
                }

                precision += validationHits / validations;
                recall += recallHits / validations;

                Console.WriteLine(validationHits / validations);
            }

            Console.WriteLine("Precision: {0} Recall: {1}", precision / models.Count, recall / models.Count);
            Console.ReadKey();
        }

        private static void Eval()
        {
            Dictionary<string, List<ContextInfo>>[] trainingModels;

            var trainingModel = ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training"));
            var namespaces = trainingModel.Contexts.ToList();
            namespaces.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));

            trainingModels = new Dictionary<string, List<ContextInfo>>[10];

            for (int i = 0; i < 10; i++)
            {
                trainingModels[i] = new Dictionary<string, List<ContextInfo>>();
            }

            for (int i = 0; i < namespaces.Count / 2; i++)
            {
                trainingModels[i % 10].Add(namespaces[i].Key, namespaces[i].Value);
                trainingModels[i % 10].Add(namespaces[namespaces.Count - i - 1].Key, namespaces[namespaces.Count - i - 1].Value);
            }

            var result = 0.0;
            foreach (var training in trainingModels)
            {
                //result += new EvaluationModel(training).Evaluate();
            }

            Console.WriteLine(result / 10);
            Console.ReadKey();
        }

        private static void Evaluation()
        {
            Dictionary<string, List<CsccContextInfo>>[] trainingModels;

            var trainingModel = ContextModel<CsccContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training_cscc"));
            var namespaces = trainingModel.Contexts.ToList();
            namespaces.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));

            trainingModels = new Dictionary<string, List<CsccContextInfo>>[10];

            for (int i = 0; i < 10; i++)
            {
                trainingModels[i] = new Dictionary<string, List<CsccContextInfo>>();
            }

            for (int i = 0; i < namespaces.Count / 2; i++)
            {
                trainingModels[i % 10].Add(namespaces[i].Key, namespaces[i].Value);
                trainingModels[i % 10].Add(namespaces[namespaces.Count - i - 1].Key, namespaces[namespaces.Count - i - 1].Value);
            }

            var result = 0.0;
            foreach (var training in trainingModels)
            {
                result += new CsccEvaluationModel(training).Evaluate().AverageFmeasure;
                Console.WriteLine(result);
            }

            Console.WriteLine(result / 10);
            Console.ReadKey();
        }

        private static Dictionary<string, List<CsccContextInfo>> CombineCscc()
        {
            var modelDirectory = Model.GetModelDirectory();

            var combinedModel = new Dictionary<string, List<CsccContextInfo>>();

            var modelFiles = Directory.GetFiles(modelDirectory, "*_cscc").Where(file => !file.Equals("base_cscc"));
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<CsccContextInfo>.Load(modelFile).Contexts;
                foreach (var type in model.Keys)
                {
                    List<CsccContextInfo> contexts = combinedModel.TryGet(type);

                    contexts.AddRange(model[type]);
                    combinedModel[type] = contexts;
                }
            }

            /*foreach (var type in combinedModel.Keys.ToList())
            {
                combinedModel[type] = new HashSet<CsccContextInfo>(combinedModel[type]).ToList();
            }*/

            var flatModel = combinedModel.ToList();
            flatModel.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));
            var sortedTop = flatModel.Take(100);

            return sortedTop.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static void TopNamespaces()
        {
            foreach (var kvp in ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "training")).Contexts)
            {
                Console.WriteLine(kvp.Key + " " + kvp.Value.Count);
            }
            Console.ReadKey();
        }

        /*private static void TestCV()
        {
            var model = new TrainingModel<CsccContextInfo>(GetContexts(@"C:\Users\M\Desktop\CSharp_Source\CSharp_Source\roslyn-master"));
            var trainingModel = new EvaluationModel(model.Contexts);
            Console.WriteLine(trainingModel.Evaluate());
            Console.ReadKey();
        }*/

        private static void Test()
        {
            var training = ContextModel<CsccContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "Source_cscc"));
            var model = new CSCC.CSCC(training);

            var validation = ContextModel<CsccContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "NewtonsoftJson-master_cscc")).Contexts;
            var list = validation.SelectMany(kvp => kvp.Value.Select(ci => new Tuple<string, CsccContextInfo>(kvp.Key, ci))).ToList();

            var validationError = 0.0;
            var validations = 0;

            for (var i = 0; i < validation.Count; i++)
            {
                if (!training.Contexts.ContainsKey(list[i].Item1))
                {
                    continue;
                }

                var predictions = model.GetPredictions(list[i].Item2, list[i].Item1);

                validations++;

                if (predictions.Count == 0)
                {
                    continue;
                }

                if (list[i].Item2.Invocation.Equals(predictions[0]))
                {
                    validationError++;
                }
            }

            Console.WriteLine(validationError / validations);
            Console.ReadKey();
        }

        private static void TestGene()
        {
            var training = ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "Source"));
            var model = new GeneCSCC.GeneCSCC(training);

            var validation = ContextModel<ContextInfo>.Load(Path.Combine(Model.GetModelDirectory(), "NewtonsoftJson-master")).Contexts;
            var list = validation.SelectMany(kvp => kvp.Value.Select(ci => new Tuple<string, ContextInfo>(kvp.Key, ci))).ToList();

            var validationError = 0.0;
            var validations = 0;

            for (var i = 0; i < validation.Count; i++)
            {
                if (!training.Contexts.ContainsKey(list[i].Item1))
                {
                    continue;
                }

                var predictions = model.GetPredictions(list[i].Item2, list[i].Item1);

                validations++;

                if (predictions.Count == 0)
                {
                    continue;
                }

                if (list[i].Item2.Invocation.Equals(predictions[0]))
                {
                    validationError++;
                }
            }

            Console.WriteLine(validationError / validations);
            Console.ReadKey();
        }

        /*private static void TestCombined()
        {
            var model = new TrainingModel<CsccContextInfo>(Combine());
            var trainingModel = new EvaluationModel(model.Contexts);
            Console.WriteLine(trainingModel.Evaluate());
            Console.ReadKey();
        }*/

        private static Dictionary<string, List<ContextInfo>> Combine()
        {
            var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (directoryInfo == null)
            {
                return null;
            }

            var applicationDirectory = directoryInfo.FullName;
            var modelDirectory = Path.Combine(applicationDirectory, "ModelStorage");

            var combinedModel = new Dictionary<string, List<ContextInfo>>();

            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc") && !file.Equals("base"));
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile).Contexts;
                foreach (var type in model.Keys)
                {
                    List<ContextInfo> contexts = combinedModel.TryGet(type);

                    contexts.AddRange(model[type]);
                    combinedModel[type] = contexts;
                }
            }

            foreach (var type in combinedModel.Keys.ToList())
            {
                combinedModel[type] = new HashSet<ContextInfo>(combinedModel[type]).ToList();
            }

            var flatModel = combinedModel.ToList();
            flatModel.Sort((a, b) => b.Value.Count.CompareTo(a.Value.Count));
            var sortedTop = flatModel.Take(100);

            return sortedTop.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        private static void CombineAllModels()
        {
            var modelDirectory = Model.GetModelDirectory();

            var combinedModel = new Dictionary<string, HashSet<ContextInfo>>();

            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc") && !file.Equals("training"));
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile).Contexts;
                foreach (var type in model.Keys)
                {
                    HashSet<ContextInfo> contexts;

                    if (!combinedModel.TryGetValue(type, out contexts))
                    {
                        contexts = new HashSet<ContextInfo>();
                    }

                    contexts.UnionWith(model[type]);
                    combinedModel[type] = contexts;
                }
            }

            Console.WriteLine(combinedModel.Sum(x => x.Value.Count));

            Console.ReadKey();
        }

        private static void ModelSizeEvaluation(string path)
        {
            var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (directoryInfo == null)
            {
                return;
            }

            var applicationDirectory = directoryInfo.FullName;
            var modelDirectory = Path.Combine(applicationDirectory, "ModelStorage");

            var directories = Directory.GetDirectories(path);

            var sw = new Stopwatch();

            foreach (var directory in directories)
            {
                sw.Start();
                var contexts = GetUsageContexts(directory);
                sw.Stop();
                Console.WriteLine($"{directory} {sw.Elapsed}");
                sw.Reset();

                new ContextModel<ContextInfo>(contexts).Save(Path.Combine(modelDirectory, directory.Split('\\').Last()));

                Console.WriteLine(contexts.Sum(x => x.Value.Count));
                Console.WriteLine(contexts.Sum(x => new HashSet<ContextInfo>(x.Value).Count));
            }

            Console.ReadKey();
        }

        private static void CalculateCsccModels(string path)
        {
            var directoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (directoryInfo == null)
            {
                return;
            }

            var applicationDirectory = directoryInfo.FullName;
            var modelDirectory = Path.Combine(applicationDirectory, "ModelStorage");

            var directories = Directory.GetDirectories(path);

            var sw = new Stopwatch();

            foreach (var directory in directories)
            {
                sw.Start();
                var contexts = GetContexts(directory);
                sw.Stop();
                Console.WriteLine($"{directory} {sw.Elapsed}");
                sw.Reset();

                new ContextModel<CsccContextInfo>(contexts).Save(Path.Combine(modelDirectory, directory.Split('\\').Last() + "_cscc"));
            }

            Console.ReadKey();
        }

        private static Dictionary<string, List<ContextInfo>> GetUsageContexts(string directory)
        {
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            var contexts = new Dictionary<string, List<ContextInfo>>();

            foreach (var file in files)
            {
                //Console.WriteLine(file);
                var code = File.ReadAllText(file, Encoding.UTF8);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("Test")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(syntaxTree);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var walker = new ContextWalker<ContextInfo>(semanticModel, contexts);
                walker.Visit(syntaxTree.GetRoot());

                contexts = walker.Contexts;
            }

            return contexts;
        }

        private static Dictionary<string, List<CsccContextInfo>> GetContexts(string directory)
        {
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
            var contexts = new Dictionary<string, List<CsccContextInfo>>();

            foreach (var file in files)
            {
                //Console.WriteLine(file);
                var code = File.ReadAllText(file, Encoding.UTF8);

                var syntaxTree = CSharpSyntaxTree.ParseText(code);
                var compilation = CSharpCompilation.Create("Test")
                    .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                    .AddSyntaxTrees(syntaxTree);
                var semanticModel = compilation.GetSemanticModel(syntaxTree);

                var walker = new ContextWalker<CsccContextInfo>(semanticModel, contexts);
                walker.Visit(syntaxTree.GetRoot());

                contexts = walker.Contexts;
            }

            return contexts;
        }
    }
}