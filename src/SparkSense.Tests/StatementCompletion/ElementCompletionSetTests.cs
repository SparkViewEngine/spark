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
    public class ElementCompletionSetTests
    {
        private ICompletionSession _stubSession;
        private ITextBuffer _stubTextBuffer;
        private ITrackingSpan _stubTrackingSpan;
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
            _stubTrackingSpan = MockRepository.GenerateStub<ITrackingSpan>();
            _stubSnapshot = MockRepository.GenerateStub<ITextSnapshot>();

            _stubTextBuffer.Stub(x => x.CurrentSnapshot).Return(_stubSnapshot);
            _stubSession.Stub(x => x.GetTriggerPoint(_stubTextBuffer)).Return(_stubTrackingPoint);
            _stubTrackingPoint.Stub(x => x.GetPoint(_stubSnapshot)).Return(new SnapshotPoint(_stubSnapshot, 0));
            _stubViewExplorer.Stub(x => x.GetRelatedPartials()).Return(new List<string>());
        }

        [Test]
        public void ShouldReturnSparkSpecialNodes()
        {
            var set = CompletionSetFactory.Create<ElementCompletionSet>(new SnapshotPoint(_stubSnapshot, 0), _stubTrackingSpan, _stubViewExplorer);
            List<Completion> elementList = set.Completions.ToList();

            Assert.That(elementList.Exists(c => c.DisplayText == "var"));
            Assert.That(elementList.Exists(c => c.DisplayText == "def"));
            Assert.That(elementList.Exists(c => c.DisplayText == "default"));
            Assert.That(elementList.Exists(c => c.DisplayText == "global"));
            Assert.That(elementList.Exists(c => c.DisplayText == "viewdata"));
            Assert.That(elementList.Exists(c => c.DisplayText == "set"));
            Assert.That(elementList.Exists(c => c.DisplayText == "for"));
            Assert.That(elementList.Exists(c => c.DisplayText == "test"));
            Assert.That(elementList.Exists(c => c.DisplayText == "if"));
            Assert.That(elementList.Exists(c => c.DisplayText == "else"));
            Assert.That(elementList.Exists(c => c.DisplayText == "elseif"));
            Assert.That(elementList.Exists(c => c.DisplayText == "content"));
            Assert.That(elementList.Exists(c => c.DisplayText == "use"));
            Assert.That(elementList.Exists(c => c.DisplayText == "macro"));
            Assert.That(elementList.Exists(c => c.DisplayText == "render"));
            Assert.That(elementList.Exists(c => c.DisplayText == "section"));
            Assert.That(elementList.Exists(c => c.DisplayText == "cache"));
        }

        [Test]
        public void ShouldLookForRelatedPartials()
        {
            var mockViewExplorer = MockRepository.GenerateMock<IViewExplorer>();

            mockViewExplorer.Expect(x => x.GetRelatedPartials()).Return(new List<string> { "partial1", "partial2" });

            var set = CompletionSetFactory.Create<ElementCompletionSet>(new SnapshotPoint(), null, mockViewExplorer);
            var elementList = set.Completions.ToList();
            Assert.That(elementList.Exists(c => c.DisplayText == "partial1"));
            Assert.That(elementList.Exists(c => c.DisplayText == "partial2"));

            mockViewExplorer.VerifyAllExpectations();
        }
    }
}