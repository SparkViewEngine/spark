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
using System.Linq;
using System.Text;
using Spark.Parser.Code;

namespace Spark.Compiler
{
    public class ExpressionBuilder
    {
        readonly IList<string> _parts = new List<string>();
        StringBuilder _literal;

        void Flush()
        {
            if (_literal != null)
            {
                _parts.Add(_literal + "\"");
                _literal = null;
            }
        }

        public string ToCode()
        {
            Flush();
            if (_parts.Count == 0)
            {
                return "\"\"";
            }
            if (_parts.Count == 1)
            {
                return _parts[0];    
            }
            return "string.Concat(" + string.Join(",", _parts.ToArray()) + ")";
        }

        public void AppendLiteral(string text)
        {
            if (_literal == null)
            {
                _literal = new StringBuilder("\"" + EscapeStringContents(text));
            }
            else
            {
                _literal.Append(EscapeStringContents(text));
            }
        }

        public void AppendExpression(Snippets code)
        {
            Flush();
            _parts.Add(code);
        }

        static string EscapeStringContents(string text)
        {
            return text.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\"", "\\\"");
        }
    }

}
