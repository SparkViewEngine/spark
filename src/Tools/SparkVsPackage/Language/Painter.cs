using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Spark.Parser;
using Spark.Parser.Markup;

namespace SparkVsPackage.Language
{
    public class Painter : Colorizer
    {
        private readonly SparkLanguageService _languageService;
        private readonly Parser _parser;

        public Painter(SparkLanguageService languageService, Parser parser)
            : base(languageService, parser.Buffer, new Scanner(languageService, parser))
        {
            _languageService = languageService;
            _parser = parser;
        }

        public override int GetStateMaintenanceFlag(out int flag)
        {
            flag = 0;
            return VSConstants.S_OK;
        }

        public override int ColorizeLine(int iLine, int iLength, IntPtr pszText, int iState, uint[] pAttributes)
        {
            _parser.Refresh();

            // grab all of the file information
            int lineStart;
            _parser.Buffer.GetPositionOfLine(iLine, out lineStart);

            // locate all the paint that overlaps this line and apply the token types

            int lineEnd = lineStart + iLength;
            var paints = _parser.GetPaint()
                .OfType<Paint<SparkTokenType>>()
                .Where(paint => paint.End.Offset >= lineStart && paint.Begin.Offset < lineEnd);

            foreach (var paint in paints)
            {
                int lowIndex = Math.Max(lineStart, paint.Begin.Offset) - lineStart;
                int highIndex = Math.Min(lineEnd, paint.End.Offset) - lineStart;
                for (int index = lowIndex; index < highIndex; ++index)
                {
                    if (index >= 0 && index < iLength)
                    {
                        pAttributes[index] = (uint)paint.Value;
                    }
                }
            }
            return VSConstants.S_OK;
        }

    }
}
