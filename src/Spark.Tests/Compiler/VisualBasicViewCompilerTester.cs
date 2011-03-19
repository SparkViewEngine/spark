// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using NUnit.Framework.SyntaxHelpers;
using Spark.Compiler;
using NUnit.Framework;
using Spark.Compiler.VisualBasic;
using Spark.Tests.Models;
using Spark.Tests.Stubs;

namespace Spark.Tests.Compiler
{
    [TestFixture]
    public class VisualBasicViewCompilerTester
    {

        [SetUp]
        public void Init()
        {
        }

        private static void DoCompileView(ViewCompiler compiler, IList<Chunk> chunks)
        {
            compiler.CompileView(new[] { chunks }, new[] { chunks });
        }

        [Test]
        public void MakeAndCompile()
        {
            var compiler = CreateCompiler();

            DoCompileView(compiler, new[] { new SendLiteralChunk { Text = "hello world" } });

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents.Contains("hello world"));
        }

        [Test]
        public void StronglyTypedBase()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.Tests.Stubs.StubSparkView" };

            DoCompileView(compiler, new Chunk[]
            {
                new SendLiteralChunk { Text = "hello world" }, 
                new ViewDataModelChunk { TModel="Global.System.String"}
            });

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents.Contains("hello world"));
        }

        [Test]
        public void UnsafeLiteralCharacters()
        {
            var text = "hello\t\r\n\"world";
            var compiler = CreateCompiler();
            DoCompileView(compiler, new[] { new SendLiteralChunk { Text = text } });

            Assert.That(compiler.SourceCode.Contains("Write(\"hello"));

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo(text));
        }

        private static VisualBasicViewCompiler CreateCompiler()
        {
            return new VisualBasicViewCompiler
            {
                BaseClass = "Spark.AbstractSparkView",
                UseAssemblies = new[] { "Microsoft.VisualBasic, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" },
                UseNamespaces = new[] { "Microsoft.VisualBasic" }
            };
        }

        [Test]
        public void SimpleOutput()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
            DoCompileView(compiler, new[] { new SendExpressionChunk { Code = "3 + 4" } });
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();
            Assert.AreEqual("7", contents);
        }

        [Test]
        public void LenientNullBehavior()
        {
            var compiler = CreateCompiler();

            DoCompileView(compiler, new[] { new SendExpressionChunk { Code = "CType(Nothing, String).Length" } });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.That(contents, Is.EqualTo("${CType(Nothing, String).Length}"));
        }

        [Test]
        public void SilentNullBehavior()
        {
            var compiler = CreateCompiler();

            DoCompileView(compiler, new[] { new SendExpressionChunk { Code = "CType(Nothing, String).Length", SilentNulls = true } });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.That(contents, Is.EqualTo(""));
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void RethrowNullBehavior()
        {
            var compiler = CreateCompiler();
            compiler.NullBehaviour = NullBehaviour.Strict;

            DoCompileView(compiler, new[] { new SendExpressionChunk { Code = "CType(Nothing, String).Length" } });
            var instance = compiler.CreateInstance();
            instance.RenderView();
        }

        [Test]
        public void LocalVariableDecl()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new LocalVariableChunk { Name = "i", Value = "5" }, 
                                        new SendExpressionChunk { Code = "i" }
                                    });
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.AreEqual("5", contents);
        }

        [Test]
        public void ForEachLoop()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
			DoCompileView(compiler, new Chunk[]
                                    {
                                        new LocalVariableChunk {Name = "data", Value = "new Integer(){3,4,5}"},
                                        new SendLiteralChunk {Text = "<ul>"},
                                        new ForEachChunk
                                        {
                                            Code = "item As Integer in data",
                                            Body = new Chunk[]
                                                   { 
                                                       new SendLiteralChunk {Text = "<li>"},
                                                       new SendExpressionChunk {Code = "item"},
                                                       new SendLiteralChunk {Text = "</li>"}
                                                   }
                                        },
                                        new SendLiteralChunk {Text = "</ul>"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<ul><li>3</li><li>4</li><li>5</li></ul>", contents);
        }

        [Test]
        public void ForEachAutoVariables()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new LocalVariableChunk {Name = "data", Value = "new Integer(){3,4,5}"},
                                        new SendLiteralChunk {Text = "<ul>"},
                                        new ForEachChunk
                                        {
                                            Code = "item As Integer in data",
                                            Body = new Chunk[]
                                                   { 
                                                       new SendLiteralChunk {Text = "<li>"},
                                                       new SendExpressionChunk {Code = "item"},
                                                       new SendExpressionChunk {Code = "itemIsFirst"},
                                                       new SendExpressionChunk {Code = "itemIsLast"},
                                                       new SendExpressionChunk {Code = "itemIndex"},
                                                       new SendExpressionChunk {Code = "itemCount"},
                                                       new SendLiteralChunk {Text = "</li>"}
                                                   }
                                        },
                                        new SendLiteralChunk {Text = "</ul>"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<ul><li>3TrueFalse03</li><li>4FalseFalse13</li><li>5FalseTrue23</li></ul>", contents);
        }

        [Test]
        public void GlobalVariables()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new SendExpressionChunk{Code="title"},
                                        new AssignVariableChunk{ Name="item", Value="8"},
                                        new SendLiteralChunk{ Text=":"},
                                        new SendExpressionChunk{Code="item"},
                                        new GlobalVariableChunk{ Name="title", Value="\"hello world\""},
                                        new GlobalVariableChunk{ Name="item", Value="3"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("hello world:8", contents);
        }

        [Test]
        public void TargetNamespace()
        {
            // TODO: Fix on Mono/Linux - ahjohannessen
            var platform = Environment.OSVersion.Platform;
            if(platform == PlatformID.Unix || platform == PlatformID.MacOSX)
            {
                Assert.Ignore();
            }

            var compiler = new VisualBasicViewCompiler
            {
                BaseClass = "Spark.AbstractSparkView",
                Descriptor = new SparkViewDescriptor { TargetNamespace = "Testing.Target.Namespace" }
            };
            
            DoCompileView(compiler, new Chunk[] { new SendLiteralChunk { Text = "Hello" } });
            var instance = compiler.CreateInstance();
            Assert.AreEqual("Testing.Target.Namespace", instance.GetType().Namespace);
        }


        [Test, ExpectedException(typeof(BatchCompilerException))]
        public void ProvideFullException()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new SendExpressionChunk {Code = "NoSuchVariable"}
                                    });
        }

        [Test]
        public void IfTrueCondition()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(compiler, new Chunk[]
                                    {
                                        new SendLiteralChunk {Text = "<p>"},
                                        new LocalVariableChunk{Name="arg", Value="5"},
                                        new ConditionalChunk{Type=ConditionalType.If, Condition="arg=5", Body=trueChunks},
                                        new SendLiteralChunk {Text = "</p>"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p>wastrue</p>", contents);
        }

        [Test]
        public void IfFalseCondition()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(compiler, new Chunk[]
                                    {
                                        new SendLiteralChunk {Text = "<p>"},
                                        new LocalVariableChunk{Name="arg", Value="5"},
                                        new ConditionalChunk{Type=ConditionalType.If, Condition="arg=6", Body=trueChunks},
                                        new SendLiteralChunk {Text = "</p>"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p></p>", contents);
        }

        [Test]
        public void IfElseFalseCondition()
        {
            var compiler = new VisualBasicViewCompiler { BaseClass = "Spark.AbstractSparkView" };

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };
            var falseChunks = new Chunk[] { new SendLiteralChunk { Text = "wasfalse" } };

            DoCompileView(compiler, new Chunk[]
                                    {
                                        new SendLiteralChunk {Text = "<p>"},
                                        new LocalVariableChunk{Name="arg", Value="5"},
                                        new ConditionalChunk{Type=ConditionalType.If, Condition="arg=6", Body=trueChunks},
                                        new ConditionalChunk{Type=ConditionalType.Else, Body=falseChunks},
                                        new SendLiteralChunk {Text = "</p>"}
                                    });
            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();
            Assert.AreEqual("<p>wasfalse</p>", contents);
        }

        [Test]
        public void StrictNullUsesException()
        {
            var compiler = new VisualBasicViewCompiler()
                           {
                               BaseClass = "Spark.Tests.Stubs.StubSparkView",
                               NullBehaviour = NullBehaviour.Strict
                           };
            var chunks = new Chunk[]
                         {
                             new ViewDataChunk { Name="comment", Type="Spark.Tests.Models.Comment"},
                             new SendExpressionChunk {Code = "comment.Text", SilentNulls = false}
                         };
            compiler.CompileView(new[] { chunks }, new[] { chunks });
            Assert.That(compiler.SourceCode.Contains("Catch ex As Global.System.NullReferenceException"));
            Assert.That(compiler.SourceCode.Contains("ArgumentNullException("));
            Assert.That(compiler.SourceCode.Contains(", ex)"));
        }

        [Test]
        public void PageBaseTypeOverridesBaseClass()
        {
            var compiler = new VisualBasicViewCompiler()
                           {
                               BaseClass = "Spark.Tests.Stubs.StubSparkView",
                               NullBehaviour = NullBehaviour.Strict
                           };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new PageBaseTypeChunk {  BaseClass="Spark.Tests.Stubs.StubSparkView2"},
                                        new SendLiteralChunk{ Text = "Hello world"}
                                    });
            var instance = compiler.CreateInstance();
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView2)));
        }


        [Test]
        public void PageBaseTypeWorksWithOptionalModel()
        {
            var compiler = new VisualBasicViewCompiler()
                           {
                               BaseClass = "Spark.Tests.Stubs.StubSparkView",
                               NullBehaviour = NullBehaviour.Strict
                           };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new PageBaseTypeChunk {BaseClass = "Spark.Tests.Stubs.StubSparkView2"},
                                        new ViewDataModelChunk {TModel = "Spark.Tests.Models.Comment"},
                                        new SendLiteralChunk {Text = "Hello world"}
                                    });
            var instance = compiler.CreateInstance();
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView2)));
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView2<Comment>)));
        }

        [Test]
        public void PageBaseTypeWorksWithGenericParametersIncluded()
        {
            var compiler = new VisualBasicViewCompiler()
                           {
                               BaseClass = "Spark.Tests.Stubs.StubSparkView",
                               NullBehaviour = NullBehaviour.Strict
                           };
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new PageBaseTypeChunk {BaseClass = "Spark.Tests.Stubs.StubSparkView3(Of Spark.Tests.Models.Comment, string)"},
                                        new SendLiteralChunk {Text = "Hello world"}
                                    });
            var instance = compiler.CreateInstance();
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView2)));
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView2<Comment>)));
            Assert.That(instance, Is.InstanceOfType(typeof(StubSparkView3<Comment, string>)));
        }
    }
}