using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Helpers;
using Spark;

namespace Castle.MonoRail.Views.Spark
{
	public abstract class SparkView : AbstractSparkView
	{
	    protected SparkView()
        {
            ViewData = new SparkViewData(this);
        }

		private IEngineContext _context;
		private IControllerContext _controllerContext;

        public IEngineContext Context { get { return _context; } }
        public IControllerContext ControllerContext {get { return _controllerContext; } }

        public IController Controller { get { return _context.CurrentController; } }
        public IRequest Request { get { return _context.Request; } }
		public IResponse Response { get { return _context.Response; } }
		public IDictionary Session { get { return _context.Session; } }
		public Flash Flash { get { return _context.Flash; } }
		public string SiteRoot { get { return _context.ApplicationPath; } }

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


		public string RenderView(IEngineContext context, IControllerContext controllerContext)
		{
			_context = context;
			_controllerContext = controllerContext;
			return RenderView();
		}
	}
}
