// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

using Castle.Core.Logging;
using Spark.Compiler;

namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using Castle.MonoRail.Framework;

    using global::Spark;
    using global::Spark.FileSystem;
    using global::Spark.Parser.Markup;

    public class SparkViewFactory : ViewEngineBase, IViewFolder, ISparkExtensionFactory
    {
        public override void Service(IServiceProvider provider)
        {
            base.Service(provider);
            Engine = (ISparkViewEngine)provider.GetService(typeof(ISparkViewEngine));
        }

        private ISparkViewEngine _engine;
        public ISparkViewEngine Engine
        {
            get
            {
                if (_engine == null)
                    Engine = new SparkViewEngine();
                return _engine;
            }
            set
            {
                _engine = value;
                if (_engine != null)
                {
                    _engine.ViewFolder = this;
                    _engine.ExtensionFactory = this;
                    if (string.IsNullOrEmpty(_engine.Settings.PageBaseType))
                        _engine.Settings.PageBaseType = typeof (SparkView).FullName;
                }
            }
        }

        public override string ViewFileExtension
        {
            get { return "spark"; }
        }

        public override bool HasTemplate(string templateName)
        {
            return base.HasTemplate(Path.ChangeExtension(templateName, ViewFileExtension));
        }

        public override void Process(string templateName, TextWriter output, IEngineContext context, IController controller,
                                     IControllerContext controllerContext)
        {
            string masterName = null;
            if (controllerContext.LayoutNames != null)
                masterName = string.Join(" ", controllerContext.LayoutNames);

            var descriptor = new SparkViewDescriptor { TargetNamespace = controller.GetType().Namespace };
            descriptor.Templates.Add(Path.ChangeExtension(templateName, ViewFileExtension));

            if (controllerContext.LayoutNames != null)
            {
                foreach (var layoutName in controllerContext.LayoutNames)
                {
                    if (HasTemplate("Layouts\\" + masterName))
                    {
                        descriptor.Templates.Add(Path.ChangeExtension("Layouts\\" + masterName, ViewFileExtension));
                    }
                    else if (HasTemplate("Shared\\" + masterName))
                    {
                        descriptor.Templates.Add(Path.ChangeExtension("Shared\\" + masterName, ViewFileExtension));
                    }
                    else
                    {
                        throw new CompilerException(string.Format(
                                                        "Unable to find templates layouts\\{0} or shared\\{0}",
                                                        layoutName));
                    }
                }
            }

            var entry = Engine.CreateEntry(descriptor);
            var view = (SparkView)entry.CreateInstance();
            view.Contextualize(context, controllerContext, this);
            if (view.Logger == null || view.Logger == NullLogger.Instance)
                view.Logger = Logger;
            view.RenderView(output);
            entry.ReleaseInstance(view);
        }

        public override void Process(string templateName, string layoutName, TextWriter output,
                                     IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public override void ProcessPartial(string partialName, TextWriter output, IEngineContext context,
                                            IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override void RenderStaticWithinLayout(string contents, IEngineContext context, IController controller,
                                                      IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override bool SupportsJSGeneration
        {
            get { return false; }
        }

        public override string JSGeneratorFileExtension
        {
            get { return null; }
        }

        public override object CreateJSGenerator(JSCodeGeneratorInfo generatorInfo, IEngineContext context,
                                                 IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        public override void GenerateJS(string templateName, TextWriter output, JSCodeGeneratorInfo generatorInfo,
                                        IEngineContext context, IController controller, IControllerContext controllerContext)
        {
            throw new NotImplementedException();
        }

        IList<string> IViewFolder.ListViews(string path)
        {
            return ViewSourceLoader.ListViews(path);
        }

        bool IViewFolder.HasView(string path)
        {
            return ViewSourceLoader.HasSource(path);
        }

        IViewFile IViewFolder.GetViewSource(string path)
        {
            return new ViewFile(ViewSourceLoader.GetViewSource(Path.ChangeExtension(path, ViewFileExtension)));
        }

        private class ViewFile : IViewFile
        {
            private readonly IViewSource _source;

            public ViewFile(IViewSource source)
            {
                _source = source;
            }

            public long LastModified
            {
                get { return _source.LastModified; }
            }

            public Stream OpenViewStream()
            {
                return _source.OpenViewStream();
            }
        }

        readonly Dictionary<string, Type> _cachedViewComponent = new Dictionary<string, Type>();

        ISparkExtension ISparkExtensionFactory.CreateExtension(ElementNode node)
        {
            var componentFactory = (IViewComponentFactory)serviceProvider.GetService(typeof(IViewComponentFactory));

            Type viewComponent;
            lock (_cachedViewComponent)
            {

                if (!_cachedViewComponent.TryGetValue(node.Name, out viewComponent))
                {
                    try
                    {
                        viewComponent = componentFactory.Registry.GetViewComponent(node.Name);
                        _cachedViewComponent.Add(node.Name, viewComponent);
                    }
                    catch
                    {
                        _cachedViewComponent.Add(node.Name, null);
                    }
                }
            }
            if (viewComponent != null)
                return new ViewComponentExtension(node);

            return null;
        }
    }
}
