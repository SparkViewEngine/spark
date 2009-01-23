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
using System.IO;
using System.Web;
using System.Web.Mvc;
using HttpContextWrapper = Spark.Web.Mvc.Wrappers.HttpContextWrapper;

namespace Spark.Web.Mvc
{
    public class MvcViewContext
    {
        public ViewContext ViewContext { get; set; }
        public ViewDataDictionary ViewData { get; set; }

        public HtmlHelper Html { get; set; }
        public UrlHelper Url { get; set; }
        public AjaxHelper Ajax { get; set; }

        public string SiteRoot { get; set; }
        public IResourcePathManager ResourcePathManager { get; set; }
    }

    public abstract class SparkView : SparkViewDecorator<MvcViewContext>, IViewDataContainer, IView
    {
        protected SparkView(SparkViewBase<MvcViewContext> decorated)
            : base(decorated)
        {

        }


        public ViewContext ViewContext
        {
            get { return ExtendedContext.ViewContext; }
            set { ExtendedContext.ViewContext = value; }
        }

        public TempDataDictionary TempData { get { return ViewContext.TempData; } }

        public HtmlHelper Html { get { return ExtendedContext.Html; } }
        public UrlHelper Url { get { return ExtendedContext.Url; } }
        public AjaxHelper Ajax { get { return ExtendedContext.Ajax; } }

        public HttpContextBase Context { get { return ViewContext.HttpContext; } }

        public HttpRequestBase Request { get { return ViewContext.HttpContext.Request; } }

        public HttpResponseBase Response { get { return ViewContext.HttpContext.Response; } }

        public IResourcePathManager ResourcePathManager
        {
            get { return ExtendedContext.ResourcePathManager; }
            set { ExtendedContext.ResourcePathManager = value; }
        }

        public override bool TryGetViewData(string name, out object value)
        {
            if (ViewData.ContainsKey(name))
            {
                value = ViewData[name];
                return true;
            }
            if (ViewData.Model != null)
            {
                var property = ViewData.Model.GetType().GetProperty(name);
                if (property != null)
                {
                    value = property.GetValue(ViewData.Model, null);
                    return true;
                }
            }
            value = null;
            return false;
        }

        public string SiteRoot
        {
            get
            {
                if (ExtendedContext.SiteRoot == null)
                {
                    var appPath = ViewContext.HttpContext.Request.ApplicationPath;
                    if (string.IsNullOrEmpty(appPath) || string.Equals(appPath, "/"))
                    {
                        ExtendedContext.SiteRoot = string.Empty;
                    }
                    else
                    {
                        ExtendedContext.SiteRoot = "/" + appPath.Trim('/');
                    }
                }
                return ExtendedContext.SiteRoot;
            }
        }
        public string SiteResource(string path)
        {
            return ResourcePathManager.GetResourcePath(SiteRoot, path);
        }


        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var wrappedHttpContext = new HttpContextWrapper(viewContext.HttpContext, this);
            var wrappedViewContext = new ViewContext(wrappedHttpContext, viewContext.RouteData, viewContext.Controller,
                                          viewContext.View, viewContext.ViewData, viewContext.TempData);

            ExtendedContext.ViewContext = wrappedViewContext;
            ViewData = wrappedViewContext.ViewData;
            ExtendedContext.Html = new HtmlHelper(wrappedViewContext, this);
            ExtendedContext.Url = new UrlHelper(wrappedViewContext);
            ExtendedContext.Ajax = new AjaxHelper(wrappedViewContext);

            var outerView = ViewContext.View as SparkView;
            if (outerView != null && !ReferenceEquals(this, outerView))
            {
                // assume the values of the outer view collections
                foreach (var kv in outerView.Content)
                    Content.Add(kv.Key, kv.Value);
                foreach (var kv in outerView.OnceTable)
                    OnceTable.Add(kv.Key, kv.Value);
            }

            RenderView(writer);

            if (outerView != null && !ReferenceEquals(this, outerView))
            {
                // inject added values into outer view collections
                foreach (var kv in Content)
                {
                    if (!outerView.Content.ContainsKey(kv.Key))
                        Content.Add(kv.Key, kv.Value);
                }
                foreach (var kv in OnceTable)
                {
                    if (!outerView.OnceTable.ContainsKey(kv.Key))
                        outerView.OnceTable.Add(kv.Key, kv.Value);
                }
            }
            else
            {
                // proactively dispose named content. pools spoolwriter pages. avoids finalizers.
                foreach (var content in Content.Values)
                    content.Close();
            }
            Content.Clear();
        }

        
        public ViewDataDictionary ViewData
        {
            get
            {
                if (ExtendedContext.ViewData == null)
                    SetViewData(new ViewDataDictionary());
                return ExtendedContext.ViewData;
            }
            set { SetViewData(value); }
        }

        protected virtual void SetViewData(ViewDataDictionary viewData)
        {
            ExtendedContext.ViewData = viewData;
        }
        

        public string H(object value)
        {
            return Html.Encode(value);
        }

        public object Eval(string expression)
        {
            return ViewData.Eval(expression);
        }
        public string Eval(string expression, string format)
        {
            return ViewData.Eval(expression, format);
        }

    }

    public abstract class SparkView<TModel> : SparkView where TModel : class
    {
        protected SparkView()
            : base(null)
        {
        }
        protected SparkView(SparkViewBase<MvcViewContext> decorated)
            : base(decorated)
        {
        }

        private ViewDataDictionary<TModel> _viewData;

        public new ViewDataDictionary<TModel> ViewData
        {
            get
            {
                if (_viewData == null)
                    SetViewData(new ViewDataDictionary<TModel>());
                return _viewData;
            }
            set { SetViewData(value); }
        }

        public TModel Model
        {
            get { return ViewData.Model; }
        }

        protected override void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = new ViewDataDictionary<TModel>(viewData);
            base.SetViewData(_viewData);
        }
    }
}
