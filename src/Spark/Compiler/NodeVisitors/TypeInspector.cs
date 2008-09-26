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
using System.Linq;

namespace Spark.Compiler.NodeVisitors
{
    public class TypeInspector
    {
        public TypeInspector(string dataDeclaration)
        {
            var decl = dataDeclaration.Trim();
            var lastSpace = decl.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            if (lastSpace < 0)
            {
                Type = dataDeclaration;
                return;
            }

            Name = decl.Substring(lastSpace + 1);

            if (!Name.ToCharArray().All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '@'))
            {
                Name = null;
                Type = dataDeclaration;
                return;
            }

            Type = decl.Substring(0, lastSpace).Trim();
        }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}