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

using System.Linq;
using NUnit.Framework;
using Spark.Tests.Precompiled;

namespace Spark
{
    [TestFixture]
    public class SparkBatchDescriptorTester
    {
        [Test]
        public void BatchForControllerWithSimplePrecompileAttrib()
        {
            var batch = new SparkBatchDescriptor()
                .FromAttributes<SimplePrecompileController>();

            Assert.AreEqual(1, batch.Entries.Count);
            Assert.AreSame(typeof(SimplePrecompileController), batch.Entries[0].ControllerType);
            Assert.AreEqual(0, batch.Entries[0].ExcludeViews.Count);
            Assert.AreEqual(0, batch.Entries[0].IncludeViews.Count);
            Assert.AreEqual(0, batch.Entries[0].LayoutNames.Count);
        }

        [Test]
        public void BatchForControllerWithComplexPrecompileAttrib()
        {
            var batch = new SparkBatchDescriptor()
                .FromAttributes<ComplexPrecompileController>();

            Assert.AreEqual(3, batch.Entries.Count);
            var forDefault = batch.Entries.First(e => e.LayoutNames[0][0] == "Default");
            var forAjax = batch.Entries.First(e => e.LayoutNames[0][0] == "Ajax");
            var forShowing = batch.Entries.First(e => e.LayoutNames[0][0] == "Showing");

            Assert.AreEqual(0, forDefault.IncludeViews.Count);
            Assert.AreEqual(1, forDefault.ExcludeViews.Count);
            Assert.AreEqual(2, forAjax.IncludeViews.Count);
            Assert.AreEqual(0, forAjax.ExcludeViews.Count);
            Assert.AreEqual(1, forShowing.IncludeViews.Count);
            Assert.AreEqual(0, forShowing.ExcludeViews.Count);
        }

        [Test]
        public void BatchFromAssembly()
        {
            var batch = new SparkBatchDescriptor()
                .FromAssemblyNamed("Spark.Web.Tests");

            Assert.AreEqual(1, batch.Entries.Count(e => e.ControllerType == typeof(SimplePrecompileController)));
            Assert.AreEqual(3, batch.Entries.Count(e => e.ControllerType == typeof(ComplexPrecompileController)));
        }
    }
}
