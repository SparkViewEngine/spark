using System.Collections.Generic;

namespace SparkSense.Parsing
{
    public interface IProjectExplorer
    {
        string ActiveDocumentPath { get; }
        List<string> ViewMap { get; }
        bool ViewFolderExists();
        bool IsCurrentDocumentASparkFile();
    }
}
