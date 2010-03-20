using System.Collections.Generic;

namespace Spark.Bindings
{
    public class BindingPhrase
    {
        public enum PhraseType
        {
            Expression,
            Statement,
        };

        public PhraseType Type { get; set; }
        public IList<BindingNode> Nodes { get; set; }
    }
}