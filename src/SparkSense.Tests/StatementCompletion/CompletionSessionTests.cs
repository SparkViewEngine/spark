using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionTests
    {
        private const string PATH_CONTAINING_A_VIEWS_FOLDER = "C:\\Views\\Home\\index.spark";
        [Test, Ignore("Way too complex - gotta strip out a few classes, too many responsibilities going on here.")]
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
    }
}