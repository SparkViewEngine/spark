using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Utilities
{
    public static class CacheUtilities
    {
        public static string ToIdentifier(string site, object[] key)
        {
            if (key.Length == 0)
                return site;

            if (key.Length == 1)
                return site + key[0];

            const string unitSeperator = "\u001f";
            var parts = new object[key.Length * 2];
            parts[0] = site;
            parts[1] = key[0];
            for (var index = 1; index != key.Length; ++index)
            {
                parts[index * 2] = unitSeperator;
                parts[index * 2 + 1] = key[index];
            }
            return string.Concat(parts);
        }
    }
}
