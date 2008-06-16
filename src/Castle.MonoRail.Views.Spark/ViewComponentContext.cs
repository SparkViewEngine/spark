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
        private readonly SparkViewFactory _viewEngine;
        private readonly IDictionary _componentParameters;
        private readonly Action body;
        private readonly IDictionary<string, Action> sections;
        private readonly IDictionary _contextVarsAdapter;


        public ViewComponentContext(SparkView view, SparkViewFactory viewEngine, IDictionary componentParameters, Action body, IDictionary<string, Action> sections)
        {
            _view = view;
            _viewEngine = viewEngine;
            _componentParameters = componentParameters;
            this.body = body;
            this.sections = sections;
            //_contextVarsAdapter = new ContextVarsAdapter(this);
            _contextVarsAdapter = new Hashtable(view.PropertyBag);
        }



        public bool HasSection(string sectionName)
        {
            return sections.ContainsKey(sectionName);
        }

        public void RenderView(string name, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void RenderBody()
        {
            body();
        }

        public void RenderBody(TextWriter writer)
        {
            using(_view.OutputScope(writer))
            {
                RenderBody();
            }
        }

        public void RenderSection(string sectionName)
        {
            sections[sectionName]();
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
            get { throw new NotImplementedException(); }
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

        public string ViewToRender
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public IViewEngine ViewEngine
        {
            get { return _viewEngine; }
        }

    }
}