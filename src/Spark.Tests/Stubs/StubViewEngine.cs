using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Tests.Stubs
{
	public class StubViewEngine
	{
		public ISparkViewFactory Factory { get; set;}

		public void RenderView(StubViewContext viewContext)
		{
			var sparkView = Factory.CreateInstance(viewContext.ControllerName, viewContext.ViewName, viewContext.MasterName);
			var result = sparkView.RenderView();
			viewContext.Output.Append(result);
		}
	}
}