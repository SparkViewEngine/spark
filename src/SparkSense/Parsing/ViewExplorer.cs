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

        public ViewExplorer(IViewFolder viewRoot, string viewPath)
        {
            _viewLoader = new ViewLoader { ViewFolder = viewRoot, SyntaxProvider = new DefaultSyntaxProvider(new ParserSettings()) };
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
    }
}
