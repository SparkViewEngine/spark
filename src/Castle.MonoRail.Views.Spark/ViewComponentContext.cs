using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Views.Spark
{
	public class ViewComponentContext : IViewComponentContext
	{
		private readonly SparkViewFactory _viewEngine;
		private readonly IDictionary _componentParameters;
		private readonly IEngineContext _context;
		private readonly StringWriter _writer;
		private readonly Action<StringBuilder> body;
		private IDictionary _contextVarsAdapter;


		public ViewComponentContext(SparkViewFactory viewEngine, IEngineContext context, IDictionary componentParameters, StringWriter writer, Action<StringBuilder> body)
		{
			_viewEngine = viewEngine;
			_componentParameters = componentParameters;
			_context = context;
			_writer = writer;
			this.body = body;
			//_contextVarsAdapter = new ContextVarsAdapter(this);
			_contextVarsAdapter = new Hashtable(_context.CurrentControllerContext.PropertyBag);
		}



		public bool HasSection(string sectionName)
		{
			return false;
		}

		public void RenderView(string name, TextWriter writer)
		{
			throw new NotImplementedException();
		}

		public void RenderBody()
		{
			RenderBody(_writer);
		}

		public void RenderBody(TextWriter writer)
		{
			var output = new StringBuilder();
			body(output);
			writer.Write(output);
		}

		public void RenderSection(string sectionName)
		{
			RenderSection(sectionName, _writer);
		}

		public void RenderSection(string sectionName, TextWriter writer)
		{
			throw new NotImplementedException();
		}

		public string ComponentName
		{
			get { throw new NotImplementedException(); }
		}

		public TextWriter Writer
		{
			get { return _writer; }
		}

		public IDictionary ContextVars
		{
			get { return _contextVarsAdapter; }
		}

		public IDictionary ComponentParameters
		{
			get { return _componentParameters; }
		}

		public string ViewToRender
		{
			get { throw new NotImplementedException(); }
			set { throw new NotImplementedException(); }
		}

		public IViewEngine ViewEngine
		{
			get { return _viewEngine; }
		}

	}
}