using System.Collections.Generic;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rhino.Mocks;
using SparkSense.Parsing;
using SparkSense.StatementCompletion;

namespace SparkSense.Tests.StatementCompletion
{
    [TestFixture]
    public class CompletionSessionConfigurationTests
    {

        [Test]
        public void ConfigurationShouldCreateASessionFromCompletionBroker()
        {
            var mockBroker = MockRepository.GenerateMock<ICompletionBroker>();
            var mockTextExplorer = MockRepository.GenerateMock<ITextExplorer>();
            var stubTextView = MockRepository.GenerateStub<ITextView>();
            var stubTrackingPoint = MockRepository.GenerateStub<ITrackingPoint>();
            var stubSession = MockRepository.GenerateStub<ICompletionSession>();

            mockTextExplorer.Expect(x => x.TextView).Return(stubTextView);
            mockTextExplorer.Expect(x => x.GetTrackingPoint()).Return(stubTrackingPoint);
            mockBroker.Expect(x => x.CreateCompletionSession(stubTextView, stubTrackingPoint, true)).Return(stubSession);

            var config = new CompletionSessionConfiguration(mockBroker);

            ICompletionSession session;
            Assert.That(config.TryCreateCompletionSession(mockTextExplorer, out session));
            Assert.That(session, Is.EqualTo(stubSession));

            mockTextExplorer.VerifyAllExpectations();
            mockBroker.VerifyAllExpectations();
        }

        [Test]
        public void ConfigurationShouldAddPropertiesRequiredForCompletionSourceToSession()
        {
            var stubSession = MockRepository.GenerateStub<ICompletionSession>();
            var stubBroker = MockRepository.GenerateStub<ICompletionBroker>();
            var stubTextExplorer = MockRepository.GenerateStub<ITextExplorer>();

            var mockProperties = MockRepository.GenerateMock<PropertyCollection>();

            var propertiesToAdd =
                new Dictionary<object, object> {
                    { typeof(ITrackingSpan), "SomeObject" },
                    { typeof(IViewExplorer), "SomeObject" }, 
                    { typeof(SparkSyntaxTypes), "SomeObject" } 
                };

            stubBroker.Stub(x => x.CreateCompletionSession(null, null, true)).IgnoreArguments().Return(stubSession);
            stubSession.Stub(x => x.Properties).Return(mockProperties);

            var config = new CompletionSessionConfiguration(stubBroker);
            ICompletionSession session;
            config.TryCreateCompletionSession(stubTextExplorer, out session);
            config.AddCompletionSourceProperties(propertiesToAdd);

            Assert.That(mockProperties.ContainsProperty(typeof(ITrackingSpan)));
            Assert.That(mockProperties.ContainsProperty(typeof(IViewExplorer)));
            Assert.That(mockProperties.ContainsProperty(typeof(SparkSyntaxTypes)));
            mockProperties.VerifyAllExpectations();
        }
    }
}
