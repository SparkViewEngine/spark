using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Text;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion.CompletionSets;
using NUnit.Framework.SyntaxHelpers;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class AttributeCompletionSetTests
    {
        private IViewExplorer _mockViewExplorer;
        private ITextSnapshot _stubSnapshot;
        private ITrackingSpan _stubTrackingSpan;
        private IViewExplorer _stubViewExplorer;

        [SetUp]
        public void Setup()
        {
            _mockViewExplorer = MockRepository.GenerateMock<IViewExplorer>();
            _stubSnapshot = MockRepository.GenerateStub<ITextSnapshot>();
            _stubViewExplorer = MockRepository.GenerateStub<IViewExplorer>();
            _stubTrackingSpan = MockRepository.GenerateStub<ITrackingSpan>();
        }

        [TearDown]
        public void Cleanup()
        {
            _mockViewExplorer.VerifyAllExpectations();
            _mockViewExplorer = null;
        }

        private SnapshotPoint GetSnapShotPoint(string content, int position)
        {
            _stubSnapshot.Stub(x => x.Length).Return(content.Length);
            _stubSnapshot.Stub(x => x.GetText()).Return(content);
            return new SnapshotPoint(_stubSnapshot, position);
        }

        [Test]
        public void ShouldReturnSpecialNodeAttributes()
        {
            var point = GetSnapShotPoint("<div><use </div>", 10);
            var completionSet = CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            var completions = completionSet.Completions.ToList();

            Assert.That(completionSet, Is.InstanceOfType(typeof(AttributeCompletionSet)));
            Assert.That(completions.Count, Is.EqualTo(7));
            Assert.That(completions.Exists(a => a.DisplayText == "content"));
            Assert.That(completions.Exists(a => a.DisplayText == "master"));
        }

        [Test]
        public void ShouldReturnContentNamesAsAttributeValues()
        {
            var point = GetSnapShotPoint("<div><content name=\"\" </div>", 20);
            _mockViewExplorer.Expect(x => x.GetContentNames()).Return(new List<string> { "title", "head", "view", "footer" });

            var completionSet = CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _mockViewExplorer);
            var completions = completionSet.Completions.ToList();

            Assert.That(completionSet, Is.InstanceOfType(typeof(AttributeCompletionSet)));
            Assert.That(completions.Count, Is.EqualTo(4));
            Assert.That(completions.Exists(a => a.DisplayText == "title"));
            Assert.That(completions.Exists(a => a.DisplayText == "footer"));
        }

        [Test]
        public void ShouldReturnVariableNamesAsAttributeValues()
        {
            var point = GetSnapShotPoint("<div><test if=\"\" </div>", 15);
            _mockViewExplorer.Expect(x => x.GetLocalVariables()).Return(new List<string> { "x", "y" });
            _mockViewExplorer.Expect(x => x.GetGlobalVariables()).Return(new List<string> { "g1", "g2" });

            var completionSet = CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _mockViewExplorer);
            var completions = completionSet.Completions.ToList();

            Assert.That(completionSet, Is.InstanceOfType(typeof(AttributeCompletionSet)));
            Assert.That(completions.Count, Is.EqualTo(4));
            Assert.That(completions.Exists(a => a.DisplayText == "x"));
            Assert.That(completions.Exists(a => a.DisplayText == "y"));
            Assert.That(completions.Exists(a => a.DisplayText == "g1"));
            Assert.That(completions.Exists(a => a.DisplayText == "g2"));
        }

        [Test]
        public void ShouldReturnMasterNamesAsAttributeValues()
        {
            var point = GetSnapShotPoint("<div><use master=\"\" </div>", 18);
            _mockViewExplorer.Expect(x => x.GetPossibleMasterLayouts()).Return(new List<string> { "html", "master" });

            var completionSet = CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _mockViewExplorer);
            var completions = completionSet.Completions.ToList();

            Assert.That(completionSet, Is.InstanceOfType(typeof(AttributeCompletionSet)));
            Assert.That(completions.Count, Is.EqualTo(2));
            Assert.That(completions.Exists(a => a.DisplayText == "html"));
            Assert.That(completions.Exists(a => a.DisplayText == "master"));
        }

        [Test]
        public void ShouldReturnAttributeNameContextWhenPositionedAfterElementName()
        {
            var point = GetSnapShotPoint("<div><set </div>", 10);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Name));
        }

        [Test]
        public void ShouldReturnAttributeNameContextWhenPositionedAfterPreviousClosedAttribute()
        {
            var point = GetSnapShotPoint("<div><set x='5' </div>", 16);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Name));
        }

        [Test]
        public void ShouldReturnAttributeValueContextWhenPositionedInsideEmptyQuotes()
        {
            var point = GetSnapShotPoint("<div><set x=\"\" </div>", 13);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Value));
        }

        [Test]
        public void ShouldReturnAttributeValueContextWhenPositionedAfterASpaceInsideQuotes()
        {
            var point = GetSnapShotPoint("<div><set x='500' y='x + '</div>", 25);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Value));
        }

        [Test]
        public void ShouldReturnAttributeValueContextWhenPositionedAnywhereInsideQuotes()
        {
            var point = GetSnapShotPoint("<div><set x='500' </div>", 16);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Value));
        }

        [Test]
        public void ShouldReturnAttributeValueContextWhenPositionedOnAnyAttributeInsideQuotes()
        {
            var point = GetSnapShotPoint("<div><set x='500' y='60' </div>", 22);

            var completionSet = (AttributeCompletionSet)CompletionSetFactory.GetCompletionSetFor(point, _stubTrackingSpan, _stubViewExplorer);
            Assert.That(completionSet.AttributeContext, Is.EqualTo(AttributeContexts.Value));
        }
    }
}