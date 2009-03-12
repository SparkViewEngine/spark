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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.FileSystem;

namespace Spark.Tests
{
    [TestFixture]
    public class ClientsideCompilerTester
    {
        [Test]
        public void GenerateSimpleTemplate()
        {
            var descriptor = new SparkViewDescriptor()
                .SetLanguage(LanguageType.Javascript)
                .AddTemplate("Clientside\\simple.spark");

            var engine = new SparkViewEngine { ViewFolder = new FileSystemViewFolder("Spark.Tests.Views") };
            var entry = engine.CreateEntry(descriptor);

            Assert.IsNotNull(entry.SourceCode);
            Assert.IsNotEmpty(entry.SourceCode);
        }
    }
}
