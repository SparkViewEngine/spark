using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Parser
{
	public class SourceContext
	{
		public SourceContext(string content)
		{
			Content = content;
		}
		public SourceContext(string content, long lastModified)
		{
			Content = content;
			LastModified = lastModified;
		}
		public string Content { get; set; }
		public long LastModified { get; set; }
	}
}
