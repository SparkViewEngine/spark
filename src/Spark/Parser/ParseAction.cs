namespace Spark.Parser
{
	public delegate ParseResult<TValue> ParseAction<TValue>(Position position);
}
