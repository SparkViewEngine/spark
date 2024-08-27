// Copyright 2008-2024 Louis DeJardin
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
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
