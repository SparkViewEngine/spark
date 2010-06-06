using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.Generic;
using Spark.Parser.Markup;
using System;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public class SparkAttributeCompletionSet : SparkCompletionSetFactory
    {
        private List<Completion> _completionList;
        public override IList<Completion> Completions
        {
            get
            {
                if (_completionList != null) return _completionList;

                _completionList = new List<Completion>();
                _completionList.AddRange(CheckForSpecialNodes());

                return _completionList;
            }
        }

        private List<Completion> CheckForSpecialNodes()
        {
            var tag = _textExplorer.GetTagAtPosition(_textExplorer.GetStartPosition());
            var node = _textExplorer.GetParsedNodes(tag)[0];

            var attributesForSpecialNode = new List<Completion>();
            if (node is SpecialNode)
            {
                var knownAttributesForNode = GetKnownAttributesForSpecialNode((SpecialNode)node);
                knownAttributesForNode.ForEach(attribute => attributesForSpecialNode.Add(
                    new Completion(
                        attribute, 
                        String.Format("{0}=\"\"", attribute), 
                        String.Format("'{0}' attribute for '{1}' element", attribute, ((SpecialNode)node).Element.Name), 
                        null, null)));
            }

            return attributesForSpecialNode;
        }
        private List<string> GetKnownAttributesForSpecialNode(SpecialNode node)
        {
            var allKnown = new Dictionary<string, List<string>>
            {
                {"var", new List<string>{"type"}},
                {"def", new List<string>{"type"}},
                {"default", new List<string>{"type"}},
                {"global", new List<string>{"type"}},
                {"viewdata", new List<string>{"default","model"}},
                {"set", new List<string>()},
                {"for", new List<string>{"each"}},
                {"test", new List<string>{"condition", "if", "once"}},
                {"if", new List<string>{"condition", "once"}},
                {"else", new List<string>{"if"}},
                {"elseif", new List<string>{"condition"}},
                {"content", new List<string>{"add","def", "name", "set", "var"}},
                {"use", new List<string>{"assembly", "content", "file", "import", "master", "namespace", "pageBaseType" }},
                {"macro", new List<string>{"name"}},
                {"render", new List<string>{"partial", "section"}},
                {"section", new List<string>{"name"}},
                {"cache", new List<string>{"expires", "key", "signal"}}
            };

            List<string> knownAttributes;

            return allKnown.TryGetValue(node.Element.Name, out knownAttributes)
                ? knownAttributes
                : new List<string>();
        }
    }
}
