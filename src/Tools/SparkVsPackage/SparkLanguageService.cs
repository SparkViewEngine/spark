using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using Spark.Parser;
using Spark.Parser.Markup;
using SparkVsPackage.Language;

namespace SparkVsPackage
{
    [Guid(Constants.languageGuidString)]
    public class SparkLanguageService : LanguageService
    {
        private LanguagePreferences _preferences;

        public override string Name
        {
            get { return "Spark"; }
        }

        public override LanguagePreferences GetLanguagePreferences()
        {
            if (_preferences == null)
            {
                _preferences = new LanguagePreferences(Site, typeof(SparkLanguageService).GUID, Name);
                _preferences.Init();
            }
            return _preferences;
        }

        public override string GetFormatFilterList()
        {
            return "Spark File (*.spark)\r\n*.spark";
        }


        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return new Scanner(this, new Parser(buffer));
        }

        public override Colorizer GetColorizer(IVsTextLines buffer)
        {
            return new Painter(this, new Parser(buffer));
        }

        public override int GetItemCount(out int count)
        {
            count = ColorableItemList.Items.Count - 1;
            return VSConstants.S_OK;
        }

        public override int GetColorableItem(int index, out IVsColorableItem item)
        {
            item = ColorableItemList.Items[index];
            return VSConstants.S_OK;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            var parser = new Parser(req.Text);
            parser.Refresh();
            foreach (var paint in parser.GetPaint().OfType<Paint<Node>>())
            {
                // this is unsufficient... It'll need to go all the way to IList<Chunk> to be accurate
                if (paint.Value is ExpressionNode || paint.Value is StatementNode)
                {
                    req.Sink.CodeSpan(new TextSpan
                    {
                        iStartLine = paint.Begin.Line,
                        iStartIndex = paint.Begin.Column,
                        iEndLine = paint.End.Line,
                        iEndIndex = paint.End.Column
                    });
                }
            }
            return new Scope(this, parser);
        }

        public override int ValidateBreakpointLocation(IVsTextBuffer buffer, int line, int col, TextSpan[] pCodeSpan)
        {
            pCodeSpan[0].iStartLine = line;
            pCodeSpan[0].iStartIndex = 0;
            pCodeSpan[0].iEndLine = line;
            pCodeSpan[0].iEndIndex = 0;
            return VSConstants.E_NOTIMPL;
        }

        
    }
}