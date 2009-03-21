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
using Spark.FileSystem;
using Spark.Parser;

namespace Spark.Compiler.NodeVisitors
{
    public enum NamespacesType
    {
        Unqualified,
        Qualified
    }

    public class VisitorContext
    {
        public VisitorContext()
        {
            Namespaces = NamespacesType.Unqualified;
            Paint = new Paint[0];
            PartialFileNames = new string[0];
        }

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        public string ViewPath { get; set; }
        public IViewFolder ViewFolder { get; set; }

        public string Prefix { get; set; }
        public NamespacesType Namespaces { get; set; }
        public IEnumerable<Paint> Paint { get; set; }
        public IList<string> PartialFileNames { get; set; }
        public ISparkExtensionFactory ExtensionFactory { get; set; }

    }
}
