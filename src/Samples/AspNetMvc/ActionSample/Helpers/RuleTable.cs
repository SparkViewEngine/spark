using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace ActionSample.Helpers
{
    public class RuleTable
    {
        public RuleTable()
        {
            Rules = new List<Rule>();
        }

        public IList<Rule> Rules { get; set; }

        public RuleTable For(Func<XNode, bool> predicate, Action<XNode> action)
        {
            Rules.Add(new Rule { Predicate = predicate, Action = action });
            return this;
        }

        public RuleTable For<T>(Func<T, bool> predicate, Action<T> action) where T : XNode
        {
            Rules.Add(new Rule
            {
                Predicate = node => node is T && predicate((T)node),
                Action = node => { action((T)node); }
            });
            return this;
        }


        public void Dispatch<T>(IEnumerable<T> nodes) where T:XNode
        {
            foreach(var node in nodes)
                Dispatch(node);
        }

        public void Dispatch(XNode node)
        {
            Rules.First(rule => rule.Predicate(node)).Action(node);
        }
    }
}
