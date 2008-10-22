using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.Compiler.ChunkVisitors;

namespace Spark.Compiler.CodeDom.ChunkVisitors
{
    public class GeneratedCodeVisitor : ChunkVisitor
    {
        private readonly CodeMemberMethod _method;

        public GeneratedCodeVisitor(CodeMemberMethod method)
        {
            _method = method;
        }

        protected override void Visit(SendLiteralChunk chunk)
        {
            _method.Statements.Add(
                new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Output"), "Write",
                        new CodePrimitiveExpression(chunk.Text))));
        }

        protected override void Visit(SendExpressionChunk chunk)
        {
            _method.Statements.Add(
                new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Output"), "Write",
                        new CodeSnippetExpression(chunk.Code))) {LinePragma = new CodeLinePragma("foo.spark", 23)});
        }
    }
}
