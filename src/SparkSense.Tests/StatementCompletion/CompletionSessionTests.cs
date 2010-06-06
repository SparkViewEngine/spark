using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion;
using Microsoft.VisualStudio.Text.Operations;

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

            var completionSession = 
                new CompletionSessionManager(
                    mockConfig, 
                    MockRepository.GenerateStub<IProjectExplorer>(), 
                    MockRepository.GenerateStub<IWpfTextView>(), 
                    MockRepository.GenerateStub<ITextStructureNavigator>()
                    );

            Assert.That(completionSession.StartCompletionSession(SparkSyntaxTypes.Element));

            mockConfig.VerifyAllExpectations();
            mockSession.VerifyAllExpectations();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfConfigIsNull()
        {
            new CompletionSessionManager(
                null, 
                MockRepository.GenerateStub<IProjectExplorer>(), 
                MockRepository.GenerateStub<IWpfTextView>(),
                MockRepository.GenerateStub<ITextStructureNavigator>()
                );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfProjectExplorerIsNull()
        {
            new CompletionSessionManager(
                MockRepository.GenerateStub<ICompletionSessionConfiguration>(), 
                null, 
                MockRepository.GenerateStub<IWpfTextView>(),
                MockRepository.GenerateStub<ITextStructureNavigator>()
                );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTextViewIsNull()
        {
            new CompletionSessionManager(
                MockRepository.GenerateStub<ICompletionSessionConfiguration>(),
                MockRepository.GenerateStub<IProjectExplorer>(),
                null,
                MockRepository.GenerateStub<ITextStructureNavigator>()
                );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTextNavigatorIsNull()
        {
            new CompletionSessionManager(
                MockRepository.GenerateStub<ICompletionSessionConfiguration>(),
                MockRepository.GenerateStub<IProjectExplorer>(),
                MockRepository.GenerateStub<IWpfTextView>(),
                null
                );
        }
    }
}