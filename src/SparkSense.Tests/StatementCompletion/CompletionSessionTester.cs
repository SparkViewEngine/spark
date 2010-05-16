using NUnit.Framework;
using SparkSense.StatementCompletion;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Editor;
using SparkSense.Parsing;
using Rhino.Mocks;
using Rhino.Mocks.Constraints;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text;
using System;
using Microsoft.VisualStudio.Utilities;
using NUnit.Framework.SyntaxHelpers;
using System.Collections.Generic;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionTester
    {
        private const string PATH_CONTAINING_A_VIEWS_FOLDER = "C:\\Views\\Home\\index.spark";
        [Test, Ignore("Way too complex - gotta strip out a few classes, too many responsibilities going on here")]
        public void ShouldStartACompletionSessionForTag()
        {
            var stubCompletionBroker = MockRepository.GenerateStub<ICompletionBroker>();
            var stubProjectExplorer = MockRepository.GenerateStub<IProjectExplorer>();
            var stubTextView = MockRepository.GenerateStub<IWpfTextView>();
            var stubTriggerPoint = MockRepository.GenerateStub<ITrackingPoint>();
            var stubCaret = MockRepository.GenerateStub<ITextCaret>();

            var mappingPoint = MockRepository.GenerateStub<IMappingPoint>();
            var stubSnapshot = MockRepository.GenerateStub<ITextSnapshot>();
            var bufferPosition = new VirtualSnapshotPoint(stubSnapshot, 1);

            stubTextView.Stub(x => x.Caret).Return(stubCaret);
            stubCaret.Stub(x => x.Position).Return(new CaretPosition(bufferPosition, mappingPoint, PositionAffinity.Predecessor));
            stubProjectExplorer.Stub(x => x.ActiveDocumentPath).Return(PATH_CONTAINING_A_VIEWS_FOLDER);

            stubCompletionBroker.Expect(x => x.CreateCompletionSession(stubTextView, stubTriggerPoint, true));

            var completionSession = new CompletionSessionManager(stubCompletionBroker, stubProjectExplorer, stubTextView);

            Assert.That(completionSession.StartCompletionSession(SparkSyntaxTypes.Tag));
        }

        [Test]
        public void ConfigurationShouldAddPropertiesRequiredForCompletionSourceToSession()
        {
            var mockSession = MockRepository.GenerateMock<ICompletionSession>();
            var stubTrackingSpan = MockRepository.GenerateStub<ITrackingSpan>();
            var stubViewVolder = MockRepository.GenerateStub<IViewExplorer>();
            var stubSyntaxType = SparkSyntaxTypes.Tag;
            var mockProperties = new PropertyCollection();
            var properties = new List<object> { stubTrackingSpan, stubViewVolder, stubSyntaxType };

            mockSession.Expect(x => x.Properties).Return(mockProperties).Repeat.Times(3);

            var config = new CompletionSessionConfiguration(mockSession);
            config.AddCompletionSourceProperties(properties);

            Assert.That(mockProperties.ContainsProperty(stubTrackingSpan.GetType()));
            Assert.That(mockProperties.ContainsProperty(stubViewVolder.GetType()));
            Assert.That(mockProperties.ContainsProperty(typeof(SparkSyntaxTypes)));
            mockSession.VerifyAllExpectations();
        }
    }
}
