using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spark.FileSystem;
using Spark.Parser;

namespace Spark.Bindings {
    public abstract class BindingProvider : IBindingProvider {
        public IEnumerable<Binding> LoadStandardMarkup(TextReader reader) {
            var document = XDocument.Load(reader);
            var elements = document.Elements("bindings").Elements("element");

            var grammar = new BindingGrammar();
            var bindings = elements.Select(element => ParseBinding(element, grammar));

            return bindings;
        }

        private static Binding ParseBinding(XElement element, BindingGrammar grammar)
        {
            var binding = new Binding
                          {
                              ElementName = (string) element.Attribute("name")
                          };

            var start = element.Element("start");
            var end = element.Element("end");

            if (start != null && end != null)
            {
                binding.Phrases = new[]
                                  {
                                      ParsePhrase(start, grammar),
                                      ParsePhrase(end, grammar)
                                  };
            }
            else
            {
                binding.Phrases = new[]
                                  {
                                      ParsePhrase(element, grammar)
                                  };
            }
            return binding;
        }

        private static BindingPhrase ParsePhrase(XElement element, BindingGrammar grammar) {
            return grammar.Phrase(new Position(new SourceContext(element.Value))).Value;
        }

        public abstract IEnumerable<Binding> GetBindings(IViewFolder viewFolder);
    }
}
