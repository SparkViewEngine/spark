// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spark.Web.Mvc
{
    public static class SparkEngineStarter
    {
        /// <summary>
        /// Adds Asp.Net Mvc specific IViewEngine implementation.
        /// </summary>
        /// <param name="container">An instance of the spark service container to modify</param>
        public static void ConfigureContainer(ISparkServiceContainer container)
        {
            container.SetServiceBuilder<IViewEngine>(c => new SparkViewFactory(c.GetService<ISparkSettings>()));
            container.SetServiceBuilder<IDescriptorBuilder>(c => new DefaultDescriptorBuilder());
            container.SetServiceBuilder<ICacheServiceProvider>(c => new DefaultCacheServiceProvider());
        }

        /// <summary>
        /// Create a CSharp enabled Spark service container
        /// </summary>
        /// <returns>A configured service container. Additional service builders may
        /// me added. </returns>
        public static ISparkServiceContainer CreateContainer()
        {
            var container = new SparkServiceContainer();
            ConfigureContainer(container);
            return container;
        }

        /// <summary>
        /// Create a CSharp enabled service container with explicit spark settings.
        /// </summary>
        /// <param name="settings">Typically an instance of SparkSettings object</param>
        /// <returns>A configured service container. Additional service builders may
        /// me added. May be passed to RegisterViewEngine.</returns>
        public static ISparkServiceContainer CreateContainer(ISparkSettings settings)
        {
            var container = new SparkServiceContainer(settings);
            ConfigureContainer(container);
            return container;
        }

        /// <summary>
        /// Creates a spark IViewEngine with CSharp as the default language.
        /// Settings come from config or are defaulted.
        /// </summary>
        /// <returns>An IViewEngine interface of the SparkViewFactory</returns>
        public static IViewEngine CreateViewEngine()
        {
            return CreateContainer().GetService<IViewEngine>();
        }

        /// <summary>
        /// Creates a spark IViewEngine with CSharp as the default language.
        /// </summary>
        /// <param name="settings">Typically an instance of SparkSettings object</param>
        /// <returns>An IViewEngine interface of the SparkViewFactory</returns>
        public static IViewEngine CreateViewEngine(ISparkSettings settings)
        {
            return CreateContainer(settings).GetService<IViewEngine>();
        }


        /// <summary>
        /// Installs the Spark view engine. Settings come from config or are defaulted.
        /// </summary>
        public static void RegisterViewEngine()
        {
            ViewEngines.Engines.Add(CreateViewEngine());
        }

        /// <summary>
        /// Installs the Spark view engine. Settings passed in.
        /// </summary>
        public static void RegisterViewEngine(ISparkSettings settings)
        {
            ViewEngines.Engines.Add(CreateViewEngine(settings));
        }

        /// <summary>
        /// Installs the Spark view engine. Container passed in.
        /// </summary>
        public static void RegisterViewEngine(ISparkServiceContainer container)
        {
            ViewEngines.Engines.Add(container.GetService<IViewEngine>());
        }

        /// <summary>
        /// Installs the Spark view engine. Settings come from config or are defaulted.
        /// </summary>
        /// <param name="engines">Typically in the ViewEngines.Engines collection</param>
        public static void RegisterViewEngine(ICollection<IViewEngine> engines)
        {
            engines.Add(CreateViewEngine());
        }

        /// <summary>
        /// Installs the Spark view engine. Settings passed in.
        /// </summary>
        /// <param name="engines">Typically in the ViewEngines.Engines collection</param>
        /// <param name="settings">Typically an instance of SparkSettings object</param>
        public static void RegisterViewEngine(ICollection<IViewEngine> engines, ISparkSettings settings)
        {
            engines.Add(CreateViewEngine(settings));
        }

        /// <summary>
        /// Install the view engine from the container. Typical usage is to call CreateContainer,
        /// provide additinal service builder functors to override certain classes, then call this
        /// method.
        /// </summary>
        /// <param name="engines">Typically the ViewEngines.Engines collection</param>
        /// <param name="container">A service container, often created with CreateContainer</param>
        public static void RegisterViewEngine(ICollection<IViewEngine> engines, ISparkServiceContainer container)
        {
            engines.Add(container.GetService<IViewEngine>());
        }
    }
}
