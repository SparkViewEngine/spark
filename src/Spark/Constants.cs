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
using Spark.Compiler.NodeVisitors;

namespace Spark
{
    public class Constants
    {
        public const string Namespace = "http://sparkviewengine.com/";
        public const string MacroNamespace = "http://sparkviewengine.com/macro/";
        public const string ContentNamespace = "http://sparkviewengine.com/content/";
        public const string UseNamespace = "http://sparkviewengine.com/use/";
        public const string SegmentNamespace = "http://sparkviewengine.com/segment/";
        public const string RenderNamespace = "http://sparkviewengine.com/render/";

        public const string XIncludeNamespace = "http://www.w3.org/2001/XInclude";

        public const string SectionNamespace = "http://sparkviewengine.com/section/";
		
		public static readonly string Shared = "Shared";
		public static readonly string Layouts = "Layouts";
		public static readonly string GlobalSpark = "_global.spark";
		public static readonly string DotSpark = ".spark";
    }
}