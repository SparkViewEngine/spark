using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Compiler;
using SparkSense.Parsing;
using System;

namespace SparkSense.Tests.Parsing
{
    [TestFixture]
    public class ParseContextChunkTests
    {
        [Test]
        public void ShouldReturnContentChunkGivenPositionAfterContentElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><content:</div>", position: 14);

            Assert.That(nodeType, Is.EqualTo(typeof(ContentChunk)));
        }

        [Test]
        public void ShouldReturnMacroChunkGivenPositionAfterMacroElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><macro:SomeMacro </div>", position: 12);

            Assert.That(nodeType, Is.EqualTo(typeof(MacroChunk)));
        }

        [Test]
        public void ShouldReturnRenderChunkGivenPositionAfterRenderElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><render:SomePartial </div>", position: 13);

            Assert.That(nodeType, Is.EqualTo(typeof(RenderSectionChunk)));
        }

        //TODO: Rob, I need to figure out why "section" cannot be used like this according to the Spark compiler
        //[Test] 
        //public void ShouldReturnSectionChunkGivenPositionAfterSectionElementColon()
        //{
        //    Type nodeType = SparkSyntax.ParseContext("<section:", position: 9);

        //    Assert.That(nodeType, Is.EqualTo(typeof(MacroChunk)));
        //}

        [Test]
        public void ShouldReturnUseChunkGivenPositionAfterUseElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><use:header /></div>", position: 10);

            Assert.That(nodeType, Is.EqualTo(typeof(UseContentChunk)));
        }
    }
}
