using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense
{
    internal static class SparkSenseClassificationDefinition
    {
        /// <summary>
        /// Defines the "SparkSense" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name("SparkSense")]
        internal static ClassificationTypeDefinition SparkSenseType = null;
    }
}
