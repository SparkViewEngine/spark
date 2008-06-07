namespace MvcContrib.SparkViewEngine.Parser
{
	public class Chain<TLeft, TDown>
	{
		private readonly TLeft _left;
		private readonly TDown _down;

		public Chain(TLeft left, TDown down)
		{
			_left = left;
			_down = down;
		}

		public TLeft Left
		{
			get { return _left; }
		}

		public TDown Down
		{
			get { return _down; }
		}
	}
}