using System.Configuration;
using System.Reflection;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;

namespace Spark.AspNetCore.Mvc;

public abstract class SparkView<TModel> : SparkView
{
    private ViewDataDictionary<TModel>? viewData;
    private IHtmlHelper<TModel>? htmlHelper;

    public TModel? Model => this.ViewData.Model;

    public new ViewDataDictionary<TModel> ViewData => this.viewData ??= new ViewDataDictionary<TModel>(this.ViewContext.ViewData);

    public new IHtmlHelper<TModel> Html
    {
        get
        {
            if (this.htmlHelper == null)
            {
                if (this.ViewContext.ViewData.TryGetValue("Html", out object value))
                {
                    var plainHelper = (IHtmlHelper)value;

                    // TODO: Improve this as using reflection will fail if Microsoft's HtmlHelper implementation fails
                    var htmlGenerator = (IHtmlGenerator) typeof(HtmlHelper).GetField("_htmlGenerator", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value);
                    var compositeViewEngine = (ICompositeViewEngine) typeof(HtmlHelper).GetField("_viewEngine", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value);
                    var viewBufferScope = (IViewBufferScope) typeof(HtmlHelper).GetField("_bufferScope", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value);
                    var htmlEncoder = (HtmlEncoder) typeof(HtmlEncoder).GetField("_htmlEncoder", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(value);

                    this.htmlHelper = new HtmlHelper<TModel>(
                        htmlGenerator,
                        compositeViewEngine,
                        plainHelper.MetadataProvider,
                        viewBufferScope,
                        htmlEncoder,
                        plainHelper.UrlEncoder,
                        new ModelExpressionProvider(plainHelper.MetadataProvider));
                }
                else
                {
                    throw new ConfigurationErrorsException($"Html not set in ViewData, is {nameof(Filters.HtmlHelperResultFilter)} configured?");
                }
            }

            return this.htmlHelper;
        }
    }
}