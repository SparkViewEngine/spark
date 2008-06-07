using System;
using System.Collections.Generic;
using MvcContrib.SparkViewEngine.Compiler;
using MvcContrib.SparkViewEngine.Parser;

namespace MvcContrib.SparkViewEngine
{
	public class CompiledViewHolder
	{
		static private CompiledViewHolder _current;

		readonly Dictionary<Key, Entry> _cache = new Dictionary<Key, Entry>();

		public static CompiledViewHolder Current
		{
			get
			{
				if (_current == null)
					_current = new CompiledViewHolder();
				return _current;
			}
			set { _current = value; }
		}

		public Entry Lookup(Key key)
		{
			Entry entry;

			lock (_cache)
			{
				if (!_cache.TryGetValue(key, out entry))
					return null;
			}

			return entry.Loader.IsCurrent() ? entry : null;
		}

		public void Store(Entry entry)
		{
			lock (_cache)
			{
				_cache[entry.Key] = entry;
			}
		}

		public class Key
		{
			public string ControllerName { get; set; }
			public string ViewName { get; set; }
			public string MasterName { get; set; }

			public override int GetHashCode()
			{
				return (ControllerName ?? "").ToLowerInvariant().GetHashCode() ^
					(ViewName ?? "").ToLowerInvariant().GetHashCode() ^
					(MasterName ?? "").ToLowerInvariant().GetHashCode();
			}

			public override bool Equals(object obj)
			{
				var that = obj as Key;
				if (that == null || GetType() != that.GetType())
					return false;
				return string.Equals(ControllerName, that.ControllerName, StringComparison.InvariantCultureIgnoreCase) &&
					   string.Equals(ViewName, that.ViewName, StringComparison.InvariantCultureIgnoreCase) &&
					   string.Equals(MasterName, that.MasterName, StringComparison.InvariantCultureIgnoreCase);
			}
		}

		public class Entry
		{
			public Key Key { get; set; }
			public ViewLoader Loader { get; set; }
			public ViewCompiler Compiler { get; set; }
		}
	}
}
