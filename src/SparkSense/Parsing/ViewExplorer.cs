using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;
using System.IO;
using System;

namespace SparkSense.Parsing
{
    public enum MasterFolderTypes { Layouts, Shared }

    public class ViewExplorer : IViewExplorer
    {
        private ViewLoader _viewLoader;
        private string _viewPath;

        public ViewExplorer(IViewFolder viewFolder, string viewPath)
        {
            _viewLoader = new ViewLoader { ViewFolder = viewFolder, SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings()) };
            _viewPath = viewPath;
        }

        public string BasePath
        {
            get
            {
                return ((FileSystemViewFolder)_viewLoader.ViewFolder).BasePath;
            }
        }

        public IList<string> GetRelatedPartials()
        {
            return _viewLoader.FindPartialFiles(_viewPath);
        }

        public IList<string> GetGlobalVariables()
        {
            var masterFiles = new List<string>();

            GetMasterFiles(MasterFolderTypes.Shared, masterFiles);
            GetMasterFiles(MasterFolderTypes.Layouts, masterFiles);

            _viewLoader.Load(_viewPath);
            masterFiles.ToList().ForEach(filePath =>
            {
                var type = filePath.Contains(MasterFolderTypes.Shared.ToString()) ? MasterFolderTypes.Shared : MasterFolderTypes.Layouts;
                _viewLoader.Load(filePath.Substring(filePath.LastIndexOf(type.ToString())));
            });

            var globalVariables = new List<string>();
            var chunkLists = _viewLoader.GetEverythingLoaded();

            chunkLists.ToList().ForEach(list =>
            {
                var globals = list.Where(chunk => chunk is GlobalVariableChunk);
                globals.ToList().ForEach(globalVar => globalVariables.Add(((GlobalVariableChunk)globalVar).Name));
            });

            return globalVariables;
        }

        private void GetMasterFiles(MasterFolderTypes masterFolderType, List<string> masterFiles)
        {
            var masterFilePath = String.Format("{0}\\{1}", BasePath, masterFolderType);
            var masterFilePaths = Directory.Exists(masterFilePath) ? Directory.GetFiles(masterFilePath).Where(filePath => IsNonPartialSparkFile(filePath)) : new List<string>();
            masterFiles.AddRange(masterFilePaths.Where(filePath => IsValidMasterFile(filePath)));
        }

        private bool IsValidMasterFile(string filePath)
        {
            var controllerName = Path.GetDirectoryName(_viewPath);
            return 
                Path.GetFileName(filePath).Equals("Application.spark") || 
                filePath.EndsWith(String.Format("{0}.spark", controllerName));
        }
        private static bool IsNonPartialSparkFile(string filePath)
        {
            return 
                filePath.EndsWith(".spark") && 
                !Path.GetFileName(filePath).StartsWith("_");
        }

        public IList<string> GetLocalVariables()
        {
            var localVariables = new List<string>();
            var chunks = _viewLoader.Load(_viewPath);
            var locals = chunks.Where(chunk => chunk is LocalVariableChunk);
            locals.ToList().ForEach(local => localVariables.Add(((LocalVariableChunk)local).Name));
            return localVariables;
        }

        public static IViewExplorer CreateFromActiveDocument(IProjectExplorer projectExplorer)
        {
            if (projectExplorer == null || string.IsNullOrEmpty(projectExplorer.ActiveDocumentPath)) return null;
            var activeDocumentPath = projectExplorer.ActiveDocumentPath;

            int viewsLocationStart = activeDocumentPath.LastIndexOf("Views");
            var viewRoot = activeDocumentPath.Substring(0, viewsLocationStart + 5);
            var currentView = activeDocumentPath.Replace(viewRoot, string.Empty).TrimStart('\\');
            var viewFolder = new FileSystemViewFolder(viewRoot);

            return new ViewExplorer(viewFolder, currentView);
        }

    }
}
