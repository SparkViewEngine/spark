using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Spark.AspNetCore.Mvc.Helpers;
using Spark.Descriptors;

namespace Spark.AspNetCore.Mvc;

public interface ISparkViewEngine : IViewEngine;

public class SparkViewEngine : ISparkViewEngine
{
    private readonly Spark.ISparkViewEngine ViewEngine;
    private readonly IDescriptorBuilder DescriptorBuilder;
    private readonly ICacheService CacheService;
    
    private readonly Dictionary<BuildDescriptorParams, ISparkViewEntry> _cache;

    public SparkViewEngine(Spark.ISparkViewEngine viewEngine, IDescriptorBuilder descriptorBuilder, ICacheService cacheService)
    {
        ViewEngine = viewEngine;
        DescriptorBuilder = descriptorBuilder;
        CacheService = cacheService;

        _cache = new Dictionary<BuildDescriptorParams, ISparkViewEntry>();
    }

    public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
    {
        return FindViewInternal(context, viewName, null, true, false);
    }

    public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
    { 
        var applicationRelativePath = PathHelper.GetAbsolutePath(executingFilePath, viewPath);

        return ViewEngineResult.NotFound(applicationRelativePath, Enumerable.Empty<string>());
    }

    private ViewEngineResult FindViewInternal(ActionContext context, string viewName, string masterName, bool findDefaultMaster, bool useCache)
    {
        var searchedLocations = new List<string>();

        var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
        if (controllerActionDescriptor == null)
        {
            return ViewEngineResult.NotFound(viewName, searchedLocations);
        }

        var targetNamespace = controllerActionDescriptor.ControllerTypeInfo.Namespace;
        var controllerName = controllerActionDescriptor.ControllerName;

        var descriptorParams = new BuildDescriptorParams(
            targetNamespace,
            controllerName,
            viewName,
            masterName,
            findDefaultMaster,
            DescriptorBuilder.GetExtraParameters(new SparkRouteData(context.RouteData.Values)));

        ISparkViewEntry entry;
        if (useCache)
        {
            if (TryGetCacheValue(descriptorParams, out entry) && entry.IsCurrent())
            {
                return BuildResult(viewName, entry);
            }

            return ViewEngineResult.NotFound(viewName, searchedLocations);
        }

        var descriptor = DescriptorBuilder.BuildDescriptor(
            descriptorParams,
            searchedLocations);

        if (descriptor == null)
        {
            return ViewEngineResult.NotFound(viewName, searchedLocations);
        }

        entry = ViewEngine.CreateEntry(descriptor);
            
        SetCacheValue(descriptorParams, entry);

        return BuildResult(viewName, entry);
    }

    private bool TryGetCacheValue(BuildDescriptorParams descriptorParams, out ISparkViewEntry entry)
    {
        lock (_cache) return _cache.TryGetValue(descriptorParams, out entry);
    }

    private void SetCacheValue(BuildDescriptorParams descriptorParams, ISparkViewEntry entry)
    {
        lock (_cache) _cache[descriptorParams] = entry;
    }

    private ViewEngineResult BuildResult(string viewName, ISparkViewEntry entry)
    {
        var view = (IView) entry.CreateInstance();

        if (view is SparkView sparkView)
        {
            sparkView.Path = entry.Descriptor.Templates[0];
            sparkView.CacheService = this.CacheService;
        }

        return ViewEngineResult.Found(viewName, view);
    }
}