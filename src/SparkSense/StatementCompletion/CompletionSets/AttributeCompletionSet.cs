using Microsoft.VisualStudio.Language.Intellisense;
using System.Collections.Generic;
using Spark.Parser.Markup;
using System;
using System.Linq;
using SparkSense.Parsing;
using Spark.Compiler;

namespace SparkSense.StatementCompletion.CompletionSets
{
    public enum AttributeContexts
    {
        None, Name, Value
    }
    public class AttributeCompletionSet : CompletionSetFactory
    {
        private List<Completion> _completionList;

        public AttributeContexts AttributeContext
        {
            get
            {
                if (SparkSyntax.IsPositionInAttributeValue(CurrentContent, _triggerPoint))
                    return AttributeContexts.Value;
                else if (SparkSyntax.IsPositionInAttributeName(CurrentContent, _triggerPoint))
                    return AttributeContexts.Name;
                return AttributeContexts.None;
            }
        }

        protected override IList<Completion> GetCompletionSetForNodeAndContext()
        {
            if (_completionList != null) return _completionList;
            _completionList = new List<Completion>();

            switch (AttributeContext)
            {
                case AttributeContexts.Name:
                    _completionList.AddRange(GetForElementTypeAndName());
                    break;
                case AttributeContexts.Value:
                    _completionList.AddRange(GetForAttributeValue());
                    break;
                case AttributeContexts.None:
                default:
                    break;
            }
            return _completionList.SortAlphabetically();
        }

        private IEnumerable<Completion> GetForElementTypeAndName()
        {
            var attributesForNode = new List<Completion>();
            Node specialNode;
            if (SparkSyntax.IsSpecialNode(CurrentNode, out specialNode))
                attributesForNode.AddRange(GetForSpecialNode(specialNode));
            else
            {
                attributesForNode.AddRange(GetHtmlNodeExtensions());
                attributesForNode.AddRange(GetPossiblePartialDefaults());
            }

            RemoveAttributesAlreadyUsed(attributesForNode);
            return attributesForNode.Distinct();
        }

        private static void RemoveAttributesAlreadyUsed(List<Completion> attributesForNode)
        {
            if (!(CurrentNode is ElementNode)) return;
            foreach (var attribute in ((ElementNode)CurrentNode).Attributes)
                attributesForNode.RemoveAll(c => c.DisplayText == attribute.Name);
        }
        private IEnumerable<Completion> GetForSpecialNode(Node specialNode)
        {
            var knownCompletions = new List<Completion>();
            var knownAttributes = GetKnownAttributesForSpecialNode((SpecialNode)specialNode);
            knownAttributes.ForEach(attribute => knownCompletions.Add(
                new Completion(
                    attribute,
                    GetInsertionTextForContext(attribute),
                    String.Format("'{0}' attribute for '{1}' element", attribute, ((SpecialNode)specialNode).Element.Name),
                    GetIcon(Constants.ICON_SparkAttribute), null)));
            return knownCompletions;
        }

        private IEnumerable<Completion> GetForAttributeValue()
        {
            var attributeValues = new List<Completion>();

            var chunk = SparkSyntax.ParseContextChunk(CurrentContent, _triggerPoint);

            if (chunk == typeof(ContentChunk))
                attributeValues.AddRange(GetContentNames());
            else if (chunk == typeof(ConditionalChunk))
                attributeValues.AddRange(GetVariables());
            else if (chunk == typeof(ForEachChunk))
                attributeValues.AddRange(GetVariables());
            // TODO: Rob G first?{bool} last?{bool} ?{bool}
            // TODO: Rob G for "Index", "Count", "IsFirst", and "IsLast"
            else if (chunk == typeof(UseMasterChunk))
                attributeValues.AddRange(GetPossibleMasterNames());

            return attributeValues;
        }

        private static List<string> GetKnownAttributesForSpecialNode(SpecialNode node)
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

        private string GetInsertionTextForContext(string item)
        {
            if (item == "each")
                return string.Format("{0}=\"var item in\"", item);

            if (AttributeContext == AttributeContexts.Name)
                return String.Format("{0}=\"\"", item);

            return item;
        }

        private IEnumerable<Completion> GetHtmlNodeExtensions()
        {
            var extensions = new List<Completion>();
            extensions.Add(new Completion("each", GetInsertionTextForContext("each"), "Inline 'each' used to repeat elements in a loop", GetIcon(Constants.ICON_SparkAttribute), null));
            extensions.Add(new Completion("if", GetInsertionTextForContext("if"), "Inline 'if' used to render elements conditionally", GetIcon(Constants.ICON_SparkAttribute), null));
            extensions.Add(new Completion("elseif", GetInsertionTextForContext("elseif"), "Inline 'elseif' used to render elements conditionally", GetIcon(Constants.ICON_SparkAttribute), null));
            extensions.Add(new Completion("once", GetInsertionTextForContext("once"), "Inline 'once' used to ensure elements rendered only once per request ", GetIcon(Constants.ICON_SparkAttribute), null));
            return extensions;
        }

        private IEnumerable<Completion> GetVariables()
        {
            var variables = new List<Completion>();
            if (_viewExplorer == null) return variables;

            _viewExplorer.GetGlobalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, String.Format("{0}=\"\"", variable), string.Format("Global Variable: '{0}'", variable), GetIcon(Constants.ICON_SparkGlobalVariable), null)));

            _viewExplorer.GetLocalVariables().ToList().ForEach(
                variable => variables.Add(
                    new Completion(variable, GetInsertionTextForContext(variable), string.Format("Local Variable: '{0}'", variable), GetIcon(Constants.ICON_SparkLocalVariable), null)));

            return variables;
        }

        private IEnumerable<Completion> GetContentNames()
        {
            var contentNames = new List<Completion>();
            if (_viewExplorer == null) return contentNames;

            _viewExplorer.GetContentNames().ToList().ForEach(
                contentName => contentNames.Add(
                    new Completion(contentName, contentName, string.Format("Content Name: '{0}'", contentName), GetIcon(Constants.ICON_SparkContentName), null)));
            return contentNames;
        }

        private IEnumerable<Completion> GetPossibleMasterNames()
        {
            var possibleMasters = new List<Completion>();
            if (_viewExplorer == null) return possibleMasters;

            _viewExplorer.GetPossibleMasterLayouts().ToList().ForEach(
                possibleMaster => possibleMasters.Add(
                    new Completion(possibleMaster, possibleMaster, string.Format("Possible Master: '{0}'", possibleMaster), GetIcon(Constants.ICON_SparkMacroParameter), null)));
            return possibleMasters;
        }

        private IEnumerable<Completion> GetPossiblePartialDefaults()
        {
            var possibleDefaults = new List<Completion>();
            if (_viewExplorer == null || !(CurrentNode is ElementNode)) return possibleDefaults;

            var possiblePartial = ((ElementNode)CurrentNode).Name;
            if (!_viewExplorer.GetRelatedPartials().Contains(possiblePartial)) return possibleDefaults;

            _viewExplorer.GetPossiblePartialDefaults(possiblePartial).ToList().ForEach(
                possibleDefault => possibleDefaults.Add(
                    new Completion(possibleDefault, possibleDefault, string.Format("Partial Default Param: '{0}'", possibleDefault), GetIcon(Constants.ICON_SparkPartialParameter), null)));
            return possibleDefaults;
        }
    }
}
