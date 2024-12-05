// Copyright 2008-2024 Louis DeJardin
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
using System.IO;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Spark.Tests.Stubs;

namespace Spark.Python.Tests
{
    [TestFixture]
    public class ScriptingViewSymbolDictionaryTests
    {
        private TestView _view;
        private ScriptingViewSymbolDictionary _symbols;
        private ScriptScope _scope;

        [SetUp]
        public void Init()
        {
            _view = new TestView();
            _symbols = new ScriptingViewSymbolDictionary(_view);
            _scope = IronPython.Hosting.Python.CreateEngine().CreateScope(_symbols);
        }

        public class TestView : StubSparkView, IScriptingSparkView
        {
            public override void Render()
            {
                
            }

            public override Guid GeneratedViewId
            {
                get { return new Guid("12345678-1234-1234-1234-123456123456"); }
            }

            public override bool TryGetViewData(string name, out object value)
            {
                throw new NotImplementedException();
            }

            public string ScriptSource
            {
                get { throw new NotImplementedException(); }
            }

            public CompiledCode CompiledCode
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        [Test]
        public void GuidCanBeLocated()
        {
            var viewId = (Guid)_scope.GetVariable("GeneratedViewId");
            Assert.AreEqual(new Guid("12345678-1234-1234-1234-123456123456"), viewId);
        }

        [Test]
        public void ViewDataCanBeUsed()
        {
            _view.ViewData["foo"] = "bar";

            var viewData = (IDictionary<string, object>)_scope.GetVariable("ViewData");
            Assert.AreEqual("bar", viewData["foo"]);
        }

        [Test]
        public void MembersCanBeCalled()
        {
            var siteResource = (Delegate)_scope.GetVariable("SiteResource");
            Assert.AreEqual("/TestApp/Hello", siteResource.DynamicInvoke("/Hello"));
        }
    }
}