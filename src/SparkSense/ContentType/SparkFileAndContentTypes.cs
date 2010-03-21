using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace SparkSense.ContentType
{
    internal static class SparkFileAndContentTypes
    {
        [Export] [Name("spark")] [BaseDefinition("text")] 
        internal static ContentTypeDefinition SparkContentTypeDefinition;

        [Export] [ContentType("spark")] [FileExtension(".spark")] 
        internal static FileExtensionToContentTypeDefinition SparkFileExtensionDefinition;
    }
}