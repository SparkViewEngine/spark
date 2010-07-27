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
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;
using Spark.Parser.Code;

namespace Spark.Compiler.Javascript.ChunkVisitors
{
    public class JavascriptGeneratedCodeVisitor : GeneratedCodeVisitorBase
    {
        private readonly StringBuilder _source;

        public JavascriptGeneratedCodeVisitor(StringBuilder source)
        {
            _source = source;
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            if (string.IsNullOrEmpty(chunk.Text))
                return;

            var text =
                chunk.Text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace(
                    "\"", "\\\"");
            _source.Append("Output.Write(\"").Append(text).AppendLine("\");");
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            if (chunk.SilentNulls)
                _source.Append("if(typeof(")
                       .Append(chunk.Code)
                       .Append(") != 'undefined') ");

            _source.Append("Output.Write(").Append(chunk.Code).AppendLine(");");
        }

        protected override void Visit(CodeStatementChunk chunk)
        {
            _source.Append(chunk.Code);
        }

        protected override void Visit(MacroChunk chunk)
        {
            // Must not do anything here. Macro is written out in PreRender.
        }

        protected override void Visit(LocalVariableChunk chunk)
        {
            if (Snippets.IsNullOrEmpty(chunk.Value))
            {
                _source.Append("var ").Append(chunk.Name).AppendLine(" = null;");
            }
            else
            {
                _source.Append("var ").Append(chunk.Name).Append(" = ").Append(chunk.Value).AppendLine(";");
            }
        }

        protected override void Visit(AssignVariableChunk chunk)
        {
            _source.Append(chunk.Name).Append(" = ").Append(chunk.Value).AppendLine(";");
        }

        protected override void Visit(ConditionalChunk chunk)
        {
            switch (chunk.Type)
            {
                case ConditionalType.If:
                    {
                        _source.Append("if (").Append(chunk.Condition).AppendLine(") {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
                case ConditionalType.ElseIf:
                    {
                        _source.Append("else if (").Append(chunk.Condition).AppendLine(") {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
                case ConditionalType.Else:
                    {
                        _source.AppendLine("else {");
                        Accept(chunk.Body);
                        _source.AppendLine("}");
                    }
                    break;
            }
        }

        protected override void Visit(ForEachChunk chunk)
        {
            var inspector = new ForEachInspector(chunk.Code);
            if (inspector.Recognized)
            {
                var detect = new DetectCodeExpressionVisitor(OuterPartial);
                var autoIndex = detect.Add(inspector.VariableName + "Index");
                var autoCount = detect.Add(inspector.VariableName + "Count");
                var autoIsFirst = detect.Add(inspector.VariableName + "IsFirst");
                var autoIsLast = detect.Add(inspector.VariableName + "IsLast");
                detect.Accept(chunk.Body);
                if (autoIsLast.Detected)
                {
                    autoIndex.Detected = true;
                    autoCount.Detected = true;
                }

            	string iteratorName = "__iter__" + inspector.VariableName;

                if (autoCount.Detected)
                {
                    // var itemCount=0;for(var __iter__item in coll){if(typeof(__iter__item)!='function'){++itemCount;}}
                    _source
                        .Append("var ")
                        .Append(inspector.VariableName)
                        .Append("Count=0;for(var ")
                        .Append(iteratorName)
                        .Append(" in ")
                        .Append(inspector.CollectionCode)
                        .Append("){ if(typeof(")
						.Append(inspector.CollectionCode)
						.Append("[")
						.Append(iteratorName)
						.Append("])!='function') {")
						.Append("++")
                        .Append(inspector.VariableName)
                        .Append("Count;}}");
                }

                if (autoIndex.Detected)
                {
                    // var itemIndex=0;
                    _source.Append("var ").Append(inspector.VariableName).Append("Index=0;");
                }

                if (autoIsFirst.Detected)
                {
                    // var itemIsFirst=true;
                    _source.Append("var ").Append(inspector.VariableName).Append("IsFirst=true;");
                }

                // for(var __iter__item in coll) {
                _source
                    .Append("for (var ")
                    .Append(iteratorName)
                    .Append(" in ")
                    .Append(inspector.CollectionCode)
                    .Append(") {");

                // var item=coll[__iter__item];
                _source
                    .Append("var ")
                    .Append(inspector.VariableName)
                    .Append("=")
                    .Append(inspector.CollectionCode)
                    .Append("[__iter__")
                    .Append(inspector.VariableName)
                    .Append("];");

            	// if(typeof(item)!='function') {
				_source.Append("if(typeof(")
					.Append(inspector.VariableName)
					.Append(")!='function') {");

                if (autoIsLast.Detected)
                {
                    // var itemIsLast=(itemIndex==itemCount-1);
                    _source
                        .Append("var ")
                        .Append(inspector.VariableName)
                        .Append("IsLast=(")
                        .Append(inspector.VariableName)
                        .Append("Index==")
                        .Append(inspector.VariableName)
                        .Append("Count-1);");
                }

                _source.AppendLine();

                Accept(chunk.Body);

                if (autoIsFirst.Detected)
                {
                    // itemIsFirst=false;
                    _source.Append(inspector.VariableName).Append("IsFirst=false;");
                }

                if (autoIndex.Detected)
                {
                    // ++itemIndex;
                    _source.Append("++").Append(inspector.VariableName).Append("Index;");
                }

                _source.AppendLine("}}");
            }
            else
            {
                _source.Append("for (").Append(chunk.Code).AppendLine(") {");
                Accept(chunk.Body);
                _source.Append("}");
            }
        }

        

        protected override void Visit(ScopeChunk chunk)
        {
            _source.AppendLine("{");
            base.Visit(chunk);
            _source.AppendLine("}");
        }

        protected override void Visit(ContentChunk chunk)
        {
            _source.Append("OutputScope('").Append(chunk.Name).AppendLine("'); {");
            Accept(chunk.Body);
            _source.AppendLine("} DisposeOutputScope();");
        }

        protected override void Visit(UseContentChunk chunk)
        {
            _source.Append("if (Content['").Append(chunk.Name).AppendLine("']) {");
            _source.Append("Output.Write(Content['").Append(chunk.Name).AppendLine("']);}");
            if (chunk.Default.Count != 0)
            {
                _source.AppendLine("else {");
                Accept(chunk.Default);
                _source.AppendLine("}");
            }
        }

        protected override void Visit(ContentSetChunk chunk)
        { 
            _source.AppendLine("OutputScope(new StringWriter()); {");
            Accept(chunk.Body);
            switch (chunk.AddType)
            {
                case ContentAddType.AppendAfter:
                    //format = "{0} = {0} + Output.ToString();";
                    _source
                        .Append(chunk.Variable)
                        .Append(" = ")
                        .Append(chunk.Variable)
                        .AppendLine(" + Output.toString();");
                    break;
                case ContentAddType.InsertBefore:
                    //format = "{0} = Output.ToString() + {0};";
                    _source
                        .Append(chunk.Variable)
                        .Append(" = Output.toString() + ")
                        .Append(chunk.Variable)
                        .AppendLine(";");
                    break;
                default:
                    //format = "{0} = Output.ToString();";
                     _source
                        .Append(chunk.Variable)
                        .Append(" = Output.toString();");
                    break;
            }

            _source.AppendLine("DisposeOutputScope();}");
        }

    }
}
