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

    public class SparkCompletionType
    {
        private ITextBuffer _textBuffer;
        private int _cursorPosition;
        private SparkProjectExplorer _sparkFileAnalyzer;

        public SparkCompletionType(SparkProjectExplorer sparkFileAnalyzer, ITextBuffer textBuffer, int cursorPosition)
        {
            _sparkFileAnalyzer = sparkFileAnalyzer;
            _textBuffer = textBuffer;
            _cursorPosition = cursorPosition;
        }

        public SparkCompletionTypes GetCompletionType(char key)
        {
            if (!_sparkFileAnalyzer.ViewFolderExists())
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