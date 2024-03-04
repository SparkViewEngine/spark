using System.Web.Mvc;

namespace Spark.Web.Mvc
{
    public abstract class SparkView<TModel> : SparkView
    {
        private ViewDataDictionary<TModel> _viewData;
        private HtmlHelper<TModel> _htmlHelper;
        private AjaxHelper<TModel> _ajaxHelper;

        public TModel Model => ViewData.Model;

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
            get => _htmlHelper;
            set
            {
                _htmlHelper = value; 
                base.Html = value;
            }
        }

        public new AjaxHelper<TModel> Ajax
        {
            get => _ajaxHelper;
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
