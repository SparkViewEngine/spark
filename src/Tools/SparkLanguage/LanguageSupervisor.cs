using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Parser.Markup;
using SparkLanguagePackageLib;
using System.Runtime.InteropServices;

namespace SparkLanguage
{
    public class LanguageSupervisor : ILanguageSupervisor, IVsProvideColorableItems
    {
        public void OnSourceAssociated(ISparkSource pSource)
        {
            var sourceSupervisor = new SourceSupervisor(pSource);
            pSource.SetSupervisor(sourceSupervisor);
        }

        private static readonly Dictionary<SparkTokenType, IVsColorableItem> _colors =
            new Dictionary<SparkTokenType, IVsColorableItem>
                {
                    {SparkTokenType.HtmlTagDelimiter, new ColorableItem("Spark HTML Tag Delimiter", _COLORINDEX.CI_BLUE)},
                    {SparkTokenType.HtmlOperator, new ColorableItem("Spark HTML Operator", _COLORINDEX.CI_BLACK)},
                    {SparkTokenType.HtmlElementName, new ColorableItem("Spark HTML Element Name", _COLORINDEX.CI_MAROON)},
                    {SparkTokenType.HtmlAttributeName, new ColorableItem("Spark HTML Attribute Name", _COLORINDEX.CI_RED)},
                    {SparkTokenType.HtmlAttributeValue, new ColorableItem("Spark HTML Attribute Value", _COLORINDEX.CI_BLUE)},
                    {SparkTokenType.HtmlComment, new ColorableItem("Spark HTML Comment", _COLORINDEX.CI_DARKGREEN)},
                    {SparkTokenType.HtmlEntity, new ColorableItem("Spark HTML Entity", _COLORINDEX.CI_RED)},
                    {SparkTokenType.HtmlServerSideScript, new ColorableItem("Spark HTML Server-Side Script", _COLORINDEX.CI_BLACK, _COLORINDEX.CI_YELLOW)},
                    {SparkTokenType.String, new ColorableItem("String", _COLORINDEX.CI_MAROON)},
                    {SparkTokenType.SparkDelimiter, new ColorableItem("Spark Code Delimiter", _COLORINDEX.CI_AQUAMARINE)},
                };

        public void GetItemCount(out int piCount)
        {
            // SparkTokenType.PlainText is 0 and is implied
            // all other color indexes are 1 based, and will be
            // returned by GetColorableItem from the _color collection

            var greatestTokenType = Enum.GetValues(typeof(SparkTokenType)).Cast<SparkTokenType>().Max();
            piCount = (int)greatestTokenType;
        }

        public void GetColorableItem(int iIndex, out IVsColorableItem ppItem)
        {
            if (!_colors.TryGetValue((SparkTokenType)iIndex, out  ppItem))
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}
