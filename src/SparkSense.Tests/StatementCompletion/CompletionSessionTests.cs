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
                MockRepository.GenerateStub<ICompletionBroker>(), 
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
                MockRepository.GenerateStub<ICompletionBroker>(),
                MockRepository.GenerateStub<IProjectExplorer>(),
                MockRepository.GenerateStub<IWpfTextView>(),
                null
                );
        }
    }
}