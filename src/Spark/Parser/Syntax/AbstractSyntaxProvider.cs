using System;
using System.Collections.Generic;
using System.IO;
using Spark.Compiler;
using Spark.FileSystem;

namespace Spark.Parser.Syntax
{
    public abstract class AbstractSyntaxProvider : ISparkSyntaxProvider
    {
        public abstract IList<Chunk> GetChunks(string viewPath, IViewFolder viewFolder, ISparkExtensionFactory extensionFactory, string prefix);

        protected SourceContext CreateSourceContext(string viewPath, IViewFolder viewFolder)
        {
            var viewSource = viewFolder.GetViewSource(viewPath);

            if (viewSource == null)
                throw new FileNotFoundException("View file not found", viewPath);

            using (var stream = viewSource.OpenViewStream())
            {
                string fileName = viewPath;
                if (stream is FileStream)
                    fileName = ((FileStream) stream).Name;

                using (TextReader reader = new StreamReader(stream))
                {
                    return new SourceContext(reader.ReadToEnd(), viewSource.LastModified, fileName);
                }
            }
        }

        public IList<string> FindPartialFiles(string viewPath, IViewFolder viewFolder)
        {
            var results = new List<string>();

            string controllerPath = Path.GetDirectoryName(viewPath);
            foreach (var view in viewFolder.ListViews(controllerPath))
            {
                string baseName = Path.GetFileNameWithoutExtension(view);
                if (baseName.StartsWith("_"))
                    results.Add(baseName.Substring(1));
            }
            foreach (var view in viewFolder.ListViews("Shared"))
            {
                string baseName = Path.GetFileNameWithoutExtension(view);
                if (baseName.StartsWith("_"))
                    results.Add(baseName.Substring(1));
            }
            return results;
        }

        protected void ThrowParseException(string viewPath, Position position, Position rest)
        {
            string message = string.Format("Unable to parse view {0} around line {1} column {2}", viewPath,
                                           rest.Line, rest.Column);

            int beforeLength = Math.Min(30, rest.Offset);
            int afterLength = Math.Min(30, rest.PotentialLength());
            string before = position.Advance(rest.Offset - beforeLength).Peek(beforeLength);
            string after = rest.Peek(afterLength);

            throw new CompilerException(message + Environment.NewLine + before + "[error:]" + after);
        }
    }
}