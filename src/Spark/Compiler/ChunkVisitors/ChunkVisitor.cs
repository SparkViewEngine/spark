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
namespace Spark.Compiler.ChunkVisitors
{
    public class ChunkVisitor : AbstractChunkVisitor
    {
        protected override void Visit(SendLiteralChunk chunk)
        {
        }

        protected override void Visit(ForEachChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
        }

        protected override void Visit(DefaultVariableChunk chunk)
        {
        }

        protected override void Visit(ContentChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
        }

        protected override void Visit(ScopeChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(RenderPartialChunk chunk)
        {
            Accept(chunk.Body);
            foreach (var chunks in chunk.Sections.Values)
                Accept(chunks);
        }

        protected override void Visit(UseImportChunk chunk)
        {
            
        }

        protected override void Visit(ContentSetChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(RenderSectionChunk chunk)
        {
            
        }

        protected override void Visit(ViewDataChunk chunk)
        {
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
        }

        protected override void Visit(GlobalVariableChunk chunk)
        {
        }

        protected override void Visit(UseContentChunk chunk)
        {
            Accept(chunk.Default);
        }

        protected override void Visit(UseNamespaceChunk chunk)
        {
        }

        protected override void Visit(UseAssemblyChunk chunk)
        {
        }

        protected override void Visit(UseMasterChunk chunk)
        {
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(ViewDataModelChunk chunk)
        {

        }

        protected override void Visit(ExtensionChunk chunk)
        {
            chunk.Extension.VisitChunk(this, OutputLocation.NotWriting, chunk.Body, null);
            //Accept(chunk.Body);
        }

        protected override void Visit(MacroChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
        }

        protected override void Visit(PageBaseTypeChunk chunk)
        {
        }

        protected override void Visit(CacheChunk chunk)
        {
            Accept(chunk.Body);
        }

        protected override void Visit(MarkdownChunk chunk)
        {
            Accept(chunk.Body);
        }
    }
}
