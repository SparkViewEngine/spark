using System.Collections.Generic;
using System.Linq;
using Spark.Parser.Markup;

namespace Spark.Parser.Offset
{
    static class ParserAssistance
    {
        public static ParseAction<TValue1> Skip<TValue1, TValue2>(
            this ParseAction<TValue1> p1,
            ParseAction<TValue2> p2)
        {
            return input =>
                       {
                           var r1 = p1(input);
                           if (r1 == null)
                           {
                               return null;
                           }

                           var r2 = p2(r1.Rest);
                           if (r2 == null)
                           {
                               return null;
                           }
                           return new ParseResult<TValue1>(
                               r2.Rest,
                               r1.Value);
                       };
        }
    }

    public class OffsetGrammar : MarkupGrammar
    {
        public OffsetGrammar()
            : this(ParserSettings.DefaultBehavior)
        {
        }

        public OffsetGrammar(IParserSettings settings)
            : base(settings)
        {
            var lineBreak = Opt(Ch('\r')).And(Ch('\n').Or(ChSTX()).Or(ChETX()));
            var lineBreakOrStart = Opt(Ch('\r')).And(Ch('\n').Or(ChSTX()));
            var lineBreakOrEnd = Opt(Ch('\r')).And(Ch('\n').Or(ChETX()));

            var indentation = lineBreakOrStart.And(Rep(Ch(' ', '\t'))).NotNext(lineBreakOrEnd)
                .Build(hit => new IndentationNode(hit.Down));

            var ws = Rep1(Ch(' ', '\t'));
            var ows = Rep(Ch(' ', '\t'));

            var whiteLine = lineBreakOrStart.And(Rep(Ch(' ', '\t'))).IfNext(lineBreakOrEnd)
                .Build(hit => new Node[0]);

            //[4]   	NameChar	   ::=   	 Letter | Digit | '.' | '-' | '_' | ':' | CombiningChar | Extender  
            var NameChar = Ch(char.IsLetterOrDigit).Or(Ch(/*'.',*/ '-', '_', ':'))/*.Or(CombiningChar).Or(Extener)*/;

            //[5]   	Name	   ::=   	(Letter | '_' | ':') (NameChar)*
            var Name =
                Ch(char.IsLetter).Or(Ch('_', ':')).And(Rep(NameChar))
                .Build(hit => hit.Left + new string(hit.Down.ToArray()));

            var text = Ch('|').And(Rep(Ch(_ => true).Unless(lineBreak)))
                .Build(hit => new TextNode(hit.Down));


            var nonTextHunk = AsNode(EntityRef).Or(AsNode(Code));
            var textHunk = Rep1(Ch(ch => true).Unless(lineBreakOrEnd).Unless(nonTextHunk))
                .Build(hit => new TextNode(hit));

            var texts = Ch('|').And(Rep(AsNode(textHunk).Or(nonTextHunk)))
                .Build(hit => hit.Down);


            var expression = Ch('=').And(LimitedExpression(lineBreakOrEnd.Build(x => "")))
                .Build(hit => new ExpressionNode(hit.Down));

            var statementDash = Ch('-').And(Statement1)
                .Build(hit => new StatementNode(hit.Down));

            var statementAt = Ch("@{").And(LimitedExpression(Ch("}"))).And(Ch('}'))
                .Build(hit => new StatementNode(hit.Left.Down));

            var statement = statementDash.Or(statementAt);

            var elementId = Ch('#').And(Rep(Ch(char.IsLetterOrDigit))).Skip(ows).Build(hit => new string(hit.Down.ToArray()));
            var elementClass = Ch('.').And(Rep(Ch(char.IsLetterOrDigit))).Skip(ows).Build(hit => new string(hit.Down.ToArray()));

            var elementClassOrId = Rep(elementClass).And(Opt(elementId)).And(Rep(elementClass))
                .Build(hit => new { id = hit.Left.Down, classes = hit.Left.Left.Concat(hit.Down) });
            var elementClassOrId1 = Rep(elementClass).And(elementId).And(Rep(elementClass))
                .Or(Rep1(elementClass).And(Opt(elementId)).And(Rep(elementClass)))
                .Or(Rep(elementClass).And(Opt(elementId)).And(Rep1(elementClass)))
                .Build(hit => new { id = hit.Left.Down, classes = hit.Left.Left.Concat(hit.Down) });

            var elementLeadin = Name.Skip(ows).And(elementClassOrId)
                .Or(Opt(Name).And(elementClassOrId1))
                .Build(hit => new
                                  {
                                      name = hit.Left ?? "div",
                                      attrs =
                                  (hit.Down.id != null
                                       ? new[] { new AttributeNode("id", hit.Down.id) }
                                       : new AttributeNode[0])
                                  .Concat(hit.Down.classes.Any()
                                              ? new[]
                                                    {
                                                        new AttributeNode("class",
                                                                          string.Join(" ", hit.Down.classes.ToArray()))
                                                    }
                                              : new AttributeNode[0])
                                  });

            var element = elementLeadin.And(Rep(Attribute.Skip(ows)))
                .Build(hit => new ElementNode(
                    hit.Left.name,
                    hit.Left.attrs.Concat(hit.Down).ToList(),
                    false));

            var elementFollower = texts
                .Or(expression.Build(hit => (IList<Node>)new Node[] { hit }))
                .Or(statement.Build(hit => (IList<Node>)new Node[] { hit }));

            var elementPlus = element.Skip(ows).And(Opt(elementFollower));

            var line1 = indentation.And(elementPlus).Build(hit => new[] { hit.Left, (Node)hit.Down.Left }.Concat(hit.Down.Down??new Node[0]).ToArray());
            //var line2 = indentation.And(text).Build(hit => new Node[] { hit.Left, hit.Down });
            var line2 = indentation.And(texts).Build(hit => new Node[] { hit.Left }.Concat(hit.Down).ToArray());
            var line3 = indentation.And(expression).Build(hit => new Node[] { hit.Left, hit.Down });
            var line4 = indentation.And(statement).Build(hit => new Node[] { hit.Left, hit.Down });

            var line = whiteLine.Or(line1).Or(line2).Or(line3).Or(line4).Skip(ows);

            var lines = Rep(line);

            Indentation = indentation;

            TestLine = line;
            OffsetElement = element;
            OffsetText = text;
            OffsetTexts = texts;
            OffsetExpression = expression;
            OffsetStatement = statement;

            OffsetNodes = lines.Build(hit => (IList<Node>)hit.SelectMany(nodes => nodes.Where(node => node != null)).ToList());
        }

        public ParseAction<IndentationNode> Indentation;
        public ParseAction<ElementNode> OffsetElement;
        public ParseAction<TextNode> OffsetText;
        public ParseAction<IList<Node>> OffsetTexts;
        public ParseAction<ExpressionNode> OffsetExpression;
        public ParseAction<StatementNode> OffsetStatement;

        public ParseAction<IList<Node>> OffsetNodes;
        public ParseAction<Node[]> TestLine;
    }
}
