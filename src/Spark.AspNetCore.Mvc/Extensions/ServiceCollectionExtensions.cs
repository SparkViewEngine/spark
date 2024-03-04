using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Spark.Bindings;
using Spark.Compiler;
using Spark.Compiler.Roslyn;
using Spark.Descriptors;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace Spark.AspNetCore.Mvc.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSpark(this IServiceCollection services, Action<SparkSettings>? setupAction = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services
                .AddOptions()
                .AddSingleton<IConfigureOptions<SparkSettings>, SparkOptionsSetup>()
                .AddTransient<ISparkSettings>(f => f.GetService<IOptions<SparkSettings>>()?.Value)
                .AddTransient<IParserSettings>(f => f.GetService<IOptions<SparkSettings>>()?.Value);

            services
                .AddMemoryCache()
                .AddTransient<ICacheService, InMemoryCacheService>();

            if (setupAction != null)
            {
                services.Configure(setupAction);
            }

            // TODO: Reduce memory consumption cause by loading the assembly of the views compiled dynamically
            services
                .AddSingleton<ISparkLanguageFactory, DefaultLanguageFactory>()
                .AddSingleton<IRoslynCompilationLink, CSharpLink>()
                .AddSingleton<IRoslynCompilationLink, VisualBasicLink>()
                .AddSingleton<IBatchCompiler, RoslynBatchCompiler>();

            services
                .AddSingleton<Spark.ISparkViewEngine, Spark.SparkViewEngine>()
                .AddSingleton<ISparkSyntaxProvider, DefaultSyntaxProvider>()
                .AddSingleton<IViewActivatorFactory, DefaultViewActivator>()
                .AddSingleton<IResourcePathManager, DefaultResourcePathManager>()
                .AddSingleton<ITemplateLocator, DefaultTemplateLocator>()
                .AddSingleton<IBindingProvider, DefaultBindingProvider>()
                .AddSingleton<IViewFolder>(f => f.GetService<ISparkSettings>().CreateDefaultViewFolder())
                .AddSingleton<ICompiledViewHolder, CompiledViewHolder>()
                .AddSingleton<IPartialProvider, DefaultPartialProvider>()
                .AddSingleton<IPartialReferenceProvider, DefaultPartialReferenceProvider>();

            services.AddSingleton<ISparkExtensionFactory>(c => null);

            services
                .AddSingleton<IDescriptorBuilder, AspNetCoreDescriptorBuilder>();

            services
                .AddSingleton<ISparkPrecompiler, SparkPrecompiler>();

            services
                .AddTransient<IConfigureOptions<MvcOptions>, SparkMvcOptionsSetup>()
                .AddTransient<IConfigureOptions<MvcViewOptions>, SparkMvcViewOptionsSetup>()
                .AddSingleton<ISparkViewEngine, SparkViewEngine>();

            return services;
        }
    }
}
