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
    /// <summary>
    /// Generic version of the spark setting to set the BaseClassTypeName more conveniently.
    /// </summary>
    /// <typeparam name="TBaseClassOfView"></typeparam>
    public class SparkSettings<TBaseClassOfView> : SparkSettings, ISparkSettings
        where TBaseClassOfView : SparkViewBase
    {
        private string baseClassTypeName;

        public override string BaseClassTypeName => this.baseClassTypeName ??= typeof(TBaseClassOfView).FullName;
    }

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
            _excludeAssemblies = new List<string>();
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
        public virtual string BaseClassTypeName { get; private set; }
        public LanguageType DefaultLanguage { get; set; }
        public bool ParseSectionTagAsSegment { get; set; }

        public AttributeBehaviour AttributeBehaviour { get; set; }

        private readonly IList<string> _useNamespaces;
        public IEnumerable<string> UseNamespaces => _useNamespaces;

        private readonly IList<string> _useAssemblies;

        /// <summary>
        /// A list of names, full names or absolute paths to .dll for assemblies to load before compiling views.
        /// </summary>
        public IEnumerable<string> UseAssemblies => _useAssemblies;

        private readonly IList<string> _excludeAssemblies;
        public IEnumerable<string> ExcludeAssemblies => _excludeAssemblies;

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
        /// Sets the type each spark view will inherit from.
        /// </summary>
        /// <param name="typeFullName">The full name of the type.</param>
        /// <returns></returns>
        public SparkSettings SetBaseClassTypeName(string typeFullName)
        {
            this.BaseClassTypeName = typeFullName;

            return this;
        }

        /// <summary>
        /// Sets the type each spark view will inherit from.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public SparkSettings SetBaseClassTypeName(Type type)
        {
            this.BaseClassTypeName = type.FullName;

            return this;
        }

        /// <summary>
        /// Sets the type each spark view will inherit from.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public SparkSettings SetBaseClass<T>()
        {
            return this.SetBaseClassTypeName(typeof(T));
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
        /// Adds the name, full name or absolute path of an assembly for the view compiler to be aware of.
        /// </summary>
        /// <param name="assembly">The full name of an assembly.</param>
        /// <returns></returns>
        public SparkSettings AddAssembly(string assembly)
        {
            _useAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Adds an assembly for the view compiler to be aware of.
        /// </summary>
        public SparkSettings AddAssembly(Assembly assembly)
        {
            _useAssemblies.Add(assembly.Location);

            return this;
        }

        /// <summary>
        /// Adds the full name of an assembly for the view compiler to exclude (it won't be added as a reference when compiling).
        /// </summary>
        /// <param name="assembly">The full name of an assembly.</param>
        /// <returns></returns>
        public SparkSettings ExcludeAssembly(string assembly)
        {
            this._excludeAssemblies.Add(assembly);

            return this;
        }

        /// <summary>
        /// Keeps track of an assembly for the view compiler to exclude (it won't be added as a reference when compiling).
        /// </summary>
        public SparkSettings ExcludeAssembly(Assembly assembly)
        {
            this._excludeAssemblies.Add(assembly.FullName);

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
