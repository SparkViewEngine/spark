namespace Spark.Parser
{
	public class ParseResult<TValue>
	{
		private readonly Position _rest;
		private readonly TValue _value;

		public ParseResult(Position rest, TValue value)
		{
			_rest = rest;
			_value = value;
		}

		public Position Rest
		{
			get { return _rest; }
		}

		public TValue Value
		{
			get { return _value; }
		}
	}
}