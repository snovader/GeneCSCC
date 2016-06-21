using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Prototype.Extensions;

namespace Prototype.Models
{
    // TODO Have this class implement caching?
    [Serializable]
    public class ContextModel<T> where T : AbstractContextInfo
    {
        internal Dictionary<string, List<T>> Contexts { get; }

        // TODO: Eliminate ducplicates. Compare difference.
        public ContextModel(Dictionary<string, List<T>> contexts)
        {
            Contexts = contexts;
        }

        /// <summary>
        /// Currently does not indicate whether the type exists or not.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<T> GetContextsForType(string type)
        {
            List<T> invocations;

            if (!Contexts.TryGetValue(type, out invocations))
            {
                invocations = new List<T>();
            }

            return invocations;
        }

        public void KeepTypes(List<string> keysToInclude)
        {
            var keysToRemove = Contexts.Keys.Except(keysToInclude).ToList();

            foreach (var keyToRemove in keysToRemove)
            {
                Contexts.Remove(keyToRemove);
            }
        }

        public List<string> GetAllTypes()
        {
            return Contexts.Keys.ToList();
        }

        public List<T> GetAllContexts()
        {
            return Contexts.Values.SelectMany(tuples => tuples).ToList();
        }

        public void RemoveDuplicates()
        {
            foreach (var type in Contexts.Keys.ToList())
            {
                Contexts[type] = new HashSet<T>(Contexts[type]).ToList();
            }
        }

        public static ContextModel<T> Combine(params ContextModel<T>[] models)
        {
            var combinedModel = new Dictionary<string, List<T>>();
            foreach (var model in models)
            {
                foreach (var type in model.Contexts.Keys)
                {
                    var contexts = combinedModel.TryGet(type);

                    contexts.AddRange(model.Contexts[type]);
                    combinedModel[type] = contexts;
                }
            }

            return new ContextModel<T>(combinedModel);
        }

        public void Save(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            {
                new BinaryFormatter().Serialize(fs, this);
            }
        }

        public static ContextModel<T> Load(string path)
        {
            ContextModel<T> cfModel;

            using (var fs = new FileStream(path, FileMode.Open))
            {
                cfModel = new BinaryFormatter().Deserialize(fs) as ContextModel<T>;
            }

            return cfModel;
        }
    }
}