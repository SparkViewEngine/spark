using System;
using System.Collections.Generic;
using Castle.Core;
using Castle.MonoRail.Framework;
using Spark;
using Spark.Parser.Markup;

namespace Castle.MonoRail.Views.Spark
{
    public class ViewComponentExtensionFactory : ISparkExtensionFactory
    {
        readonly Dictionary<string, ViewComponentInfo> _cachedViewComponent = new Dictionary<string, ViewComponentInfo>();
        private IServiceProvider _serviceProvider;

        public ViewComponentExtensionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISparkExtension CreateExtension(ElementNode node)
        {
            var componentFactory = (IViewComponentFactory)_serviceProvider.GetService(typeof(IViewComponentFactory));

            ViewComponentInfo viewComponentInfo;
            lock (_cachedViewComponent)
            {

                if (!_cachedViewComponent.TryGetValue(node.Name, out viewComponentInfo))
                {
                    try
                    {
                        viewComponentInfo = new ViewComponentInfo(componentFactory.Registry.GetViewComponent(node.Name));
                        _cachedViewComponent.Add(node.Name, viewComponentInfo);
                    }
                    catch
                    {
                        _cachedViewComponent.Add(node.Name, null);
                    }
                }
            }

            if (viewComponentInfo != null)
            {
                return new ViewComponentExtension(node, viewComponentInfo);
            }

            return null;
        }

    }
}
