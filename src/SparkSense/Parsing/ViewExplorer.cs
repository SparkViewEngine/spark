using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Parser;
using Spark.Parser.Syntax;
using System.IO;
using System;
using Spark;
using System.Diagnostics;

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

        public IList<Chunk> ViewChunks
        {
            get
            {
                try
                {
                    if (_viewChunks == null)
                    {
                        _viewChunks = _viewLoader != null && _viewPath != null ? _viewLoader.Load(_viewPath) : new List<Chunk>();
                    }
                }
                catch (FileNotFoundException fileNotFound)
                {
                    Debug.WriteLine(fileNotFound.Message);
                    // TODO: Rob G : These are partials/include files being referenced from disk when they don't yet exist. 
                    // Highly likely to occur when writing new code, but the Spark Compiler complains of course.
                    // Need to add this to sqigglies notification later but for now just swallow the exceptions.
                    _viewChunks = new List<Chunk>();
                }
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

        public IList<string> GetPossibleMasterLayouts()
        {
            var possibleMasters = new List<string>();
            possibleMasters.AddRange(GetPossibleMasterFiles("Layouts"));
            possibleMasters.AddRange(GetPossibleMasterFiles("Shared"));
            return possibleMasters;
        }

        public IList<string> GetPossiblePartialDefaults(string partialName)
        {
            var partialDefaults = new List<string>();
            var scopeChunks = ViewChunks.Where(c => c is ScopeChunk);
            var renderPartialChunks = scopeChunks.SelectMany(sc => ((ScopeChunk)sc).Body).Where(c => c is RenderPartialChunk);
            var partialChunk = renderPartialChunks.Where(pc => ((RenderPartialChunk)pc).Name == String.Format("_{0}", partialName)).FirstOrDefault() as RenderPartialChunk;
            if (partialChunk == null) return partialDefaults;

            var paramenters = partialChunk.FileContext.Contents.Where(c => c is DefaultVariableChunk);
            paramenters.ToList().ForEach(p => partialDefaults.Add(((DefaultVariableChunk)p).Name));
            return partialDefaults;
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
        
        private IEnumerable<string> GetPossibleMasterFiles(string folder)
        {
            var possibleMasters = new List<string>();
            var masterFilePaths = _viewLoader.ViewFolder.ListViews(folder).Where(filePath => filePath.IsNonPartialSparkFile());
            masterFilePaths.ToList().ForEach(x => possibleMasters.Add(Path.GetFileNameWithoutExtension(x)));
            return possibleMasters;
        }

    }
}
