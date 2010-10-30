using System;
using System.IO;
using System.Xml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.xml;

namespace Castle.MonoRail.Framework
{
    [AttributeUsage(AttributeTargets.ReturnValue, AllowMultiple = false, Inherited = false)]
    public class PdfReturnBinderAttribute : Attribute, IReturnBinder
    {
        private IEngineContext engineContext;
        private IController controller;
        private IControllerContext controllerContext;
        private Type returnType;
        private object returnValue;

        public void Bind(IEngineContext engineContext, IController controller,
            IControllerContext controllerContext, Type returnType, object returnValue)
        {
            this.returnValue = returnValue;
            this.returnType = returnType;
            this.controllerContext = controllerContext;
            this.controller = controller;
            this.engineContext = engineContext;

            ProcessResponse(returnValue);
        }

        private void ProcessResponse(object returnValue)
        {
            var document = new StringWriter();
            ModifyControllerContext();
            ProcessTemplate(document);
            CancelRenderView();
            SetContentType();
            RenderPdf(document);
        }

        private void RenderPdf(StringWriter document)
        {
            var pdfDocument = new Document();
            pdfDocument.Open();
            var pdfWriter = PdfWriter.GetInstance(pdfDocument, engineContext.Response.OutputStream);
            pdfWriter.CloseStream = false;

            using (var reader = new XmlTextReader(new StringReader(document.ToString())))
            {
                var parser = new XmlParser();
                parser.Go(pdfDocument, reader);
            }

            pdfWriter.Close();
        }

        private void ModifyControllerContext()
        {
            controllerContext.LayoutNames = null;
            controllerContext.PropertyBag.Add("Model", returnValue);
        }

        private void CancelRenderView()
        {
            controllerContext.SelectedViewName = null;
        }

        private void ProcessTemplate(StringWriter document)
        {
            engineContext.Services.ViewEngineManager
                .Process(controllerContext.SelectedViewName + ".pdf.spark", document, engineContext, controller, controllerContext);
        }

        private void SetContentType()
        {
            engineContext.Response.ContentType = "application/pdf";
        }
    }
}