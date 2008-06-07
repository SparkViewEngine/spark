using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MvcContrib.SparkViewEngine.Parser.Markup;

namespace MvcContrib.SparkViewEngine.Parser
{
	public interface IParserFactory
	{
		ParseAction<IList<Node>> CreateParser();
	}

	public class ParserFactory : IParserFactory
	{
		private readonly MarkupGrammar _grammar;

		public ParserFactory()
		{
			_grammar = new MarkupGrammar();
		}

		public ParseAction<IList<Node>> CreateParser()
		{
			return _grammar.Nodes;
		}
	}
}
