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
using System.Configuration;

namespace Spark.Configuration
{
    public class CompilationElement : ConfigurationElement
    {
        [ConfigurationProperty("debug")]
        public bool Debug
        {
            get => (bool)this["debug"];
            set => this["debug"] = value;
        }

        [ConfigurationProperty("nullBehaviour", DefaultValue = NullBehaviour.Lenient)]
        public NullBehaviour NullBehaviour
        {
            get => (NullBehaviour)this["nullBehaviour"];
            set => this["nullBehaviour"] = value;
        }

        [ConfigurationProperty("attributeBehaviour", DefaultValue = AttributeBehaviour.CodeOriented)]
        public AttributeBehaviour AttributeBehaviour
        {
            get => (AttributeBehaviour)this["attributeBehaviour"];
            set => this["attributeBehaviour"] = value;
        }

        [ConfigurationProperty("defaultLanguage", DefaultValue = LanguageType.Default)]
        public LanguageType DefaultLanguage
        {
            get => (LanguageType)this["defaultLanguage"];
            set => this["defaultLanguage"] = value;
        }

        [ConfigurationProperty("assemblies")]
        [ConfigurationCollection(typeof(AssemblyElementCollection))]
        public AssemblyElementCollection Assemblies
        {
            get => (AssemblyElementCollection)this["assemblies"];
            set => this["assemblies"] = value;
        }

        [ConfigurationProperty("excludeAssemblies")]
        [ConfigurationCollection(typeof(ExcludeAssemblyElementCollection))]
        public ExcludeAssemblyElementCollection ExcludeAssemblies
        {
            get => (ExcludeAssemblyElementCollection)this["excludeAssemblies"];
            set => this["excludeAssemblies"] = value;
        }
    }
}
