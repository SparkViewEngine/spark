using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Spark.Compiler;

namespace Spark.Tests.Compiler
{
    [TestFixture]
    public class SourceWriterTester
    {		
        [Test]
        public void IndentationShouldAddLeadingSpace()
        {
            var writer = new StringWriter();
            var source = new SourceWriter(writer);

            source
                .WriteLine()
                .WriteLine("one")
                .AddIndent()
                .WriteLine("two")
                .RemoveIndent()
                .WriteLine("three");
			
			var expected = new StringBuilder()
				.AppendLine()
				.AppendLine("one")
				.AppendLine("    two")
				.AppendLine("three");
									
			Assert.That(source.ToString(), Is.EqualTo(expected.ToString()));
        }		
		
        [Test]
        public void EscrowLineWritesFirstAtIndentationWhenItWasAdded()
        {
            var writer = new StringWriter();
            var source = new SourceWriter(writer);
            source.WriteLine().AddIndent();

            source
                .WriteLine("one")
                .AddIndent()
                .WriteLine("two")
                .EscrowLine("two-b")
                .RemoveIndent()
                .WriteLine("three")
                .RemoveIndent();
			
			var expected = new StringBuilder()
				.AppendLine()
				.AppendLine("    one")
				.AppendLine("        two")
				.AppendLine("        two-b")
				.AppendLine("    three");
						
            Assert.That(source.ToString(), Is.EqualTo(expected.ToString()));
        }		
		
		[Test]
        public void CancelEscrowPreventsOutputOfPendingLine()
        {
            var writer = new StringWriter();
            var source = new SourceWriter(writer);

            source
                .WriteLine()
                .WriteLine("begin")
                .AddIndent()
                .WriteLine("if")
                .AddIndent()
                .WriteLine("do this")
                .RemoveIndent()
                .EscrowLine("endif")
                .ClearEscrowLine()
                .WriteLine("else")
                .AddIndent()
                .WriteLine("do that")
                .RemoveIndent()
                .EscrowLine("endif")
                .RemoveIndent()
                .WriteLine("done");
			
			var expected = new StringBuilder()
				.AppendLine()
				.AppendLine("begin")
				.AppendLine("    if")
				.AppendLine("        do this")
				.AppendLine("    else")
				.AppendLine("        do that")
				.AppendLine("    endif")
				.AppendLine("done");
			
            Assert.That(source.ToString(), Is.EqualTo(expected.ToString()));
        }				
    }
}