using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using SparkSense.Parsing;
using Spark.Parser;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.Operations;
using Spark.Parser.Markup;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public abstract class SparkCompletionSetFactory : CompletionSet
    {
        private ImageSource _sparkElementIcon;
        private ImageSource _sparkPartialIcon;
        private ImageSource _sparkAttributeIcon;
        protected static IViewExplorer _viewExplorer;
        protected static Node _currentNode;

        internal SparkCompletionSetFactory() : base("Spark", "Spark", null, null, null) { }

        public ImageSource SparkElementIcon
        {
            get
            {
                if (_sparkElementIcon == null)
                    _sparkElementIcon = GetIcon("SparkElement");
                return _sparkElementIcon;
            }
        }
        public ImageSource SparkPartialIcon
        {
            get
            {
                if (_sparkPartialIcon == null)
                    _sparkPartialIcon = GetIcon("SparkPartial");
                return _sparkPartialIcon;
            }
        }
        public ImageSource SparkAttributeIcon
        {
            get
            {
                if (_sparkAttributeIcon == null)
                    _sparkAttributeIcon = GetIcon("SparkAttribute");
                return _sparkAttributeIcon;
            }
        }

        public static CompletionSet Create<T>(IViewExplorer viewExplorer, ITrackingSpan trackingSpan, Node currentNode) where T : SparkCompletionSetFactory, new()
        {
            _viewExplorer = viewExplorer;
            _currentNode = currentNode;
            return new T { ApplicableTo = trackingSpan };
        }

        private static BitmapImage GetIcon(string iconName)
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
    }
}