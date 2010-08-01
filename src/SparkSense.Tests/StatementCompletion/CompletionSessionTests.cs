using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.StatementCompletion;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Text;
using NUnit.Framework.SyntaxHelpers;

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
        public void ListenerShouldAttemptToGetAnInstanceOfTheVisualStudioEnvironment()
        {
            IServiceProvider _mockServiceProvider;
            _mockServiceProvider = new MockServiceProvider();

            var _listener = new CompletionListener();
            _listener.ServiceProvider = _mockServiceProvider;
            var _mockTextBuffer = MockRepository.GenerateStub<ITextBuffer>();
            _listener.TryCreateCompletionSource(_mockTextBuffer);
            Assert.That(((MockServiceProvider)_mockServiceProvider).ServiceTypeName, Is.EqualTo("DTE"));
        }
        public class MockServiceProvider : IServiceProvider
        {
            public string ServiceTypeName { get; private set; }
            public object GetService(Type serviceType)
            {
                ServiceTypeName = serviceType.Name;
                return null;
            }
        }

    }
}