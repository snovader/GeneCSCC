using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Prototype.CSCC;
using Prototype.Models;

namespace Prototype
{
    internal class SizeEvaluation
    {
        public static void CsccSize()
        {
            var modelDirectory = Model.GetModelDirectory();
            var modelFiles = Directory.GetFiles(modelDirectory, "*_cscc").Where(file => !file.Equals("training_cscc"));

            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<CsccContextInfo>.Load(modelFile).Contexts;

                long extendedLength = 0;
                long localLength = 0;
                long invocationLength = 0;
                long length = 0;

                foreach (var contexts in model.Values)
                {
                    foreach (var context in contexts)
                    {
                        length++;
                        extendedLength += context.ExtendedContext.Length;
                        localLength += context.LocalContext.Length;
                        invocationLength += context.Invocation.Length;
                    }
                }

                Console.WriteLine(modelFile + " " + extendedLength + " " + " " + localLength + " " + " " + invocationLength + " " + length);
            }

            Console.ReadKey();
        }

        public static void GenCsccSize()
        {
            var modelDirectory = Model.GetModelDirectory();

            var modelFiles = Directory.GetFiles(modelDirectory).Where(file => !file.EndsWith("_cscc") && !file.Equals("training"));
            foreach (var modelFile in modelFiles)
            {
                var model = ContextModel<ContextInfo>.Load(modelFile).Contexts;

                long extendedLength = 0;
                long localLength = 0;
                long invocationLength = 0;
                long length = 0;

                foreach (var contexts in model.Values)
                {
                    foreach (var context in new HashSet<ContextInfo>(contexts))
                    {
                        length++;
                        extendedLength += context.ExtendedContext.Length;
                        localLength += context.LocalContext.Length;
                        invocationLength += context.Invocation.Length;
                    }
                }

                Console.WriteLine(modelFile + " " + extendedLength + " " + " " + localLength + " " + " " + invocationLength + " " + length);
            }

            Console.ReadKey();
        }
    }
}