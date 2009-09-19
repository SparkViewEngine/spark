using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
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
            var bindings = elements.Select(
                element =>
                new Binding
                    {
                        ElementName = (string) element.Attribute("name"),
                        Nodes = grammar.Nodes(new Position(new SourceContext(element.Value))).Value
                    });

            return bindings;
        }

        public abstract IEnumerable<Binding> GetBindings(IViewFolder viewFolder);
    }
}
