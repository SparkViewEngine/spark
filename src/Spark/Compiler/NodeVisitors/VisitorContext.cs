using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;
using Spark.Parser;

namespace Spark.Compiler.NodeVisitors
{
    public enum NamespacesType
    {
        Unqualified,
        Qualified
    }

    public class VisitorContext
    {
        public VisitorContext()
        {
            Namespaces = NamespacesType.Unqualified;
            Paint = new Paint[0];
            PartialFileNames = new string[0];
        }

        public ISparkSyntaxProvider SyntaxProvider { get; set; }

        public string ViewPath { get; set; }
        public IViewFolder ViewFolder { get; set; }

        public string Prefix { get; set; }
        public NamespacesType Namespaces { get; set; }
        public IEnumerable<Paint> Paint { get; set; }
        public IList<string> PartialFileNames { get; set; }
        public ISparkExtensionFactory ExtensionFactory { get; set; }

    }
}
