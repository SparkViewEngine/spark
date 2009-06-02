// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spark.Compiler;
using Spark.Compiler.CSharp.ChunkVisitors;
using Spark.Compiler.NodeVisitors;
using Spark.Parser.Markup;
using Spark.FileSystem;
using Spark.Parser.Syntax;

namespace Spark.Parser
{
    public class ViewLoader
    {
        private const string templateFileExtension = ".spark";

        private IViewFolder _viewFolder;

        readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
        readonly List<string> _pending = new List<string>();

        public IViewFolder ViewFolder
        {
            get { return _viewFolder; }
            set { _viewFolder = value; }
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

            public IViewFile ViewFile { get; set; }

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

            var viewSource = _viewFolder.GetViewSource(referencePath);

            var newEntry = new Entry { ViewPath = referencePath, ViewFile = viewSource, LastModified = viewSource.LastModified };
            _entries.Add(referencePath, newEntry);
            _pending.Add(referencePath);
            return newEntry;
        }

        public virtual bool IsCurrent()
        {
            // The view is current if all entries' last modified value is the
            // same as when it was created. 
            return _entries.All(entry => entry.Value.ViewFile.LastModified == entry.Value.LastModified);
        }

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

            var context = new VisitorContext
                                         {
                                             ViewFolder = ViewFolder,
                                             Prefix = Prefix,
                                             ExtensionFactory = ExtensionFactory,
                                             PartialFileNames = FindPartialFiles(viewPath)
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

        private static IEnumerable<string> PartialViewFolderPaths(string viewPath)
        {
            var folderPath = Path.GetDirectoryName(viewPath);
            for(;;)
            {
                yield return folderPath;
                yield return Path.Combine(folderPath, "Shared");

                if (string.IsNullOrEmpty(folderPath))
                    yield break;

                folderPath = Path.GetDirectoryName(folderPath);
            }
        }

        private IEnumerable<string> FindAllPartialFiles(IEnumerable<string> folderPaths)
        {
            foreach(var folderPath in folderPaths.Distinct())
            {
                foreach(var view in ViewFolder.ListViews(folderPath))
                {
                    var baseName = Path.GetFileNameWithoutExtension(view);
                    if (baseName.StartsWith("_"))
                        yield return baseName.Substring(1);
                }
            }
        }

        public IList<string> FindPartialFiles(string viewPath)
        {
            var folderPaths = PartialViewFolderPaths(viewPath);
            var partialNames = FindAllPartialFiles(folderPaths);
            return partialNames.Distinct().ToArray();
        }

        string ResolveReference(string existingViewPath, string viewName)
        {
            var viewNameWithExtension = EnsureSparkExtension(viewName);
            var folderPaths = PartialViewFolderPaths(existingViewPath);
            
            var partialPaths = folderPaths.Select(x => Path.Combine(x, viewNameWithExtension));
            var partialViewLocation = partialPaths.FirstOrDefault(x => ViewFolder.HasView(x));

            if (partialViewLocation == null)
            {
                var message = string.Format("Unable to locate {0} in {1}", viewName, string.Join(", ", partialPaths.ToArray()));
                throw new FileNotFoundException(message, viewName);
            }

            return partialViewLocation;
        }

        static string EnsureSparkExtension(string viewName)
        {
            var needsSparkExtension = string.Equals(
                Path.GetExtension(viewName),
                templateFileExtension,
                StringComparison.OrdinalIgnoreCase) == false;

            return needsSparkExtension
                ? viewName + templateFileExtension
                : viewName;
        }
    }
}
