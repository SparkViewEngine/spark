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
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Spark;
using Spark.FileSystem;
using Spark.Web.Mvc;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl
{
    public class Application
    {
        public void ConfigureIoC(string configPath)
        {
            // create a Windsor container with various component parameters established
            var container = new WindsorContainer(configPath);

            // Replaces the default IViewEngine. 
            container.AddComponent<IViewEngine, SparkViewFactory>();
            container.AddComponent<IViewActivatorFactory, WindsorViewActivator>();

            // Add anything descended from IController/Controller 
            container.Register(
                AllTypes.Of<IController>()
                    .FromAssembly(typeof (Global).Assembly)
                    .Configure(c => c.LifeStyle.Transient.Named(c.Implementation.Name.ToLowerInvariant())));

            // Some more components from the sample
            container.AddComponent<IViewFolder, FileSystemViewFolder>();
            container.AddComponent<ISampleRepository, SampleRepository>();
            container.AddComponent<INavRepository, NavRepository>();

            // Place this container as the dependency resolver and hook it into
            // the controller factory mechanism
            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(container.Kernel));
            ViewEngines.Engines.Add(container.Resolve<IViewEngine>());
        }

        public void AddRoutes(RouteCollection routes)
        {
            routes.Add(new Route("{controller}/{action}/{id}", new MvcRouteHandler())
                           {
                               Defaults = new RouteValueDictionary(new {action = "Index", id = ""}),
                           });

            routes.Add(new Route("Default.aspx", new MvcRouteHandler())
                           {
                               Defaults = new RouteValueDictionary(new {controller = "Home", action = "Index", id = ""}),
                           });
        }
    }
}