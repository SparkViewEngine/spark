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
using HttpContextWrapper=Spark.Web.Mvc.Wrappers.HttpContextWrapper;

namespace Spark.Web.Mvc
{
    public abstract class SparkView : AbstractSparkView, IViewDataContainer, IView
    {
        private string _siteRoot;
        private ViewDataDictionary _viewData;
        public ViewContext ViewContext { get; set; }

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

        #region IView Members

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var httpContext = new HttpContextWrapper(viewContext.HttpContext, this);
            var wrapped = new ViewContext(httpContext, viewContext.RouteData, viewContext.Controller,
                                          viewContext.View, viewContext.ViewData, viewContext.TempData);

            ViewContext = wrapped;
            ViewData = wrapped.ViewData;
            Html = new HtmlHelper(wrapped, this);
            Url = new UrlHelper(wrapped);
            Ajax = new AjaxHelper(wrapped);

            RenderView(writer);

            // proactively dispose named content. pools spoolwriter pages. avoids finalizers.
            foreach (var content in Content.Values)
                content.Close();

            Content.Clear();
        }

        #endregion

        #region IViewDataContainer Members

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

        #endregion

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

        protected virtual void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = viewData;
        }
    }

    public abstract class SparkView<TModel> : SparkView where TModel : class
    {
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
