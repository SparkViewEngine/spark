using System.Collections.Generic;
using System.Linq;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser;
using Spark.Parser.Syntax;

namespace SparkSense.Parsing
{
    public class ViewExplorer : IViewExplorer
    {
        private ViewLoader _viewLoader;
        private string _viewPath;

        public ViewExplorer(IViewFolder viewFolder, string viewPath)
        {
            _viewLoader = new ViewLoader { ViewFolder = viewFolder, SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings()) };
            _viewPath = viewPath;
        }

        public IList<string> GetRelatedPartials()
        {
            return _viewLoader.FindPartialFiles(_viewPath);
        }

        public IList<string> GetLocalVariables()
        {
            var localVariables = new List<string>();
            var chunks = _viewLoader.Load(_viewPath);
            var locals = chunks.Where(chunk => chunk is LocalVariableChunk);
            locals.ToList().ForEach(local => localVariables.Add(((LocalVariableChunk)local).Name));
            return localVariables;
        }

        public static IViewExplorer CreateFromActiveDocumentPath(string activeDocumentPath)
        {
            int viewsLocationStart = activeDocumentPath.LastIndexOf("Views");
            var viewRoot = activeDocumentPath.Substring(0, viewsLocationStart + 5);
            var currentView = activeDocumentPath.Replace(viewRoot, string.Empty).TrimStart('\\');
            var viewFolder = new FileSystemViewFolder(viewRoot);

            return new ViewExplorer(viewFolder, currentView);
        }

    }
}
