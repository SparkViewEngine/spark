using Microsoft.Extensions.Options;

namespace Spark.AspNetCore.Mvc;

public class SparkOptionsSetup : IConfigureOptions<SparkSettings>
{
    public void Configure(SparkSettings options)
    {
    }
}