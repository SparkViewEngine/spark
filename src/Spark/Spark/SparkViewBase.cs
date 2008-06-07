using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;

namespace MvcContrib.SparkViewEngine
{
	public abstract class SparkViewBase : ISparkView, IViewDataContainer
	{
		private readonly Dictionary<string, StringBuilder> _content = new Dictionary<string, StringBuilder>();

		public ViewContext ViewContext { get; set; }
		public TempDataDictionary TempData { get { return ViewContext.TempData; } }
		public HtmlHelper Html { get; set; }
		public UrlHelper Url { get; set; }
		public AjaxHelper Ajax { get; set; }
		public Dictionary<string, StringBuilder> Content { get { return _content; } }

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


		public abstract string ProcessRequest();

		public virtual string RenderView(ViewContext viewContext)
		{
			ViewContext = viewContext;
			ViewData = viewContext.ViewData;
			Html = new HtmlHelper(viewContext, this);
			Url = new UrlHelper(viewContext);
			Ajax = new AjaxHelper(viewContext);
			return ProcessRequest();
		}


		protected StringBuilder BindContent(string name)
		{
			StringBuilder sb;
			if (!_content.TryGetValue(name, out sb))
			{
				sb = new StringBuilder();
				_content.Add(name, sb);
			}
			return sb;
		}

	}

	public abstract class SparkViewBase<TModel> : SparkViewBase where TModel : class
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
