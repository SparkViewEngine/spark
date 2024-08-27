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
using Spark.Parser;

namespace Spark
{
    public enum NullBehaviour
    {
        /// <summary>Catch NullReferenceExceptions, and either render the literal expression, or render nothing, respectively, when ${expression} or $!{expression} syntax is used</summary>
        /// <remarks><c>Lenient</c> is the default setting.</remarks>
        Lenient,
        /// <summary>Do not wrap expressions in try/catch blocks.  Intended for fail-fast in development environment.</summary>
        Strict
    }

    public enum AttributeBehaviour
    {
        /// <summary>
        /// Follows original Spark behavior, where all attributes that contain code are treated as
        /// direct c# snippets. e.g. hello="'string no. ' + index"
        /// </summary>
        CodeOriented,
        /// <summary>
        /// Follows a text-oriented behavior, where many attributes that contain code are treated as
        /// text which may contain expression. e.g. hello="string no. ${index}"
        /// Expressions are not html encoded.
        /// </summary>
        TextOriented,
    }

    public interface ISparkSettings : IParserSettings
    {
        /// <summary>
        /// Gets the base path of the directory containing the application.
        /// </summary>
        string RootPath { get; }

        bool Debug { get; }
        NullBehaviour NullBehaviour { get; }
        string Prefix { get; }
        string BaseClassTypeName { get; }
        LanguageType DefaultLanguage { get; }

        IEnumerable<string> UseNamespaces { get; }

        /// <summary>
        /// A list of name, fullname or absolute paths to .dll for assemblies to load before compiling views.
        /// </summary>
        IEnumerable<string> UseAssemblies { get; }

        /// <summary>
        /// A list of assemblies to avoid adding as reference when compiling views (e.g. any precompiled assemblies)
        /// </summary>
        /// <remarks>
        /// Leave this empty unless you are pre-compiling views. Example value: "HelloWorld.Views.dll"
        /// </remarks>
        IEnumerable<string> ExcludeAssemblies { get; }

        IEnumerable<IResourceMapping> ResourceMappings { get; }
        IEnumerable<IViewFolderSettings> ViewFolders { get; }
        bool ParseSectionTagAsSegment { get; }
        AttributeBehaviour AttributeBehaviour { get; }
    }

    public interface IResourceMapping
    {
        bool IsMatch(string path);
        string Map(string path);
        bool Stop { get; }
    }

    public class SimpleResourceMapping : IResourceMapping
    {
        public string Match { get; set; }
        public string Location { get; set; }
        public bool Stop { get; set; }
        
        public bool IsMatch(string path)
        {
            return path.StartsWith(Match, StringComparison.InvariantCultureIgnoreCase);
        }

        public string Map(string path)
        {
            return Location + path.Substring(Match.Length);
        }
    }

    public interface IViewFolderSettings
    {
        string Name { get; set; }
        string Type { get; set; }
        string Subfolder { get; set; }
        IDictionary<string, string> Parameters { get; set; }
    }

    internal class ViewFolderSettings : IViewFolderSettings
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Subfolder { get; set; }
        public IDictionary<string, string> Parameters { get; set; }
    }
}

