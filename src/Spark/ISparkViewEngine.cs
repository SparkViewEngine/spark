using Spark;

namespace Spark
{
	public interface ISparkViewEngine
	{
		ISparkViewEntry GetEntry(string controllerName, string viewName, string masterName);
		ISparkView CreateInstance(string controllerName, string viewName, string masterName);
	}
}
