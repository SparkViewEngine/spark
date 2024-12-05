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
using System.IO;
using NUnit.Framework;
using Spark.Compiler;
using Spark.Parser;
using Spark.Ruby.Compiler;
using Spark.Tests.Models;
using Spark.Tests.Stubs;

namespace Spark.Ruby.Tests
{
    [TestFixture]
    public class RubyViewCompilerTests
    {
        private ISparkSettings _settings;
        private RubyViewCompiler _compiler;
        private RubyLanguageFactory _languageFactory;

        [SetUp]
        public void Init()
        {
            _settings = new SparkSettings<StubSparkView>
            {
                Debug = true
            };
            _compiler = new RubyViewCompiler(this._settings);
            _languageFactory = new RubyLanguageFactory(new RoslynBatchCompiler(this._settings), this._settings);

            //load assemblies
            global::IronRuby.Ruby.CreateEngine();
        }

        static IList<IList<Chunk>> Chunks(params Chunk[] chunks)
        {
            return new[] { (IList<Chunk>)chunks };
        }

        private string ExecuteView()
        {
            return ExecuteView(new StubViewData());
        }

        private string ExecuteView(StubViewData viewData)
        {
            var view = FastActivator<StubSparkView>.New(_compiler.CompiledType);
            _languageFactory.InstanceCreated(_compiler, view);
            view.ViewData = viewData;
            var contents = new StringWriter();
            view.RenderView(contents);
            _languageFactory.InstanceReleased(_compiler, view);

            return contents.ToString();
        }


        [Test]
        public void CreatedViewHasScriptProperty()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello" });
            _compiler.CompileView(chunks, chunks);
            var view = FastActivator<IScriptingSparkView>.New(_compiler.CompiledType);
            var source = _compiler.SourceCode;
            var script = view.ScriptSource;

