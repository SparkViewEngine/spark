using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public abstract class SparkCompletionSetFactory : CompletionSet
    {
        internal static ImageSource SparkTagIcon = new BitmapImage(new Uri(("Resources/SparkTag.png"), UriKind.Relative));
        internal static ITextBuffer _textBuffer;

        internal SparkCompletionSetFactory() : base("Spark Elements", "Spark Elements", null, null, null)
        {
        }

        public static CompletionSet Create<T>(ITextBuffer textBuffer, SnapshotPoint completionStartPoint) where T : SparkCompletionSetFactory, new()
        {
            _textBuffer = textBuffer;

            var completionSet = new T
            {
                ApplicableTo = completionStartPoint.Snapshot.CreateTrackingSpan(new Span(completionStartPoint, 0), SpanTrackingMode.EdgeInclusive)
            };

            return completionSet;
        }

        public static CompletionSet GetCompletionSetFor(ITextBuffer textBuffer, SnapshotPoint completionStartPoint, SparkCompletionTypes completionType)
        {
            switch (completionType)
            {
                case SparkCompletionTypes.Tag:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(textBuffer, completionStartPoint);
                case SparkCompletionTypes.Variable:
                    return SparkCompletionSetFactory.Create<SparkVariableCompletionSet>(textBuffer, completionStartPoint);
                case SparkCompletionTypes.Invalid:
                    return SparkCompletionSetFactory.Create<SparkInvalidCompletionSet>(textBuffer, completionStartPoint);
                case SparkCompletionTypes.None:
                default:
                    return null;
            }
        }

    }
}