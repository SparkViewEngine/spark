using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Spark.Compiler.CodeDom.ChunkVisitors;

namespace Spark.Compiler.CodeDom
{
    public class CodeDomViewCompiler : ViewCompiler
    {
        private string _language;

        public CodeDomViewCompiler(string language)
        {
            _language = language;
        }

        public override void CompileView(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            GenerateSourceCode(viewTemplates, allResources);

            var batchCompiler = new BatchCompiler();
            var assembly = batchCompiler.Compile(GetCodeDomProvider(), Debug, SourceCode);
            CompiledType = assembly.GetType(ViewClassFullName);
        }

        public override void GenerateSourceCode(IEnumerable<IList<Chunk>> viewTemplates, IEnumerable<IList<Chunk>> allResources)
        {
            var compileUnit = new CodeCompileUnit();

            var viewNamespace = new CodeNamespace(TargetNamespace);
            compileUnit.Namespaces.Add(viewNamespace);


            AddImports(compileUnit, viewNamespace, allResources);

            var viewClass = BuildViewClass(viewNamespace, allResources);

            AddSparkViewAttribute(viewClass);

            AddGeneratedViewId(viewClass);


            // public void RenderViewLevelx()
            int renderLevel = 0;
            foreach (var viewTemplate in viewTemplates)
            {
                var renderViewLevel = new CodeMemberMethod { Name = "RenderViewLevel" + renderLevel };
                viewClass.Members.Add(renderViewLevel);
                var viewGenerator = new GeneratedCodeVisitor(renderViewLevel);
                viewGenerator.Accept(viewTemplate);
                ++renderLevel;
            }

            // public void RenderView(TextWriter writer)
            var renderView = new CodeMemberMethod { Name = "RenderView", Attributes = MemberAttributes.Public | MemberAttributes.Override };
            viewClass.Members.Add(renderView);
            renderView.Parameters.Add(new CodeParameterDeclarationExpression(typeof(TextWriter), "writer"));

            renderView.Statements.Add(new CodeVariableDeclarationStatement(typeof(IDisposable), "scope"));
            for (int invokeLevel = 0; invokeLevel != renderLevel; ++invokeLevel)
            {
                if (invokeLevel != renderLevel - 1)
                {
                    //using (OutputScope()) {{RenderViewLevel{0}(); Content[\"view\"] = Output;}}

                    var set = new CodeAssignStatement(
                        new CodeVariableReferenceExpression("scope"),
                        new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "OutputScope"));
                    var call = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "RenderViewLevel" + invokeLevel);
                    var store = new CodeAssignStatement(
                        new CodeArrayIndexerExpression(
                            new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Content"),
                            new CodePrimitiveExpression("view")),
                        new CodePropertyReferenceExpression(new CodeThisReferenceExpression(), "Output"));
                    var dispose = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("scope"),
                        "Dispose");

                    renderView.Statements.Add(set);
                    renderView.Statements.Add(new CodeTryCatchFinallyStatement(new CodeStatement[] { new CodeExpressionStatement(call), store },
                                                                               new CodeCatchClause[0],
                                                                               new CodeStatement[] { new CodeExpressionStatement(dispose) }));
                }
                else
                {
                    //using (OutputScope(writer)) {{RenderViewLevel{0}();}}                    

                    var set = new CodeAssignStatement(
                        new CodeVariableReferenceExpression("scope"),
                        new CodeMethodInvokeExpression(
                            new CodeThisReferenceExpression(), "OutputScope", new CodeVariableReferenceExpression("writer")));
                    var call = new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "RenderViewLevel" + invokeLevel);
                    var dispose = new CodeMethodInvokeExpression(
                        new CodeVariableReferenceExpression("scope"),
                        "Dispose");

