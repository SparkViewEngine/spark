using System;
using Microsoft.VisualStudio;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;


namespace SparkSense.StatementCompletion
{
    public static class KeyPressExtensions
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
    }
}
