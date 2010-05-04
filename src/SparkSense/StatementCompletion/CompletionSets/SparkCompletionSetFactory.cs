using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SparkSense.StatementCompletion.CompletionSets
{
    internal abstract class SparkCompletionSetFactory : CompletionSet
    {
        internal static ImageSource SparkTagIcon = new BitmapImage(new Uri(("Resources/SparkTag.png"), UriKind.Relative));
        internal static SparkCompletionSourceProvider _sourceProvider;
        internal static ITextBuffer _textBuffer;

        internal SparkCompletionSetFactory() : base("Spark Elements", "Spark Elements", null, null, null)
        {
        }

        public static CompletionSet Create<T>(SparkCompletionSourceProvider sourceProvider, ITextBuffer textBuffer, SnapshotPoint completionStartPoint) where T : SparkCompletionSetFactory, new()
        {
            _sourceProvider = sourceProvider;
            _textBuffer = textBuffer;

            var completionSet = new T();
            completionSet.ApplicableTo = completionStartPoint.Snapshot.CreateTrackingSpan(new Span(completionStartPoint, 0), SpanTrackingMode.EdgeInclusive);


            return completionSet;
        }
    }
}