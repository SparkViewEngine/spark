
namespace MvcContrib.SparkViewEngine
{
	public interface ISparkView
	{
		string RenderView(ISparkViewContext viewContext);
	}

	public interface ISparkViewContext
	{
	}
}
