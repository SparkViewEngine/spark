using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ActionSample.Helpers
{
    public class Rule
    {
        public Func<XNode, bool> Predicate { get; set; }
        public Action<XNode> Action { get; set; }
    }
}
