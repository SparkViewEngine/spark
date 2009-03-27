using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using iTextSharp.text;
using PdfRendering.Model;
using Spark.Web.Mvc.Pdf;

namespace PdfRendering.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index(string format)
        {
            if (format == "pdf")
                return new PdfViewResult();

            return View();
        }


        public ActionResult Invoice(string invoiceNumber, string format)
        {
            var repos = new InvoiceRepository();
            var invoice = repos.GetInvoice(invoiceNumber);

            if (format == "xml")
            {
                Response.ContentType = "text/xml";
                return View(invoice);
            }

            return new PdfViewResult {ViewData = new ViewDataDictionary(invoice)};
        }

        public ActionResult iTextMarkup()
        {
            var markup = new List<ElementInfo>()
                .Add("registerfont", "path", "alias")
                .Add("header", "numbered", "align", "border")
                .Add("footer", "numbered", "align", "border")
                .Add("before", "leading", "line-height", "itext")
                .Add("after", "leading", "line-height", "itext")
                .Add("chunk",
                     "itext",
                     "vertical-align",
                     "backgroundcolor",
                     "localgoto", "localdestination", "subupscript", "generictag",
                     "remotegoto", "page", "destination",
                     ":@font")
                .Add("entity", "id")
                .Add("phrase", "leading", "line-height", "itext")
                .Add("anchor", "name", "reference")
                .Add("paragraph", "align", "indentationleft", "indentationright", ":phrase")
                .Add("title", "align", "indentationleft", "indentationright")
                .Add("list",
                     "numbered", "lettered", "lowercase", "authindent", "alignindent", "first",
                     "listsymbol", "indentationleft", "indentationright", "symbolindent")
                .Add("listitem", ":paragraph")

                .Add("cell", "horizontalalign", "verticalalign", "width", "colspan", "rowspan", "leading", "header",
                     "nowrap", ":@rectangle")
                .Add("table", "widths", "columns", "lastHeaderRow", "align", "cellspacing", "cellpadding", "offset",
                     "width",
                     "tablefitspage", "cellsfitpage", "convert2pdfp", ":@rectangle")
                .Add("section", "numberdepth", "indent", "indentationleft", "indentationright")
                .Add("chapter", ":section")
                .Add("image", "align", "left", "right", "middle", "underlying", "textwrap",
                     "absolutex", "absolutey", "plainwidth", "plainheight", "rotation")
                .Add("@font", "style", "encoding", "embedded", "font", "size", "style", "fontstyle", "red", "green",
                     "blue",
                     "color")
                .Add("@rectangle", "borderwidth", "left", "right", "top", "bottom",
                     "red", "green", "blue", "bordercolor",
                     "bgred", "bggreen", "bgblue", "backgroundcolor",
                     "grayfill");

            return View(markup);
        }

    }
}
