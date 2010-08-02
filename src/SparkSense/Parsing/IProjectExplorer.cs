using Spark.FileSystem;

namespace SparkSense.Parsing
{
    public interface IProjectExplorer
    {
        bool ViewFolderExists();
        IViewFolder GetViewFolder();
        string GetCurrentViewPath();
        void SetViewContent(string viewPath, string content);
    }
}
