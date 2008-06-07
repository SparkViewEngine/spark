using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Compiler;
using MvcContrib.SparkViewEngine.Compiler.ChunkVisitors;
using MvcContrib.SparkViewEngine.Compiler.NodeVisitors;
using MvcContrib.SparkViewEngine.Parser.Markup;
using Spark.FileSystem;

namespace MvcContrib.SparkViewEngine.Parser
{
	public class ViewLoader
	{
		private IFileSystem fileSystem;
		private IParserFactory _parserFactory;

		readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();
		readonly List<string> _pending = new List<string>();

		public IFileSystem FileSystem
		{
			get { return fileSystem; }
			set { fileSystem = value; }
		}

		public IParserFactory ParserFactory
		{
			get { return _parserFactory; }
			set { _parserFactory = value; }
		}

		private class Entry
		{
			private readonly FileContext fileContext = new FileContext();

			public string ViewPath
			{
				get { return FileContext.ViewSourcePath; }
				set { FileContext.ViewSourcePath = value; }
			}

			public long LastModified { get; set; }

			public IList<Chunk> Chunks
			{
				get { return FileContext.Contents; }
				set { FileContext.Contents = value; }
			}

			public FileContext FileContext
			{
				get { return fileContext; }
			}
		}

		Entry BindEntry(string referencePath)
		{
			if (_entries.ContainsKey(referencePath))
				return _entries[referencePath];

			var viewSource = fileSystem.GetViewSource(referencePath);

			var newEntry = new Entry { ViewPath = referencePath, LastModified = viewSource.LastModified };
			_entries.Add(referencePath, newEntry);
			_pending.Add(referencePath);
			return newEntry;
		}

		public virtual bool IsCurrent()
		{
			foreach (var entry in _entries.Values)
			{
				var viewSource = fileSystem.GetViewSource(entry.ViewPath);
				if (viewSource.LastModified != entry.LastModified)
					return false;
			}
			return true;
		}


		public IList<Chunk> Load(string controllerName, string viewName)
		{
			return Load(ResolveView(controllerName, viewName));
		}

		public IList<Chunk> Load(string viewPath)
		{
			if (string.IsNullOrEmpty(viewPath))
				return null;

			var entry = BindEntry(viewPath);
			if (entry == null)
				return null;

			while (_pending.Count != 0)
			{
				string nextPath = _pending.First();
				_pending.Remove(nextPath);
				LoadInternal(nextPath);
			}

			return entry.Chunks;
		}

		void LoadInternal(string viewPath)
		{
			if (string.IsNullOrEmpty(viewPath))
				return;

			var newEntry = BindEntry(viewPath);

			var sourceContext = CreateSourceContext(viewPath);
			var position = new Position(sourceContext);

			var parser = _parserFactory.CreateParser();
			var nodes = parser(position);

			var partialFileNames = FindPartialFiles(viewPath);

			var specialNodeVisitor = new SpecialNodeVisitor(partialFileNames);
			specialNodeVisitor.Accept(nodes.Value);

			var chunkBuilder = new ChunkBuilderVisitor();
			chunkBuilder.Accept(specialNodeVisitor.Nodes);
			newEntry.Chunks = chunkBuilder.Chunks;

			var fileReferenceVisitor = new FileReferenceVisitor();
			fileReferenceVisitor.Accept(newEntry.Chunks);

			foreach (var useFile in fileReferenceVisitor.References)
			{
				var referencePath = ResolveReference(viewPath, useFile.Name);

				if (!string.IsNullOrEmpty(referencePath))
					useFile.FileContext = BindEntry(referencePath).FileContext;
			}
		}

		public IList<string> FindPartialFiles(string viewPath)
		{
			var results = new List<string>();

			string controllerPath = Path.GetDirectoryName(viewPath);
			foreach (var view in FileSystem.ListViews(controllerPath))
			{
				string baseName = Path.GetFileNameWithoutExtension(view);
				if (baseName.StartsWith("_"))
					results.Add(baseName.Substring(1));
			}
			foreach (var view in FileSystem.ListViews("Shared"))
			{
				string baseName = Path.GetFileNameWithoutExtension(view);
				if (baseName.StartsWith("_"))
					results.Add(baseName.Substring(1));
			}
			return results;
		}

		string ResolveReference(string existingViewPath, string viewName)
		{
			string controllerPath = Path.GetDirectoryName(existingViewPath);

			return ResolveView(controllerPath, viewName);
		}

		string ResolveView(string controllerName, string viewName)
		{
			if (string.IsNullOrEmpty(viewName))
				return null;

			string attempt1 = Path.Combine(controllerName, Path.ChangeExtension(viewName, "xml"));
			if (FileSystem.HasView(attempt1))
				return attempt1;

			string attempt2 = Path.Combine("Shared", Path.ChangeExtension(viewName, "xml"));
			if (FileSystem.HasView(attempt2))
				return attempt2;

			throw new FileNotFoundException(
				string.Format("Unable to find {0} or {1}", attempt1, attempt2),
				attempt1);
		}

		private SourceContext CreateSourceContext(string viewPath)
		{
			var viewSource = fileSystem.GetViewSource(viewPath);

			if (viewSource == null)
				throw new FileNotFoundException("View file not found", viewPath);

			using (TextReader reader = new StreamReader(viewSource.OpenViewStream()))
			{
				return new SourceContext(reader.ReadToEnd(), viewSource.LastModified);
			}
		}
	}
}
