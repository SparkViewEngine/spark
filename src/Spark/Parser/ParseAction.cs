//-------------------------------------------------------------------------
// <copyright file="ParseAction.cs">
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
    /// Encapsulates an attempt to match an input at a position.
    /// </summary>
    /// <typeparam name="TValue">The type of the result.</typeparam>
    /// <param name="position">The position at which to match.</param>
    /// <returns>A <see cref="ParseResult&lt;TValue&gt;"/> containing the value of the match, if a match can be found; null, otherwise.</returns>
    public delegate ParseResult<TValue> ParseAction<TValue>(Position position);
}
