// Copyright 2008 Louis DeJardin - http://whereslou.com
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
using Spark.FileSystem;

namespace Spark
{
    public class SparkSettings : ISparkSettings
    {
        public SparkSettings()
        {
            _useNamespaces = new List<string>();
            _useAssemblies = new List<string>();
            _resourceMappings = new List<ResourceMapping>();
            _viewFolders = new List<IViewFolderSettings>();
 			NullBehaviour = NullBehaviour.Lenient;
        }

        public bool Debug { get; set; }
		public NullBehaviour NullBehaviour { get; set; }
    	public string Prefix { get; set; }
        public string PageBaseType { get; set; }

        private readonly IList<string> _useNamespaces;
        public IEnumerable<string> UseNamespaces
        {
            get { return _useNamespaces; }
        }

        private readonly IList<string> _useAssemblies;
        public IEnumerable<string> UseAssemblies
        {
            get { return _useAssemblies; }
        }

        private readonly IList<ResourceMapping> _resourceMappings;
        public IEnumerable<ResourceMapping> ResourceMappings
        {
            get { return _resourceMappings; }
        }

        private readonly IList<IViewFolderSettings> _viewFolders;
        public IEnumerable<IViewFolderSettings> ViewFolders
        {
            get { return _viewFolders; }
        }

        public SparkSettings SetDebug(bool debug)
        {
            Debug = debug;
            return this;
        }

		public SparkSettings SetNullBehaviour(NullBehaviour nullBehaviour)
		{
            NullBehaviour = nullBehaviour;
			return this;
		}

        public SparkSettings SetPageBaseType(string typeName)
        {
            PageBaseType = typeName;
            return this;
        }

        public SparkSettings SetPageBaseType(Type type)
        {
            PageBaseType = type.FullName;
            return this;
        }

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

        public SparkSettings AddResourceMapping(string match, string replace)
        {
            _resourceMappings.Add(new ResourceMapping { Match = match, Location = replace });
            return this;
        }

        public SparkSettings AddViewFolder(ViewFolderType type, IDictionary<string, string> parameters)
        {
            _viewFolders.Add(new ViewFolderSettings
                                 {
                                     FolderType = type,
                                     Parameters = parameters
                                 });
            return this;
        }

        public SparkSettings AddViewFolder(Type customType, IDictionary<string, string> parameters)
        {
            _viewFolders.Add(new ViewFolderSettings
                                 {
                                     FolderType = ViewFolderType.Custom,
                                     Type = customType.AssemblyQualifiedName,
                                     Parameters = parameters
                                 });
            return this;
        }
    }
}
