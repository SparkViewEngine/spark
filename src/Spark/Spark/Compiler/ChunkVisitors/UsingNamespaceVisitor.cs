using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MvcContrib.SparkViewEngine.Compiler.ChunkVisitors
{
	public class UsingNamespaceVisitor : ChunkVisitor
	{
		private readonly StringBuilder _source;

		private readonly Dictionary<string, object> _used = new Dictionary<string, object>();

		Stack<string> _noncyclic = new Stack<string>();


		public UsingNamespaceVisitor(StringBuilder output)
		{
			_source = output;
		}

		protected override void Visit(UseNamespaceChunk chunk)
		{
			Using(chunk.Namespace);
		}

		protected override void Visit(RenderPartialChunk chunk)
		{
			if (_noncyclic.Contains(chunk.FileContext.ViewSourcePath))
				return;

			_noncyclic.Push(chunk.FileContext.ViewSourcePath);
			Accept(chunk.FileContext.Contents);
			_noncyclic.Pop();
		}

		public void Using(string ns)
		{
			if (_used.ContainsKey(ns))
				return;

			_used.Add(ns, null);
			_source.AppendLine(string.Format("using {0};", ns));
		}
	}
}
