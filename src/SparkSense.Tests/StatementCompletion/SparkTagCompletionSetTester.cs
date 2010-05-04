using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Microsoft.VisualStudio.Language.Intellisense;
using SparkSense.StatementCompletion.CompletionSets;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class SparkTagCompletionSetTester
    {
        [Test]
        public void ShouldReturnSparkSpecialNodes()
        {
            var tag = new SparkTagCompletionSet();
            Assert.IsTrue(tag.Completions.ToList().Exists(c => c.DisplayText == "content"));
            Assert.IsTrue(tag.Completions.ToList().Exists(c => c.DisplayText == "default"));
        }
    }
}
