using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Spark.Compiler;

namespace Spark.Tests.Compiler
{
    [TestFixture]
    public class ExpressionBuilderTester
    {
        [Test]
        public void AddSeveralLiterals()
        {
            var builder = new ExpressionBuilder();
            builder.AppendLiteral("hello");
            builder.AppendLiteral("world");
            Assert.AreEqual("\"helloworld\"", builder.ToCode());
        }

        [Test]
        public void TextIsEscaped()
        {
            var builder = new ExpressionBuilder();
            builder.AppendLiteral("\"\\\t");            
            Assert.AreEqual("\"\\\"\\\\\\t\"", builder.ToCode());
        }

        [Test]
        public void CodeEndsUpConcatinated()
        {
            var builder = new ExpressionBuilder();
            builder.AppendExpression("x");
            builder.AppendExpression("y");
            builder.AppendExpression("z");
            Assert.AreEqual("string.Concat(x,y,z)", builder.ToCode());
        }
    }
}
