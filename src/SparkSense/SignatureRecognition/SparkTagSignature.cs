using System;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace SparkSense.SignatureRecognition
{
    internal class SparkTagSignature : ISignature
    {
        private readonly ITextBuffer _textBuffer;
        private IParameter _currentParameter;

        public SparkTagSignature(ITextBuffer textBuffer, string content, string documentation, ReadOnlyCollection<IParameter> parameters)
        {
            Content = content;
            PrettyPrintedContent = content; //TODO: Rob G - Figure this out later
            Documentation = documentation;
            Parameters = parameters;
            _textBuffer = textBuffer;
            _textBuffer.Changed += OnTextBufferChanged;
        }

        #region ISignature Members

        public ITrackingSpan ApplicableToSpan { get; internal set; }

        public string Content { get; private set; }

        public string PrettyPrintedContent { get; private set; }

        public string Documentation { get; private set; }

        public ReadOnlyCollection<IParameter> Parameters { get; internal set; }

        public IParameter CurrentParameter
        {
            get { return _currentParameter; }
            set
            {
                if (_currentParameter == value) return;
                IParameter previousParameter = _currentParameter;
                _currentParameter = value;
                InvokeCurrentParameterChanged(previousParameter, _currentParameter);
            }
        }

        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;

        #endregion

        public void InvokeCurrentParameterChanged(IParameter previousCurrentParameter, IParameter newCurrentParameter)
        {
            EventHandler<CurrentParameterChangedEventArgs> handler = CurrentParameterChanged;
            if (handler != null) handler(this, new CurrentParameterChangedEventArgs(previousCurrentParameter, newCurrentParameter));
        }

        internal void InitCurrentParameter()
        {
            if (Parameters.Count == 0)
            {
                CurrentParameter = null;
                return;
            }

            string signatureText = ApplicableToSpan.GetText(_textBuffer.CurrentSnapshot);

            int currentIndex = 0;
            int spaceCount = 0;

            while (currentIndex < signatureText.Length)
            {
                int commaIndex = signatureText.IndexOf(' ', currentIndex);
                if (commaIndex == -1)
                {
                    break;
                }
                spaceCount++;
                currentIndex = commaIndex + 1;
            }

            CurrentParameter = spaceCount < Parameters.Count
                                   ? Parameters[spaceCount]
                                   : Parameters[Parameters.Count - 1];
        }

        internal void OnTextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            InitCurrentParameter();
        }
    }
}