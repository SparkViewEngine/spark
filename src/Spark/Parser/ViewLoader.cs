//-------------------------------------------------------------------------
// <copyright file="ViewLoader.cs">
// Copyright 2008-2024 Louis DeJardin
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Spark.Bindings;
    using Spark.Compiler;
    using Spark.Compiler.CSharp.ChunkVisitors;
    using Spark.Compiler.NodeVisitors;
    using Spark.FileSystem;

    public class ViewLoader(
        ISparkSettings settings,
        IViewFolder viewFolder,
        IPartialProvider partialProvider,
        IPartialReferenceProvider partialReferenceProvider,
        ISparkExtensionFactory extensionFactory,
        ISparkSyntaxProvider syntaxProvider,
        IBindingProvider bindingProvider)
    {
        private readonly Dictionary<string, Entry> entries = new();
        private readonly List<string> pending = new();

        public string Prefix => settings.Prefix;

        public bool ParseSectionTagAsSegment => settings.ParseSectionTagAsSegment;

        public AttributeBehaviour AttributeBehaviour => settings.AttributeBehaviour;

        /// <summary>
        /// Returns a value indicating whether this view loader is current.
        /// </summary>
        /// <returns>
        /// True, if all entries' files' last modified values are the same as when this
        /// view loader was created; false, otherwise.
        /// </returns>
        public virtual bool IsCurrent()
        {
            // The view is current if all entries' last modified value is the
            // same as when it was created. 
            return this.entries.All(entry => entry.Value.ViewFile.LastModified == entry.Value.LastModified);
        }

        public IList<Chunk> Load(string viewPath)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                return null;
            }

            var entry = this.BindEntry(viewPath);
            if (entry == null)
            {
                return null;
            }

            // import _global.spark files from template path and shared path
            var perFolderGlobal = Path.Combine(Path.GetDirectoryName(viewPath), Constants.GlobalSpark);
            if (viewFolder.HasView(perFolderGlobal))
            {
                this.BindEntry(perFolderGlobal);
            }

            var sharedGlobal = Path.Combine(Constants.Shared, Constants.GlobalSpark);
            if (viewFolder.HasView(sharedGlobal))
            {
                this.BindEntry(sharedGlobal);
            }

            while (this.pending.Count != 0)
            {
                string nextPath = this.pending.First();
                this.pending.Remove(nextPath);
                this.LoadInternal(nextPath);
            }

            return entry.Chunks;
        }

        public IEnumerable<IList<Chunk>> GetEverythingLoaded()
        {
            return this.entries.Values.Select(e => e.Chunks);
        }

        public void EvictEntry(string referencePath)
        {
            if (this.entries.ContainsKey(referencePath))
                this.entries.Remove(referencePath);
        }

        /// <summary>
        /// Returns all partial files available based on the location of a view.
        /// </summary>
        /// <param name="viewPath">The location of the view used for reference.</param>
        /// <returns>All partial files available.</returns>
        public IList<string> FindPartialFiles(string viewPath)
        {
            var folderPaths = PartialViewFolderPaths(viewPath, false);
            var partialNames = this.FindAllPartialFiles(folderPaths);
            return partialNames.Distinct().ToList().AsReadOnly();
        }

        /// <summary>
        /// Walks up the passed <paramref name="viewName">view name</paramref>'s directory sturcture, returning all possible directories that could contain partial views.
        /// </summary>
        /// <param name="viewPath">The view path for which to return partial view paths.</param>
        /// <returns>The full list of possible partial view paths.</returns>
        private IEnumerable<string> PartialViewFolderPaths(string viewPath, bool allowCustomReferencePath)
        {
            if (allowCustomReferencePath)
            {
                return partialReferenceProvider.GetPaths(viewPath, allowCustomReferencePath);
            }
            else
            {
                return partialProvider.GetPaths(viewPath);
            }
        }

        /// <summary>
        /// Appends the propper extenstion to the passed <paramref name="viewName">view name</paramref>, if it does not already have it.
        /// </summary>
        /// <param name="viewName">The name of the view to which to append the extension.</param>
        /// <returns>The view name with the proper Spark extenstion.</returns>
        private static string EnsureSparkExtension(string viewName)
        {
            var needsSparkExtension = string.Equals(
                Path.GetExtension(viewName),
                Constants.DotSpark,
                StringComparison.OrdinalIgnoreCase) == false;

            return needsSparkExtension
                ? viewName + Constants.DotSpark
                : viewName;
        }

        private static string EnsureShadeExtension(string viewName)
        {
            var needsSparkExtension = string.Equals(
                Path.GetExtension(viewName),
                Constants.DotShade,
                StringComparison.OrdinalIgnoreCase) == false;

            return needsSparkExtension
                ? viewName + Constants.DotShade
                : viewName;
        }

        private Entry BindEntry(string referencePath)
        {
            if (this.entries.ContainsKey(referencePath))
            {
                return this.entries[referencePath];
            }

            var viewSource = viewFolder.GetViewSource(referencePath);

            var newEntry = new Entry
            {
                ViewPath = referencePath,
                ViewFile = viewSource,
                LastModified = viewSource.LastModified
            };
            this.entries.Add(referencePath, newEntry);
            this.pending.Add(referencePath);
            return newEntry;
        }

        private void LoadInternal(string viewPath)
        {
            if (string.IsNullOrEmpty(viewPath))
            {
                return;
            }

            var newEntry = this.BindEntry(viewPath);

            var context = new VisitorContext
            {
                ViewFolder = viewFolder,
                Prefix = this.Prefix,
                ExtensionFactory = extensionFactory,
                PartialFileNames = this.FindPartialFiles(viewPath),
                Bindings = this.FindBindings(viewPath),
                ParseSectionTagAsSegment = this.ParseSectionTagAsSegment,
                AttributeBehaviour = this.AttributeBehaviour,
            };

            newEntry.Chunks = syntaxProvider.GetChunks(context, viewPath);

            var fileReferenceVisitor = new FileReferenceVisitor();
            fileReferenceVisitor.Accept(newEntry.Chunks);

            foreach (var useFile in fileReferenceVisitor.References)
            {
                var referencePath = this.ResolveReference(viewPath, useFile.Name);

                if (!string.IsNullOrEmpty(referencePath))
                {
                    useFile.FileContext = this.BindEntry(referencePath).FileContext;
                }
            }
        }

        /// <summary>
        /// Finds all files in an enumerable list of possible locations that are named in the format of a partial file.
        /// </summary>
        /// <param name="folderPaths">The paths in which to search.</param>
        /// <returns>All files in the given paths that match the format of a partial file.</returns>
        /// <remarks>
        /// Partial files are files that begin with an underscore.
        /// </remarks>
        private IEnumerable<string> FindAllPartialFiles(IEnumerable<string> folderPaths)
        {
            foreach (var folderPath in folderPaths.Distinct())
            {
                foreach (var view in viewFolder.ListViews(folderPath))
                {
                    var baseName = Path.GetFileNameWithoutExtension(view);
                    if (baseName.StartsWith("_"))
                    {
                        yield return baseName.Substring(1);
                    }
                }
            }
        }

        private IEnumerable<Binding> FindBindings(string viewPath)
        {
            if (bindingProvider == null)
            {
                return Array.Empty<Binding>();
            }

            return bindingProvider.GetBindings(new BindingRequest(viewFolder) { ViewPath = viewPath });
        }

        private string ResolveReference(string existingViewPath, string viewName)
        {
            var viewNameWithSparkExtension = EnsureSparkExtension(viewName);
            var viewNameWithShadeExtension = EnsureShadeExtension(viewName);
            var folderPaths = PartialViewFolderPaths(existingViewPath, true);

            var partialPaths = folderPaths.SelectMany(x => new[]
            {
                Path.Combine(x, viewNameWithSparkExtension),
                Path.Combine(x, viewNameWithShadeExtension)
            });

            var partialViewLocation = partialPaths.FirstOrDefault(x => viewFolder.HasView(x));

            if (partialViewLocation == null)
            {
                var message = $"Unable to locate {viewName} in {string.Join(", ", partialPaths.ToArray())}";
                throw new FileNotFoundException(message, viewName);
            }

            return partialViewLocation;
        }

        private class Entry
        {
            private readonly FileContext fileContext = new FileContext();

            public string ViewPath
            {
                get => this.fileContext.ViewSourcePath;
                set => this.fileContext.ViewSourcePath = value;
            }

            public IList<Chunk> Chunks
            {
                get => this.fileContext.Contents;
                set => this.fileContext.Contents = value;
            }

            public FileContext FileContext => this.fileContext;

            public long LastModified { get; set; }

            public IViewFile ViewFile { get; set; }
        }
    }
}
