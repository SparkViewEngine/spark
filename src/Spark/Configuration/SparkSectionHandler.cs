/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System.Collections.Generic;
using System.Configuration;
using System.Reflection;

namespace Spark.Configuration
{
    public class SparkSectionHandler : ConfigurationSection, ISparkSettings
    {
        [ConfigurationProperty("compilation")]
        public CompilationElement Compilation
        {
            get { return (CompilationElement)this["compilation"]; }
            set { this["compilation"] = value; }
        }

        public SparkSectionHandler SetDebug(bool debug)
        {
            Compilation.Debug = debug;
            return this;
        }

        public SparkSectionHandler AddAssembly(string assembly)
        {
            Compilation.Assemblies.Add(assembly);
            return this;
        }

        public SparkSectionHandler AddNamespace(string ns)
        {
            Compilation.Namespaces.Add(ns);
            return this;
        }

        public SparkSectionHandler AddAssembly(Assembly assembly)
        {
            Compilation.Assemblies.Add(assembly.FullName);
            return this;
        }

        bool ISparkSettings.Debug
        {
            get { return Compilation.Debug; }
        }

        IList<string> ISparkSettings.UseNamespaces
        {
            get
            {
                var result = new List<string>();
                foreach (NamespaceElement ns in Compilation.Namespaces)
                    result.Add(ns.Namespace);
                return result;
            }
        }

        IList<string> ISparkSettings.UseAssemblies
        {
            get
            {
                var result = new List<string>();
                foreach (AssemblyElement assembly in Compilation.Assemblies)
                    result.Add(assembly.Assembly);
                return result;
            }
        }
    }
}
