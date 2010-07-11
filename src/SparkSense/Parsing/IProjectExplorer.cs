using System.Collections.Generic;
using Spark.FileSystem;

namespace SparkSense.Parsing
{
    public interface IProjectExplorer
    {
        List<string> ViewMap { get; }
        bool TryGetActiveDocumentPath(out string activeDocumentPath);
        bool ViewFolderExists();
        IViewFolder GetViewFolder();
        string GetCurrentView();
        bool IsCurrentDocumentASparkFile();
    }
}
