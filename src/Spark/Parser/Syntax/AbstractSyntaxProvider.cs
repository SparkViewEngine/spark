// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using Spark.Compiler;
using Spark.Compiler.NodeVisitors;
using Spark.FileSystem;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark.Parser.Syntax
{
    public abstract class AbstractSyntaxProvider : ISparkSyntaxProvider
    {
        public abstract IList<Chunk> GetChunks(VisitorContext context, string path);
        public abstract IList<Node> IncludeFile(VisitorContext context, string path, string parse);
        public abstract Snippets ParseFragment(Position begin, Position end);

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