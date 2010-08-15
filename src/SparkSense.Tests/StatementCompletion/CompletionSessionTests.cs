using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.StatementCompletion;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionTests
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfConfigIsNull()
        {
            new CompletionSessionManager(
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
                MockRepository.GenerateStub<ICompletionBroker>(),
                null,
                MockRepository.GenerateStub<ITextStructureNavigator>()
                );
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowIfTextNavigatorIsNull()
        {
            new CompletionSessionManager(
                MockRepository.GenerateStub<ICompletionBroker>(),
                MockRepository.GenerateStub<IWpfTextView>(),
                null
                );
        }

        [Test]
        public void ListenerShouldAttemptToGetAnInstanceOfTheProjectExplorer()
        {
            var mockServiceProvider = MockRepository.GenerateMock<ISparkServiceProvider>();
            var stubTextBuffer = MockRepository.GenerateStub<ITextBuffer>();
            var listener = new CompletionListener { ServiceProvider = mockServiceProvider };

            mockServiceProvider.Expect(x => x.ProjectExplorer).Return(null);
            listener.TryCreateCompletionSource(stubTextBuffer);

            mockServiceProvider.VerifyAllExpectations();
        }
    }
}