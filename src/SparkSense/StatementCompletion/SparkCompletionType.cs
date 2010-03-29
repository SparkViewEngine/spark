using System;
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
        public static SparkCompletionTypes GetCompletionType(char key, ITextBuffer textBuffer, int position)
        {
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