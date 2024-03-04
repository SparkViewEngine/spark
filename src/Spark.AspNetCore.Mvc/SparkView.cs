using System.Configuration;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Spark.AspNetCore.Mvc;

public abstract class SparkView : SparkViewBase, IView
{
    /// <remarks>Set by <see cref="SparkView.RenderAsync"/> method.</remarks>
    protected ViewContext? ViewContext;
    
    public string Path { get; set; }

    #region Exposing properties

    /// <summary>
    /// Html encoder used to encode content.
    /// </summary>
    protected HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;

    /// <summary>
    /// Url encoder used to encode content.
    /// </summary>
    protected UrlEncoder UrlEncoder { get; set; } = UrlEncoder.Default;

    /// <summary>
    /// JavaScript encoder used to encode content.
    /// </summary>
    protected JavaScriptEncoder JavaScriptEncoder { get; set; } = JavaScriptEncoder.Default;

    public ViewDataDictionary ViewData => this.ViewContext.ViewData;

    public dynamic ViewBag => this.ViewContext.ViewBag;

    public ITempDataDictionary TempData => this.ViewContext.TempData;

    public HttpContext Context => this.ViewContext.HttpContext;

    public HttpRequest Request => this.ViewContext.HttpContext.Request;

    public HttpResponse Response => this.ViewContext.HttpContext.Response;

    public IHtmlHelper Html
    {
        get
        {
            if (this.ViewContext.ViewData.TryGetValue("Html", out object value))
            {
                return (IHtmlHelper) value;
            }

            throw new ConfigurationErrorsException($"Html not set in ViewData, is {nameof(Spark.AspNetCore.Mvc.Filters.HtmlHelperResultFilter)} configured?");
        }
    }

    #endregion

    public override void OutputValue(object value, bool automaticEncoding)
    {
        // Always encode when automatic encoding is enabled or value is IHtmlContent
        if (automaticEncoding || value is IHtmlContent)
        {
            OutputEncodedValue(value);
        }
        else
        {
            Output.Write(value);
        }
    }

    public void OutputEncodedValue(object value)
    {
        if (value is string stringValue)
        {
            var encoded = HtmlEncoder.Default.Encode(stringValue);
            
            Output.Write(encoded);
        }
        else if (value is IHtmlContent htmlContent)
        {
            htmlContent.WriteTo(Output, HtmlEncoder.Default);
        }
        else
        {
            Output.Write(value.ToString());
        }
    }

    public IHtmlContent HTML(object value)
    {
        return new HtmlString(Convert.ToString(value));
    }

    public object Eval(string expression)
    {
        return ViewData.Eval(expression);
    }
    public string Eval(string expression, string format)
    {
        return ViewData.Eval(expression, format);
    }

    public Task RenderAsync(ViewContext context)
    {
        this.ViewContext = context;

        // Checks if HtmlHelperResultFilter set the HTML helper in the view data
        if (this.ViewContext.ViewData.TryGetValue("Html", out var htmlHelper))
        {
            if (htmlHelper is IViewContextAware viewContextAware)
            {
                viewContextAware.Contextualize(this.ViewContext);
            }
        }

        var outerView = this.ViewContext.View as SparkViewBase;
        var isNestedView = outerView != null && ReferenceEquals(this, outerView) == false;

        var priorContent = this.Content;
        var priorOnce = this.OnceTable;
        TextWriter priorContentView = null;

        if (isNestedView)
        {
            // set aside the "view" content, to avoid modification
            if (outerView.Content.TryGetValue("view", out priorContentView))
            {
                outerView.Content.Remove("view");
            }

            // assume the values of the outer view collections
            this.Content = outerView.Content;
            this.OnceTable = outerView.OnceTable;
        }

        this.RenderView(context.Writer);

        if (isNestedView)
        {
            this.Content = priorContent;
            this.OnceTable = priorOnce;

            // restore previous state of "view" content
            if (priorContentView != null)
            {
                outerView.Content["view"] = priorContentView;
            }
            else if (outerView.Content.ContainsKey("view"))
            {
                outerView.Content.Remove("view");
            }
        }
        else
        {
            // proactively dispose named content. pools spoolwriter pages. avoids finalizers.
            foreach (var content in this.Content.Values)
            {
                content.Close();
            }
        }

        this.Content.Clear();

        return Task.CompletedTask;
    }
}
