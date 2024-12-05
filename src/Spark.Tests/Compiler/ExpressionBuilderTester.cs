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
