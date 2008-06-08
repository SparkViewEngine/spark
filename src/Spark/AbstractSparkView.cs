using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
	public abstract class AbstractSparkView : ISparkView
	{
		private readonly Dictionary<string, StringBuilder> _content = new Dictionary<string, StringBuilder>();

		public Dictionary<string, StringBuilder> Content { get { return _content; } }

		protected StringBuilder BindContent(string name)
		{
			StringBuilder sb;
			if (!_content.TryGetValue(name, out sb))
			{
				sb = new StringBuilder();
				_content.Add(name, sb);
			}
			return sb;
		}

		public abstract string RenderView();
	}
}
