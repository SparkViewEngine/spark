using System;
using System.Configuration;
using System.Web;
using Microsoft.Extensions.DependencyInjection;
using Spark.Bindings;
using Spark.Compiler;
using Spark.Compiler.Roslyn;
using Spark.Descriptors;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace Spark.Web.Mvc.Extensions
{
    public static class ServiceCollectionExtensions
    {
        internal static string MissingSparkSettingsConfigurationErrorExceptionMessage
            = "Spark setting not configured. Missing spark section app configuration or no ISparkSetting instance registered in IoC container.";

        /// <summary>
        /// Registers spark dependencies in the service collection.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IServiceCollection AddSpark(this IServiceCollection services, ISparkSettings settings = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (settings == null)
            {
                settings = (ISparkSettings)ConfigurationManager.GetSection("spark");

                if (settings == null)
                {
                    throw new ConfigurationErrorsException(MissingSparkSettingsConfigurationErrorExceptionMessage);
                }
            }

            // Roslyn
            services
                .AddSingleton<ISparkLanguageFactory, DefaultLanguageFactory>()
                .AddSingleton<IRoslynCompilationLink, CSharpLink>()
                .AddSingleton<IRoslynCompilationLink, VisualBasicLink>()
                .AddSingleton<IBatchCompiler, RoslynBatchCompiler>();

            services
                .AddSingleton<ISparkSettings>(settings)
                .AddSingleton<IParserSettings>(settings)
                .AddSingleton<ISparkViewEngine, SparkViewEngine>()
                .AddSingleton<ISparkSyntaxProvider, DefaultSyntaxProvider>()
                .AddSingleton<IViewActivatorFactory, DefaultViewActivator>()
                .AddSingleton<IResourcePathManager, DefaultResourcePathManager>()
                .AddSingleton<ITemplateLocator, DefaultTemplateLocator>()
                .AddSingleton<IBindingProvider, DefaultBindingProvider>()
                .AddSingleton<IViewFolder>(settings.CreateDefaultViewFolder())
                .AddSingleton<ICompiledViewHolder, CompiledViewHolder>()
                .AddSingleton<IPartialProvider, DefaultPartialProvider>()
                .AddSingleton<IPartialReferenceProvider, DefaultPartialReferenceProvider>()
                .AddSingleton<ISparkPrecompiler, SparkWebPrecompiler>();

            services.AddSingleton<ISparkExtensionFactory>(c => null);

            services
                .AddSingleton<IDescriptorBuilder, DescriptorBuilder>()
                .AddTransient<ICacheService>(sp =>
                {
                    if (HttpContext.Current != null && HttpContext.Current.Cache != null)
                    {
                        return new WebCacheService(HttpContext.Current.Cache);
                    }

                    return null;
                })
                .AddSingleton<SparkViewFactory>();
            
            return services;
        }
    }
}