                    renderView.Statements.Add(set);
                    renderView.Statements.Add(new CodeTryCatchFinallyStatement(new CodeStatement[] { new CodeExpressionStatement(call) },
                                                                               new CodeCatchClause[0],
                                                                               new CodeStatement[] { new CodeExpressionStatement(dispose) }));
                }
            }

            var options = new CodeGeneratorOptions { IndentString = "" };
            var source = new StringBuilder();
            GetCodeDomProvider().GenerateCodeFromCompileUnit(compileUnit, new StringWriter(source), options);
            SourceCode = source.ToString();
        }

        private CodeDomProvider GetCodeDomProvider()
        {
            var language = _language;
            if (CodeDomProvider.IsDefinedExtension(language))
                language = CodeDomProvider.GetLanguageFromExtension(language);

            var info = CodeDomProvider.GetCompilerInfo(language);

            var providerOptions = new Dictionary<string, string> { { "CompilerVersion", "v3.5" } };
            var constructor = info.CodeDomProviderType.GetConstructor(new[] { providerOptions.GetType() });
            if (constructor != null)
            {
                return (CodeDomProvider)constructor.Invoke(new object[] { providerOptions });
            }
            return (CodeDomProvider)Activator.CreateInstance(info.CodeDomProviderType);
        }


        private void AddImports(CodeCompileUnit compileUnit, CodeNamespace viewNamespace, IEnumerable<IList<Chunk>> allResources)
        {
            var usingGenerator = new UsingNamespaceVisitor(compileUnit, viewNamespace);
            foreach (var ns in UseNamespaces ?? new string[0])
                usingGenerator.UsingNamespace(ns);
            foreach (var resource in allResources)
                usingGenerator.Accept(resource);
        }

        private CodeTypeDeclaration BuildViewClass(CodeNamespace viewNamespace, IEnumerable<IList<Chunk>> allResources)
        {
            var viewClassName = "View" + GeneratedViewId.ToString("n");

            var viewClass = new CodeTypeDeclaration(viewClassName);
            viewNamespace.Types.Add(viewClass);

            var baseClassGenerator = new CSharp.ChunkVisitors.BaseClassVisitor { BaseClass = BaseClass };
            foreach (var resource in allResources)
                baseClassGenerator.Accept(resource);

            var viewBaseType = new CodeTypeReference(baseClassGenerator.BaseClassTypeName);
            if (!string.IsNullOrEmpty(baseClassGenerator.TModel))
                viewBaseType.TypeArguments.Add(baseClassGenerator.TModel);

            viewClass.BaseTypes.Add(viewBaseType);

            if (string.IsNullOrEmpty(viewNamespace.Name))
                ViewClassFullName = viewClassName;
            else
                ViewClassFullName = viewNamespace.Name + "." + viewClassName;

            return viewClass;
        }

        private void AddSparkViewAttribute(CodeTypeMember viewClass)
        {
            var viewAttribute = new CodeAttributeDeclaration(new CodeTypeReference("Spark.SparkViewAttribute"));
            viewClass.CustomAttributes.Add(viewAttribute);
            if (TargetNamespace != null)
                viewAttribute.Arguments.Add(new CodeAttributeArgument("TargetNamespace",
                                                                      new CodePrimitiveExpression(TargetNamespace)));

            if (Descriptor != null)
            {
                var templateStrings = Descriptor.Templates.Select(t => new CodePrimitiveExpression(t)).ToArray();
                viewAttribute.Arguments.Add(new CodeAttributeArgument("Templates", new CodeArrayCreateExpression("string", templateStrings)));
            }
        }

        private void AddGeneratedViewId(CodeTypeDeclaration viewClass)
        {
            var viewIdProperty = new CodeMemberProperty { Name = "GeneratedViewId", Type = new CodeTypeReference(typeof(Guid)), HasGet = true, Attributes = MemberAttributes.Override | MemberAttributes.Public };
            viewClass.Members.Add(viewIdProperty);
            viewIdProperty.GetStatements.Add(
                new CodeMethodReturnStatement(new CodeObjectCreateExpression(typeof(Guid),
                                                                             new CodePrimitiveExpression(GeneratedViewId.ToString()))));
        }
    }
}
