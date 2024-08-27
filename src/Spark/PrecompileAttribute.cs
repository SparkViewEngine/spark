// Copyright 2008-2024 Louis DeJardin
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

namespace Spark
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PrecompileAttribute : Attribute
    {
        public PrecompileAttribute()
        {
            
        }
        public PrecompileAttribute(string include)
        {
            Include = include;
        }
        public PrecompileAttribute(string include, string layout)
        {
            Include = include;
            Layout = layout;
        }
        public string Include { get; set; }
        public string Exclude { get; set; }
        public string Layout { get; set; }
    }
}
