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
    using System.Collections.Specialized;

    using Castle.Core.Logging;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;

    using global::Spark;

    public abstract class SparkView : AbstractSparkView
    {
        protected SparkView()
        {
            ViewData = new SparkViewData(this);
        }

        private IEngineContext _context;
        private IControllerContext _controllerContext;
        private SparkViewFactory _viewEngine;
        private IDictionary _contextVars;

        private ILogger _logger = NullLogger.Instance;
        public ILogger Logger { get { return _logger; } set { _logger = value; } }


        public IEngineContext Context { get { return _context; } }
        public IControllerContext ControllerContext { get { return _controllerContext; } }

        public IController Controller { get { return _context.CurrentController; } }
        public IServerUtility Server { get { return _context.Server; } }
        public IRequest Request { get { return _context.Request; } }
        public IResponse Response { get { return _context.Response; } }
        public IDictionary Session { get { return _context.Session; } }
        public Flash Flash { get { return _context.Flash; } }
        public string SiteRoot { get { return _context.ApplicationPath; } }

        public IDictionary PropertyBag { get { return _contextVars ?? _controllerContext.PropertyBag; } }
        public NameValueCollection Params { get { return Request.Params; } }

        public AjaxHelper Ajax { get { return Helper<AjaxHelper>(); } }
        public BehaviourHelper Behaviour { get { return Helper<BehaviourHelper>(); } }
        public UrlHelper Url { get { return Helper<UrlHelper>(); } }
        public TextHelper Text { get { return Helper<TextHelper>(); } }
        public EffectsFatHelper EffectsFat { get { return Helper<EffectsFatHelper>(); } }
        public ScriptaculousHelper Scriptaculous { get { return Helper<ScriptaculousHelper>(); } }
        public DateFormatHelper DateFormat { get { return Helper<DateFormatHelper>(); } }
        public HtmlHelper Html { get { return Helper<HtmlHelper>(); } }
        public ValidationHelper Validation { get { return Helper<ValidationHelper>(); } }
        public DictHelper Dict { get { return Helper<DictHelper>(); } }
        public PaginationHelper Pagination { get { return Helper<PaginationHelper>(); } }
        public FormHelper Form { get { return Helper<FormHelper>(); } }
        public JSONHelper JSON { get { return Helper<JSONHelper>(); } }
        public ZebdaHelper Zebda { get { return Helper<ZebdaHelper>(); } }

        public SparkViewData ViewData { get; set; }

        public T Helper<T>() where T : class { return ControllerContext.Helpers[typeof(T).Name] as T; }

        public virtual void Contextualize(IEngineContext context, IControllerContext controllerContext, SparkViewFactory viewEngine)
        {
            _context = context;
            _controllerContext = controllerContext;
            _viewEngine = viewEngine;            
        }

        public string H(object value)
        {
            return Server.HtmlEncode(Convert.ToString(value));
        }

        public void RenderComponent(
            string name,
            IDictionary parameters,
            Action body,
            IDictionary<string, Action> sections)
        {
            var service = (IViewComponentFactory)_context.GetService(typeof(IViewComponentFactory));
            var component = service.Create(name);

            IViewComponentContext viewComponentContext = new ViewComponentContext(this, _viewEngine, parameters, body, sections);

            var oldContextVars = _contextVars;
            try
            {
                _contextVars = viewComponentContext.ContextVars;
                component.Init(_context, viewComponentContext);
                component.Render();
            }
            finally
            {
                _contextVars = oldContextVars;
            }

            foreach (string key in viewComponentContext.ContextVars.Keys)
            {
                if (key.EndsWith(".@bubbleUp"))
                {
                    string key2 = key.Substring(0, key.Length - ".@bubbleUp".Length);
                    PropertyBag[key2] = viewComponentContext.ContextVars[key2];
                }
            }
        }

    }
}
