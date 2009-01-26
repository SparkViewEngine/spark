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
namespace Castle.MonoRail.Views.Spark
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    using Castle.Core.Logging;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Helpers;

    using global::Spark;

    public class MonoRailViewContext
    {
        public IEngineContext EngineContext {get;set;}
        public IControllerContext ControllerContext { get; set; }
        public SparkViewFactory ViewEngine { get; set; }
        public IDictionary ContextVars { get; set; }
    }

    public abstract class SparkView : SparkViewDecorator<MonoRailViewContext>
    {
        protected SparkView()
            : this(null)
        {
        }

        protected SparkView(SparkViewBase<MonoRailViewContext> decorated)
            : base(decorated)
        {
            ViewData = new SparkViewData(this);
        }


        private ILogger _logger = NullLogger.Instance;
        public ILogger Logger { get { return _logger; } set { _logger = value; } }


        public IEngineContext Context { get { return SparkContext.Extended.EngineContext; } }
        public IControllerContext ControllerContext { get { return SparkContext.Extended.ControllerContext; } }

        public IController Controller { get { return Context.CurrentController; } }
        public IServerUtility Server { get { return Context.Server; } }
        public IRequest Request { get { return Context.Request; } }
        public IResponse Response { get { return Context.Response; } }
        public IDictionary Session { get { return Context.Session; } }
        public Flash Flash { get { return Context.Flash; } }
        public string SiteRoot { get { return Context.ApplicationPath; } }
        public string SiteResource(string path)
        {
            return SparkContext.Extended.ViewEngine.Engine.ResourcePathManager.GetResourcePath(SiteRoot, path);
        }

        public IDictionary PropertyBag { get { return SparkContext.Extended.ContextVars ?? ControllerContext.PropertyBag; } }
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
        public override bool TryGetViewData(string name, out object value)
        {
            return ViewData.TryGetViewData(name, out value);
        }

        public T Helper<T>() where T : class { return ControllerContext.Helpers[typeof(T).Name] as T; }
        public T Helper<T>(string name) where T : class { return ControllerContext.Helpers[name] as T; }

        public virtual void Contextualize(IEngineContext context, IControllerContext controllerContext, SparkViewFactory viewEngine, SparkView outerView)
        {
            SparkContext.Extended.EngineContext = context;
            SparkContext.Extended.ControllerContext = controllerContext;
            SparkContext.Extended.ViewEngine = viewEngine;

            if (outerView != null)
                OnceTable = outerView.OnceTable;
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
            var service = (IViewComponentFactory)Context.GetService(typeof(IViewComponentFactory));
            var component = service.Create(name);

            IViewComponentContext viewComponentContext = new ViewComponentContext(this, SparkContext.Extended.ViewEngine, name, parameters, body, sections);

            var oldContextVars = SparkContext.Extended.ContextVars;
            try
            {
                SparkContext.Extended.ContextVars = viewComponentContext.ContextVars;
                component.Init(Context, viewComponentContext);
                component.Render();

                if (viewComponentContext.ViewToRender != null)
                {
                    viewComponentContext.RenderView(viewComponentContext.ViewToRender, Output);
                }

            }
            finally
            {
                SparkContext.Extended.ContextVars = oldContextVars;
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
