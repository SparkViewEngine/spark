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

namespace Spark.Parser
{
    public interface IParserSettings
    {
        bool AutomaticEncoding { get; }

        string StatementMarker { get; }
    }

    public class ParserSettings : IParserSettings
    {
        /// <summary>
        /// For now the default is to have ${expr} and !{expr} both output raw html.
        /// This could change very soon, so developers are encouraged to provide an explicit setting.
        /// </summary>
        public const bool DefaultAutomaticEncoding = false;

        public bool AutomaticEncoding { get; set; }

        static public IParserSettings LegacyBehavior
        {
            get { return new ParserSettings { AutomaticEncoding = false }; }
        }

        static public IParserSettings DefaultBehavior
        {
            get { return new ParserSettings { AutomaticEncoding = DefaultAutomaticEncoding }; }
        }

        /// <summary>
        /// Optional character sequence that denotes a line of code when it's the first
        /// non-whitespace text in a line. Default is "#"
        /// </summary>
        public string StatementMarker { get; set; }
    }
}
