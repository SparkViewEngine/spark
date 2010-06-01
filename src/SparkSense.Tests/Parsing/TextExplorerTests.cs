using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;
using NUnit.Framework.SyntaxHelpers;
using Spark.Parser.Markup;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class TextExplorerTests
    {
        [Test]
        public void ShouldReturnTheStartPositionFromTheCaretPosition()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockCaret = MockRepository.GenerateMock<ITextCaret>();

            mockCaret.Expect(x => x.Position).Return(new CaretPosition());
            mockTextView.Expect(x => x.Caret).Return(mockCaret).Repeat.Any();

            var textExplorer = new TextExplorer(mockTextView);
            textExplorer.GetStartPosition();

            mockCaret.VerifyAllExpectations();
            mockTextView.VerifyAllExpectations();
        }

        [Test]
        public void ShouldReturnTheTrackingPointFromTheCaretAndTextView()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockSnapShot = MockRepository.GenerateMock<ITextSnapshot>();
            var mockCaret = MockRepository.GenerateMock<ITextCaret>();

            mockTextView.Expect(x => x.TextSnapshot).Return(mockSnapShot).Repeat.Any();
            mockTextView.Expect(x => x.Caret).Return(mockCaret).Repeat.Any();
            mockSnapShot.Expect(x => x.CreateTrackingPoint(0, PointTrackingMode.Positive)).Return(null);

            var textExplorer = new TextExplorer(mockTextView);
            textExplorer.GetTrackingPoint();

            mockTextView.VerifyAllExpectations();
            mockSnapShot.VerifyAllExpectations();
            mockCaret.VerifyAllExpectations();
        }

        [Test]
        public void ShouldReturnTheTrackingSpanFromTheCaretAndTextView()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockSnapShot = MockRepository.GenerateMock<ITextSnapshot>();
            var mockCaret = MockRepository.GenerateMock<ITextCaret>();

            mockTextView.Expect(x => x.TextSnapshot).Return(mockSnapShot).Repeat.Any();
            mockTextView.Expect(x => x.Caret).Return(mockCaret).Repeat.Any();
            mockSnapShot.Expect(x => x.CreateTrackingSpan(0, 0, SpanTrackingMode.EdgeInclusive)).Return(null);

            var textExplorer = new TextExplorer(mockTextView);
            textExplorer.GetTrackingSpan();

            mockTextView.VerifyAllExpectations();
            mockSnapShot.VerifyAllExpectations();
            mockCaret.VerifyAllExpectations();
        }

        [Test]
        public void ShouldGetNodesInCurrentDocument()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockSnapShot = MockRepository.GenerateMock<ITextSnapshot>();

            mockTextView.Expect(x => x.TextSnapshot).Return(mockSnapShot).Repeat.Any();
            mockSnapShot.Expect(x => x.GetText()).Return("<html><body><use content=\"main\" /></body></html>");

            var textExplorer = new TextExplorer(mockTextView);
            var nodes = textExplorer.GetParsedNodes();

            mockTextView.VerifyAllExpectations();
            mockSnapShot.VerifyAllExpectations();
            Assert.That(nodes.Count, Is.EqualTo(5));
        }

        [Test]
        public void ShouldGetNodeInWhichTheCaretIsPositioned()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockSnapShot = MockRepository.GenerateMock<ITextSnapshot>();

            mockTextView.Expect(x => x.TextSnapshot).Return(mockSnapShot).Repeat.Any();
            mockSnapShot.Expect(x => x.GetText()).Return("<html><body><use content=\"main\" /></body></html>").Repeat.Any();

            var textExplorer = new TextExplorer(mockTextView);
            Node node = textExplorer.GetNodeAtPosition(17);

            mockTextView.VerifyAllExpectations();
            mockSnapShot.VerifyAllExpectations();

            Assert.That(node is ElementNode);
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
        }

        [Test]
        public void ShouldGetNodeInWhichTheCaretIsPositionedIfTagIsNotClosed()
        {
            var mockTextView = MockRepository.GenerateMock<ITextView>();
            var mockSnapShot = MockRepository.GenerateMock<ITextSnapshot>();

            mockTextView.Expect(x => x.TextSnapshot).Return(mockSnapShot).Repeat.Any();
            mockSnapShot.Expect(x => x.GetText()).Return("<html><body><use </body></html>").Repeat.Any();

            var textExplorer = new TextExplorer(mockTextView);
            Node node = textExplorer.GetNodeAtPosition(17);

            mockTextView.VerifyAllExpectations();
            mockSnapShot.VerifyAllExpectations();

            Assert.That(node is ElementNode);
            Assert.That(((ElementNode)node).Name, Is.EqualTo("use"));
        }
    }
}
