using Spark.FileSystem;
using Microsoft.VisualStudio.Text;

namespace SparkSense.Parsing
{
    public interface IProjectExplorer
    {
        bool ViewFolderExists();
        IViewFolder GetViewFolder();
        string GetCurrentViewPath();
        void SetViewContent(string viewPath, ITextSnapshot currentSnapshot);
    }
}
