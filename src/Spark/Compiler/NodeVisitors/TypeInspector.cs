using System.Linq;

namespace Spark.Compiler.NodeVisitors
{
    public class TypeInspector
    {
        public TypeInspector(string dataDeclaration)
        {
            var decl = dataDeclaration.Trim();
            var lastSpace = decl.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            if (lastSpace < 0)
            {
                Type = dataDeclaration;
                return;
            }

            Name = decl.Substring(lastSpace + 1);

            if (!Name.ToCharArray().All(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == '@'))
            {
                Name = null;
                Type = dataDeclaration;
                return;
            }

            Type = decl.Substring(0, lastSpace).Trim();
        }

        public string Name { get; set; }

        public string Type { get; set; }
    }
}