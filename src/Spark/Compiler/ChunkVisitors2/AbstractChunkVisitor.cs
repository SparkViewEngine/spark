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

namespace Spark.Compiler.ChunkVisitors
{
    public abstract class AbstractChunkVisitor : IChunkVisitor
    {
        public void Accept(IList<Chunk> chunks)
        {
            if (chunks == null) throw new ArgumentNullException("chunks");

            foreach (var chunk in chunks)
                Accept(chunk);
        }

        public void Accept(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException("chunk");

            if (chunk is SendLiteralChunk)
            {
                Visit((SendLiteralChunk)chunk);
            }
            else if (chunk is LocalVariableChunk)
            {
                Visit((LocalVariableChunk)chunk);
            }
            else if (chunk is SendExpressionChunk)
            {
                Visit((SendExpressionChunk)chunk);
            }
            else if (chunk is ForEachChunk)
            {
                Visit((ForEachChunk)chunk);
            }
            else if (chunk is ScopeChunk)
            {
                Visit((ScopeChunk)chunk);
            }
            else if (chunk is GlobalVariableChunk)
            {
                Visit((GlobalVariableChunk)chunk);
            }
            else if (chunk is AssignVariableChunk)
            {
                Visit((AssignVariableChunk)chunk);
            }
            else if (chunk is ContentChunk)
            {
                Visit((ContentChunk)chunk);
            }
            else if (chunk is ContentSetChunk)
            {
                Visit((ContentSetChunk)chunk);
            }
            else if (chunk is UseContentChunk)
            {
                Visit((UseContentChunk)chunk);
            }
            else if (chunk is RenderPartialChunk)
            {
                Visit((RenderPartialChunk)chunk);
            }
            else if (chunk is RenderSectionChunk)
            {
                Visit((RenderSectionChunk)chunk);
            }
            else if (chunk is ViewDataChunk)
            {
                Visit((ViewDataChunk)chunk);
            }
            else if (chunk is ViewDataModelChunk)
            {
                Visit((ViewDataModelChunk)chunk);
            }
            else if (chunk is UseNamespaceChunk)
            {
                Visit((UseNamespaceChunk)chunk);
            }
            else if (chunk is ConditionalChunk)
            {
                Visit((ConditionalChunk)chunk);
            }
            else if (chunk is ExtensionChunk)
            {
                Visit((ExtensionChunk)chunk);
            }
            else if (chunk is CodeStatementChunk)
            {
                Visit((CodeStatementChunk) chunk);
            }
            else if (chunk is MacroChunk)
            {
                Visit((MacroChunk) chunk);
            }
            else if (chunk is UseAssemblyChunk)
            {
                Visit((UseAssemblyChunk)chunk);
            }
            else if (chunk is UseImportChunk)
            {
                Visit((UseImportChunk) chunk);
            }
            else
            {
                throw new CompilerException(string.Format("Unknown chunk type {0}", chunk.GetType().Name));
            }
        }

        protected abstract void Visit(UseImportChunk chunk);

        protected abstract void Visit(ContentSetChunk chunk);

        protected abstract void Visit(RenderSectionChunk chunk);

        protected abstract void Visit(UseAssemblyChunk chunk);

        protected abstract void Visit(MacroChunk chunk);

        protected abstract void Visit(CodeStatementChunk chunk);

        protected abstract void Visit(ExtensionChunk chunk);

        protected abstract void Visit(ConditionalChunk chunk);
        protected abstract void Visit(ViewDataModelChunk chunk);
        protected abstract void Visit(ViewDataChunk chunk);
        protected abstract void Visit(RenderPartialChunk chunk);
        protected abstract void Visit(AssignVariableChunk chunk);
        protected abstract void Visit(UseContentChunk chunk);
        protected abstract void Visit(GlobalVariableChunk chunk);
        protected abstract void Visit(ScopeChunk chunk);
        protected abstract void Visit(ForEachChunk chunk);
        protected abstract void Visit(SendLiteralChunk chunk);
        protected abstract void Visit(LocalVariableChunk chunk);
        protected abstract void Visit(SendExpressionChunk chunk);
        protected abstract void Visit(ContentChunk chunk);
        protected abstract void Visit(UseNamespaceChunk chunk);
    }
}