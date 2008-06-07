using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CSharp;
using MvcContrib.SparkViewEngine.Compiler.ChunkVisitors;

namespace MvcContrib.SparkViewEngine.Compiler
{
	public class ViewCompiler
	{
		public ViewCompiler(string baseClass)
		{
			BaseClass = baseClass;
		}
		public string BaseClass { get; set; }
		public string SourceCode { get; set; }
		public Type CompiledType { get; set; }

		public void CompileView(IList<Chunk> view)
		{
			CompileView(view, new Chunk[0]);
		}

		public void CompileView(IList<Chunk> view, IList<Chunk> master)
		{
			StringBuilder source = new StringBuilder();
			var usingGenerator = new UsingNamespaceVisitor(source);
			var baseClassGenerator = new BaseClassVisitor() { BaseClass = BaseClass };
			var globalsGenerator = new GlobalMembersVisitor(source);
			var viewGenerator = new GeneratedCodeVisitor(source);

			//usingGenerator.Using("System.Web.Mvc");
			usingGenerator.Accept(view);
			usingGenerator.Accept(master);

			baseClassGenerator.Accept(view);
			if (string.IsNullOrEmpty(baseClassGenerator.TModel))
				baseClassGenerator.Accept(master);

			source.AppendLine(string.Format("public class CompiledSparkView : {0} {{", baseClassGenerator.BaseClassTypeName));

			globalsGenerator.Accept(view);
			globalsGenerator.Accept(master);

			source.AppendLine("public void ProcessView() {");
			source.AppendLine("var output = BindContent(\"view\");");
			viewGenerator.Accept(view);
			source.AppendLine("}");

			source.AppendLine("public override string ProcessRequest() {");
			source.AppendLine("ProcessView();");
			if (master == null || master.Count == 0)
			{
				source.AppendLine("return Content[\"view\"].ToString();}");
			}
			else
			{
				source.AppendLine("var output = BindContent(\"master\");");
				viewGenerator.Accept(master);
				source.AppendLine("return Content[\"master\"].ToString();}");
			}

			source.AppendLine("}");

			SourceCode = source.ToString();

			var providerOptions = new Dictionary<string, string>();
			providerOptions.Add("CompilerVersion", "v3.5");
			CSharpCodeProvider codeProvider = new CSharpCodeProvider(providerOptions);

			var compilerParameters = new CompilerParameters();
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				string location;
				try
				{
					location = assembly.Location;
				}
				catch (NotSupportedException)
				{
					continue;
				}
				compilerParameters.ReferencedAssemblies.Add(location);
			}

			var compilerResults = codeProvider.CompileAssemblyFromSource(compilerParameters, SourceCode);

			if (compilerResults.Errors.Count != 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.AppendLine("Dynamic view compilation failed.");
				foreach (var err in compilerResults.Output)
				{
					sb.AppendLine(err);
				}

				sb.AppendLine();
				using (TextReader reader = new StringReader(SourceCode))
				{
					for (int lineNumber = 1; ; ++lineNumber)
					{
						string line = reader.ReadLine();
						if (line == null)
							break;
						sb.Append(lineNumber).Append(' ').AppendLine(line);
					}
				}
				throw new CompilerException(sb.ToString());
			}

			CompiledType = compilerResults.CompiledAssembly.GetType("CompiledSparkView");
		}

		public ISparkView CreateInstance()
		{
			return (ISparkView)Activator.CreateInstance(CompiledType);
		}
	}
}