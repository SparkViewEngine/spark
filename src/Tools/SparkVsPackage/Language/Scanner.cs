using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Package;
using Spark.Parser;
using Spark.Parser.Markup;

namespace SparkVsPackage.Language
{
    public class Scanner : IScanner
    {
        private readonly SparkLanguageService _languageService;
        private readonly Parser _parser;
        private IEnumerator<Paint<SparkTokenType>> _paintEnumerator;

        public Scanner(SparkLanguageService languageService, Parser parser)
        {
            _languageService = languageService;
            _parser = parser;
        }



        public void SetSource(string source, int offset)
        {
            _parser.Refresh();

            int lineStart = offset;
            int lineEnd = lineStart + source.Length;

            _paintEnumerator = _parser.GetPaint()
                .OfType<Paint<SparkTokenType>>()
                .Where(paint => paint.End.Offset >= lineStart && paint.Begin.Offset < lineEnd)
                .OrderBy(paint => paint.Begin.Offset)
                .GetEnumerator();

        }

        private static TokenType ToTokenType(SparkTokenType sparkTokenType)
        {
            switch(sparkTokenType)
            {
                default:
                    return TokenType.Unknown;
            }
        }

        public bool ScanTokenAndProvideInfoAboutIt(TokenInfo tokenInfo, ref int state)
        {
            if (_paintEnumerator == null || _paintEnumerator.MoveNext() == false)
            {
                _paintEnumerator = null;
                return false;
            }

            tokenInfo.StartIndex = _paintEnumerator.Current.Begin.Offset;
            tokenInfo.EndIndex = _paintEnumerator.Current.End.Offset;
            tokenInfo.Type = ToTokenType(_paintEnumerator.Current.Value);
            return true;
        }

    }
}
