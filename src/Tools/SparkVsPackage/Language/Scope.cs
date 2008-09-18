using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;

namespace SparkVsPackage.Language
{
    public class Scope : AuthoringScope
    {
        private readonly Parser _parser;

        public Scope(SparkLanguageService languageService, Parser parser)
        {
            _parser = parser;
        }

        public override string GetDataTipText(int line, int col, out TextSpan span)
        {
            span = new TextSpan();
            return null;
        }

        public override Declarations GetDeclarations(IVsTextView view, int line, int col, TokenInfo info, ParseReason reason)
        {
            throw new System.NotImplementedException();
        }

        public override Methods GetMethods(int line, int col, string name)
        {
            throw new System.NotImplementedException();
        }

        public override string Goto(VSConstants.VSStd97CmdID cmd, IVsTextView textView, int line, int col, out TextSpan span)
        {
            throw new System.NotImplementedException();
        }
    }
}
