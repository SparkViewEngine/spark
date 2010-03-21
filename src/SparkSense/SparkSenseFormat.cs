using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense
{
    #region Format definition
    /// <summary>
    /// Defines an editor format for the SparkSense type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = "SparkSense")]
    [Name("SparkSense")]
    [UserVisible(true)] //this should be visible to the end user
    [Order(Before = Priority.Default)] //set the priority to be after the default classifiers
    internal sealed class SparkSenseFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Defines the visual format for the "SparkSense" classification type
        /// </summary>
        public SparkSenseFormat()
        {
            this.DisplayName = "SparkSense"; //human readable version of the name
            this.BackgroundColor = Colors.BlueViolet;
            this.TextDecorations = System.Windows.TextDecorations.Underline;
        }
    }
    #endregion //Format definition
}
