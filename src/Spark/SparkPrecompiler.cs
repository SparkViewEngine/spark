using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Spark.Compiler;
using Spark.Descriptors;

namespace Spark
{
    public class SparkPrecompiler(Spark.ISparkViewEngine engine, IDescriptorBuilder descriptorBuilder) : ISparkPrecompiler
    {
        public Assembly Precompile(SparkBatchDescriptor batch)
        {
            var descriptors = CreateDescriptors(batch);

            return engine.BatchCompilation(batch.OutputAssembly, descriptors);
        }

        private static bool TestMatch(string potentialMatch, string pattern)
        {
            if (!pattern.EndsWith("*"))
            {
                return string.Equals(potentialMatch, pattern, StringComparison.InvariantCultureIgnoreCase);
            }

            // raw wildcard matches anything that's not a partial
            if (pattern == "*")
            {
                return !potentialMatch.StartsWith("_");
            }

            // otherwise the only thing that's supported is "starts with"
            return potentialMatch.StartsWith(pattern.Substring(0, pattern.Length - 1), StringComparison.InvariantCultureIgnoreCase);
        }

        private static string RemoveSuffix(string value, string suffix)
        {
            return value.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase) 
                ? value.Substring(0, value.Length - suffix.Length) 
                : value;
        }

        public List<SparkViewDescriptor> CreateDescriptors(SparkBatchDescriptor batch)
        {
            var descriptors = new List<SparkViewDescriptor>();
            
            foreach (var entry in batch.Entries)
            {
                descriptors.AddRange(CreateDescriptors(entry));
            }

            return descriptors;
        }

        public IList<SparkViewDescriptor> CreateDescriptors(SparkBatchEntry entry)
        {
            var descriptors = new List<SparkViewDescriptor>();

            string controllerName = null;

            if (entry.ControllerType.ContainsGenericParameters)
            {
                // generic controller have a backtick suffix in their (name e.g. "SomeController`2")
                var indexOfBacktick = entry.ControllerType.Name.IndexOf("Controller`", StringComparison.Ordinal);
                if (indexOfBacktick > -1)
                {
                    // removing it otherwise locating the view templates will fail
                    controllerName = entry.ControllerType.Name.Substring(0, indexOfBacktick);
                }
            }
            else
            {
                controllerName = RemoveSuffix(entry.ControllerType.Name, "Controller");
            }

            var viewNames = new List<string>();

            var includeViews = entry.IncludeViews;

            if (includeViews.Count == 0)
            {
                includeViews = ["*"];
            }

            foreach (var include in includeViews)
            {
                if (include.EndsWith("*"))
                {
                    foreach (var fileName in engine.ViewFolder.ListViews(controllerName))
                    {
                        if (!string.Equals(Path.GetExtension(fileName), ".spark", StringComparison.InvariantCultureIgnoreCase))
                        {
                            continue;
                        }

                        var potentialMatch = Path.GetFileNameWithoutExtension(fileName);
                        if (!TestMatch(potentialMatch, include))
                        {
                            continue;
                        }

                        var isExcluded = false;
                        foreach (var exclude in entry.ExcludeViews)
                        {
                            if (!TestMatch(potentialMatch, RemoveSuffix(exclude, ".spark")))
                            {
                                continue;
                            }

                            isExcluded = true;
                            break;
                        }
                        if (!isExcluded)
                        {
                            viewNames.Add(potentialMatch);
                        }
                    }
                }
                else
                {
                    // explicitly included views don't test for exclusion
                    viewNames.Add(RemoveSuffix(include, ".spark"));
                }
            }

            foreach (var viewName in viewNames)
            {
                if (entry.LayoutNames.Count == 0)
                {
                    descriptors.Add(
                        CreateDescriptor(
                            entry.ControllerType.Namespace,
                            controllerName,
                            viewName,
                            null /*masterName*/,
                            true));
                }
                else
                {
                    foreach (var masterName in entry.LayoutNames)
                    {
                        descriptors.Add(
                            CreateDescriptor(
                                entry.ControllerType.Namespace,
                                controllerName,
                                viewName,
                                string.Join(" ", masterName.ToArray()),
                                false));
                    }
                }
            }

            return descriptors;
        }

        public SparkViewDescriptor CreateDescriptor(
            string targetNamespace, 
            string controllerName, 
            string viewName,
            string masterName, 
            bool findDefaultMaster)
        {
            var searchedLocations = new List<string>();

            var descriptor = descriptorBuilder.BuildDescriptor(
                new BuildDescriptorParams(
                    targetNamespace /*areaName*/,
                    controllerName,
                    viewName,
                    masterName,
                    findDefaultMaster, 
                    null),
                searchedLocations);

            if (descriptor == null)
            {
                throw new CompilerException($"Unable to find templates at {string.Join(", ", searchedLocations.ToArray())}");
            }

            return descriptor;
        }
    }
}
