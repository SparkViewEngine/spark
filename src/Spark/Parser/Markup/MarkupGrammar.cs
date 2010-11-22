//-------------------------------------------------------------------------
// <copyright file="Node.cs">
// Copyright 2008-2010 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// <author>Louis DeJardin</author>
// <author>John Gietzen</author>
//-------------------------------------------------------------------------

namespace Spark.Parser.Markup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Spark.Parser.Code;

    public class MarkupGrammar : CodeGrammar
    {
        public MarkupGrammar()
            : this(ParserSettings.DefaultBehavior)
        {
        }

        public MarkupGrammar(IParserSettings settings)
        {
            var Apos = Ch('\'');
            var Quot = Ch('\"');
            var Lt = Ch('<');
            var Gt = Ch('>');


            //var CombiningChar = Ch('*');
            //var Extener = Ch('*');

            //[4]   	NameChar	   ::=   	 Letter | Digit | '.' | '-' | '_' | ':' | CombiningChar | Extender  
            var NameChar = Ch(char.IsLetterOrDigit).Or(Ch('.', '-', '_', ':'))/*.Or(CombiningChar).Or(Extener)*/;

            //[5]   	Name	   ::=   	(Letter | '_' | ':') (NameChar)*
            var Name =
                Ch(char.IsLetter).Or(Ch('_', ':')).And(Rep(NameChar))
                .Build(hit => hit.Left + new string(hit.Down.ToArray()));

            //[7]   	Nmtoken	   ::=   	(NameChar)+
            var NmToken =
                Rep1(NameChar)
                .Build(hit => new string(hit.ToArray()));

            //[3]   	S	   ::=   	(#x20 | #x9 | #xD | #xA)+
            Whitespace = Rep1(Ch(char.IsWhiteSpace));

            //[25]   	Eq	   ::=   	 S? '=' S?
            var Eq = Opt(Whitespace).And(Ch('=')).And(Opt(Whitespace));



            var paintedStatement1 = Statement1.Build(hit => new StatementNode(hit)).Paint<StatementNode, Node>();

            var statementMarker = string.IsNullOrEmpty(settings.StatementMarker) ? "#" : settings.StatementMarker;

            // Syntax 1: '\r'? ('\n' | '\u0002') S? '#' (statement ^('\r' | '\n' | '\u0003') )
            var StatementNode1 = Opt(Ch('\r')).And(Ch('\n').Or(ChSTX())).And(Rep(Ch(' ', '\t'))).And(TkCode(Ch(statementMarker))).And(paintedStatement1).IfNext(Ch('\r', '\n').Or(ChETX()))
                .Build(hit => hit.Down);


            var paintedStatement2 = Statement2.Build(hit => new StatementNode(hit)).Paint<StatementNode, Node>();

            // Syntax 2: '<%' (statement ^'%>')  '%>' 
            var StatementNode2 = TkAspxCode(Ch("<%")).NotNext(Ch('=')).And(paintedStatement2).And(TkAspxCode(Ch("%>")))
                .Build(hit => hit.Left.Down);

            Statement = StatementNode1.Or(StatementNode2);



            // Syntax 1: ${csharp_expression}
            var Code1 = TkCode(Ch("${")).And(Expression).And(TkCode(Ch('}')))
                .Build(hit => new ExpressionNode(hit.Left.Down) { AutomaticEncoding = settings.AutomaticEncoding });

            // Syntax 3: <%=csharp_expression%>;
            var Code3 = TkAspxCode(Ch("<%")).And(TkAttDelim(Ch('='))).And(Expression).And(TkAspxCode(Ch("%>")))
                .Build(hit => new ExpressionNode(hit.Left.Down));

            // Syntax 4: $!{csharp_expression}
            var Code4 = TkCode(Ch("$!{")).And(Expression).And(TkCode(Ch('}')))
                .Build(hit => new ExpressionNode(hit.Left.Down) { SilentNulls = true, AutomaticEncoding = settings.AutomaticEncoding });

            // Syntax 5: !{sharp_expression}
            var Code5 = TkCode(Ch("!{")).And(Expression).And(TkCode(Ch('}')))
                .Build(hit => new ExpressionNode(hit.Left.Down));

            Code = Code1.Or(Code3).Or(Code4).Or(Code5);

            var Condition = TkCode(Ch("?{")).And(Expression).And(TkCode(Ch('}')))
                .Build(hit => new ConditionNode(hit.Left.Down));

            var LessThanTextNode = Ch('<')
                .Build(hit => (Node)new TextNode("<"));

            //[68]   	EntityRef	   ::=   	'&' Name ';'
            EntityRef =
                TkEntity(Ch('&').And(Name).And(Ch(';')))
                .Build(hit => new EntityNode(hit.Left.Down));

            var EntityRefOrAmpersand = AsNode(EntityRef).Or(Ch('&').Build(hit => (Node)new TextNode("&")));

            //[10]   	AttValue	   ::=   	'"' ([^<&"] | Reference)* '"' |  "'" ([^<&'] | Reference)* "'"
            var AttValueSingleText = TkAttVal(Rep1(ChNot('<', '&', '\'').Unless(Code).Unless(Condition))).Build(hit => new TextNode(hit));
            var AttValueSingle = TkAttQuo(Apos).And(Rep(AsNode(AttValueSingleText).Or(EntityRefOrAmpersand).Or(AsNode(Code)).Or(AsNode(Condition)).Or(LessThanTextNode).Paint())).And(TkAttQuo(Apos));
            var AttValueDoubleText = TkAttVal(Rep1(ChNot('<', '&', '\"').Unless(Code).Unless(Condition))).Build(hit => new TextNode(hit));
            var AttValueDouble = TkAttQuo(Quot).And(Rep(AsNode(AttValueDoubleText).Or(EntityRefOrAmpersand).Or(AsNode(Code)).Or(AsNode(Condition)).Or(LessThanTextNode).Paint())).And(TkAttQuo(Quot));
            var AttValue = AttValueSingle.Or(AttValueDouble).Left().Down();


            //[41]   	Attribute	   ::=   	 Name  Eq  AttValue  
            Attribute =
                TkAttNam(Name).And(TkAttDelim(Eq)).And(AttValue)
                .Build(hit => new AttributeNode(hit.Left.Left, hit.Down)).Paint<AttributeNode, Node>();


            //[40]   	STag	   ::=   	'<' Name (S  Attribute)* S? '>'
            //[44]   	EmptyElemTag	   ::=   	'<' Name (S  Attribute)* S? '/>'
            Element =
                Opt(Ch("\r\n").Or(Ch("\n")).And(StringOf(Ch(char.IsWhiteSpace).Unless(Ch('\r', '\n'))))).And(TkTagDelim(Lt)).And(TkEleNam(Name)).And(Rep(Whitespace.And(Attribute).Down())).And(Opt(Whitespace)).And(Opt(TkTagDelim(Ch('/')))).And(TkTagDelim(Gt))
                .Build(hit => new ElementNode(
                    hit.Left.Left.Left.Left.Down,
                    hit.Left.Left.Left.Down,
                    hit.Left.Down != default(char),
                    hit.Left.Left.Left.Left.Left.Left == null ? string.Empty : hit.Left.Left.Left.Left.Left.Left.Left + hit.Left.Left.Left.Left.Left.Left.Down));

            //[42]   	ETag	   ::=   	'</' Name  S? '>'
            EndElement =
                Opt(Ch("\r\n").Or(Ch("\n")).And(StringOf(Ch(char.IsWhiteSpace).Unless(Ch('\r', '\n'))))).And(TkTagDelim(Lt.And(Ch('/')))).And(TkEleNam(Name)).And(Opt(Whitespace)).And(TkTagDelim(Gt))
                .Build(hit => new EndElementNode(hit.Left.Left.Down, hit.Left.Left.Left.Left == null ? string.Empty : hit.Left.Left.Left.Left.Left + hit.Left.Left.Left.Left.Down));

            Text =
                Rep1(ChNot('&', '<').Unless(Statement).Unless(Code).Unless(Element).Unless(EndElement))
                .Build(hit => new TextNode(hit));

            //[15]   	Comment	   ::=   	'<!--' ((Char - '-') | ('-' (Char - '-')))* '-->'
            Comment =
                TkComm(Ch("<!--").And(Rep(ChNot('-').Or(Ch('-').IfNext(ChNot('-'))))).And(Ch("-->")))
                .Build(hit => new CommentNode(hit.Left.Down));

            //[11]   	SystemLiteral	   ::=   	('"' [^"]* '"') | ("'" [^']* "'")
            var SystemLiteral =
                Quot.And(Rep(ChNot('\"'))).And(Quot).Or(Apos.And(Rep(ChNot('\''))).And(Apos))
                .Build(hit => new string(hit.Left.Down.ToArray()));

            //[13]   	PubidChar	   ::=   	#x20 | #xD | #xA | [a-zA-Z0-9] | [-'()+,./:=?;!*#@$_%]
            var PubidChar1 = Ch(char.IsLetterOrDigit).Or(Ch(" \r\n-()+,./:=?;!*#@$_%".ToArray()));
            var PubidChar2 = PubidChar1.Or(Apos);

            //[12]   	PubidLiteral	   ::=   	'"' PubidChar* '"' | "'" (PubidChar - "'")* "'"
            var PubidLiteral =
                Quot.And(Rep(PubidChar2)).And(Quot).Or(Apos.And(Rep(PubidChar1)).And(Apos))
                .Build(hit => new string(hit.Left.Down.ToArray()));

            //[75]   	ExternalID	   ::=   	'SYSTEM' S  SystemLiteral | 'PUBLIC' S PubidLiteral S SystemLiteral 
            var ExternalIDSystem =
                Ch("SYSTEM").And(Whitespace).And(SystemLiteral)
                .Build(hit => new ExternalIdInfo
                                  {
                                      ExternalIdType = hit.Left.Left,
                                      SystemId = hit.Down
                                  });
            var ExternalIDPublic =
                Ch("PUBLIC").And(Whitespace).And(PubidLiteral).And(Whitespace).And(SystemLiteral)
                .Build(hit => new ExternalIdInfo
                                  {
                                      ExternalIdType = hit.Left.Left.Left.Left,
                                      PublicId = hit.Left.Left.Down,
                                      SystemId = hit.Down
                                  });
            var ExternalID = ExternalIDSystem.Or(ExternalIDPublic);

            //[28]   	doctypedecl	   ::=   	'<!DOCTYPE' S  Name (S  ExternalID)? S? ('[' intSubset ']' S?)? '>'
            DoctypeDecl = Ch("<!DOCTYPE").And(Whitespace).And(Name).And(Opt(Whitespace.And(ExternalID).Down())).And(Opt(Whitespace)).And(Ch('>'))
                .Build(hit => new DoctypeNode { Name = hit.Left.Left.Left.Down, ExternalId = hit.Left.Left.Down });

            //[26]   	VersionNum	   ::=   	'1.0'
            var VersionNum = Ch("1.0");

            //[24]   	VersionInfo	   ::=   	 S 'version' Eq ("'" VersionNum "'" | '"' VersionNum '"')
            var VersionInfo = Whitespace.And(Ch("version")).And(Eq).And(
                    Apos.And(VersionNum).And(Apos).Or(Quot.And(VersionNum).And(Quot)));

            //[81]   	EncName	   ::=   	[A-Za-z] ([A-Za-z0-9._] | '-')*
            var EncName = Ch(char.IsLetter).And(Rep(Ch(char.IsLetterOrDigit).Or(Ch('.', '_', '-'))))
                .Build(hit => hit.Left + new string(hit.Down.ToArray()));

            //[80]   	EncodingDecl	   ::=   	 S 'encoding' Eq ('"' EncName '"' | "'" EncName "'" ) 
            var EncodingDecl = Whitespace.And(Ch("encoding")).And(Eq).And(
                Apos.And(EncName).And(Apos).Or(Quot.And(EncName).And(Quot)))
                .Build(hit => hit.Down.Left.Down);

            //[32]   	SDDecl	   ::=   	 S 'standalone' Eq (("'" ('yes' | 'no') "'") | ('"' ('yes' | 'no') '"')) 
            var SSDecl = Whitespace.And(Ch("standalone")).And(Eq).And(
                Apos.And(Ch("yes").Or(Ch("no"))).And(Apos).Or(Quot.And(Ch("yes").Or(Ch("no"))).And(Quot)))
                .Build(hit => hit.Down.Left.Down);

            //[23]   	XMLDecl	   ::=   	'<?xml' VersionInfo  EncodingDecl? SDDecl? S? '?>'
            XMLDecl =
                Ch("<?xml").And(VersionInfo).And(Opt(EncodingDecl)).And(Opt(SSDecl)).And(Opt(Whitespace)).And(Ch("?>"))
                .Build(hit => new XMLDeclNode { Encoding = hit.Left.Left.Left.Down, Standalone = hit.Left.Left.Down });

            //[17]   	PITarget	   ::=   	Name - (('X' | 'x') ('M' | 'm') ('L' | 'l'))
            var PITarget = Name.Unless(Ch('X', 'x').And(Ch('M', 'm')).And(Ch('L', 'l')));

            //[16]   	PI	   ::=   	'<?' PITarget (S (Char* - (Char* '?>' Char*)))? '?>'
            ProcessingInstruction = Ch("<?").And(PITarget).And(Opt(Whitespace)).And(Rep(Ch(ch => true).Unless(Ch("?>")))).And(Ch("?>"))
                .Build(hit => new ProcessingInstructionNode { Name = hit.Left.Left.Left.Down, Body = new string(hit.Left.Down.ToArray()) });


            AnyNode = AsNode(Element).Paint()
                .Or(AsNode(EndElement).Paint())
                .Or(AsNode(Text).Paint())
                .Or(EntityRefOrAmpersand.Paint())
                .Or(AsNode(Statement))
                .Or(AsNode(Code).Paint())
                .Or(AsNode(DoctypeDecl).Paint())
                .Or(AsNode(Comment).Paint())
                .Or(AsNode(XMLDecl).Paint())
                .Or(AsNode(ProcessingInstruction).Paint())
                .Or(AsNode(LessThanTextNode).Paint());

            Nodes = Rep(AnyNode);
        }

        public ParseAction<IList<char>> Whitespace;

        public ParseAction<IList<Node>> Nodes;
        public ParseAction<Node> AnyNode;

        public ParseAction<DoctypeNode> DoctypeDecl;
        public ParseAction<TextNode> Text;
        public ParseAction<EntityNode> EntityRef;
        public ParseAction<ElementNode> Element;
        public ParseAction<EndElementNode> EndElement;
        public ParseAction<AttributeNode> Attribute;
        public ParseAction<CommentNode> Comment;
        public ParseAction<ProcessingInstructionNode> ProcessingInstruction;
        public ParseAction<XMLDeclNode> XMLDecl;

        public ParseAction<ExpressionNode> Code;
        public ParseAction<StatementNode> Statement;


        public ParseAction<Node> AsNode<TValue>(ParseAction<TValue> parser) where TValue : Node
        {
            return delegate(Position input)
            {
                var result = parser(input);
                if (result == null)
                {
                    return null;
                }

                return new ParseResult<Node>(result.Rest, result.Value);
            };
        }
    }
}
