/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.Compiler;
using Spark.Compiler.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark.Parser
{
    public class ViewLoader
    {
        private const string templateFileExtension = "spark";

        private IViewFolder viewFolder;

        readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
        readonly List<string> _pending = new List<string>();

        public IViewFolder ViewFolder
        {
            get { return viewFolder; }
            set { viewFolder = value; }
        }

        public ParseAction<IList<Node>> Parser { get; set; }

        public ISparkExtensionFactory ExtensionFactory { get; set; }

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        private class Entry
        {
            private readonly FileContext fileContext = new FileContext();

            public string ViewPath
            {
                get { return FileContext.ViewSourcePath; }
                set { FileContext.ViewSourcePath = value; }
            }

            public long LastModified { get; set; }

            public IList<Chunk> Chunks
            {
                get { return FileContext.Contents; }
                set { FileContext.Contents = value; }
            }

            public FileContext FileContext
            {
                get { return fileContext; }
            }
        }

        Entry BindEntry(string referencePath)
        {
            if (_entries.ContainsKey(referencePath))
                return _entries[referencePath];

            var viewSource = viewFolder.GetViewSource(referencePath);

            var newEntry = new Entry { ViewPath = referencePath, LastModified = viewSource.LastModified };
            _entries.Add(referencePath, newEntry);
            _pending.Add(referencePath);
            return newEntry;
        }

        public virtual bool IsCurrent()
        {
            foreach (var entry in _entries.Values)
            {
                var viewSource = viewFolder.GetViewSource(entry.ViewPath);
                if (viewSource.LastModified != entry.LastModified)
                    return false;
            }
            return true;
        }


        //public IList<Chunk> Load(string controllerName, string viewName)
        //{
        //    return Load(ResolveView(controllerName, viewName));
        //}

        public IList<Chunk> Load(string viewPath)
        {
            if (string.IsNullOrEmpty(viewPath))
                return null;

            var entry = BindEntry(viewPath);
            if (entry == null)
                return null;

            while (_pending.Count != 0)
            {
                string nextPath = _pending.First();
                _pending.Remove(nextPath);
                LoadInternal(nextPath);
            }

            return entry.Chunks;
        }

        public IEnumerable<IList<Chunk>> GetEverythingLoaded()
        {
            return _entries.Values.Select(e => e.Chunks);
        }

        void LoadInternal(string viewPath)
        {
            if (string.IsNullOrEmpty(viewPath))
                return;

            var newEntry = BindEntry(viewPath);

            newEntry.Chunks = SyntaxProvider.GetChunks(viewPath, ViewFolder, ExtensionFactory);

            var fileReferenceVisitor = new FileReferenceVisitor();
            fileReferenceVisitor.Accept(newEntry.Chunks);

            foreach (var useFile in fileReferenceVisitor.References)
            {
                var referencePath = ResolveReference(viewPath, useFile.Name);

                if (!string.IsNullOrEmpty(referencePath))
                {
                    useFile.FileContext = BindEntry(referencePath).FileContext;
                }
                else
                {
                    int x = 5;
                }
            }
        }

        //private IList<Chunk> GetChunks(string viewPath, SourceContext sourceContext)
        //{
        //    var position = new Position(sourceContext);

        //    var nodes = Parser(position);
        //    if (nodes.Rest.PotentialLength() != 0)
        //    {
        //        string message = string.Format("Unable to parse view {0} around line {1} column {2}", viewPath,
        //                                       nodes.Rest.Line, nodes.Rest.Column);

        //        int beforeLength = Math.Min(30, nodes.Rest.Offset);
        //        int afterLength = Math.Min(30, nodes.Rest.PotentialLength());
        //        string before = position.Advance(nodes.Rest.Offset - beforeLength).Peek(beforeLength);
        //        string after = nodes.Rest.Peek(afterLength);

        //        throw new CompilerException(message + Environment.NewLine + before + "[error:]" + after);
        //    }

        //    var partialFileNames = FindPartialFiles(viewPath);

        //    var specialNodeVisitor = new SpecialNodeVisitor(partialFileNames, ExtensionFactory);
        //    specialNodeVisitor.Accept(nodes.Value);

        //    var forEachAttributeVisitor = new ForEachAttributeVisitor();
        //    forEachAttributeVisitor.Accept(specialNodeVisitor.Nodes);

        //    var conditionalAttributeVisitor = new ConditionalAttributeVisitor();
        //    conditionalAttributeVisitor.Accept(forEachAttributeVisitor.Nodes);

        //    var testElseElementVisitor = new TestElseElementVisitor();
        //    testElseElementVisitor.Accept(conditionalAttributeVisitor.Nodes);

        //    var chunkBuilder = new ChunkBuilderVisitor();
        //    chunkBuilder.Accept(testElseElementVisitor.Nodes);
        //    return chunkBuilder.Chunks;
        //}

        public IList<string> FindPartialFiles(string viewPath)
        {
            var results = new List<string>();

            string controllerPath = Path.GetDirectoryName(viewPath);
            foreach (var view in ViewFolder.ListViews(controllerPath))
            {
                string baseName = Path.GetFileNameWithoutExtension(view);
                if (baseName.StartsWith("_"))
                    results.Add(baseName.Substring(1));
            }
            foreach (var view in ViewFolder.ListViews("Shared"))
            {
                string baseName = Path.GetFileNameWithoutExtension(view);
                if (baseName.StartsWith("_"))
                    results.Add(baseName.Substring(1));
            }
            return results;
        }

        string ResolveReference(string existingViewPath, string viewName)
        {
            string controllerPath = Path.GetDirectoryName(existingViewPath);

            return ResolveView(controllerPath, viewName);
        }

        string ResolveView(string controllerName, string viewName)
        {
            if (string.IsNullOrEmpty(viewName))
                return null;

            string attempt1 = Path.Combine(controllerName, Path.ChangeExtension(viewName, templateFileExtension));
            if (ViewFolder.HasView(attempt1))
                return attempt1;

            string attempt2 = Path.Combine("Shared", Path.ChangeExtension(viewName, templateFileExtension));
            if (ViewFolder.HasView(attempt2))
                return attempt2;

            throw new FileNotFoundException(
                string.Format("Unable to find {0} or {1}", attempt1, attempt2),
                attempt1);
        }

        private SourceContext CreateSourceContext(string viewPath)
        {
            var viewSource = viewFolder.GetViewSource(viewPath);

            if (viewSource == null)
                throw new FileNotFoundException("View file not found", viewPath);

            using (TextReader reader = new StreamReader(viewSource.OpenViewStream()))
            {
                return new SourceContext(reader.ReadToEnd(), viewSource.LastModified);
            }
        }
    }
}
