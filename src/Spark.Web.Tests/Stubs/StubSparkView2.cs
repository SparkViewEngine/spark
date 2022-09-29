using System;
using System.Web;

namespace Spark.Tests.Stubs
{
    public abstract class StubSparkView2 : AbstractSparkView
    {
        protected StubSparkView2()
        {
            ViewData = new StubViewData();
        }

        public StubViewData ViewData { get; set; }

        public string SiteRoot
        {
            get { return "/TestApp"; }
        }

        public string SiteResource(string path)
        {
            return SiteRoot + path.TrimStart('~');
        }

        public override bool TryGetViewData(string name, out object value)
        {
            return ViewData.TryGetValue(name, out value);
        }

        public string H(object content)
        {
            return HttpUtility.HtmlEncode(Convert.ToString(content));
        }

        public object Eval(string expression)
        {
            return ViewData.Eval(expression);
        }
    }

    public abstract class StubSparkView2<TModel> : StubSparkView2
    {
        public new StubViewData<TModel> ViewData
        {
            get { return (StubViewData<TModel>)base.ViewData; }
            set { base.ViewData = value; }
        }
    }

    public abstract class StubSparkView3<TModel, TMore> : StubSparkView2<TModel>
    {
        public TMore GetMore()
        {
            return default(TMore);
        }
    }
}