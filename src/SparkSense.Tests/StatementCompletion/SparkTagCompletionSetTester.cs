using Microsoft.VisualStudio.Language.Intellisense;
using NUnit.Framework;
using SparkSense.StatementCompletion.CompletionSets;
using System.Collections.Generic;
using System.Linq;


namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class SparkTagCompletionSetTester
    {
        [Test]
        public void ShouldReturnSparkSpecialNodes()
        {
            var tag = new SparkTagCompletionSet();
            List<Completion> tagList = tag.Completions.ToList();

            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "var"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "def"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "default"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "global"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "viewdata"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "set"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "for"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "test"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "if"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "else"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "elseif"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "content"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "use"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "macro"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "render"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "section"));
            Assert.IsTrue(tagList.Exists(c => c.DisplayText == "cache"));
        }
    }
}
