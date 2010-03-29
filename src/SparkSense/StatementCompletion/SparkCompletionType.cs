using System;
using System.Windows.Input;
using Microsoft.VisualStudio.Text;

namespace SparkSense.StatementCompletion
{
    public enum SparkCompletionTypes
    {
        None,
        Tag,
        Variable,
    }

    public class SparkCompletionType
    {
        public static SparkCompletionTypes GetCompletionType(Key key, ITextBuffer textBuffer, int position)
        {
            switch(key.ToString())
            {
                case "<":
                    return SparkCompletionTypes.Tag;
                default:
                    //if (Char.IsLetterOrDigit(key.ToString(),0))
                    //    return SparkCompletionTypes.Variable;
                    return SparkCompletionTypes.None;
            }
        }
    }
}