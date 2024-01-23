using System;
using Microsoft.Extensions.DependencyInjection;
using Spark.Web.Mvc.Extensions;

namespace Spark.Web.Mvc.Ruby
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers spark dependencies in the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddSparkRuby(this IServiceCollection services, ISparkSettings settings = null)
        {
            services.AddSpark(settings);

            // Override ISparkLanguageFactory
            services.AddSingleton<ISparkLanguageFactory, RubyLanguageFactoryWithExtensions>();
            
            return services;
        }
    }
}
