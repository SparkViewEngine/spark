using System;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text;
using System.Windows;
using System.Windows.Data;

namespace SparkSense.Presenter
{
    public class SparkSensePresenter : IPopupIntellisensePresenter, IIntellisenseCommandTarget
    {
        private ICompletionSession _completionSession;
        private ITrackingSpan _presentationSpan;
        private SparkSenseView _view;
       
        public SparkSensePresenter(ICompletionSession completionSession)
        {
            _view = new SparkSenseView(this);
            InitCompletionSession(completionSession);
            InitCompletionItems();
        }

        public CollectionViewSource Items { get; private set; }

        private void InitCompletionSession(ICompletionSession completionSession)
        {
            _completionSession = completionSession;
            if (_completionSession == null) return;
            _completionSession.SelectedCompletionSet.SelectionStatusChanged += SelectedCompletionSet_SelectionStatusChanged;
            _completionSession.Dismissed += CompletionSession_Dismissed;
        }

        private void InitCompletionItems()
        {
            Items = new CollectionViewSource();
            Items.Source = _completionSession.SelectedCompletionSet.Completions;

            if (!IsCompletionListValid()) return;
            Items.View.MoveCurrentToPrevious();
        }

        private void SelectedCompletionSet_SelectionStatusChanged(object sender, ValueChangedEventArgs<CompletionSelectionStatus> e)
        {
            if (!IsCompletionListValid()) return;

            if (e.NewValue.IsSelected && e.NewValue.Completion != Items.View.CurrentItem)
                Items.View.MoveCurrentTo(e.NewValue.Completion);
        }

        private void CompletionSession_Dismissed(object sender, EventArgs e)
        {
            if (_completionSession == null) return;
            _completionSession.SelectedCompletionSet.SelectionStatusChanged -= SelectedCompletionSet_SelectionStatusChanged;
            _completionSession.Dismissed -= CompletionSession_Dismissed;
        }

        private void Move(int offset)
        {
            if (!IsCompletionListValid()) return;

            var newPosition = Items.View.CurrentPosition + offset;
            if (PositionIsInBounds(newPosition))
                Items.View.MoveCurrentToPosition(newPosition);
            else
                MoveToFirstOrLast(newPosition);

            _completionSession.SelectedCompletionSet.SelectionStatus = new CompletionSelectionStatus((Completion)Items.View.CurrentItem, true, true);
        }

        private void MoveToFirstOrLast(int newPosition)
        {
            if (newPosition < 0)
                Items.View.MoveCurrentToFirst();
            else
                Items.View.MoveCurrentToLast();

            if (Items.View.IsCurrentBeforeFirst) Items.View.MoveCurrentToFirst();
            if (Items.View.IsCurrentAfterLast) Items.View.MoveCurrentToLast();
        }

        private bool IsCompletionListValid()
        {
            return Items != null && Items.View != null && _completionSession != null;
        }

        private bool PositionIsInBounds(int newPosition)
        {
            return newPosition < ((ListCollectionView)Items.View).Count && newPosition > -1;
        }

        private ITrackingSpan GetPresentationSpan()
        {
            SnapshotSpan span = _completionSession.SelectedCompletionSet.ApplicableTo.GetSpan(_completionSession.TextView.TextSnapshot);
            NormalizedSnapshotSpanCollection spans = _completionSession.TextView.BufferGraph.MapUpToBuffer(span, _completionSession.SelectedCompletionSet.ApplicableTo.TrackingMode, _completionSession.TextView.TextBuffer);
            if (spans.Count <= 0)
            {
                throw new InvalidOperationException("Completion Session Applicable To Span is invalid.  It doesn't map to a span in the session's text view.");
            }
            SnapshotSpan span2 = spans[0];
            return _completionSession.TextView.TextBuffer.CurrentSnapshot.CreateTrackingSpan(span2.Span, SpanTrackingMode.EdgeInclusive);
        }

        #region IPopupIntellisensePresenter Members

        public event EventHandler<ValueChangedEventArgs<PopupStyles>> PopupStylesChanged;
        public event EventHandler SurfaceElementChanged;
        public event EventHandler PresentationSpanChanged;

        public double Opacity
        {
            get
            {
                return _view.Opacity;
            }
            set
            {
                _view.Opacity = value;
            }
        }

        public PopupStyles PopupStyles
        {
            get { return PopupStyles.PositionClosest; }
        }

        public ITrackingSpan PresentationSpan
        {
            get
            {
                if (_presentationSpan == null)
                    _presentationSpan = GetPresentationSpan();
                return _presentationSpan;
            }
        }

        public string SpaceReservationManagerName
        {
            get { return "completion"; }
        }

        public UIElement SurfaceElement
        {
            get { return _view; }
        }
        #endregion

        #region IIntellisensePresenter Members

        public IIntellisenseSession Session
        {
            get { return _completionSession; }
        }

        #endregion

        #region IIntellisenseCommandTarget Members

        public bool ExecuteKeyboardCommand(IntellisenseKeyboardCommand command)
        {
            switch (command)
            {
                case IntellisenseKeyboardCommand.Up:
                    Move(-1);
                    return true;
                case IntellisenseKeyboardCommand.Down:
                    Move(1);
                    return true;
                case IntellisenseKeyboardCommand.PageUp:
                    Move(-10);
                    return true;
                case IntellisenseKeyboardCommand.PageDown:
                    Move(10);
                    return true;
                case IntellisenseKeyboardCommand.Enter:
                    _completionSession.Commit();
                    return true;
                case IntellisenseKeyboardCommand.Escape:
                    _completionSession.Dismiss();
                    return true;
                //Maybe one day we'll do something with these below
                case IntellisenseKeyboardCommand.End:
                case IntellisenseKeyboardCommand.Home:
                case IntellisenseKeyboardCommand.DecreaseFilterLevel:
                case IntellisenseKeyboardCommand.IncreaseFilterLevel:
                case IntellisenseKeyboardCommand.TopLine:
                case IntellisenseKeyboardCommand.BottomLine:
                    break;
            }
            return false;
        }

        #endregion

    }
}
