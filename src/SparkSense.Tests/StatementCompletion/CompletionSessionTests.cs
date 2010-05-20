using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion;
using System;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionTests
    {
        [Test]
        public void ShouldStartACompletionSessionForTag()
        {
            var mockConfig = MockRepository.GenerateMock<ICompletionSessionConfiguration>();
            var mockSession = MockRepository.GenerateMock<ICompletionSession>();
            var stubTextExplorer = MockRepository.GenerateStub<ITextExplorer>();

            ICompletionSession session;
            mockConfig.Expect(x => x.TryCreateCompletionSession(stubTextExplorer, out session))
                .OutRef(new object[] { mockSession })
                .IgnoreArguments()
                .Return(true);

            mockSession.Expect(x => x.Start());
            mockSession.Expect(x => x.IsDismissed).Return(false);

            var completionSession = new CompletionSessionManager(mockConfig, MockRepository.GenerateStub<IProjectExplorer>(), MockRepository.GenerateStub<IWpfTextView>());

            Assert.That(completionSession.StartCompletionSession(SparkSyntaxTypes.Tag));

            mockConfig.VerifyAllExpectations();
            mockSession.VerifyAllExpectations();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfConfigIsNull()
        {
            var stubProjectExplorer = MockRepository.GenerateStub<IProjectExplorer>();
            var stubTextView = MockRepository.GenerateStub<IWpfTextView>();

            new CompletionSessionManager(null, stubProjectExplorer, stubTextView);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfProjectExplorerIsNull()
        {
            var stubConfig = MockRepository.GenerateStub<ICompletionSessionConfiguration>();
            var stubTextView = MockRepository.GenerateStub<IWpfTextView>();

            new CompletionSessionManager(stubConfig, null, stubTextView);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTextViewIsNull()
        {
            var stubConfig = MockRepository.GenerateStub<ICompletionSessionConfiguration>();
            var stubProjectExplorer = MockRepository.GenerateStub<IProjectExplorer>();

            new CompletionSessionManager(stubConfig, stubProjectExplorer, null);
        }
    }
}