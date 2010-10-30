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
using System.Collections.Generic;
using Spark.Parser;
using Spark.Parser.Code;
using Spark.Parser.Markup;

namespace Spark.Compiler
{
    public class Chunk
    {
        public Position Position { get; set; }
    }

    public class SendLiteralChunk : Chunk
    {
        public string Text { get; set; }
    }

    public class SendExpressionChunk : Chunk
    {
        public Snippets Code { get; set; }
        //public IList<Snippet> Snippets { get; set; }

        public bool SilentNulls { get; set; }
        public bool AutomaticallyEncode { get; set; }
    }

    public class CodeStatementChunk : Chunk
    {
        public Snippets Code { get; set; }
        //public IList<Snippet> Snippets { get; set; }
    }

    public class GlobalVariableChunk : Chunk
    {
        public GlobalVariableChunk()
        {
            Type = "object";
        }
        public Snippets Name { get; set; }
        public Snippets Type { get; set; }
        public Snippets Value { get; set; }
    }

    public class LocalVariableChunk : Chunk
    {
        public LocalVariableChunk()
        {
            Type = "var";
        }

        public Snippets Name { get; set; }
        public Snippets Type { get; set; }
        public Snippets Value { get; set; }
    }

    public class DefaultVariableChunk : Chunk
    {
        public DefaultVariableChunk()
        {
            Type = "var";
        }
        public string Name { get; set; }
        public Snippets Type { get; set; }
        public Snippets Value { get; set; }
    }

    public class ViewDataChunk : Chunk
    {
        public ViewDataChunk()
        {
            Type = "object";
        }
        public Snippets Name { get; set; }
        public Snippets Type { get; set; }
        public string Key { get; set; }
        public Snippets Default { get; set; }
    }

    public class ViewDataModelChunk : Chunk
    {
        public Snippets TModel { get; set; }
        public Snippets TModelAlias { get; set; }
    }

    public class AssignVariableChunk : Chunk
    {
        public string Name { get; set; }
        public Snippets Value { get; set; }
    }

    public class UseContentChunk : Chunk
    {
        public UseContentChunk()
        {
            Default = new List<Chunk>();
        }
        public string Name { get; set; }
        public IList<Chunk> Default { get; set; }
    }

    public class RenderPartialChunk : Chunk
    {
        public RenderPartialChunk()
        {
            Body = new List<Chunk>();
            Sections = new Dictionary<string, IList<Chunk>>();
        }

        public string Name { get; set; }
        public FileContext FileContext { get; set; }
        public IList<Chunk> Body { get; set; }
        public IDictionary<string, IList<Chunk>> Sections { get; set; }
    }

    public class UseImportChunk : Chunk
    {
        public string Name { get; set; }
    }

    public class UseMasterChunk : Chunk
    {
        public string Name { get; set; }
    }

    public class RenderSectionChunk : Chunk
    {
        public RenderSectionChunk()
        {
            Default = new List<Chunk>();
        }

        public string Name { get; set; }
        public IList<Chunk> Default { get; set; }
    }

    public class UseNamespaceChunk : Chunk
    {
        public Snippets Namespace { get; set; }
    }

    public class UseAssemblyChunk : Chunk
    {
        public string Assembly { get; set; }
    }

    public class ContentChunk : Chunk
    {
        public ContentChunk()
        {
            Body = new List<Chunk>();
        }
        public string Name { get; set; }
        public IList<Chunk> Body { get; set; }
    }

    public enum ContentAddType
    {
        Replace,
        InsertBefore,
        AppendAfter
    }

    public class ContentSetChunk : Chunk
    {
        public ContentSetChunk()
        {
            Body = new List<Chunk>();
            AddType = ContentAddType.Replace;
        }
        public Snippets Variable { get; set; }
        public IList<Chunk> Body { get; set; }
        public ContentAddType AddType { get; set; }
    }

    public class ForEachChunk : Chunk
    {
        public ForEachChunk()
        {
            Body = new List<Chunk>();
        }
        public Snippets Code { get; set; }
        public IList<Chunk> Body { get; set; }
    }

    public class MacroChunk : Chunk
    {
        public MacroChunk()
        {
            Body = new List<Chunk>();
            Parameters = new List<MacroParameter>();
        }
        public string Name { get; set; }
        public IList<MacroParameter> Parameters { get; set; }
        public IList<Chunk> Body { get; set; }
    }

    public class MacroParameter
    {
        public string Name { get; set; }
        public Snippets Type { get; set; }
    }

    public class ScopeChunk : Chunk
    {
        public ScopeChunk()
        {
            Body = new List<Chunk>();
        }
        public IList<Chunk> Body { get; set; }
    }

    public class ConditionalChunk : Chunk
    {
        public ConditionalChunk()
        {
            Body = new List<Chunk>();
        }

        public ConditionalType Type { get; set; }
        public Snippets Condition { get; set; }
        public IList<Chunk> Body { get; set; }
        //public IList<Snippet> Snippets { get; set; }
    }

    public enum ConditionalType
    {
        If,
        Else,
        ElseIf,
        Once
    }

    public class ExtensionChunk : Chunk
    {
        public ExtensionChunk()
        {
            Body = new List<Chunk>();
        }

        public ISparkExtension Extension { get; set; }
        public IList<Chunk> Body { get; set; }
    }

    public class PageBaseTypeChunk : Chunk
    {
        public Snippets BaseClass { get; set; }
    }

    public class CacheChunk : Chunk
    {
        public CacheChunk()
        {
            Body = new List<Chunk>();
        }

        public Snippets Key { get; set; }
        public Snippets Expires { get; set; }
        public Snippets Signal { get; set; }

        public IList<Chunk> Body { get; set; }
    }

    public class MarkdownChunk : Chunk
    {
        public MarkdownChunk()
        {
            Body = new List<Chunk>();
        }
        public IList<Chunk> Body { get; set; }
    }
}
