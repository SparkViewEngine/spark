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

            Assert.That(batch.Entries.Count, Is.EqualTo(1));
            Assert.That(batch.Entries[0].ControllerType, Is.SameAs(typeof(SimplePrecompileController)));
            Assert.That(batch.Entries[0].ExcludeViews.Count, Is.EqualTo(0));
            Assert.That(batch.Entries[0].IncludeViews.Count, Is.EqualTo(0));
            Assert.That(batch.Entries[0].LayoutNames.Count, Is.EqualTo(0));
        }

        [Test]
        public void BatchForControllerWithComplexPrecompileAttrib()
        {
            var batch = new SparkBatchDescriptor()
                .FromAttributes<ComplexPrecompileController>();

            Assert.That(batch.Entries.Count, Is.EqualTo(3));
            var forDefault = batch.Entries.First(e => e.LayoutNames[0][0] == "Default");
            var forAjax = batch.Entries.First(e => e.LayoutNames[0][0] == "Ajax");
            var forShowing = batch.Entries.First(e => e.LayoutNames[0][0] == "Showing");

            Assert.That(forDefault.IncludeViews.Count, Is.EqualTo(0));
            Assert.That(forDefault.ExcludeViews.Count, Is.EqualTo(1));
            Assert.That(forAjax.IncludeViews.Count, Is.EqualTo(2));
            Assert.That(forAjax.ExcludeViews.Count, Is.EqualTo(0));
            Assert.That(forShowing.IncludeViews.Count, Is.EqualTo(1));
            Assert.That(forShowing.ExcludeViews.Count, Is.EqualTo(0));
        }

        [Test]
        public void BatchFromAssembly()
        {
            var batch = new SparkBatchDescriptor()
                .FromAssemblyNamed("Spark.Web.Tests");

            Assert.That(batch.Entries.Count(e => e.ControllerType == typeof(SimplePrecompileController)), Is.EqualTo(1));
            Assert.That(batch.Entries.Count(e => e.ControllerType == typeof(ComplexPrecompileController)), Is.EqualTo(3));
        }
    }
}
