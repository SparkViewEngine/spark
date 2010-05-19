
using Microsoft.VisualStudio.Text;

using Microsoft.VisualStudio.Text.Editor;
using System;
using SparkSense.StatementCompletion;
namespace SparkSense.Parsing
{
    public enum SparkSyntaxTypes
    {
        None,
        Tag,
        Variable,
        Invalid,
    }

    public class SparkSyntax
    {
        private readonly IProjectExplorer _projectExplorer;
        private readonly IWpfTextView _textView;

        public SparkSyntax(IProjectExplorer projectExplorer, IWpfTextView textView)
        {
            _textView = textView;
            _projectExplorer = projectExplorer;
        }

        public bool IsSparkSyntax(char inputCharacter, out SparkSyntaxTypes syntaxType)
        {
            syntaxType = SparkSyntaxTypes.None;
            if (inputCharacter.Equals(char.MinValue)) return false;

            SnapshotPoint caretPoint;
            if (!TryGetCurrentCaretPoint(out caretPoint)) return false;

            if (!_projectExplorer.IsCurrentDocumentASparkFile()) return false;

            var sparksyntaxType = new CompletionTypeSelector(_projectExplorer, caretPoint.Snapshot.TextBuffer, caretPoint.Position);
            syntaxType = sparksyntaxType.GetsyntaxType(inputCharacter);
            return SparkSyntaxTypes.None != syntaxType;
        }

        private bool TryGetCurrentCaretPoint(out SnapshotPoint caretPoint)
        {
            caretPoint = new SnapshotPoint();
            SnapshotPoint? caret = _textView.Caret.Position.Point.GetPoint
                (textBuffer => _textView.TextBuffer == textBuffer, PositionAffinity.Predecessor);

            if (!caret.HasValue)
                return false;

            caretPoint = caret.Value;
            return true;
        }
    }
}
