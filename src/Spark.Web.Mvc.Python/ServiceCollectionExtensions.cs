using System;
using Microsoft.Extensions.DependencyInjection;
using Spark.Web.Mvc.Extensions;

namespace Spark.Web.Mvc.Python
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers spark dependencies in the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddSparkPython(this IServiceCollection services, ISparkSettings settings = null)
        {
            services.AddSpark(settings);

            // Override ISparkLanguageFactory
            services.AddSingleton<ISparkLanguageFactory, PythonLanguageFactoryWithExtensions>();
            
            return services;
        }
    }
}
