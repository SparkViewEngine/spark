using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;
using Spark.Spool;

namespace Spark.Web.Mvc.Pdf
{
    public class PdfViewResult : ViewResult
    {
        protected override ViewEngineResult FindView(ControllerContext context)
        {
            var result = base.FindView(context);
            if (result.View == null)
                return result;

            var pdfView = new PdfView(result);
            return new ViewEngineResult(pdfView, pdfView);
        }

        class PdfView : IView, IViewEngine
        {
            private readonly ViewEngineResult _result;

            public PdfView(ViewEngineResult result)
            {
                _result = result;
            }

            public void Render(ViewContext viewContext, TextWriter writer)
            {
                // generate view in memory
                var spoolWriter = new SpoolWriter();
                _result.View.Render(viewContext, spoolWriter);

                // detect itext (or html) format of response
                XmlParser parser;
                using (var reader = GetXmlReader(spoolWriter))
                {
                    while (reader.Read() && reader.NodeType != XmlNodeType.Element)
                    {
                        // no-op
                    }

                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "itext")
                        parser = new XmlParser();
                    else
                        parser = new HtmlParser();
                }

                // Create a document processing context
                var document = new Document();
                document.Open();

                // associate output with response stream
                var pdfWriter = PdfWriter.GetInstance(document, viewContext.HttpContext.Response.OutputStream);
                pdfWriter.CloseStream = false;

                // this is as close as we can get to being "success" before writing output
                // so set the content type now
                viewContext.HttpContext.Response.ContentType = "application/pdf";

                // parse memory through document into output
                using (var reader = GetXmlReader(spoolWriter))
                {
                    parser.Go(document, reader);
                }

                pdfWriter.Close();
            }

            private static XmlTextReader GetXmlReader(IEnumerable<string> source)
            {
                return new XmlTextReader(new SpoolReader(source));
            }

            public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
            {
                throw new System.NotImplementedException();
            }

            public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
            {
                throw new System.NotImplementedException();
            }

            public void ReleaseView(ControllerContext controllerContext, IView view)
            {
                _result.ViewEngine.ReleaseView(controllerContext, _result.View);
            }
        }
    }
}
