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
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Services;
using Castle.MonoRail.Framework.Test;
using NUnit.Framework;
using Rhino.Mocks;
using Spark;

namespace Castle.MonoRail.Views.Spark.Tests.ViewComponents
{
    public class BaseViewComponentTests
    {
        protected DefaultViewComponentFactory viewComponentFactory;
        protected MockRepository mocks;
        protected ControllerContext controllerContext;
        protected StubEngineContext engineContext;
        protected SparkViewFactory factory;
        protected IController controller;
        
        [SetUp]
        public virtual void Init()
        {
            mocks = new MockRepository();

            var services = new StubMonoRailServices();
            services.ViewSourceLoader = new FileAssemblyViewSourceLoader("MonoRail.Tests.Views");
            services.AddService(typeof(IViewSourceLoader), services.ViewSourceLoader);

            viewComponentFactory = new DefaultViewComponentFactory();
            viewComponentFactory.Initialize();
            services.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            services.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);

            var settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(SparkView));
            services.AddService(typeof(ISparkSettings), settings);

            services.AddService(typeof(IResourcePathManager), new DefaultResourcePathManager(settings));

            factory = new SparkViewFactory();
            factory.Service(services);

            controller = MockRepository.GenerateMock<IController>();
            controllerContext = new ControllerContext();
            var request = new StubRequest();
            request.FilePath = "";
            var response = new StubResponse();
            engineContext = new StubEngineContext(request, response, new UrlInfo("", "Home", "Index", "/", "castle"));
            engineContext.AddService(typeof(IViewComponentFactory), viewComponentFactory);
            engineContext.AddService(typeof(IViewComponentRegistry), viewComponentFactory.Registry);
        }
    }
}