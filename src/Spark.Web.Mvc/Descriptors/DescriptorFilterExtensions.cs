using System;

namespace Spark.Web.Mvc.Descriptors
{
    public static class DescriptorFilterExtensions
    {
        public static void AddFilter(this SparkViewFactory target, IDescriptorFilter filter)
        {
            target.DescriptorBuilder.AddFilter(filter);
        }

        public static void AddFilter(this IDescriptorBuilder target, IDescriptorFilter filter)
        {
            if (!(target is DefaultDescriptorBuilder))
                throw new InvalidCastException($"IDescriptorFilters may only be added to {nameof(DefaultDescriptorBuilder)}");

            ((DefaultDescriptorBuilder) target).AddFilter(filter);
        }

        public static void AddFilter(this DefaultDescriptorBuilder target, IDescriptorFilter filter)
        {
            target.Filters.Add(filter);
        }
    }
}
