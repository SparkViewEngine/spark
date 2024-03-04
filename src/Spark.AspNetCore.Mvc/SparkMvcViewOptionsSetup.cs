using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Spark.AspNetCore.Mvc;

public class SparkMvcViewOptionsSetup(ISparkViewEngine sparkCoreViewEngine) : IConfigureOptions<MvcViewOptions>
{
    private readonly ISparkViewEngine SparkCoreViewEngine = sparkCoreViewEngine ?? throw new ArgumentNullException(nameof(sparkCoreViewEngine));

    /// <summary>
    /// Configures <paramref name="options"/> to use <see cref="SparkCoreViewEngine"/>.
    /// </summary>
    /// <param name="options">The <see cref="MvcViewOptions"/> to configure.</param>
    public void Configure(MvcViewOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        options.ViewEngines.Add(SparkCoreViewEngine);
    }
}