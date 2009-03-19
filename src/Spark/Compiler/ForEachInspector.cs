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
using Spark.Parser.Code;

namespace Spark.Compiler
{
    public class ForEachInspector
    {
        public ForEachInspector(Snippets code)
        {
            var terms = code.ToString().Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.IndexOf("in");
            if (inIndex >= 1)
            {
                Recognized = true;
                VariableType = string.Join(" ", terms.ToArray(), 0, inIndex - 1);
                VariableName = terms[inIndex - 1];
                CollectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
            }
        }

        public bool Recognized { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string CollectionCode { get; set; }
    }
}
