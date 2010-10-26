using System;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;
using Spark.Parser.Markup;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public abstract class CompletionSetFactory : CompletionSet
    {
        protected static SnapshotPoint _triggerPoint;
        protected static IViewExplorer _viewExplorer;

        internal CompletionSetFactory() : base("Spark", "Spark", null, null, null) { }

        protected static Node CurrentNode
        {
            get { return _triggerPoint != null ? SparkSyntax.ParseNode(CurrentContent, _triggerPoint) : null; }
        }
        protected static Type CurrentContext
        {
            get { return _triggerPoint != null ? SparkSyntax.ParseContext(CurrentContent, _triggerPoint) : null; }
        }
        protected static string CurrentContent
        {
            get { return _triggerPoint != null ? _triggerPoint.Snapshot.GetText() : string.Empty; }
        }
        public override IList<Completion> Completions
        {
            get
            {
                //TODO: Rob G This is a general catch all trap because if something goes wrong during the Beta.
                // We don't was Visual Studio to explode, but rather just that intellisense stops working for 
                // this particular key press - it'll try again next time. The Beta will drive out most syntax issues.
                try
                {
                    return GetCompletionSetForNodeAndContext();
                }
                catch
                {
                    return new List<Completion>();
                }
            }
        }

        public static CompletionSet Create<T>(SnapshotPoint triggerPoint, ITrackingSpan trackingSpan, IViewExplorer viewExplorer) where T : CompletionSetFactory, new()
        {
            _triggerPoint = triggerPoint;
            _viewExplorer = viewExplorer;
            return new T { ApplicableTo = trackingSpan };
        }

        public static CompletionSet GetCompletionSetFor(SnapshotPoint triggerPoint, ITrackingSpan trackingSpan, IViewExplorer viewExplorer)
        {
            Type currentContext = SparkSyntax.ParseContext(triggerPoint.Snapshot.GetText(), triggerPoint);

            if (currentContext == typeof(ElementNode))
                return Create<ElementCompletionSet>(triggerPoint, trackingSpan, viewExplorer);
            if (currentContext == typeof(AttributeNode))
                return Create<AttributeCompletionSet>(triggerPoint, trackingSpan, viewExplorer);
            if (currentContext == typeof(ExpressionNode))
                return Create<ExpressionCompletionSet>(triggerPoint, trackingSpan, viewExplorer);
            return null;
        }

        protected static BitmapImage GetIcon(string iconName)
        {
            BitmapImage icon;
            try
            {
                icon = new BitmapImage(new Uri(String.Format("pack://application:,,,/SparkSense;component/Resources/{0}.png", iconName), UriKind.Absolute));
            }
            catch (UriFormatException ex)
            {
                icon = new BitmapImage();
            }
            return icon;
        }

        protected abstract IList<Completion> GetCompletionSetForNodeAndContext();
    }
}