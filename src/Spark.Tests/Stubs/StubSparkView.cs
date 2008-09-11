namespace Spark.Tests.Stubs
{
    public abstract class StubSparkView : AbstractSparkView
    {
        protected StubSparkView()
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
            return SiteRoot + path;
        }
    }

    public abstract class StubSparkView<TModel> : StubSparkView
    {
        public new StubViewData<TModel> ViewData
        {
            get { return (StubViewData<TModel>)base.ViewData; }
            set { base.ViewData = value; }
        }
    }
}
