//-------------------------------------------------------------------------
// <copyright file="SourceContext.cs">
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
    /// Holds the content and metadata for a singe source code chunk.
    /// </summary>
    public class SourceContext
    {
        /// <summary>
        /// Initializes a new instance of the SourceContext class.
        /// </summary>
        /// <param name="content">The content of this source context.</param>
        public SourceContext(string content)
        {
            this.Content = content;
        }

        /// <summary>
        /// Initializes a new instance of the SourceContext class.
        /// </summary>
        /// <param name="content">The content of this source context.</param>
        /// <param name="lastModified">The time stamp of the last time the content was modified.</param>
        public SourceContext(string content, long lastModified)
        {
            this.Content = content;
            this.LastModified = lastModified;
        }

        /// <summary>
        /// Initializes a new instance of the SourceContext class.
        /// </summary>
        /// <param name="content">The content of this source context.</param>
        /// <param name="lastModified">The time stamp of the last time the content was modified.</param>
        /// <param name="fileName">The filename from which the content was loaded.</param>
        public SourceContext(string content, long lastModified, string fileName)
        {
            this.Content = content;
            this.LastModified = lastModified;
            this.FileName = fileName;
        }

        /// <summary>
        /// Gets the content of this source context.
        /// </summary>
        public string Content { get; private set; }

        /// <summary>
        /// Gets the time stamp of the last time the content of this source context was modified.
        /// </summary>
        public long LastModified { get; private set; }

        /// <summary>
        /// Gets the filename from which the content of this source context was loaded.
        /// </summary>
        public string FileName { get; private set; }
    }
}
