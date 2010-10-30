using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.ContentType
{
    internal static class SparkFileAndContentTypes
    {
        [Export]
        [Name("spark")]
        [DisplayName("Spark")]
        [BaseDefinition("HTML")]
        internal static ContentTypeDefinition SparkContentTypeDefinition = null;

        [Export]
        [FileExtension(".spark")]
        [ContentType("spark")]
        internal static FileExtensionToContentTypeDefinition SparkFileExtensionDefinition = null;
    }
}