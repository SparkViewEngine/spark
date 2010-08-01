using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.StatementCompletion;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Language.Intellisense;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class TextViewCreationTests
    {
        [Test]
        public void ListenerShouldAttemptToGetAnInstanceOfTheWpfTextView()
        {
            var listener = new ViewCreationListener();
            var mockAdapterFactoryService = MockRepository.GenerateMock<IVsEditorAdaptersFactoryService>();
            var mockTextView = MockRepository.GenerateMock<IVsTextView>();
            var mockWpfTextView = MockRepository.GenerateMock<IWpfTextView>();
            var mockTextNav = MockRepository.GenerateStub<ITextStructureNavigatorSelectorService>();
            var mockProperties = MockRepository.GenerateStub<PropertyCollection>();
            var mockBuffer = MockRepository.GenerateStub<ITextBuffer>();
            var mockBroker = MockRepository.GenerateStub<ICompletionBroker>();
            var mockTextStructureNav = MockRepository.GenerateStub<ITextStructureNavigator>();

            listener.AdaptersFactoryService = mockAdapterFactoryService;
            listener.TextNavigator = mockTextNav;
            listener.CompletionBroker = mockBroker;
            mockAdapterFactoryService.Expect(x => x.GetWpfTextView(mockTextView)).Return(mockWpfTextView);
            mockWpfTextView.Stub(x => x.Properties).Return(mockProperties);
            mockWpfTextView.Stub(x => x.TextBuffer).Return(mockBuffer);
            mockTextNav.Stub(x => x.GetTextStructureNavigator(mockBuffer)).Return(mockTextStructureNav);

            listener.VsTextViewCreated(mockTextView);

            mockAdapterFactoryService.VerifyAllExpectations();
        }

    }
}
