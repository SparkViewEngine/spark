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
using System.Linq;
using System.Text;
using Spark.Compiler;
using Spark.Parser.Markup;

namespace Spark.Parser.Code
{
    public static class AttributeNodeExtensions
    {
        static CodeGrammar _grammar = new CodeGrammar();

        public static string AsCode(this AttributeNode attr)
        {
            var position = new Position(new SourceContext(attr.Value));
            var result = _grammar.Expression(position);
            return result.Value + result.Rest.Peek(result.Rest.PotentialLength());
        }

        public static string AsCodeInverted(this AttributeNode attr)
        {
            var expression = new ExpressionBuilder();
            foreach (var node in attr.Nodes)
            {
                if (node is TextNode)
                {
                    expression.AppendLiteral(((TextNode)node).Text);
                }
                else if (node is ExpressionNode)
                {
                    expression.AppendExpression(((ExpressionNode)node).Code);
                }
                else if (node is EntityNode)
                {
                    expression.AppendLiteral("&" + ((EntityNode)node).Name + ";");
                }
            }
            return expression.ToCode();
        }

    }
}
