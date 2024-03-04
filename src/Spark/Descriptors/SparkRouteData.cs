using System.Collections.Generic;

namespace Spark.Descriptors;

/// <summary>
/// Helper class to keep a strongly typed parameter where used but make it more explicit that we expect the route data.
/// </summary>
/// <remarks>
/// Used to pass the route data whether it comes from ASP.NET or ASP.NET Core.
/// </remarks>
public class SparkRouteData(IDictionary<string, object> values)
{
    public IDictionary<string, object> Values = values;
}