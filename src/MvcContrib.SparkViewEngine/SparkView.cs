using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Mvc;
using Spark;

namespace MvcContrib.SparkViewEngine
{
    public abstract class SparkView : AbstractSparkView, IViewDataContainer
    {
        public ViewContext ViewContext { get; set; }
        public TempDataDictionary TempData { get { return ViewContext.TempData; } }
        public HtmlHelper Html { get; set; }
        public UrlHelper Url { get; set; }
        public AjaxHelper Ajax { get; set; }

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

        [DebuggerNonUserCode]
        public string RenderView(ViewContext viewContext)
        {
            ViewContext = viewContext;
            ViewData = viewContext.ViewData;
            Html = new HtmlHelper(viewContext, this);
            Url = new UrlHelper(viewContext);
            Ajax = new AjaxHelper(viewContext);

            var writer = new StringWriter();
            RenderView(writer);
            return writer.ToString();
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
