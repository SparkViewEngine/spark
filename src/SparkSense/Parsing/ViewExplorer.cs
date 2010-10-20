using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.Parser;
using Spark.Parser.Syntax;
using System.IO;
using System;
using Spark;
using System.Diagnostics;
using System.Reflection;

namespace SparkSense.Parsing
{
    public class ViewExplorer : IViewExplorer
    {
        private IProjectExplorer _projectExplorer;
        private Dictionary<string, Type> _sparkViewMembers;
        private ViewLoader _viewLoader;
        private readonly string _viewPath;
        private Dictionary<string, Type> _referencedTypes;

        public ViewExplorer(IProjectExplorer projectExplorer, string viewPath)
        {
            if (projectExplorer == null)
                throw new ArgumentNullException("projectExplorer", "Project Explorer is null. We need a hook into the VS Environment");

            _projectExplorer = projectExplorer;
            _viewPath = viewPath;
            _viewLoader = new ViewLoader
            {
                ViewFolder = _projectExplorer.GetViewFolder(),
                SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings())
            };
            InitViewChunks();
        }

        public IList<string> GetRelatedPartials()
        {
            return _viewLoader.FindPartialFiles(_viewPath);
        }

        public IList<string> GetGlobalVariables()
        {
            var globalVariables = new List<string>();

            GetAllChunks<GlobalVariableChunk>()
                .ToList()
                .ForEach(globalVar => globalVariables.Add(globalVar.Name));

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
            var scopeChunks = GetViewChunks<ScopeChunk>();
            var renderPartialChunks = scopeChunks.SelectMany(sc => ((ScopeChunk)sc).Body).Where(c => c is RenderPartialChunk);
            var partialChunk = renderPartialChunks.Where(pc => ((RenderPartialChunk)pc).Name == String.Format("_{0}", partialName)).FirstOrDefault() as RenderPartialChunk;
            if (partialChunk == null) return partialDefaults;

            var paramenters = partialChunk.FileContext.Contents.Where(c => c is DefaultVariableChunk);
            paramenters.ToList().ForEach(p => partialDefaults.Add(((DefaultVariableChunk)p).Name));
            return partialDefaults;
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
            var macro = GetViewChunks<MacroChunk>().Where(chunk => chunk.Name == macroName).FirstOrDefault();
            if (macro == null) return macroParams;
            ((MacroChunk)macro).Parameters.ToList().ForEach(p => macroParams.Add(p.Name));
            return macroParams;
        }

        public IList<string> GetInitialTypes()
        {
            var types = new List<string>();

            _referencedTypes = new Dictionary<string, Type>();
            _sparkViewMembers = new Dictionary<string, Type>();
            var discovery = _projectExplorer.GetTypeDiscoveryService();

            var allTypes = discovery.GetTypes(typeof(object), true);

            foreach (Type type in allTypes)
            {
                if (!type.IsPublic) continue;

                if (!_referencedTypes.ContainsKey(type.FullName))
                    _referencedTypes.Add(type.FullName, type);

                if (type.Name.EndsWith("SparkView"))
                {
                    var members = type.GetMethods();
                    foreach (var member in members)
                    {
                        if(member.IsPublic && !_sparkViewMembers.ContainsKey(member.Name))
                        {
                            _sparkViewMembers.Add(member.Name, member.ReturnType);
                            types.Add(member.Name);
                        }
                    }
                }

            }

            //var resolver = _projectExplorer.GetTypeResolverService();

            //var view = _referencedTypes["Spark.ISparkView"];
            //var mem = view.GetMembers();


            return types;
        }

        public IList<string> GetMembers()
        {

            return new List<string>() { "Testing", "123" };
        }

        public void InvalidateView(string newContent)
        {
            _projectExplorer.SetViewContent(_viewPath, newContent);
            _viewLoader.EvictEntry(_viewPath);
        }

        private IEnumerable<T> GetViewChunks<T>()
        {
            var chunks =
                _viewLoader.Load(_viewPath)
                .Where(chunk => chunk is T).Cast<T>();
            return chunks;
        }

        private IEnumerable<T> GetAllChunks<T>()
        {
            _viewLoader.Load(_viewPath);
            var allChunks =
                _viewLoader.GetEverythingLoaded()
                .SelectMany(list => list)
                .Where(chunk => chunk is T).Cast<T>();
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
