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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler.NodeVisitors;

namespace Spark.Tests.Parser
{
    [TestFixture]
    public class TypeInspectorTester
    {
        [Test]
        public void SimpleFields()
        {
            var result = new TypeInspector("string");
            Assert.Multiple(() =>
            {
                Assert.That((string)result.Type, Is.EqualTo("string"));
                Assert.That(result.Name, Is.Null);
            });
        }

        [Test]
        public void Generics()
        {
            var result = new TypeInspector("IList<something>");
            Assert.Multiple(() =>
            {
                Assert.That((string)result.Type, Is.EqualTo("IList<something>"));
                Assert.That(result.Name, Is.Null);
            });
        }

        [Test]
        public void GenericsWithName()
        {
            var result = new TypeInspector("IList<something>\r\n\tSomethingList");
            Assert.Multiple(() =>
            {
                Assert.That((string)result.Type, Is.EqualTo("IList<something>"));
                Assert.That((string)result.Name, Is.EqualTo("SomethingList"));
            });
        }

        [Test]
        public void GenericWithSpacesButNoName()
        {
            var result = new TypeInspector("IList<something, int>");
            Assert.Multiple(() =>
            {
                Assert.That((string)result.Type, Is.EqualTo("IList<something, int>"));
                Assert.That(result.Name, Is.Null);
            });
        }
    }
}
