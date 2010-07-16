using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.Generic;


namespace SparkSense.StatementCompletion
{
    public static class CompletionExtensions
    {
        public static char GetInputCharacter(this uint key, Guid cmdGroup, IntPtr pvaIn)
        {
            if (cmdGroup == VSConstants.VSStd2K && key == (uint)VSConstants.VSStd2KCmdID.TYPECHAR)
                return (char)(ushort)Marshal.GetObjectForNativeVariant(pvaIn);
            return char.MinValue;
        }

        public static bool IsACommitCharacter(this uint key, char inputCharacter)
        {
            return key == (uint)VSConstants.VSStd2KCmdID.RETURN || key == (uint)VSConstants.VSStd2KCmdID.TAB || char.IsWhiteSpace(inputCharacter) || char.IsPunctuation(inputCharacter);
        }

        public static bool IsADeletionCharacter(this uint key)
        {
            return key == (uint)VSConstants.VSStd2KCmdID.BACKSPACE || key == (uint)VSConstants.VSStd2KCmdID.DELETE;
        }

        public static bool IsAMovementCharacter(this uint key)
        {
            return key == (uint)VSConstants.VSStd2KCmdID.LEFT || key == (uint)VSConstants.VSStd2KCmdID.RIGHT;
        }

        public static bool IsStartCompletionCharacter(this uint key)
        {
            return key == (uint)VSConstants.VSStd2KCmdID.COMPLETEWORD;
        }

        public static bool HasMovedOutOfIntelliSenseRange(this uint key, ITextView textView, ICompletionSession session)
        {
            if (textView == null) return true;

            var textBuffer = textView.TextBuffer;
            var caretPosition = textView.Caret.Position.BufferPosition.Position;
            var triggerPoint = session.GetTriggerPoint(textBuffer).GetPoint(textBuffer.CurrentSnapshot);
            ITrackingSpan completionSpan = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive);
            int completionSpanLength = completionSpan.GetSpan(textView.TextSnapshot).Length;

            switch (key)
            {
                case (uint)VSConstants.VSStd2KCmdID.LEFT:
                    return caretPosition < triggerPoint;
                case (uint)VSConstants.VSStd2KCmdID.RIGHT:
                    return caretPosition > triggerPoint + completionSpanLength;
                default:
                    return false;
            }
        }

        public static List<Completion> SortAlphabetically(this List<Completion> toSort) {
            if(toSort != null && toSort.Count > 1)
                toSort.Sort((c1, c2) => c1.DisplayText.CompareTo(c2.DisplayText));
            return toSort;
        }
    }
}
