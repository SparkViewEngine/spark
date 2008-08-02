// Copyright 2004-2008 Castle Project - http://www.castleproject.org/
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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using NUnit.Framework;
    using global::Spark.Compiler;
    using global::Spark.Parser;
    using global::Spark.Parser.Markup;

    [TestFixture]
    public class ViewComponentSectionChunkBuilderVisitorTests
    {
        [Test]
        public void CorrectSectionsCreated()
        {
            var grammar = new MarkupGrammar();
            var nodes = grammar.Nodes(new Position(new SourceContext(
                "<foo>1<tr>2<td>3</foo> <bar>4</td>5</tr>6</bar> stuff <baaz>yadda<baaz></baaz><quux><quux/></baaz>")));
            var visitor = new ViewComponentSectionChunkBuilderVisitor();
            visitor.Accept(nodes.Value);
            Assert.AreEqual(3, visitor.Sections.Count);
            
            Assert.AreEqual(1, visitor.Sections["foo"].Count);

            Assert.AreEqual("1<tr>2<td>3", ((SendLiteralChunk)visitor.Sections["foo"][0]).Text);
        }


    }
}
