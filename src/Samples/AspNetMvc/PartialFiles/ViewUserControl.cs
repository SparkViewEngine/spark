using System.Web.UI;

namespace PartialFiles
{
    /// <summary>
    /// Default ViewUserControl base class for aspc files in this project.
    /// Set via system.web/pages/@userControlBaseType
    /// </summary>
    public class ViewUserControl : System.Web.Mvc.ViewUserControl
    {
        /// <summary>
        /// Must substitute the ViewContext TextWriter, because 
        /// the native HttpContext.Current TextWriter will be used by default
        /// </summary>
        public override void RenderControl(HtmlTextWriter writer)
        {
            if (ViewContext != null)
                writer.InnerWriter = ViewContext.HttpContext.Response.Output;

            base.RenderControl(writer);
        }
    }
}
