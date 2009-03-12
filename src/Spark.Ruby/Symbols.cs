using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Spark.IronRuby
{
    public class Symbols : CustomSymbolDictionary
    {
        public override SymbolId[] GetExtraKeys()
        {
            return new SymbolId[0];
        }

        protected override bool TrySetExtraValue(SymbolId key, object value)
        {
            return false;
        }

        protected override bool TryGetExtraValue(SymbolId key, out object value)
        {
            value = null;
            return false;
        }
    }
}
