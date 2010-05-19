using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class TagCompletionSetTests
    {
        private ICompletionSession _stubSession;
        private ITextBuffer _stubTextBuffer;
        private IViewExplorer _stubViewExplorer;
        private ITrackingPoint _stubTrackingPoint;
        private ITextSnapshot _stubSnapshot;

        [SetUp]
        public void Setup()
        {
            _stubSession = MockRepository.GenerateStub<ICompletionSession>();
            _stubTextBuffer = MockRepository.GenerateStub<ITextBuffer>();
            _stubViewExplorer = MockRepository.GenerateStub<IViewExplorer>();
            _stubTrackingPoint = MockRepository.GenerateStub<ITrackingPoint>();
            _stubSnapshot = MockRepository.GenerateStub<ITextSnapshot>();

            _stubTextBuffer.Stub(x => x.CurrentSnapshot).Return(_stubSnapshot);
            _stubSession.Stub(x => x.GetTriggerPoint(_stubTextBuffer)).Return(_stubTrackingPoint);
            _stubTrackingPoint.Stub(x => x.GetPoint(_stubSnapshot)).Return(new SnapshotPoint(_stubSnapshot, 0));
            _stubViewExplorer.Stub(x => x.GetRelatedPartials()).Return(new List<string>());
        }

        [Test]
        public void ShouldReturnSparkSpecialNodes()
        {
            var tag = SparkCompletionSetFactory.Create<SparkTagCompletionSet>(_stubSession, _stubTextBuffer, _stubViewExplorer);
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

        [Test]
        public void ShouldLookForRelatedPartials()
        {
            var mockViewExplorer = MockRepository.GenerateMock<IViewExplorer>();

            mockViewExplorer.Expect(x => x.GetRelatedPartials()).Return(new List<string> { "partial1", "partial2" });

            var tag = SparkCompletionSetFactory.Create<SparkTagCompletionSet>(_stubSession, _stubTextBuffer, mockViewExplorer);
            var tagList = tag.Completions.ToList();
            Assert.That(tagList.Exists(c => c.DisplayText == "partial1"));
            Assert.That(tagList.Exists(c => c.DisplayText == "partial2"));

            mockViewExplorer.VerifyAllExpectations();
        }
    }
}