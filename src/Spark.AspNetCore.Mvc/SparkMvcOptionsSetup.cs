using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Spark.AspNetCore.Mvc.Filters;

namespace Spark.AspNetCore.Mvc;

public class SparkMvcOptionsSetup : IConfigureOptions<MvcOptions>
{
    public void Configure(MvcOptions options)
    {
        options.Filters.Add<HtmlHelperResultFilter>();
    }
}