using System;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.StatementCompletion;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class TextViewCreationTests
    {
        private ViewCreationListener _listener;
        private IVsEditorAdaptersFactoryService _mockAdapterFactoryService;
        private IServiceProvider _mockServiceProvider;
        private IVsTextView _mockTextView;

        [SetUp]
        public void Setup()
        {
            _listener = new ViewCreationListener();
            _mockAdapterFactoryService = MockRepository.GenerateMock<IVsEditorAdaptersFactoryService>();
            _mockServiceProvider = new MockServiceProvider();
            _mockTextView = MockRepository.GenerateMock<IVsTextView>();
            _listener.AdaptersFactoryService = _mockAdapterFactoryService;
            _listener.ServiceProvider = _mockServiceProvider;
        }

        [Test]
        public void ListenerShouldAttemptToGetAnInstanceOfTheWpfTextView()
        {
            var mockWpfTextView = MockRepository.GenerateMock<IWpfTextView>();
            _mockAdapterFactoryService.Expect(x => x.GetWpfTextView(_mockTextView)).Return(mockWpfTextView);

            _listener.VsTextViewCreated(_mockTextView);

            _mockAdapterFactoryService.VerifyAllExpectations();
        }

        [Test]
        public void ListenerShouldAttemptToGetAnInstanceOfTheVisualStudioEnvironment()
        {
            _listener.VsTextViewCreated(_mockTextView);
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
