using System.Web.Mvc;

namespace NorthwindDemo.Views.Helpers {
    public static class ValidationHelpers 
    {
        public static string Label(this HtmlHelper html, string text, string name) 
        {
            string css = string.Empty;

            if (html.ViewContext.ViewData.ContainsKey("Error:" + name)) {
                css = " class=\"error\"";
            }

            string format = "<label for=\"{0}\"{2}>{1}</label>";
            return string.Format(format, name, text, css);
        }
    }
}
