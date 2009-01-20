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

namespace Spark
{
    public enum LanguageType
    {
        Default,
        CSharp,
        Javascript,
        Python,
        Ruby
    }

    public class SparkViewDescriptor
    {
        public SparkViewDescriptor()
        {
            //TODO: make language and accessors part of entry key
            Language = LanguageType.Default;
            Templates = new List<string>();
            Accessors = new List<Accessor>();
        }

        public string TargetNamespace { get; set; }
        public IList<string> Templates { get; set; }
        public IList<Accessor> Accessors { get; set; }
        public LanguageType Language { get; set; }

        public class Accessor
        {
            public string Property { get; set; }
            public string GetValue { get; set; }
        }

        public SparkViewDescriptor SetTargetNamespace(string targetNamespace)
        {
            TargetNamespace = targetNamespace;
            return this;
        }

        public SparkViewDescriptor SetLanguage(LanguageType language)
        {
            Language = language;
            return this;
        }

        public SparkViewDescriptor AddTemplate(string template)
        {
            Templates.Add(template);
            return this;
        }

        public SparkViewDescriptor AddAccessor(string property, string getValue)
        {
            Accessors.Add(new Accessor { Property = property, GetValue = getValue });
            return this;
        }


        public override int GetHashCode()
        {
            var hashCode = 0;

            hashCode ^= 
                Language.GetHashCode() 
                ^ (TargetNamespace ?? "").GetHashCode();

            foreach (var template in Templates)
                hashCode ^= template.ToLowerInvariant().GetHashCode();

            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var that = (SparkViewDescriptor)obj;
            
            if (!string.Equals(TargetNamespace, that.TargetNamespace))
                return false;

            if (Language != that.Language)
                return false;

            if (Templates.Count != that.Templates.Count)
                return false;

            for (var index = 0; index != Templates.Count; ++index)
            {
                if (!string.Equals(Templates[index], that.Templates[index], StringComparison.InvariantCultureIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
