using Microsoft.VisualStudio.Language.Intellisense;
using System;
using System.Collections.Generic;

namespace SparkSense.StatementCompletion
{
    public class CompletionSessionConfiguration
    {
        private ICompletionSession _session;

        public CompletionSessionConfiguration(ICompletionSession session)
        {
            _session = session;
        }

        public void AddCompletionSourceProperties(List<object> properties)
        {
            if (properties == null) return;

            foreach (var property in properties)
                _session.Properties.AddProperty(property.GetType(), property);
        }
    }
}
