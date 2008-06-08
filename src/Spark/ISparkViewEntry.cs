namespace Spark
{
	public interface ISparkViewEntry
	{
		string SourceCode { get; }
		ISparkView CreateInstance();
	}
}