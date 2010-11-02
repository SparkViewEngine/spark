//-------------------------------------------------------------------------
// <copyright file="ParserSettings.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
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
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser
{
    using System;

    /// <summary>
    /// Describes a set of configuration settings for use by a parser.
    /// </summary>
    public interface IParserSettings
    {
        /// <summary>
        /// Gets a value indicating whether or not the default output action automatically encodes its results.
        /// </summary>
        bool AutomaticEncoding { get; }

        /// <summary>
        /// Gets the marker used to denote a line of code.
        /// </summary>
        string StatementMarker { get; }
    }

    /// <summary>
    /// Describes a set of configuration settings for use by the Spark parser.
    /// </summary>
    public class ParserSettings : IParserSettings
    {
        /// <summary>
        /// Holds the default value for the AutomaticEncoding property.
        /// </summary>
        /// <remarks>
        /// For now the default is to have ${expr} and !{expr} both output raw html.
        /// This could change very soon, so developers are encouraged to provide an explicit setting.
        /// </remarks>
        public const bool DefaultAutomaticEncoding = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParserSettings"/> class.
        /// </summary>
        public ParserSettings()
        {
            this.AutomaticEncoding = DefaultAutomaticEncoding;
            this.StatementMarker = "#";
        }

        /// <summary>
        /// Gets a ParserSettings object that describes the behavior of the original version of Spark.
        /// </summary>
        public static ParserSettings LegacyBehavior
        {
            get
            {
                return new ParserSettings
                {
                    AutomaticEncoding = false,
                    StatementMarker = "#",
                };
            }
        }

        /// <summary>
        /// Gets a ParserSettings object that describes the default behavior of the current version of Spark.
        /// </summary>
        public static ParserSettings DefaultBehavior
        {
            get
            {
                return new ParserSettings();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether or not the default output action automatically encodes its results.
        /// </summary>
        public bool AutomaticEncoding { get; set; }

        /// <summary>
        /// Gets or sets the marker used to denote a line of code.
        /// </summary>
        /// <remarks>
        /// The default is "#".
        /// </remarks>
        public string StatementMarker { get; set; }
    }
}
