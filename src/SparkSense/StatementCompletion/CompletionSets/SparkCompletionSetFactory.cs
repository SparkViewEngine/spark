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

        public static CompletionSet Create<T>(ITextBuffer textBuffer, SnapshotPoint triggerPoint) where T : SparkCompletionSetFactory, new()
        {
            _textBuffer = textBuffer;

            var completionSet = new T
            {
                ApplicableTo = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive)
            };

            return completionSet;
        }

        public static CompletionSet GetCompletionSetFor(ITextBuffer textBuffer, SnapshotPoint triggerPoint, SparkCompletionTypes completionType)
        {
            switch (completionType)
            {
                case SparkCompletionTypes.Tag:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(textBuffer, triggerPoint);
                case SparkCompletionTypes.Variable:
                    return SparkCompletionSetFactory.Create<SparkVariableCompletionSet>(textBuffer, triggerPoint);
                case SparkCompletionTypes.Invalid:
                    return SparkCompletionSetFactory.Create<SparkInvalidCompletionSet>(textBuffer, triggerPoint);
                case SparkCompletionTypes.None:
                default:
                    return null;
            }
        }

    }
}