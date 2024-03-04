using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Spark.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// Result filter to pass the HTML Helper down to the spark view.
    /// </summary>
    public class HtmlHelperResultFilter(IHtmlHelper htmlHelper) : IAlwaysRunResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Controller is Controller controller)
            {
                // The HtmlHelper will have to contextualised before being used in the view
                // See SparkView.RenderAsync()
                controller.ViewData["Html"] = htmlHelper;
            }
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
        }
    }
}