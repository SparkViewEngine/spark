using System;
using System.Collections.Generic;

namespace Spark
{
    public class SparkBatchDescriptor
    {
        public SparkBatchDescriptor()
        {
            Entries = new List<SparkBatchEntry>();
        }

        public IList<SparkBatchEntry> Entries { get; set; }

        public SparkBatchConfigurator For(Type controllerType)
        {
            var entry = new SparkBatchEntry { ControllerType = controllerType };
            Entries.Add(entry);
            return new SparkBatchConfigurator(this, entry);
        }

        public SparkBatchConfigurator For<TController>()
        {
            return For(typeof(TController));
        }
    }

    public class SparkBatchEntry
    {
        public SparkBatchEntry()
        {
            LayoutNames = new List<IList<string>>();
            IncludeViews = new List<string>();
            ExcludeViews = new List<string>();
        }

        public Type ControllerType { get; set; }
        public IList<IList<string>> LayoutNames { get; set; }
        public IList<string> IncludeViews { get; set; }
        public IList<string> ExcludeViews { get; set; }
    }

    public sealed class SparkBatchConfigurator
    {
        private readonly SparkBatchDescriptor descriptor;
        private readonly SparkBatchEntry entry;

        internal SparkBatchConfigurator(SparkBatchDescriptor descriptor, SparkBatchEntry entry)
        {
            this.descriptor = descriptor;
            this.entry = entry;
        }

        public SparkBatchConfigurator For(Type controllerType, params string[] layoutNames)
        {
            return descriptor.For(controllerType);
        }

        public SparkBatchConfigurator For<TController>(params string[] layoutNames)
        {
            return descriptor.For<TController>();
        }

        public SparkBatchConfigurator Layout(params string[] layouts)
        {
            entry.LayoutNames.Add(layouts);
            return this;
        }

        public SparkBatchConfigurator Include(string pattern)
        {
            entry.IncludeViews.Add(pattern);
            return this;
        }

        public SparkBatchConfigurator Exclude(string pattern)
        {
            entry.ExcludeViews.Add(pattern);
            return this;
        }
    }
}
