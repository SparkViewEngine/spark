//-------------------------------------------------------------------------
// <copyright file="ParseResult.cs">
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
    /// <summary>
    /// Encapsulates a single, atomic piece of an input.
    /// </summary>
    /// <typeparam name="TValue">The type of the result.</typeparam>
    public class ParseResult<TValue>
    {
        /// <summary>
        /// Holds the position for the end of this result.
        /// </summary>
        private readonly Position rest;

        /// <summary>
        /// Holds the value of the result.
        /// </summary>
        private readonly TValue value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParseResult&lt;TValue&gt;"/> class.
        /// </summary>
        /// <param name="rest">The position of the end of the result.</param>
        /// <param name="value">The value of the result.</param>
        public ParseResult(Position rest, TValue value)
        {
            this.rest = rest;
            this.value = value;
        }

        /// <summary>
        /// Gets the position for the end of this result.
        /// </summary>
        public Position Rest
        {
            get
            {
                return this.rest;
            }
        }

        /// <summary>
        /// Gets the value of the result.
        /// </summary>
        public TValue Value
        {
            get
            {
                return this.value;
            }
        }
    }
}