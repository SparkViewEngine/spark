using System;
using Microsoft.VisualStudio.Text;
using Spark.Parser;
using Spark.Parser.Syntax;
using Spark.FileSystem;
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

    public class CompletionTypeSelector
    {
        private ITextBuffer _textBuffer;
        private int _caretPosition;
        private SparkProjectExplorer _projectExplorer;

        public CompletionTypeSelector(SparkProjectExplorer projectExplorer, ITextBuffer textBuffer, int caretPosition)
        {
            _projectExplorer = projectExplorer;
            _textBuffer = textBuffer;
            _caretPosition = caretPosition;
        }

        public SparkCompletionTypes GetCompletionType(char key)
        {
            if (!_projectExplorer.ViewFolderExists())
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

    }
}