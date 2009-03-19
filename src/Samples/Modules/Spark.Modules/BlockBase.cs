using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Spark.Modules
{
    public abstract class BlockBase : IBlock, IViewDataContainer
    {
        public ViewContext ViewContext { get; set; }
        public ViewDataDictionary ViewData { get; set; }

        public HtmlHelper Html { get; set; }
        public AjaxHelper Ajax { get; set; }
        public UrlHelper Url { get; set; }

        void IBlock.RenderBlock(ViewContext viewContext)
        {
            ViewData = viewContext.ViewData;
            ViewContext = viewContext;
            Html = new HtmlHelper(viewContext, this);
            Ajax = new AjaxHelper(viewContext, this);
            Url = new UrlHelper(viewContext.RequestContext);

            RenderBlock();
        }

        public abstract void RenderBlock();
    }
}
