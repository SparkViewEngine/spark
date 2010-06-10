using System;
using Spark.Parser.Markup;

namespace SparkSense.Parsing
{
    public enum SparkSyntaxTypes
    {
        None,
        Element,
        Attribute,
        AttributeValue,
        Variable,
        Invalid,
    }

    public class SparkSyntax
    {
        private ITextExplorer _textExplorer;

        public SparkSyntax(ITextExplorer textExplorer)
        {
            _textExplorer = textExplorer;
        }

        public bool IsSparkSyntax(char inputCharacter, out SparkSyntaxTypes syntaxType)
        {
            syntaxType = SparkSyntaxTypes.None;
            if (inputCharacter.Equals(char.MinValue)) return false;
            syntaxType = GetSyntaxType(inputCharacter);

            return syntaxType != SparkSyntaxTypes.None;
        }

        public SparkSyntaxTypes GetSyntaxType(char key)
        {
            switch (key)
            {
                case '<':
                    return SparkSyntaxTypes.Element;
                case ' ':
                    return CheckForAttribute();
                case '{': //TODO: Check for preceeding $
                    return SparkSyntaxTypes.Variable;
                case '"': //TODO Check for preceeding =
                    return SparkSyntaxTypes.AttributeValue;
                default:
                    if (Char.IsLetterOrDigit(key.ToString(), 0))
                        return CheckWord();
                    return SparkSyntaxTypes.None;
            }
        }

        private SparkSyntaxTypes CheckWord()
        {
            if (_textExplorer.IsCurrentWordAnElement())
                return SparkSyntaxTypes.Element;
            return SparkSyntaxTypes.None;
        }

        private SparkSyntaxTypes CheckForAttribute()
        {
            if (_textExplorer.IsPositionedInsideAnElement(_textExplorer.GetStartPosition())) return SparkSyntaxTypes.None;

            var node = _textExplorer.GetNodeAtPosition(_textExplorer.GetStartPosition());
            return node is ElementNode ? SparkSyntaxTypes.Attribute : SparkSyntaxTypes.None;
        }
    }
}
