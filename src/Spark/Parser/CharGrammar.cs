using System;
using System.Collections.Generic;

namespace MvcContrib.SparkViewEngine.Parser
{
	public class CharGrammar : Grammar
	{
		public static ParseAction<char> Ch(Func<char, bool> predicate)
		{
			return delegate(Position input)
					   {
						   if (input.PotentialLength() == 0 || !predicate(input.Peek()))
							   return null;
						   return new ParseResult<char>(input.Advance(1), input.Peek());
					   };
		}

		public static ParseAction<char> ChNot(Func<char, bool> predicate)
		{
			return delegate(Position input)
					   {
						   if (input.PotentialLength() == 0 || predicate(input.Peek()))
							   return null;
						   return new ParseResult<char>(input.Advance(1), input.Peek());
					   };
		}

		public static ParseAction<string> Ch(string match)
		{
			return delegate(Position input)
					   {
						   if (input.PotentialLength() < match.Length || !string.Equals(input.Peek(match.Length), match))
							   return null;
						   return new ParseResult<string>(input.Advance(match.Length), match);
					   };
		}
		public static ParseAction<char> Ch(params char[] allowed)
		{
			return delegate(Position input)
					   {
						   if (input.PotentialLength() == 0 || !((IList<char>)allowed).Contains(input.Peek()))
							   return null;
						   return new ParseResult<char>(input.Advance(1), input.Peek());
					   };
		}
		public static ParseAction<char> ChNot(params char[] disallowed)
		{
			return delegate(Position input)
					   {
						   if (input.PotentialLength() == 0 || ((IList<char>)disallowed).Contains(input.Peek()))
							   return null;
						   return new ParseResult<char>(input.Advance(1), input.Peek());
					   };
		}
	}
}