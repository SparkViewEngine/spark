// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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
        private readonly IServiceProvider _serviceProvider;

        public ViewComponentExtensionFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ISparkExtension CreateExtension(ElementNode node)
        {
            var componentFactory = (IViewComponentFactory)_serviceProvider.GetService(typeof(IViewComponentFactory));
            if (componentFactory == null || componentFactory.Registry == null)
                return null;

            ViewComponentInfo viewComponentInfo;
            lock (_cachedViewComponent)
            {
                if (!_cachedViewComponent.TryGetValue(node.Name, out viewComponentInfo))
                {
                    if (componentFactory.Registry.HasViewComponent(node.Name))
                    {
                        viewComponentInfo = new ViewComponentInfo(componentFactory.Registry.GetViewComponent(node.Name));
                        _cachedViewComponent.Add(node.Name, viewComponentInfo);
                    }
                    else
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