            Assert.That(source, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(source, Is.Not.Empty);
                Assert.That(script, Is.Not.Null);
            });
            Assert.That(script, Is.Not.Empty);
        }

        [Test]
        public void CodeInheritsBaseClass()
        {
            var chunks = Chunks();

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain(": Spark.Tests.Stubs.StubSparkView"));
        }

        [Test]
        public void CodeInheritsBaseClassWithTModel()
        {
            var chunks = Chunks(new ViewDataModelChunk { TModel = "ThisIsTheModelClass" });

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain(": Spark.Tests.Stubs.StubSparkView<ThisIsTheModelClass>"));
        }

        [Test]
        public void LayeredTemplates()
        {
            var chunks0 = Chunks(new SendLiteralChunk { Text = "2" });
            var chunks1 = Chunks(
                new SendLiteralChunk { Text = "4" },
                new UseContentChunk { Name = "view" },
                new SendLiteralChunk { Text = "0" });
            var chunks = new[] { chunks0[0], chunks1[0] };
            _compiler.CompileView(chunks, chunks);
            var content = ExecuteView();

            Assert.That(content, Is.EqualTo("420"));
        }

        [/*Test,*/ Ignore("Not really sure if namespaces play a role in a dlr based spark view")]
        private void UsingNamespaces()
        {
            var chunks = Chunks(new UseNamespaceChunk { Namespace = "AnotherNamespace.ToBe.Used" });

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("Imports AnotherNamespace.ToBe.Used"));
        }

        [Test]
        public void TargetNamespace()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "blah" });

            _compiler.Descriptor = new SparkViewDescriptor { TargetNamespace = "TargetNamespace.ForThe.GeneratedView" };
            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("namespace TargetNamespace.ForThe.GeneratedView"));
        }

        [/*Test,*/ Ignore("Not really sure if namespaces play a role in a dlr based spark view")]
        private void TargetNamespaceWithUsingNamespaces()
        {
            var chunks = Chunks(new UseNamespaceChunk { Namespace = "AnotherNamespace.ToBe.Used" });

            _compiler.Descriptor = new SparkViewDescriptor { TargetNamespace = "TargetNamespace.ForThe.GeneratedView" };
            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("Namespace TargetNamespace.ForThe.GeneratedView"));
            Assert.That(_compiler.SourceCode, Does.Contain("Imports AnotherNamespace.ToBe.Used"));
        }

        [Test]
        public void ViewDescriptorAttribute()
        {
            var chunks = Chunks();

            _compiler.Descriptor = new SparkViewDescriptor { TargetNamespace = "TargetNamespace.ForThe.GeneratedView", Templates = new[] { "one", "two", "three" } };
            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("SparkViewAttribute"));
            Assert.That(_compiler.SourceCode, Does.Contain("one"));
            Assert.That(_compiler.SourceCode, Does.Contain("two"));
            Assert.That(_compiler.SourceCode, Does.Contain("three"));
        }

        [Test]
        public void ContainsGeneratedViewIdProperty()
        {
            var chunks = Chunks();

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("GeneratedViewId"));
            Assert.That(_compiler.SourceCode, Does.Contain("\"" + _compiler.GeneratedViewId + "\""));
        }

        [Test]
        public void SendingLiteralOutput()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("Hello World"));
        }

        [Test]
        public void SendingIndentedExpressionOutput()
        {
            var chunks = Chunks(new SendExpressionChunk { Code = "5 + 3", Position = new Position(null, 100, 0, 50, 3, null) });

            _compiler.GenerateSourceCode(chunks, chunks);

            Assert.That(_compiler.SourceCode, Does.Contain("5 + 3"));
        }


        [Test]
        public void RenderingSimpleView()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });
            _compiler.CompileView(chunks, chunks);
            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("Hello World"));
        }


        [Test]
        public void CompilingSimpleView()
        {
            var chunks = Chunks(new SendLiteralChunk { Text = "Hello World" });
            _compiler.CompileView(chunks, chunks);

            Assert.That(typeof(StubSparkView).IsAssignableFrom(_compiler.CompiledType));
        }


        [Test]
        public void SettingLocalVariable()
        {
            var chunks = Chunks(
                new LocalVariableChunk { Name = "x", Value = "4" },
                new SendExpressionChunk { Code = "x" },
                new AssignVariableChunk { Name = "x", Value = "2" },
                new SendExpressionChunk { Code = "x" });
            _compiler.CompileView(chunks, chunks);
            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("42"));
        }


        [Test]
        public void SettingGlobalVariable()
        {
            var chunks = Chunks(
                new GlobalVariableChunk { Type = "int", Name = "x", Value = "4" },
                new SendExpressionChunk { Code = "x" },
                new AssignVariableChunk { Name = "x", Value = "2" },
                new SendExpressionChunk { Code = "x" });
            _compiler.CompileView(chunks, chunks);
            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("42"));
        }

        [Test]
        public void UsingViewData()
        {
            var chunks = Chunks(
                new SendExpressionChunk { Code = "@hello" });
            _compiler.CompileView(chunks, chunks);
            var contents = ExecuteView(new StubViewData { { "hello", 42 } });

            Assert.That(contents, Is.EqualTo("42"));
        }

        [/*Test,*/ Ignore("Not really sure if namespaces play a role in a dlr based spark view")]
        private void UsingViewDataDefault()
        {
            var chunks = Chunks(
                new ViewDataChunk { Type = "int", Name = "HelloNumber", Key = "hello", Default = "55" },
                new SendExpressionChunk { Code = "HelloNumber" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView(new StubViewData { { "hello", 42 } });

            Assert.That(contents, Is.EqualTo("42"));

            var contents2 = ExecuteView();

            Assert.That(contents2, Is.EqualTo("55"));
        }

        [Test]
        public void InlineCodeStatements()
        {
            var chunks = Chunks(
                new CodeStatementChunk { Code = "x = 20" },
                new SendExpressionChunk { Code = "x + 22" });
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("42"));
        }

        [Test]
        public void ScopeTest()
        {
            var scope1 = new ScopeChunk();
            scope1.Body.Add(new LocalVariableChunk { Name = "x", Value = "4" });
            scope1.Body.Add(new SendExpressionChunk { Code = "x" });
            var scope2 = new ScopeChunk();
            scope2.Body.Add(new LocalVariableChunk { Name = "x", Value = "2" });
            scope2.Body.Add(new SendExpressionChunk { Code = "x" });

            var chunks = Chunks(scope1, scope2);
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("42"));
        }

        [Test]
        public void ForEachLoopOverArray()
        {
            var loop = new ForEachChunk { Code = "number in @numbers" };
            loop.Body.Add(new SendExpressionChunk { Code = "number" });

            var chunks = Chunks(
                loop);
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView(new StubViewData { { "numbers", new[] { 1, 2, 3, 4, 5 } } });

            Assert.That(contents, Is.EqualTo("12345"));
        }

        [Test]
        public void MacroAddsFunction()
        {
            var macro = new MacroChunk
            {
                Name = "foo",
                Parameters = new[] { new MacroParameter { Name = "x", Type = "string" } }
            };
            macro.Body.Add(new SendExpressionChunk { Code = "x" });
            var chunks = Chunks(
                new SendExpressionChunk { Code = "foo(\"hello\")" },
                macro);
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("hello"));
        }

        [Test]
        public void ConditionalChunkControlsExecution()
        {
            var condition1 = new ConditionalChunk { Condition = "x != 12" };
            condition1.Body.Add(new SendLiteralChunk { Text = "fail" });
            var condition2 = new ConditionalChunk { Condition = "x == 12" };
            condition2.Body.Add(new SendLiteralChunk { Text = "ok1" });
            var chunks = Chunks(
                new LocalVariableChunk { Name = "x", Value = "12" },
                new SendLiteralChunk { Text = "a" },
                condition1,
                condition2,
                new SendLiteralChunk { Text = "b" });
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("aok1b"));
        }

        [Test]
        public void ElseBlockFollowsIf()
        {
            var condition1 = new ConditionalChunk { Condition = "x != 12" };
            condition1.Body.Add(new SendLiteralChunk { Text = "fail" });
            var else1 = new ConditionalChunk { Type = ConditionalType.Else };
            else1.Body.Add(new SendLiteralChunk { Text = "ok1" });

            var condition2 = new ConditionalChunk { Condition = "x == 12" };
            condition2.Body.Add(new SendLiteralChunk { Text = "ok2" });
            var else2 = new ConditionalChunk { Type = ConditionalType.Else };
            else2.Body.Add(new SendLiteralChunk { Text = "fail" });

            var chunks = Chunks(
                new LocalVariableChunk { Name = "x", Value = "12" },
                new SendLiteralChunk { Text = "a" },
                condition1,
                else1,
                condition2,
                else2,
                new SendLiteralChunk { Text = "b" });
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("aok1ok2b"));
        }

        [Test]
        public void ConditionalChunkUnlessNegates()
        {
            var condition1 = new ConditionalChunk { Condition = "x != 12", Type = ConditionalType.Unless };
            condition1.Body.Add(new SendLiteralChunk { Text = "ok1" });
            var condition2 = new ConditionalChunk { Condition = "x == 12", Type = ConditionalType.Unless };
            condition2.Body.Add(new SendLiteralChunk { Text = "fail" });
            var chunks = Chunks(
                new LocalVariableChunk { Name = "x", Value = "12" },
                new SendLiteralChunk { Text = "a" },
                condition1,
                condition2,
                new SendLiteralChunk { Text = "b" });
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("aok1b"));
        }

        [Test]
        public void ChainingElseIfNestsProperly()
        {
            var condition1 = new ConditionalChunk { Type = ConditionalType.If, Condition = "x == 1" };
            condition1.Body.Add(new SendLiteralChunk { Text = "a" });
            var condition2 = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = "x == 2" };
            condition2.Body.Add(new SendLiteralChunk { Text = "b" });
            var condition3 = new ConditionalChunk { Type = ConditionalType.ElseIf, Condition = "x == 3" };
            condition3.Body.Add(new SendLiteralChunk { Text = "c" });
            var condition4 = new ConditionalChunk { Type = ConditionalType.Else };
            condition4.Body.Add(new SendLiteralChunk { Text = "d" });

            var loop = new ForEachChunk { Code = "x in @numbers" };
            loop.Body.Add(condition1);
            loop.Body.Add(condition2);
            loop.Body.Add(condition3);
            loop.Body.Add(condition4);

            var chunks = Chunks(
                loop);
            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView(new StubViewData { { "numbers", new[] { 0, 1, 2, 3, 4, 7, 2, -4 } } });

            Assert.That(contents, Is.EqualTo("dabcddbd"));
        }

        [Test]
        public void RenderPartial()
        {
            var partial = Chunks(
                new SendLiteralChunk { Text = "Hello world" });

            var chunks = Chunks(
                new SendLiteralChunk { Text = "(" },
                new RenderPartialChunk { FileContext = new FileContext { Contents = partial[0] } },
                new SendLiteralChunk { Text = ")" });

            _compiler.CompileView(chunks, new[] { chunks[0], partial[0] });

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("(Hello world)"));
        }

        [Test]
        public void RenderPartialWithContainedContent()
        {
            var partial = Chunks(
                new SendLiteralChunk { Text = "[" },
                new RenderSectionChunk(),
                new SendLiteralChunk { Text = "]" });


            var renderPartial = new RenderPartialChunk { FileContext = new FileContext { Contents = partial[0] } };
            var chunks = Chunks(
                new SendLiteralChunk { Text = "(" },
                renderPartial,
                new SendLiteralChunk { Text = ")" });

            renderPartial.Body.Add(new SendLiteralChunk { Text = "From inside caller" });

            _compiler.CompileView(chunks, new[] { chunks[0], partial[0] });

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("([From inside caller])"));
        }

        [Test]
        public void RenderPartialWithSectionContent()
        {
            var partial = Chunks(
                new SendLiteralChunk { Text = "[" },
                new RenderSectionChunk { Name = "foo" },
                new SendLiteralChunk { Text = "]" });

            var renderPartial = new RenderPartialChunk { FileContext = new FileContext { Contents = partial[0] } };
            renderPartial.Sections.Add("foo",
                                       new[]
                                       {
                                           (Chunk) new SendLiteralChunk {Text = "From inside caller"}
                                       });

            var chunks = Chunks(
                new SendLiteralChunk { Text = "(" },
                renderPartial,
                new SendLiteralChunk { Text = ")" });

            _compiler.CompileView(chunks, new[] { chunks[0], partial[0] });

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("([From inside caller])"));
        }

        [Test]
        public void CaptureContentToVariable()
        {
            var text1 = (Chunk)new SendLiteralChunk { Text = "a" };
            var text2 = (Chunk)new SendLiteralChunk { Text = "b" };
            var text3 = (Chunk)new SendLiteralChunk { Text = "c" };
            var chunks = Chunks(
                new LocalVariableChunk { Type = "string", Name = "foo" },
                new ContentSetChunk { AddType = ContentAddType.Replace, Variable = "foo", Body = new[] { text1 } },
                new ContentSetChunk { AddType = ContentAddType.InsertBefore, Variable = "foo", Body = new[] { text2 } },
                new ContentSetChunk { AddType = ContentAddType.AppendAfter, Variable = "foo", Body = new[] { text3 } },
                new SendExpressionChunk { Code = "foo" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("bac"));
        }


        [Test]
        public void CaptureContentToNamedSpool()
        {
            var chunks = Chunks(
                new ContentChunk { Name = "foo", Body = new[] { (Chunk)new SendLiteralChunk { Text = "b" } } },
                new SendLiteralChunk { Text = "a" },
                new UseContentChunk { Name = "foo" },
                new SendLiteralChunk { Text = "c" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("abc"));
        }

        [Test]
        public void DefaultVariablesWorking()
        {
            var chunks = Chunks(
                new DefaultVariableChunk { Name = "x1", Value = "\"a\"" },
                new LocalVariableChunk { Name = "x2", Value = "\"b\"" },
                new DefaultVariableChunk { Name = "x1", Value = "\"c\"" },
                new DefaultVariableChunk { Name = "x2", Value = "\"d\"" },
                new DefaultVariableChunk { Name = "x3", Value = "\"e\"" },
                new SendExpressionChunk { Code = "x1" },
                new SendExpressionChunk { Code = "x2" },
                new SendExpressionChunk { Code = "x3" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("abe"));
        }

        [Test]
        public void OnceTestSendsThingsOnce()
        {
            var chunks = Chunks(
                new ConditionalChunk
                {
                    Type = ConditionalType.Once,
                    Condition = "\"foo\"",
                    Body = new[] { (Chunk)new SendLiteralChunk { Text = "4" } }
                },
                new ConditionalChunk
                {
                    Type = ConditionalType.Once,
                    Condition = "\"foo\"",
                    Body = new[] { (Chunk)new SendLiteralChunk { Text = "3" } }
                },
                new ConditionalChunk
                {
                    Type = ConditionalType.Once,
                    Condition = "\"bar\"",
                    Body = new[] { (Chunk)new SendLiteralChunk { Text = "2" } }
                });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("42"));
        }

        [Test]
        public void HandleNullReferences()
        {
            var chunks = Chunks(
                new LocalVariableChunk { Name = "user", Type = typeof(UserInfo).FullName },
                new SendLiteralChunk { Text = "1" },
                new SendExpressionChunk { Code = "user.Name", SilentNulls = false },
                new SendLiteralChunk { Text = "2" },
                new SendExpressionChunk { Code = "user.Name", SilentNulls = true },
                new SendLiteralChunk { Text = "3" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("1${user.Name => undefined method `Name' for nil:NilClass}23"));
        }

        [Test]
        public void ForLoopAutovariables()
        {
            var loop = Chunks(
                new SendExpressionChunk { Code = "fooIndex" },
                new SendExpressionChunk { Code = "fooCount" },
                new SendExpressionChunk { Code = "fooIsFirst" },
                new SendExpressionChunk { Code = "fooIsLast" });

            var chunks = Chunks(
                new ForEachChunk { Code = "foo in @stuff", Body = loop[0] });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView(new StubViewData
                                       {
                                           {"stuff", new[] {6, 2, 7, 4}}
                                       });

            Assert.That(contents, Is.EqualTo("04TrueFalse14FalseFalse24FalseFalse34FalseTrue"));
        }

        [Test]
        public void CallingMacro()
        {
            var macroBody = Chunks(
                new SendLiteralChunk { Text = "1" });

            var chunks = Chunks(
                new MacroChunk { Name = "Foo", Body = macroBody[0] },
                new LocalVariableChunk { Name = "x", Value = "Foo()" },
                new SendLiteralChunk { Text = "2" },
                new SendExpressionChunk { Code = "x" },
                new SendLiteralChunk { Text = "3" });

            _compiler.CompileView(chunks, chunks);

            var contents = ExecuteView();

            Assert.That(contents, Is.EqualTo("213"));
        }
    }
}