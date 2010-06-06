using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public abstract class SparkCompletionSetFactory : CompletionSet
    {
        internal static ImageSource SparkTagIcon = new BitmapImage(new Uri(("Resources/SparkTag.png"), UriKind.Relative));
        internal static ITextBuffer _textBuffer;
        internal static IViewExplorer _viewExplorer;
        internal static ITextExplorer _textExplorer;

        internal SparkCompletionSetFactory() : base("Spark Elements", "Spark Elements", null, null, null)
        {
        }

        public static CompletionSet Create<T>(ICompletionSession session, ITextBuffer textBuffer, IViewExplorer viewExplorer, ITextExplorer textExplorer) where T : SparkCompletionSetFactory, new()
        {
            _viewExplorer = viewExplorer;
            _textBuffer = textBuffer;
            _textExplorer = textExplorer;

            var triggerPoint = session.GetTriggerPoint(_textBuffer).GetPoint(_textBuffer.CurrentSnapshot);
            var completionSet = new T
            {
                ApplicableTo = triggerPoint.Snapshot.CreateTrackingSpan(new Span(triggerPoint, 0), SpanTrackingMode.EdgeInclusive)
            };

            return completionSet;
        }

        public static CompletionSet GetCompletionSetFor(ICompletionSession session, ITextBuffer textBuffer, IViewExplorer viewExplorer, SparkSyntaxTypes syntaxType, ITextExplorer textExplorer)
        {
            switch (syntaxType)
            {
                case SparkSyntaxTypes.Element:
                    return SparkCompletionSetFactory.Create<SparkTagCompletionSet>(session, textBuffer, viewExplorer, textExplorer);
                case SparkSyntaxTypes.Attribute:
                    return SparkCompletionSetFactory.Create<SparkAttributeCompletionSet>(session, textBuffer, viewExplorer, textExplorer);
                case SparkSyntaxTypes.Variable:
                    return SparkCompletionSetFactory.Create<SparkVariableCompletionSet>(session, textBuffer, viewExplorer, textExplorer);
                case SparkSyntaxTypes.Invalid:
                    return SparkCompletionSetFactory.Create<SparkInvalidCompletionSet>(session, textBuffer, viewExplorer, textExplorer);
                case SparkSyntaxTypes.None:
                default:
                    return null;
            }
        }

    }
}