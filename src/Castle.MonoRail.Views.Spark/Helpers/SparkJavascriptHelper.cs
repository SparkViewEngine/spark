using System.IO;
using Castle.MonoRail.Framework.Helpers;
using Castle.MonoRail.Views.Spark.Wrappers;
using Spark;

namespace Castle.MonoRail.Views.Spark.Helpers
{
  public class SparkJavascriptHelper : AbstractHelper
  {
    public virtual string CompileView()
    {
      return CompileView(ControllerContext.SelectedViewName);
    }

    public virtual string CompileView(string view)
    {
      var viewFactory = new SparkViewFactory();
      var descriptor = new SparkViewDescriptor {Language = LanguageType.Javascript};

      descriptor.AddTemplate(string.Format("{0}{1}{2}", ControllerContext.ViewFolder, Path.DirectorySeparatorChar, view));
      ((IViewSourceLoaderContainer)viewFactory).ViewSourceLoader = Context.Services.ViewSourceLoader;

      var entry = viewFactory.Engine.CreateEntry(descriptor);
      return entry.SourceCode;
    }
  }
}