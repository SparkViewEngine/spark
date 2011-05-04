using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser;

namespace Spark.Bindings
{
    public abstract class BindingProvider : IBindingProvider
    {
        public IEnumerable<Binding> LoadStandardMarkup(TextReader reader)
        {
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
                              ElementName = (string)element.Attribute("name")
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

            binding.HasChildReference = binding.Phrases
                .SelectMany(phrase => phrase.Nodes)
                .OfType<BindingChildReference>()
                .Any();
            
            if (binding.Phrases.Count() > 1 && binding.HasChildReference)
            {
                throw new CompilerException("Binding element '" + element.Attribute("name") +
                                            "' can not have child::* in start or end phrases.");
            }

            return binding;
        }

        private static BindingPhrase ParsePhrase(XElement element, BindingGrammar grammar)
        {
            return grammar.Phrase(new Position(new SourceContext(element.Value))).Value;
        }

        public abstract IEnumerable<Binding> GetBindings(IViewFolder viewFolder, string directoryPath);
    }
}
