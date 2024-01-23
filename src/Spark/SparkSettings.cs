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
using System.Reflection;
using Spark.Parser;

namespace Spark
{
    public class SparkSettings : ISparkSettings
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public SparkSettings() : this(null)
        {
            RootPath = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
        }

        /// <summary>
        /// Constructor that lets you specify the <see cref="rootPath"/>.
        /// </summary>
        /// <param name="rootPath">The path of the web app folder (the one containing the Views folder)</param>
        public SparkSettings(string rootPath)
        {
            RootPath = rootPath;

            _useNamespaces = new List<string>();
            _useAssemblies = new List<string>();
            _resourceMappings = new List<IResourceMapping>();
            _viewFolders = new List<IViewFolderSettings>();
            NullBehaviour = NullBehaviour.Lenient;
            AttributeBehaviour = AttributeBehaviour.CodeOriented;

            AutomaticEncoding = ParserSettings.DefaultAutomaticEncoding;
        }

        /// <summary>
        /// The path of the web app folder (the one containing the Views folder).
        /// </summary>
        public string RootPath { get; set; }

        public bool Debug { get; set; }
        public NullBehaviour NullBehaviour { get; set; }
        public bool AutomaticEncoding { get; set; }
        public string StatementMarker { get; set; }
        public string Prefix { get; set; }
        public string PageBaseType { get; set; }
        public LanguageType DefaultLanguage { get; set; }
        public bool ParseSectionTagAsSegment { get; set; }

        public AttributeBehaviour AttributeBehaviour { get; set; }

        private readonly IList<string> _useNamespaces;
        public IEnumerable<string> UseNamespaces => _useNamespaces;

        private readonly IList<string> _useAssemblies;
        public IEnumerable<string> UseAssemblies => _useAssemblies;

        private readonly IList<IResourceMapping> _resourceMappings;
        public IEnumerable<IResourceMapping> ResourceMappings => _resourceMappings;

        private readonly IList<IViewFolderSettings> _viewFolders;
        public IEnumerable<IViewFolderSettings> ViewFolders => _viewFolders;

        public SparkSettings SetDebug(bool debug)
        {
            Debug = debug;

            return this;
        }

        public SparkSettings SetAutomaticEncoding(bool automaticEncoding)
        {
            AutomaticEncoding = automaticEncoding;

            return this;
        }

        public SparkSettings SetStatementMarker(string statementMarker)
        {
            StatementMarker = statementMarker;

            return this;
        }

        public SparkSettings SetNullBehaviour(NullBehaviour nullBehaviour)
        {
            NullBehaviour = nullBehaviour;

            return this;
        }

        /// <summary>
        /// Sets the fully full name of the type each spark page should inherit from.
        /// </summary>
        /// <param name="typeName">The full name of the type.</param>
        /// <returns></returns>
        public SparkSettings SetPageBaseType(string typeName)
        {
            PageBaseType = typeName;

            return this;
        }

        /// <summary>
        /// Sets the type each spark page should inherit from.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public SparkSettings SetPageBaseType(Type type)
        {
            PageBaseType = type.FullName;

            return this;
        }

        /// <summary>
        /// Specifies the <see cref="LanguageType"/> the spark view should be compiled to.
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public SparkSettings SetDefaultLanguage(LanguageType language)
        {
            DefaultLanguage = language;

            return this;
        }

        /// <summary>
        /// Adds the full name of an assembly for the view compiler to be aware of.
        /// </summary>
        /// <param name="assembly">The full name of an assembly.</param>
        /// <returns></returns>
        public SparkSettings AddAssembly(string assembly)
        {
            _useAssemblies.Add(assembly);

            return this;
        }

        public SparkSettings AddAssembly(Assembly assembly)
        {
            _useAssemblies.Add(assembly.FullName);

            return this;
        }

        public SparkSettings AddNamespace(string ns)
        {
            _useNamespaces.Add(ns);

            return this;
        }

        public SparkSettings SetPrefix(string prefix)
        {
            Prefix = prefix;

            return this;
        }

        public SparkSettings SetParseSectionTagAsSegment(bool parseSectionTagAsSegment)
        {
            ParseSectionTagAsSegment = parseSectionTagAsSegment;

            return this;
        }

        public SparkSettings SetAttributeBehaviour(AttributeBehaviour attributeBehaviour)
        {
            AttributeBehaviour = attributeBehaviour;

            return this;
        }

        public SparkSettings AddResourceMapping(string match, string replace)
        {
            return AddResourceMapping(match, replace, true);
        }

        public SparkSettings AddResourceMapping(string match, string replace, bool stopProcess)
        {
            _resourceMappings.Add(new SimpleResourceMapping { Match = match, Location = replace, Stop = stopProcess });

            return this;
        }

        public SparkSettings AddViewFolder(Type customType, IDictionary<string, string> parameters)
        {
            _viewFolders.Add(new ViewFolderSettings
            {
                Name = customType.Name,
                Type = customType.AssemblyQualifiedName,
                Parameters = parameters
            });

            return this;
        }
    }
}
