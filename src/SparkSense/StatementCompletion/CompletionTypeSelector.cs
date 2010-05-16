using System;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion
{
    public class CompletionTypeSelector
    {
        private ITextBuffer _textBuffer;
        private int _caretPosition;
        private IProjectExplorer _projectExplorer;

        public CompletionTypeSelector(IProjectExplorer projectExplorer, ITextBuffer textBuffer, int caretPosition)
        {
            _projectExplorer = projectExplorer;
            _textBuffer = textBuffer;
            _caretPosition = caretPosition;
        }

        public SparkSyntaxTypes GetsyntaxType(char key)
        {
            if (!_projectExplorer.ViewFolderExists())
                return SparkSyntaxTypes.Invalid;

            switch (key)
            {
                case '<':
                    return SparkSyntaxTypes.Tag;
                default:
                    if (Char.IsLetterOrDigit(key.ToString(), 0))
                        return SparkSyntaxTypes.Variable;
                    return SparkSyntaxTypes.None;
            }
        }

    }
}