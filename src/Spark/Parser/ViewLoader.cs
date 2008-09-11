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

        public string Prefix { get; set; }

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

            // import _global.spark files from template path and shared path

            var perFolderGlobal = Path.GetDirectoryName(viewPath) + "\\_global.spark";
            if (ViewFolder.HasView(perFolderGlobal))
                BindEntry(perFolderGlobal);

            const string sharedGlobal = "Shared\\_global.spark";
            if (ViewFolder.HasView(sharedGlobal))
                BindEntry(sharedGlobal);

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

            VisitorContext context = new VisitorContext
                                         {
                                             ViewFolder = ViewFolder,
                                             Prefix = Prefix,
                                             ExtensionFactory = ExtensionFactory
                                         };
            newEntry.Chunks = SyntaxProvider.GetChunks(context, viewPath);

            var fileReferenceVisitor = new FileReferenceVisitor();
            fileReferenceVisitor.Accept(newEntry.Chunks);

            foreach (var useFile in fileReferenceVisitor.References)
            {
                var referencePath = ResolveReference(viewPath, useFile.Name);

                if (!string.IsNullOrEmpty(referencePath))
                {
                    useFile.FileContext = BindEntry(referencePath).FileContext;
                }
            }
        }


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
    }
}
