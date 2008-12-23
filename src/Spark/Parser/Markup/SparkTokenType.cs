namespace Spark.Parser.Markup
{
    //{L"Plain Text", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"String", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Tag Delimiter", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Operator", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Element Name", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Attribute Name", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Attribute Value", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Comment", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Entity", CI_YELLOW, CI_BLACK, FF_DEFAULT},
    //{L"HTML Server-Side Script", CI_YELLOW, CI_BLACK, FF_DEFAULT},

    public enum SparkTokenType
    {
        PlainText = 0,
        HtmlTagDelimiter,
        HtmlOperator,
        HtmlElementName,
        HtmlAttributeName,
        HtmlAttributeValue,
        HtmlComment,
        HtmlEntity,
        HtmlServerSideScript,
        String,
        SparkDelimiter,
    }
}
