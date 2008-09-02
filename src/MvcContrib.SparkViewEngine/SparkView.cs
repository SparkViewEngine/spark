using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Spark;

namespace MvcContrib.SparkViewEngine
{
    public abstract class SparkView : AbstractSparkView, IViewDataContainer, IView
    {
        public ViewContext ViewContext { get; set; }
        public TempDataDictionary TempData { get { return ViewContext.TempData; } }
        public HtmlHelper Html { get; set; }
        public UrlHelper Url { get; set; }
        public AjaxHelper Ajax { get; set; }

        public string H(object value)
        {
            return Html.Encode(value);
        }

        private string _siteRoot;
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

        private ViewDataDictionary _viewData;

        protected virtual void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = viewData;
        }

        public ViewDataDictionary ViewData
        {
            get
            {
                if (_viewData == null)
                    SetViewData(new ViewDataDictionary());
                return _viewData;
            }
            set
            {
                SetViewData(value);
            }
        }

        public void Render(ViewContext viewContext, TextWriter writer)
        {
            ViewContext = viewContext;
            ViewData = viewContext.ViewData;
            Html = new HtmlHelper(viewContext, this);
            Url = new UrlHelper(viewContext);
            Ajax = new AjaxHelper(viewContext);

            RenderView(writer);
        }
    }

    public abstract class SparkView<TModel> : SparkView where TModel : class
    {
        private ViewDataDictionary<TModel> _viewData;

        protected override void SetViewData(ViewDataDictionary viewData)
        {
            _viewData = new ViewDataDictionary<TModel>(viewData);
            base.SetViewData(_viewData);
        }

        public new ViewDataDictionary<TModel> ViewData
        {
            get
            {
                if (_viewData == null)
                    SetViewData(new ViewDataDictionary<TModel>());
                return _viewData;
            }
            set
            {
                SetViewData(value);
            }
        }
    }
}
