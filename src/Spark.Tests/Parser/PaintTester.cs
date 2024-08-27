// Copyright 2008-2024 Louis DeJardin
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
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Parser;
using Spark.Parser.Markup;

namespace Spark.Tests.Parser
{
    class PaintTestGrammar : CharGrammar
    {
        public PaintTestGrammar()
        {
            var notDigit = Rep(ChNot(char.IsDigit).Unless(Ch(char.IsWhiteSpace)))
                .Build(hit => new string(hit.ToArray()));

            var isDigit = Rep1(Ch(char.IsDigit))
                .Build(hit => new string(hit.ToArray()));

            DigitPaint = notDigit.And(Paint(isDigit)).And(notDigit)
                .Build(hit => new PaintInfo { Before = hit.Left.Left, Thing = hit.Left.Down, After = hit.Down });

            ManyPaints = Rep(Opt(Ch(char.IsWhiteSpace)).And(DigitPaint.Paint()).Down());
        }

        public ParseAction<PaintInfo> DigitPaint;
        public ParseAction<IList<PaintInfo>> ManyPaints;

    }


    class PaintInfo
    {
        public string Before { get; set; }
        public string Thing { get; set; }
        public string After { get; set; }
    }

    [TestFixture]
    public class PaintTester
    {
        PaintTestGrammar _grammar;

        [SetUp]
        public void Init()
        {
            _grammar = new PaintTestGrammar();
        }

        [Test]
        public void PaintingRangeOfText()
        {
            var pos = new Position(new SourceContext("abc123def"));
            var result = _grammar.DigitPaint(pos);
            Assert.AreEqual("abc", result.Value.Before);
            Assert.AreEqual("123", result.Value.Thing);
            Assert.AreEqual("def", result.Value.After);

            Assert.AreEqual(1, result.Rest.GetPaint().Count());

            Assert.AreEqual("123", result.Rest.PaintLink.Paint.Value);
            Assert.AreEqual(3, result.Rest.PaintLink.Paint.Begin.Offset);
            Assert.AreEqual(6, result.Rest.PaintLink.Paint.End.Offset);
        }

        [Test]
        public void PaintingArray()
        {
            var pos = new Position(new SourceContext(" abc123def a5t 34w e5 "));
            var result = _grammar.ManyPaints(pos);
            Assert.AreEqual(4, result.Value.Count);

            var paints = new List<Paint>();
            foreach (var info in result.Value)
            {
                var info1 = info;
                var paint = result.Rest.GetPaint().FirstOrDefault(x => x.Value == info1);
                paints.Add(paint);
            }

            Assert.AreEqual(1, paints[0].Begin.Offset);
            Assert.AreEqual(10, paints[0].End.Offset);
            Assert.AreEqual(11, paints[1].Begin.Offset);
            Assert.AreEqual(14, paints[1].End.Offset);
            Assert.AreEqual(15, paints[2].Begin.Offset);
            Assert.AreEqual(18, paints[2].End.Offset);
            Assert.AreEqual(19, paints[3].Begin.Offset);
            Assert.AreEqual(21, paints[3].End.Offset);

            Assert.AreEqual(4, result.Rest.GetPaint().OfType<Paint<string>>().Count());
            Assert.AreEqual(4, result.Rest.GetPaint().OfType<Paint<PaintInfo>>().Count());
        }

        [Test]
        public void PaintingNodes()
        {
            var input = "<div><p class='subtle'>Hello World</p> ${Tada} </div>";
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(new Position(new SourceContext(input)));
            Assert.AreEqual(8, result.Value.Count);
            Assert.AreEqual(10, result.Rest.GetPaint().OfType<Paint<Node>>().Count());
        }


        [Test]
        public void PaintingAttributes()
        {
            var input = "<div id='rea' class='foo'/>";
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(new Position(new SourceContext(input)));
            Assert.AreEqual(1, result.Value.Count);
            
            var paints = result.Rest.GetPaint();
            Assert.AreEqual(1, paints.Select(p => p.Value).OfType<ElementNode>().Count());
            Assert.AreEqual(2, paints.Select(p => p.Value).OfType<AttributeNode>().Count());
            Assert.AreEqual(2, paints.Select(p => p.Value).OfType<TextNode>().Count());
            Assert.AreEqual(5, paints.OfType<Paint<Node>>().Count());

            var attrId = paints.Single(p => (p.Value is AttributeNode) && ((AttributeNode)p.Value).Name == "id");
            var attrClass = paints.Single(p => (p.Value is AttributeNode) && ((AttributeNode)p.Value).Name == "class");

            Assert.AreEqual(5, attrId.Begin.Offset);
            Assert.AreEqual(13, attrId.End.Offset);
            Assert.AreEqual(14, attrClass.Begin.Offset);
            Assert.AreEqual(25, attrClass.End.Offset);
        }
    }
}
