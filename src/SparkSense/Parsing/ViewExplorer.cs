using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;
using System.IO;
using System;
using Spark.Parser.Code;
using Spark;

namespace SparkSense.Parsing
{
    public class ViewExplorer : IViewExplorer
    {
        private IProjectExplorer _projectExplorer;
        private ViewLoader _viewLoader;
        private string _viewPath;
        private IList<Chunk> _viewChunks;

        public ViewExplorer(IProjectExplorer projectExplorer)
        {
            if (projectExplorer == null)
                throw new ArgumentNullException("projectExplorer", "Project Explorer is null. We need a hook into the VS Environment");

            _projectExplorer = projectExplorer;
            InitCurrentView();
        }

        public string BasePath
        {
            get
            {
                return ((FileSystemViewFolder)_viewLoader.ViewFolder).BasePath;
            }
        }

        public IList<Chunk> ViewChunks
        {
            get
            {
                if (_viewChunks == null)
                    _viewChunks = _viewLoader != null && _viewPath != null ? _viewLoader.Load(_viewPath) : new List<Chunk>();
                return _viewChunks;
            }
        }

        public IEnumerable<IList<Chunk>> AllChunks
        {
            get { return _viewLoader != null ? _viewLoader.GetEverythingLoaded() : new List<IList<Chunk>>(); }
        }

        public IList<string> GetRelatedPartials()
        {
            return _viewLoader.FindPartialFiles(_viewPath);
        }

        public IList<string> GetGlobalVariables()
        {
            var globalVariables = new List<string>();
            var chunkLists = _viewLoader.GetEverythingLoaded();

            chunkLists.ToList().ForEach(list =>
            {
                var globals = list.Where(chunk => chunk is GlobalVariableChunk);
                globals.ToList().ForEach(globalVar => globalVariables.Add(((GlobalVariableChunk)globalVar).Name));
            });

            return globalVariables;
        }

        public IList<string> GetLocalVariables()
        {
            var allLocalVariables = new List<string>();
            var locals = GetViewChunks<LocalVariableChunk>();
            var assigned = GetViewChunks<AssignVariableChunk>();
            var viewData = GetViewChunks<ViewDataChunk>();

            locals.ToList().ForEach(x => allLocalVariables.Add(x.Name));
            assigned.ToList().ForEach(x => allLocalVariables.Add(x.Name));
            viewData.ToList().ForEach(x => allLocalVariables.Add(x.Name));

            return allLocalVariables;
        }

        public IEnumerable<T> GetViewChunks<T>()
        {
            var chunks = ViewChunks.Where(chunk => chunk is T).Cast<T>();
            return chunks;
        }

        public IList<string> GetContentNames()
        {
            var contentNames = new List<string>();
            var contentChunks = GetAllChunks<UseContentChunk>();
            contentChunks.ToList().ForEach(x => contentNames.Add((x.Name)));
            return contentNames;
        }

        public IList<string> GetLocalMacros()
        {
            var localMacros = new List<string>();
            var locals = GetViewChunks<MacroChunk>();
            locals.ToList().ForEach(x => localMacros.Add(x.Name));
            return localMacros;
        }

        public IList<string> GetMacroParameters(string macroName)
        {
            var macroParams = new List<string>();
            var macro = ViewChunks.Where(chunk => chunk is MacroChunk && ((MacroChunk)chunk).Name == macroName).FirstOrDefault();
            if (macro == null) return macroParams;
            ((MacroChunk)macro).Parameters.ToList().ForEach(p => macroParams.Add(p.Name));
            return macroParams;
        }

        private void InitCurrentView()
        {
            _viewLoader = new ViewLoader { ViewFolder = _projectExplorer.GetViewFolder(), SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings()) };
            _viewPath = _projectExplorer.GetCurrentView();
            InitViewChunks();
        }

        private IEnumerable<T> GetAllChunks<T>()
        {
            var allChunks = AllChunks.SelectMany(list => list).Where(chunk => chunk is T).Cast<T>();
            return allChunks;
        }

        private bool TryLoadMaster(string masterFile)
        {
            var locator = new DefaultTemplateLocator();
            var master = locator.LocateMasterFile(_viewLoader.ViewFolder, masterFile);
            
            if (master.ViewFile == null) return false;
            
            _viewLoader.Load(master.Path);
            return true;
        }
        
        private void InitViewChunks()
        {
            if (_viewLoader == null) return;

            var useMaster = GetViewChunks<UseMasterChunk>().FirstOrDefault();
            if (useMaster != null && TryLoadMaster(useMaster.Name)) return;

            var controllerName = Path.GetDirectoryName(_viewPath);
            if (TryLoadMaster(controllerName)) return;

            TryLoadMaster("Application");

        }
    }
}
