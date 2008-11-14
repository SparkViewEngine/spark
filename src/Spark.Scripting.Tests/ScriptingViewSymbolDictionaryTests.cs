using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using NUnit.Framework;
using Spark.Tests.Stubs;

namespace Spark.Scripting.Tests
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
            _scope = Python.CreateEngine().CreateScope(_symbols);
        }

        public class TestView : StubSparkView, IScriptingSparkView
        {
            public override void RenderView(TextWriter writer)
            {
                throw new System.NotImplementedException();
            }

            public override Guid GeneratedViewId
            {
                get { return new Guid("12345678-1234-1234-1234-123456123456"); }
            }

            public override bool TryGetViewData(string name, out object value)
            {
                throw new System.NotImplementedException();
            }

            public string ScriptSource
            {
                get { throw new System.NotImplementedException(); }
            }

            public CompiledCode CompiledCode
            {
                get { throw new System.NotImplementedException(); }
                set { throw new System.NotImplementedException(); }
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
