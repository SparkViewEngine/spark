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

using System.Collections.Generic;
using NUnit.Framework;
using Spark.Compiler.CSharp;
using Spark.Tests;
using Spark.Tests.Models;
using Spark.Tests.Stubs;
using Spark.Web.Mvc;

namespace Spark.Compiler
{
    [TestFixture]
    public class CSharpViewCompilerTester
    {
        private IBatchCompiler batchCompiler;

        [SetUp]
        public void Init()
        {
            this.batchCompiler = new RoslynBatchCompiler(new SparkSettings());
        }

        private static void DoCompileView(ViewCompiler compiler, IList<Chunk> chunks)
        {
            compiler.CompileView(new[] { chunks }, new[] { chunks });
        }

        [Test]
        public void MakeAndCompile()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            DoCompileView(compiler, new[] { new SendLiteralChunk { Text = "hello world" } });

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents, Does.Contain("hello world"));
        }


        [Test]
        public void UnsafeLiteralCharacters()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var text = "hello\t\r\n\"world";
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
            DoCompileView(compiler, new[] { new SendLiteralChunk { Text = text } });

            Assert.That(compiler.SourceCode, Does.Contain("Write(\"hello\\t\\r\\n\\\"world\")"));

            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo(text));
        }

        [Test]
        public void SimpleOutput()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
            DoCompileView(compiler, new[] { new SendExpressionChunk { Code = "3 + 4" } });
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("7"));
        }

        [Test]
        public void LocalVariableDecl()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new LocalVariableChunk { Name = "i", Value = "5" },
                                        new SendExpressionChunk { Code = "i" }
                                    });
            var instance = compiler.CreateInstance();
            string contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("5"));
        }

        [Test]
        public void ForEachLoop()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
            DoCompileView(compiler, new Chunk[]
                                    {
                                        new LocalVariableChunk {Name = "data", Value = "new[]{3,4,5}"},
                                        new SendLiteralChunk {Text = "<ul>"},
                                        new ForEachChunk
                                        {
                                            Code = "var item in data",
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

            Assert.That(contents, Is.EqualTo("<ul><li>3</li><li>4</li><li>5</li></ul>"));
        }

        [Test]
        public void GlobalVariables()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
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

            Assert.That(contents, Is.EqualTo("hello world:8"));
        }

        [Test]
        public void TargetNamespace()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings)
            {
                Descriptor = new SparkViewDescriptor { TargetNamespace = "Testing.Target.Namespace" }
            };

            DoCompileView(compiler, new Chunk[] { new SendLiteralChunk { Text = "Hello" } });
            var instance = compiler.CreateInstance();

            Assert.That(instance.GetType().Namespace, Is.EqualTo("Testing.Target.Namespace"));
        }


        [Test]
        public void ProvideFullException()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            Assert.That(
                () =>
                    DoCompileView(
                        compiler,
                        new Chunk[]
                        {
                            new SendExpressionChunk { Code = "NoSuchVariable" }
                        }),
                Throws.TypeOf<RoslynCompilerException>());
        }

        [Test]
        public void IfTrueCondition()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new SendLiteralChunk { Text = "<p>" },
                    new LocalVariableChunk { Name = "arg", Value = "5" },
                    new ConditionalChunk { Type = ConditionalType.If, Condition = "arg==5", Body = trueChunks },
                    new SendLiteralChunk { Text = "</p>" }
                });

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("<p>wastrue</p>"));
        }

        [Test]
        public void IfFalseCondition()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new SendLiteralChunk { Text = "<p>" },
                    new LocalVariableChunk { Name = "arg", Value = "5" },
                    new ConditionalChunk { Type = ConditionalType.If, Condition = "arg==6", Body = trueChunks },
                    new SendLiteralChunk { Text = "</p>" }
                });

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("<p></p>"));
        }

        [Test]
        public void IfElseFalseCondition()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };
            var falseChunks = new Chunk[] { new SendLiteralChunk { Text = "wasfalse" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new SendLiteralChunk { Text = "<p>" },
                    new LocalVariableChunk { Name = "arg", Value = "5" },
                    new ConditionalChunk { Type = ConditionalType.If, Condition = "arg==6", Body = trueChunks },
                    new ConditionalChunk { Type = ConditionalType.Else, Body = falseChunks },
                    new SendLiteralChunk { Text = "</p>" }
                });

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("<p>wasfalse</p>"));
        }

        [Test]
        public void UnlessTrueCondition()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new SendLiteralChunk { Text = "<p>" },
                    new LocalVariableChunk { Name = "arg", Value = "5" },
                    new ConditionalChunk { Type = ConditionalType.Unless, Condition = "arg==5", Body = trueChunks },
                    new SendLiteralChunk { Text = "</p>" }
                });

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("<p></p>"));
        }

        [Test]
        public void UnlessFalseCondition()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var trueChunks = new Chunk[] { new SendLiteralChunk { Text = "wastrue" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new SendLiteralChunk { Text = "<p>" },
                    new LocalVariableChunk { Name = "arg", Value = "5" },
                    new ConditionalChunk { Type = ConditionalType.Unless, Condition = "arg==6", Body = trueChunks },
                    new SendLiteralChunk { Text = "</p>" }
                });

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView();

            Assert.That(contents, Is.EqualTo("<p>wastrue</p>"));
        }

        [Test]
        public void LenientSilentNullDoesNotCauseWarningCS0168()
        {
            var settings = new SparkSettings<SparkView>
            {
                NullBehaviour = NullBehaviour.Lenient
            }.AddAssembly(typeof(Spark.Tests.Models.Comment).Assembly);

            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var chunks = new Chunk[]
            {
                new ViewDataChunk { Name = "comment", Type = "Spark.Tests.Models.Comment" },
                new SendExpressionChunk { Code = "comment.Text", SilentNulls = true }
            };

            compiler.CompileView(new[] { chunks }, new[] { chunks });

            Assert.That(compiler.SourceCode, Does.Contain("catch(System.NullReferenceException)"));
        }

        [Test]
        public void LenientOutputNullDoesNotCauseWarningCS0168()
        {
            var settings = new SparkSettings<SparkView>
            {
                NullBehaviour = NullBehaviour.Lenient
            }.AddAssembly(typeof(Spark.Tests.Models.Comment).Assembly);

            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);
            var chunks = new Chunk[]
            {
                new ViewDataChunk { Name = "comment", Type = "Spark.Tests.Models.Comment" },
                new SendExpressionChunk { Code = "comment.Text", SilentNulls = false }
            };

            compiler.CompileView(new[] { chunks }, new[] { chunks });

            Assert.That(compiler.SourceCode, Does.Contain("catch(System.NullReferenceException)"));
        }

        [Test]
        public void StrictNullUsesException()
        {
            var settings = new SparkSettings<SparkView>
            {
                NullBehaviour = NullBehaviour.Strict
            };
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var chunks = new Chunk[]
            {
                new ViewDataChunk { Name = "comment", Type = "Spark.Tests.Models.Comment" },
                new SendExpressionChunk { Code = "comment.Text", SilentNulls = false }
            };

            compiler.CompileView(new[] { chunks }, new[] { chunks });

            Assert.Multiple(() =>
            {
                Assert.That(compiler.SourceCode, Does.Contain("catch(System.NullReferenceException ex)"));
                Assert.That(compiler.SourceCode, Does.Contain("ArgumentNullException("));
                Assert.That(compiler.SourceCode, Does.Contain(", ex);"));
            });
        }

        [Test]
        public void PageBaseTypeOverridesBaseClass()
        {
            var settings = new SparkSettings<StubSparkView>
            {
                NullBehaviour = NullBehaviour.Strict
            };
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            DoCompileView(compiler, new Chunk[]
                                    {
                                        new PageBaseTypeChunk {  BaseClass="Spark.Tests.Stubs.StubSparkView2"},
                                        new SendLiteralChunk{ Text = "Hello world"}
                                    });

            var instance = compiler.CreateInstance();

            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView2)));
        }


        [Test]
        public void PageBaseTypeWorksWithOptionalModel()
        {
            var settings = new SparkSettings<StubSparkView>
            {
                NullBehaviour = NullBehaviour.Strict
            };
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new PageBaseTypeChunk { BaseClass = "Spark.Tests.Stubs.StubSparkView2" },
                    new ViewDataModelChunk { TModel = "Spark.Tests.Models.Comment" },
                    new SendLiteralChunk { Text = "Hello world" }
                });

            var instance = compiler.CreateInstance();

            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView2)));
            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView2<Comment>)));
        }

        [Test]
        public void PageBaseTypeWorksWithGenericParametersIncluded()
        {
            var settings = new SparkSettings<StubSparkView>
            {
                NullBehaviour = NullBehaviour.Strict
            };
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            DoCompileView(compiler, new Chunk[]
                                    {
                                        new PageBaseTypeChunk {BaseClass = "Spark.Tests.Stubs.StubSparkView3<Spark.Tests.Models.Comment, string>"},
                                        new SendLiteralChunk {Text = "Hello world"}
                                    });

            var instance = compiler.CreateInstance();

            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView2)));
            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView2<Comment>)));
            Assert.That(instance, Is.InstanceOf(typeof(StubSparkView3<Comment, string>)));
        }

        [Test]
        public void Markdown()
        {
            var settings = new SparkSettings().SetBaseClassTypeName(typeof(SparkView).FullName);
            var compiler = new CSharpViewCompiler(this.batchCompiler, settings);

            var innerChunks = new Chunk[] { new SendLiteralChunk { Text = "*test*" } };

            DoCompileView(
                compiler,
                new Chunk[]
                {
                    new MarkdownChunk { Body = innerChunks }
                });

            Assert.That(compiler.SourceCode, Does.Contain("using(MarkdownOutputScope())"));
            Assert.That(compiler.SourceCode, Does.Contain("Output.Write(\"*test*\");"));

            var instance = compiler.CreateInstance();
            var contents = instance.RenderView().Trim();

            Assert.That(contents, Is.EqualTo("<p><em>test</em></p>"));
        }
    }
}