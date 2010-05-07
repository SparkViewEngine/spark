using System;
using Microsoft.VisualStudio.Text;
using Spark.Parser;
using Spark.Parser.Syntax;
using Spark.FileSystem;
using SparkSense.StatementCompletion.CompletionSets;
using EnvDTE;

namespace SparkSense.StatementCompletion
{
    public enum SparkCompletionTypes
    {
        None,
        Tag,
        Variable,
        Invalid,
    }

    public class SparkCompletionType
    {
        private Document _activeDocument;
        private ITextBuffer _textBuffer;
        private int _cursorPosition;

        public SparkCompletionType(Document activeDocument, ITextBuffer textBuffer, int cursorPosition)
        {
            _activeDocument = activeDocument;
            _textBuffer = textBuffer;
            _cursorPosition = cursorPosition;
        }

        public SparkCompletionTypes GetCompletionType(char key)
        {
            if (!ViewFolderExists())
                return SparkCompletionTypes.Invalid;

            switch (key)
            {
                case '<':
                    return SparkCompletionTypes.Tag;
                default:
                    if (Char.IsLetterOrDigit(key.ToString(), 0))
                        return SparkCompletionTypes.Variable;
                    return SparkCompletionTypes.None;
            }
        }

        private bool ViewFolderExists()
        {
            int viewsLocationStart = _activeDocument.FullName.LastIndexOf("Views");
            return viewsLocationStart != -1;

            //var viewRoot = CurrentDocument.FullName.Substring(0, viewsLocationStart + 5);
            //var currentView = CurrentDocument.FullName.Replace(viewRoot, string.Empty).TrimStart('\\');

            //var syntaxProvider = new DefaultSyntaxProvider(new ParserSettings());
            //var viewLoader = new ViewLoader { ViewFolder = new FileSystemViewFolder(viewRoot), SyntaxProvider = syntaxProvider };
            //viewLoader.Load(currentView);
            //var partials = viewLoader.FindPartialFiles(currentView);
        }

    }
}