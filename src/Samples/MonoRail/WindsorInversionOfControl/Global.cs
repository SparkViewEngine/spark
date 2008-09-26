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
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Routing;
using Castle.MonoRail.WindsorExtension;
using Castle.Windsor;
using Spark;
using WindsorInversionOfControl.Models;

namespace WindsorInversionOfControl
{
    public partial class Global
    {
        public static void RegisterFacilities(IWindsorContainer container)
        {
            container.AddFacility("mr", new MonoRailFacility());
            container.AddFacility("logging", new LoggingFacility(LoggerImplementation.Trace));
        }

        public static void RegisterComponents(IWindsorContainer container)
        {
            container.Register(
                // add all the controllers
                AllTypes.Of<Controller>().FromAssemblyNamed("WindsorInversionOfControl"),
                // this component will result in the views also being registered in the container
                Component.For<IViewActivatorFactory>().ImplementedBy<ViewActivator>(),
                // here's an example of a component used by the views. see the View class.
                Component.For<INavProvider>().ImplementedBy<NavProvider>());
        }

        public static void RegisterRoutes(IRoutingRuleContainer engine)
        {
            engine.Add(new PatternRoute("/<controller>/[action]/[id]")
                           .DefaultForAction().Is("index")
                           .DefaultFor("id").Is(""));
        }
    }
}