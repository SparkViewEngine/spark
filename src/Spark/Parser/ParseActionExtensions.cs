using System;
using System.Collections.Generic;

namespace MvcContrib.SparkViewEngine.Parser
{
	public static class ParseActionExtensions
	{
		public static ParseAction<IList<TValue>> Rep<TValue>(this ParseAction<TValue> parse)
		{
			return Grammar.Rep(parse);
		}
		public static ParseAction<IList<TValue>> Rep1<TValue>(this ParseAction<TValue> parse)
		{
			return Grammar.Rep1(parse);
		}

		public static ParseAction<Chain<TValue1, TValue2>> And<TValue1, TValue2>(
			this ParseAction<TValue1> p1,
			ParseAction<TValue2> p2)
		{
			return Grammar.And(p1, p2);
		}
		public static ParseAction<TValue> Or<TValue>(
			this ParseAction<TValue> p1,
			ParseAction<TValue> p2)
		{
			return Grammar.Or(p1, p2);
		}

		public static ParseAction<TValue1> IfNext<TValue1, TValue2>(
			this ParseAction<TValue1> p1,
			ParseAction<TValue2> p2)
		{
			return Grammar.IfNext(p1, p2);
		}

		public static ParseAction<TValue1> NotNext<TValue1, TValue2>(
			this ParseAction<TValue1> p1,
			ParseAction<TValue2> p2)
		{
			return Grammar.NotNext(p1, p2);
		}

		public static ParseAction<TValue2> Build<TValue1, TValue2>(
			this ParseAction<TValue1> parser,
			Func<TValue1, TValue2> builder)
		{
			return input =>
					   {
						   var result = parser(input);
						   if (result == null) return null;
						   return new ParseResult<TValue2>(result.Rest, builder(result.Value));
					   };
		}
		public static ParseAction<TValue> Opt<TValue>(ParseAction<TValue> parse)
		{
			return Grammar.Opt(parse);
		}

		public static ParseAction<TLeft> Left<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
		{
			return input =>
			{
				var result = parse(input);
				if (result == null) return null;
				return new ParseResult<TLeft>(result.Rest, result.Value.Left);
			};
		}
		public static ParseAction<TDown> Down<TLeft, TDown>(this ParseAction<Chain<TLeft, TDown>> parse)
		{
			return input =>
			{
				var result = parse(input);
				if (result == null) return null;
				return new ParseResult<TDown>(result.Rest, result.Value.Down);
			};
		}
	}
}