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

        //[Export]
        //[Name("HTML")]            //This is only necessary due a bug in VS2010 RC
        //[BaseDefinition("code")]  //http://social.msdn.microsoft.com/Forums/en-US/vseditorprerelease/thread/e427b157-08eb-4357-b0af-7370b5836b5e
        //internal static ContentTypeDefinition HTMLContentType = null;

        [Export]
        [FileExtension(".spark")]
        [ContentType("spark")]
        internal static FileExtensionToContentTypeDefinition SparkFileExtensionDefinition;
    }
}