using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark;
using System.Reflection;
using System.Web.Mvc;
using System.IO;

namespace SparkEmailMerge {
	public class Templater {

		private SparkViewEngine engine;
		
		public Templater() {
			var settings = new SparkSettings();
			settings.SetPageBaseType(typeof(TemplateBase));
			engine = new SparkViewEngine(settings);
		}

		public string Populate(string templateFilePath, object data) {
			var writer = new StringWriter();

			var descriptor = new SparkViewDescriptor();
			descriptor.AddTemplate(templateFilePath);
			var view = (TemplateBase)engine.CreateInstance(descriptor);
			try {
				view.ViewData = new ViewDataDictionary(data);
				view.RenderView(writer);
			} finally {
				engine.ReleaseInstance(view);
			}
			return writer.ToString();
		}
	}

	public abstract class TemplateBase : AbstractSparkView {
		
		public ViewDataDictionary ViewData { get; set; }

		public object Eval(string expression) {
			return ViewData.Eval(expression);
		}

		public string Eval(string expression, string format) {
			return ViewData.Eval(expression, format);
		}
	}
}
