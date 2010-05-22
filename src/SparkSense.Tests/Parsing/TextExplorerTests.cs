using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using NUnit.Framework;
using Rhino.Mocks;
using SparkSense.Parsing;

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
    }
}
