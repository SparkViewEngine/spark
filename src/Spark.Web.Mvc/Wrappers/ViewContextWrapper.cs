using System.IO;
using System.Web.Mvc;

namespace Spark.Web.Mvc.Wrappers
{
    public class ViewContextWrapper : ViewContext
    {
        private readonly ITextWriterContainer _textWriterContainer;

        public ViewContextWrapper(ViewContext viewContext, ITextWriterContainer textWriterContainer) :
            base(new ControllerContext(new HttpContextWrapper(viewContext.HttpContext, textWriterContainer), viewContext.RouteData, viewContext.Controller), viewContext.View, viewContext.ViewData, viewContext.TempData, viewContext.Writer)
        {
            _textWriterContainer = textWriterContainer;
        }

        public override TextWriter Writer
        {
            get
            {
                if (_textWriterContainer != null)
                    return _textWriterContainer.Output;
                return base.Writer;
            }
            set
            {
                if (_textWriterContainer != null)
                    _textWriterContainer.Output = value;
                base.Writer = value;
            }
        }
    }
}
