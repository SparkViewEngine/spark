using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Tests.Precompiled;

namespace Spark.Tests
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
                .FromAssemblyNamed("Spark.Tests");

            Assert.AreEqual(1, batch.Entries.Count(e => e.ControllerType == typeof(SimplePrecompileController)));
            Assert.AreEqual(3, batch.Entries.Count(e => e.ControllerType == typeof(ComplexPrecompileController)));
        }
    }
}
