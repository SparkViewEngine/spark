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
using System.IO;
using System.Linq;

namespace Spark
{	
	public class SparkViewAttribute : Attribute
    {
        public string TargetNamespace { get; set; }
        public string[] Templates { get; set; }

        public SparkViewDescriptor BuildDescriptor()
        {
            return new SparkViewDescriptor
			{
				TargetNamespace = TargetNamespace,
				Templates = Templates.Select(t => ConvertFromAttributeFormat(t)).ToList()
			};
        }

        public static string ConvertToAttributeFormat(string template)
        {
            // for compiled attribute purposes, all separators become a backslash
            // and backslashes are escaped
            return template
                .Replace(Path.DirectorySeparatorChar, '\\')
                .Replace(@"\", @"\\");
        }

        public static string ConvertFromAttributeFormat(string template)
        {
            // when compiled attributes are bound into descriptors, the
            // backslashes are treated as environment-specific seperators
            if (Path.DirectorySeparatorChar == '\\')
			{
				return template;
			}
			
            return template.Replace('\\', Path.DirectorySeparatorChar);
        }
    }
}