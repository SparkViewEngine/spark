using Spark;

namespace Spark
{
	public interface ISparkViewFactory
	{
		ISparkView CreateInstance(string controllerName, string viewName, string masterName);
	}
}
