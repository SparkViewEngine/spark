using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BindingsSample.Html {
    public static class SampleExtensions {
        public static object Colorize(this HtmlHelper html, string text, Color min, Color max, double value) {
            var color = Color.FromArgb(
                Range(min.R, max.R, value),
                Range(min.G, max.G, value),
                Range(min.B, max.B, value));

            var tagBuilder = new TagBuilder("span");
            tagBuilder.SetInnerText(text);

            tagBuilder.MergeAttribute("style", "color:" + ColorTranslator.ToHtml(color) + ";");
            return MvcHtmlString.Create(tagBuilder.ToString());
        }

        private static int Range(int min, int max, double value) {
            return min + (int)((max - min) * value);
        }
    }
}
