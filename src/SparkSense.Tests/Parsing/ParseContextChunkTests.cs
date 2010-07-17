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
        public void ShouldReturnContentChunkGivenPositionInsideDoubleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><content name=\"\" </div>", position: 20);

            Assert.That(nodeType, Is.EqualTo(typeof(ContentChunk)));
        }

        [Test]
        public void ShouldReturnContentChunkGivenPositionInsideSingleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><content name='' </div>", position: 20);

            Assert.That(nodeType, Is.EqualTo(typeof(ContentChunk)));
        }

        [Test]
        public void ShouldReturnMacroChunkGivenPositionAfterMacroElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><macro:SomeMacro </div>", position: 12);

            Assert.That(nodeType, Is.EqualTo(typeof(MacroChunk)));
        }

        [Test]
        public void ShouldReturnMacroChunkGivenPositionInsideDoubleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><macro name=\"\" </div>", position: 18);

            Assert.That(nodeType, Is.EqualTo(typeof(MacroChunk)));
        }

        [Test]
        public void ShouldReturnMacroChunkGivenPositionInsideSingleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><macro name='' </div>", position: 18);

            Assert.That(nodeType, Is.EqualTo(typeof(MacroChunk)));
        }

        [Test]
        public void ShouldReturnRenderChunkGivenPositionAfterRenderElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><render: </div>", position: 13);

            Assert.That(nodeType, Is.EqualTo(typeof(RenderSectionChunk)));
        }

        [Test]
        public void ShouldReturnRenderChunkGivenPositionInsideDoubleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><render partial=\"\" </div>", position: 22);

            Assert.That(nodeType, Is.EqualTo(typeof(RenderPartialChunk)));
        }

        [Test]
        public void ShouldReturnRenderChunkGivenPositionInsideSingleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><render partial='' </div>", position: 22);

            Assert.That(nodeType, Is.EqualTo(typeof(RenderPartialChunk)));
        }

        [Test]
        public void ShouldReturnUseChunkGivenPositionAfterUseElementColon()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><use:header /></div>", position: 10);

            Assert.That(nodeType, Is.EqualTo(typeof(UseContentChunk)));
        }

        [Test]
        public void ShouldReturnUseChunkGivenPositionInsideDoubleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><use content=\"\" </div>", position: 19);

            Assert.That(nodeType, Is.EqualTo(typeof(UseContentChunk)));
        }

        [Test]
        public void ShouldReturnUseChunkGivenPositionInsideSingleQuotes()
        {
            Type nodeType = SparkSyntax.ParseContextChunk("<div><use content='' </div>", position: 19);

            Assert.That(nodeType, Is.EqualTo(typeof(UseContentChunk)));
        }
    }
}
