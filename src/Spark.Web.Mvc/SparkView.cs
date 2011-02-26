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
using System.IO;
using System.Web;
using System.Web.Mvc;
using Spark.Web.Mvc.Wrappers;

namespace Spark.Web.Mvc
{
    public abstract class SparkView : SparkViewBase, IViewDataContainer, ITextWriterContainer, IView
    {
        private string _siteRoot;
        private ViewDataDictionary _viewData;
        private ViewContext _viewContext;


        public TempDataDictionary TempData
        {
            get { return ViewContext.TempData; }
        }

        public HtmlHelper Html { get; set; }
        public UrlHelper Url { get; set; }
        public AjaxHelper Ajax { get; set; }

        public HttpContextBase Context
        {
            get { return ViewContext.HttpContext; }
        }

        public HttpRequestBase Request
        {
            get { return ViewContext.HttpContext.Request; }
        }

        public HttpResponseBase Response
        {
            get { return ViewContext.HttpContext.Response; }
        }

        public IResourcePathManager ResourcePathManager { get; set; }

        public ViewDataDictionary ViewData
        {
            get
            {
                if (_viewData == null)
                    SetViewData(new ViewDataDictionary());
                return _viewData;
            }
            set { SetViewData(value); }
        }

        public ViewContext ViewContext
        {
            get { return _viewContext; }
            set { SetViewContext(value); }
        }

        protected virtual void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = viewData;
        }

        protected virtual void SetViewContext(ViewContext viewContext)
        {
            _viewContext = viewContext;
            CreateHelpers();
        }

        protected virtual void CreateHelpers()
        {
            Html = new HtmlHelper(ViewContext, this);
            Url = new UrlHelper(ViewContext.RequestContext);
            Ajax = new AjaxHelper(ViewContext, this);
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
                if (_siteRoot == null)
                {
                    var appPath = ViewContext.HttpContext.Request.ApplicationPath;
                    if (string.IsNullOrEmpty(appPath) || string.Equals(appPath, "/"))
                    {
                        _siteRoot = string.Empty;
                    }
                    else
                    {
                        _siteRoot = "/" + appPath.Trim('/');
                    }
                }
                return _siteRoot;
            }
        }
        public string SiteResource(string path)
        {
            return ResourcePathManager.GetResourcePath(SiteRoot, path);
        }


        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var wrappedViewContext = new ViewContextWrapper(viewContext, this);

            ViewData = wrappedViewContext.ViewData;
            ViewContext = wrappedViewContext;

            var outerView = ViewContext.View as SparkView;
            var isNestedView = outerView != null && ReferenceEquals(this, outerView) == false;

            var priorContent = Content;
            var priorOnce = OnceTable;
            TextWriter priorContentView = null;

            if (isNestedView)
            {
                // set aside the "view" content, to avoid modification
                if (outerView.Content.TryGetValue("view", out priorContentView))
                    outerView.Content.Remove("view");

                // assume the values of the outer view collections
                Content = outerView.Content;
                OnceTable = outerView.OnceTable;
            }

            RenderView(writer);

            if (isNestedView)
            {
                Content = priorContent;
                OnceTable = priorOnce;

                // restore previous state of "view" content
                if (priorContentView != null)
                    outerView.Content["view"] = priorContentView;
                else if (outerView.Content.ContainsKey("view"))
                    outerView.Content.Remove("view");
            }
            else
            {
                // proactively dispose named content. pools spoolwriter pages. avoids finalizers.
                foreach (var content in Content.Values)
                    content.Close();
            }

            Content.Clear();
        }


        public string H(object value)
        {
            if (value is MvcHtmlString)
                return H((MvcHtmlString)value);
            return Html.Encode(value);
        }

        public string H(MvcHtmlString value)
        {
            return value == null ? null : value.ToString();
        }

        public MvcHtmlString HTML(object value)
        {
            return MvcHtmlString.Create(Convert.ToString(value));
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

    public abstract class SparkView<TModel> : SparkView
    {
        private ViewDataDictionary<TModel> _viewData;
        private HtmlHelper<TModel> _htmlHelper;
        private AjaxHelper<TModel> _ajaxHelper;

        public TModel Model
        {
            get { return ViewData.Model; }
        }

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

        public new HtmlHelper<TModel> Html
        {
            get { return _htmlHelper; }
            set
            {
                _htmlHelper = value; 
                base.Html = value;
            }
        }

        public new AjaxHelper<TModel> Ajax
        {
            get { return _ajaxHelper; }
            set
            {
                _ajaxHelper = value;
                base.Ajax = value;
            }
        }

        protected override void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = new ViewDataDictionary<TModel>(viewData);
            base.SetViewData(_viewData);
        }

        protected override void CreateHelpers()
        {
            Html = new HtmlHelper<TModel>(ViewContext, this);
            Url = new UrlHelper(ViewContext.RequestContext);
            Ajax = new AjaxHelper<TModel>(ViewContext, this);
        }
    }
}
