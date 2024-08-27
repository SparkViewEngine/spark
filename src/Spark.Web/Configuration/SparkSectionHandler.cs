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
using System.Configuration;
using System.Reflection;
using Spark.Parser;

namespace Spark.Configuration
{
    public class SparkSectionHandler : ConfigurationSection, ISparkSettings
    {
        [ConfigurationProperty("xmlns")]
        public string XmlNamespace
        {
            get => (string)this["xmlns"];
            set => this["xmlns"] = value;
        }

        [ConfigurationProperty("compilation")]
        public CompilationElement Compilation
        {
            get => (CompilationElement)this["compilation"];
            set => this["compilation"] = value;
        }

        [ConfigurationProperty("pages")]
        public PagesElement Pages
        {
            get => (PagesElement)this["pages"];
            set => this["pages"] = value;
        }

        [ConfigurationProperty("views")]
        public ViewFolderElementCollection Views
        {
            get => (ViewFolderElementCollection)this["views"];
            set => this["views"] = value;
        }

        public SparkSectionHandler SetDebug(bool debug)
        {
            Compilation.Debug = debug;
            return this;
        }

        public SparkSectionHandler SetBaseClassTypeName(string typeName)
        {
            Pages.BaseClassTypeName = typeName;
            return this;
        }

        public SparkSectionHandler SetBaseClassTypeName(Type type)
        {
            Pages.BaseClassTypeName = type.FullName;
            return this;
        }

        public SparkSectionHandler AddAssembly(string assembly)
        {
            Compilation.Assemblies.Add(assembly);
            return this;
        }

        public SparkSectionHandler AddAssembly(Assembly assembly)
        {
            Compilation.Assemblies.Add(assembly.FullName);
            return this;
        }

        public SparkSectionHandler AddNamespace(string ns)
        {
            Pages.Namespaces.Add(ns);
            return this;
        }
        
        bool ISparkSettings.Debug => Compilation.Debug;

        /// <summary>
        /// Returns where the app is installed.
        /// </summary>
        string ISparkSettings.RootPath => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;

        bool IParserSettings.AutomaticEncoding => Pages.AutomaticEncoding;

        string IParserSettings.StatementMarker => Pages.StatementMarker;

        NullBehaviour ISparkSettings.NullBehaviour => Compilation.NullBehaviour;

        AttributeBehaviour ISparkSettings.AttributeBehaviour => Compilation.AttributeBehaviour;

        string ISparkSettings.Prefix => Pages.Prefix;

        bool ISparkSettings.ParseSectionTagAsSegment => Pages.ParseSectionTagAsSegment;

        string ISparkSettings.BaseClassTypeName => Pages.BaseClassTypeName;

        LanguageType ISparkSettings.DefaultLanguage => Compilation.DefaultLanguage;

        IEnumerable<string> ISparkSettings.UseNamespaces
        {
            get
            {
                foreach (NamespaceElement ns in Pages.Namespaces)
                {
                    yield return ns.Namespace;
                }
            }
        }

        IEnumerable<string> ISparkSettings.UseAssemblies
        {
            get
            {
                foreach (AssemblyElement include in Compilation.Assemblies)
                {
                    yield return include.Assembly;
                }
            }
        }

        IEnumerable<string> ISparkSettings.ExcludeAssemblies
        {
            get
            {
                foreach (ExcludeAssemblyElement exclude in Compilation.ExcludeAssemblies)
                {
                    yield return exclude.Assembly;
                }
            }
        }

        IEnumerable<IResourceMapping> ISparkSettings.ResourceMappings
        {
            get
            {
                foreach (ResourcePathElement resource in Pages.Resources)
                {
                    yield return new SimpleResourceMapping { Match = resource.Match, Location = resource.Location };
                }
            }
        }

        IEnumerable<IViewFolderSettings> ISparkSettings.ViewFolders
        {
            get
            {
                foreach (ViewFolderElement viewFolder in Views)
                {
                    yield return viewFolder;
                }
            }
        }
    }
}
