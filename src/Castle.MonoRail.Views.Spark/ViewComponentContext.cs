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
using Spark;

namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

    using Castle.MonoRail.Framework;

    public class ViewComponentContext : IViewComponentContext
    {
        private readonly SparkView _view;
        private readonly IResourcePathManager _resourcePathManager;
        private readonly SparkViewFactory _viewEngine;
        private readonly string _name;
        private readonly IDictionary _componentParameters;
        private readonly Action _body;
        private readonly IDictionary<string, Action> _sections;
        private readonly IDictionary _contextVarsAdapter;

        public ViewComponentContext(SparkView view, IResourcePathManager resourcePathManager, SparkViewFactory viewEngine, string name, IDictionary componentParameters, Action body, IDictionary<string, Action> sections)
        {
            _view = view;
            _resourcePathManager = resourcePathManager;
            _viewEngine = viewEngine;
            _name = name;
            _componentParameters = componentParameters;
            _body = body;
            _sections = sections;
            //_contextVarsAdapter = new ContextVarsAdapter(this);
            _contextVarsAdapter = new Hashtable(view.PropertyBag);
        }

        public bool HasSection(string sectionName)
        {
            return _sections.ContainsKey(sectionName);
        }

        public void RenderView(string name, TextWriter writer)
        {
            var descriptor = new SparkViewDescriptor();
            descriptor.Templates.Add(Path.ChangeExtension(name, ".spark"));
            var entry = _viewEngine.Engine.CreateEntry(descriptor);
            var componentView = (SparkView)entry.CreateInstance();

            foreach (var content in _view.Content)
            {
                componentView.Content.Add(content.Key, content.Value);
            }

            var oldPropertyBag = _view.ControllerContext.PropertyBag;
            _view.ControllerContext.PropertyBag = ContextVars;

            try
            {
                componentView.Contextualize(_view.Context, _view.ControllerContext, _resourcePathManager, _viewEngine, _view);
                componentView.RenderView(writer);
            }
            finally
            {
                _view.ControllerContext.PropertyBag = oldPropertyBag;
            }

            foreach (var content in componentView.Content)
            {
                if (!_view.Content.ContainsKey(content.Key))
                    _view.Content.Add(content.Key, content.Value);
            }
            componentView.Content.Clear();

            entry.ReleaseInstance(componentView);
        }

        public void RenderBody()
        {
            _body();
        }

        public void RenderBody(TextWriter writer)
        {
            using (_view.OutputScope(writer))
            {
                RenderBody();
            }
        }

        public void RenderSection(string sectionName)
        {
            _sections[sectionName]();
        }

        public void RenderSection(string sectionName, TextWriter writer)
        {
            using (_view.OutputScope(writer))
            {
                RenderSection(sectionName);
            }
        }

        public string ComponentName
        {
            get { return _name; }
        }

        public TextWriter Writer
        {
            get { return _view.Output; }
        }

        public IDictionary ContextVars
        {
            get { return _contextVarsAdapter; }
        }

        public IDictionary ComponentParameters
        {
            get { return _componentParameters; }
        }

        public string ViewToRender { get; set; }

        public IViewEngine ViewEngine
        {
            get { return _viewEngine; }
        }

    }
}