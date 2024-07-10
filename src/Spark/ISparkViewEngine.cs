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
using Spark.FileSystem;

namespace Spark
{
    public interface ISparkViewEngine
    {
        ISparkSettings Settings { get; }

        IViewFolder ViewFolder { get; set; }

        IViewActivatorFactory ViewActivatorFactory { get; }
        ISparkSyntaxProvider SyntaxProvider { get; }

        ISparkViewEntry GetEntry(SparkViewDescriptor descriptor);
        ISparkViewEntry CreateEntry(SparkViewDescriptor descriptor);
        ISparkView CreateInstance(SparkViewDescriptor descriptor);
        void ReleaseInstance(ISparkView view);

        Assembly BatchCompilation(IList<SparkViewDescriptor> descriptors);
        Assembly BatchCompilation(string outputAssembly, IList<SparkViewDescriptor> descriptors);
        
        /// <summary>
        /// Get all the exported types in the assembly and loads the ones assignable from <see cref="ISparkView"/>.
        /// </summary>
        /// <param name="assembly"></param>
        /// <seealso cref="LoadBatchCompilation(Type[])"/>
        /// <returns>A list of <see cref="SparkViewDescriptor"/> for every loaded type.</returns>
        IList<SparkViewDescriptor> LoadBatchCompilation(Assembly assembly);

        /// <summary>
        /// Loads the specified types (when assignable from <see cref="ISparkView"/>) into the <see cref="ICompiledViewHolder"/> implemenation.
        /// </summary>
        /// <param name="types"></param>
        /// <returns>A list of <see cref="SparkViewDescriptor"/> for every loaded type.</returns>
        IList<SparkViewDescriptor> LoadBatchCompilation(Type[] assembly);
    }
}
