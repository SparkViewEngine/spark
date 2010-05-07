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
            List<Completion> tagCompletionsList = tag.Completions.ToList();

            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "var"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "def"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "default"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "global"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "viewdata"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "set"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "for"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "test"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "if"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "else"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "elseif"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "content"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "use"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "macro"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "render"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "section"));
            Assert.IsTrue(tagCompletionsList.Exists(c => c.DisplayText == "cache"));
        }
    }
}
