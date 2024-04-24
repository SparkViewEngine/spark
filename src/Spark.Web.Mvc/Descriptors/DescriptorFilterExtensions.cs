using System;
using Spark.Descriptors;

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
            if (!(target is DescriptorBuilder))
            {
                throw new InvalidCastException($"IDescriptorFilters may only be added to {nameof(DescriptorBuilder)}");
            }

            ((DescriptorBuilder)target).AddFilter(filter);
        }

        public static void AddFilter(this DescriptorBuilder target, IDescriptorFilter filter)
        {
            target.Filters.Add(filter);
        }
    }
}
