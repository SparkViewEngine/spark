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
            Assert.Multiple(() =>
            {
                Assert.That(result.Value.Before, Is.EqualTo("abc"));
                Assert.That(result.Value.Thing, Is.EqualTo("123"));
                Assert.That(result.Value.After, Is.EqualTo("def"));

                Assert.That(result.Rest.GetPaint().Count(), Is.EqualTo(1));

                Assert.That(result.Rest.PaintLink.Paint.Value, Is.EqualTo("123"));
                Assert.That(result.Rest.PaintLink.Paint.Begin.Offset, Is.EqualTo(3));
                Assert.That(result.Rest.PaintLink.Paint.End.Offset, Is.EqualTo(6));
            });
        }

        [Test]
        public void PaintingArray()
        {
            var pos = new Position(new SourceContext(" abc123def a5t 34w e5 "));
            var result = _grammar.ManyPaints(pos);
            Assert.That(result.Value, Has.Count.EqualTo(4));

            var paints = new List<Paint>();
            foreach (var info in result.Value)
            {
                var info1 = info;
                var paint = result.Rest.GetPaint().FirstOrDefault(x => x.Value == info1);
                paints.Add(paint);
            }

            Assert.Multiple(() =>
            {
                Assert.That(paints[0].Begin.Offset, Is.EqualTo(1));
                Assert.That(paints[0].End.Offset, Is.EqualTo(10));
                Assert.That(paints[1].Begin.Offset, Is.EqualTo(11));
                Assert.That(paints[1].End.Offset, Is.EqualTo(14));
                Assert.That(paints[2].Begin.Offset, Is.EqualTo(15));
                Assert.That(paints[2].End.Offset, Is.EqualTo(18));
                Assert.That(paints[3].Begin.Offset, Is.EqualTo(19));
                Assert.That(paints[3].End.Offset, Is.EqualTo(21));

                Assert.That(result.Rest.GetPaint().OfType<Paint<string>>().Count(), Is.EqualTo(4));
                Assert.That(result.Rest.GetPaint().OfType<Paint<PaintInfo>>().Count(), Is.EqualTo(4));
            });
        }

        [Test]
        public void PaintingNodes()
        {
            var input = "<div><p class='subtle'>Hello World</p> ${Tada} </div>";
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(new Position(new SourceContext(input)));
            Assert.Multiple(() =>
            {
                Assert.That(result.Value, Has.Count.EqualTo(8));
                Assert.That(result.Rest.GetPaint().OfType<Paint<Node>>().Count(), Is.EqualTo(10));
            });
        }


        [Test]
        public void PaintingAttributes()
        {
            var input = "<div id='rea' class='foo'/>";
            var grammar = new MarkupGrammar();
            var result = grammar.Nodes(new Position(new SourceContext(input)));
            Assert.That(result.Value, Has.Count.EqualTo(1));

            var paints = result.Rest.GetPaint();
            Assert.Multiple(() =>
            {
                Assert.That(paints.Select(p => p.Value).OfType<ElementNode>().Count(), Is.EqualTo(1));
                Assert.That(paints.Select(p => p.Value).OfType<AttributeNode>().Count(), Is.EqualTo(2));
                Assert.That(paints.Select(p => p.Value).OfType<TextNode>().Count(), Is.EqualTo(2));
                Assert.That(paints.OfType<Paint<Node>>().Count(), Is.EqualTo(5));
            });

            var attrId = paints.Single(p => (p.Value is AttributeNode) && ((AttributeNode)p.Value).Name == "id");
            var attrClass = paints.Single(p => (p.Value is AttributeNode) && ((AttributeNode)p.Value).Name == "class");

            Assert.Multiple(() =>
            {
                Assert.That(attrId.Begin.Offset, Is.EqualTo(5));
                Assert.That(attrId.End.Offset, Is.EqualTo(13));
                Assert.That(attrClass.Begin.Offset, Is.EqualTo(14));
                Assert.That(attrClass.End.Offset, Is.EqualTo(25));
            });
        }
    }
}
