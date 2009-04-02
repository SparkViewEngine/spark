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
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Rhino.Mocks;
using Spark.FileSystem;

namespace Spark.Web.Mvc.Tests
{
    [TestFixture]
    public class DescriptorBuildingTester
    {
        private SparkViewFactory _factory;
        private InMemoryViewFolder _viewFolder;
        private RouteData _routeData;
        private ControllerContext _controllerContext;

        [SetUp]
        public void Init()
        {
            CompiledViewHolder.Current = null;

            _factory = new SparkViewFactory();
            _viewFolder = new InMemoryViewFolder();
            _factory.ViewFolder = _viewFolder;

            var httpContext = MockRepository.GenerateStub<HttpContextBase>();
            _routeData = new RouteData();
            var controller = MockRepository.GenerateStub<ControllerBase>();
            _controllerContext = new ControllerContext(httpContext, _routeData, controller);
        }

        private static void AssertDescriptorTemplates(
            SparkViewDescriptor descriptor, 
            ICollection<string> searchedLocations, 
            params string[] templates)
        {
            Assert.AreEqual(templates.Length, descriptor.Templates.Count, "Descriptor template count must match");
            for (var index = 0; index != templates.Length; ++index)
                Assert.AreEqual(templates[index], descriptor.Templates[index]);
            Assert.AreEqual(0, searchedLocations.Count, "searchedLocations must be empty");
        }

        [Test]
        public void NormalViewAndNoDefaultLayout()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark");
        }

        [Test]
        public void NormalViewAndDefaultLayoutPresent()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Application.spark");
        }

        
        [Test]
        public void NormalViewAndControllerLayoutOverrides()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Home.spark");
        }

        [Test]
        public void NormalViewAndNamedMaster()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");
            _viewFolder.Add(@"Layouts\Site.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", "Site", true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Site.spark");
        }

        [Test]
        public void PartialViewIgnoresDefaultLayouts()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");
            _viewFolder.Add(@"Shared\Application.spark", "");
            _viewFolder.Add(@"Shared\Home.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, false, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark");
        }

        [Test]
        public void RouteAreaPresentDefaultsToNormalLocation()
        {
            _routeData.Values.Add("area", "Admin");
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Application.spark");
        }

        [Test]
        public void AreaFolderMayContainControllerFolder()
        {
            _routeData.Values.Add("area", "Admin");
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Home\Index.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Admin\Home\Index.spark",
                @"Layouts\Application.spark");
        }

        [Test]
        public void AreaFolderMayContainLayoutsFolder()
        {
            _routeData.Values.Add("area", "Admin");
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Home\Index.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Application.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Admin\Home\Index.spark",
                @"Admin\Layouts\Application.spark");
        }

        [Test]
        public void AreaContainsNamedLayout()
        {
            _routeData.Values.Add("area", "Admin");
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Home\Index.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Site.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", "Site", true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Admin\Home\Index.spark",
                @"Admin\Layouts\Site.spark");            
        }

        [Test]
        public void PartialViewFromAreaIgnoresLayout()
        {
            _routeData.Values.Add("area", "Admin");
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "");
            _viewFolder.Add(@"Admin\Home\Index.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Application.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Home.spark", "");
            _viewFolder.Add(@"Admin\Layouts\Site.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, false, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Admin\Home\Index.spark");
        }

        [Test]
        public void UseMasterCreatesTemplateChain()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "<use master='Green'/>");
            _viewFolder.Add(@"Layouts\Red.spark", "<use master='Blue'/>");
            _viewFolder.Add(@"Layouts\Green.spark", "<use master='Red'/>");
            _viewFolder.Add(@"Layouts\Blue.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Green.spark",
                @"Layouts\Red.spark",
                @"Layouts\Blue.spark");
        }

        [Test]
        public void NamedMasterOverridesViewMaster()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "<use master='Green'/>");
            _viewFolder.Add(@"Layouts\Red.spark", "<use master='Blue'/>");
            _viewFolder.Add(@"Layouts\Green.spark", "<use master='Red'/>");
            _viewFolder.Add(@"Layouts\Blue.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", "Red", true, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark",
                @"Layouts\Red.spark",
                @"Layouts\Blue.spark");
        }

        [Test]
        public void PartialViewIgnoresUseMasterAndDefault()
        {
            _routeData.Values.Add("controller", "Home");
            _viewFolder.Add(@"Home\Index.spark", "<use master='Green'/>");
            _viewFolder.Add(@"Layouts\Red.spark", "<use master='Blue'/>");
            _viewFolder.Add(@"Layouts\Green.spark", "<use master='Red'/>");
            _viewFolder.Add(@"Layouts\Blue.spark", "");
            _viewFolder.Add(@"Layouts\Application.spark", "");
            _viewFolder.Add(@"Layouts\Home.spark", "");

            var searchedLocations = new List<string>();
            var result = _factory.CreateDescriptor(_controllerContext, "Index", null, false, searchedLocations);
            AssertDescriptorTemplates(
                result, searchedLocations,
                @"Home\Index.spark");
        }
    }
}
