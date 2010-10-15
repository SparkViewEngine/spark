using Spark.FileSystem;
using Microsoft.VisualStudio.Text;
using System.ComponentModel.Design;

namespace SparkSense.Parsing
{
    public interface IProjectExplorer
    {
        bool ViewFolderExists();
        IViewFolder GetViewFolder();
        IViewExplorer GetViewExplorer(ITextBuffer textBuffer);
        string GetCurrentViewPath(ITextBuffer textBuffer);
        void SetViewContent(string viewPath, string content);
        ITypeDiscoveryService GetTypeDiscoveryService();
        ITypeResolutionService GetTypeResolverService();
    }
}
