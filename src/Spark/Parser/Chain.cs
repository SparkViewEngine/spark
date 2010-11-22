//-------------------------------------------------------------------------
// <copyright file="Chain.cs">
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
    /// Encapsulates a chain of parse results.
    /// </summary>
    /// <typeparam name="TLeft">The type of the preceding result in the chain.</typeparam>
    /// <typeparam name="TDown">The type of the current result in the chain.</typeparam>
    public class Chain<TLeft, TDown>
    {
        /// <summary>
        /// Holds the preceding result.
        /// </summary>
        private readonly TLeft left;

        /// <summary>
        /// Holds the current result.
        /// </summary>
        private readonly TDown down;

        /// <summary>
        /// Initializes a new instance of the <see cref="Chain&lt;TLeft, TDown&gt;" /> class.
        /// </summary>
        /// <param name="left">The preceding result.</param>
        /// <param name="down">The current result.</param>
        public Chain(TLeft left, TDown down)
        {
            this.left = left;
            this.down = down;
        }

        /// <summary>
        /// Gets the preceding result.
        /// </summary>
        public TLeft Left
        {
            get
            {
                return this.left;
            }
        }

        /// <summary>
        /// Gets the current result.
        /// </summary>
        public TDown Down
        {
            get
            {
                return this.down;
            }
        }
    }
}