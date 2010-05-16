using System;
using Microsoft.VisualStudio.Text;

namespace SparkSense.StatementCompletion
{
    public enum CompletionTypes
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
        private SparkSense.Parsing.IProjectExplorer _projectExplorer;

        public CompletionTypeSelector(SparkSense.Parsing.IProjectExplorer projectExplorer, ITextBuffer textBuffer, int caretPosition)
        {
            _projectExplorer = projectExplorer;
            _textBuffer = textBuffer;
            _caretPosition = caretPosition;
        }

        public CompletionTypes GetCompletionType(char key)
        {
            if (!_projectExplorer.ViewFolderExists())
                return CompletionTypes.Invalid;

            switch (key)
            {
                case '<':
                    return CompletionTypes.Tag;
                default:
                    if (Char.IsLetterOrDigit(key.ToString(), 0))
                        return CompletionTypes.Variable;
                    return CompletionTypes.None;
            }
        }

    }
}