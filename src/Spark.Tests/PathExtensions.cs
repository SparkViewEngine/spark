using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.Tests
{
    public static class PathExtensions
    {
        private const char PlaceholderSeparatorChar = '\\';

        public static string AsPath(this string value)
        {
            if (Path.DirectorySeparatorChar == PlaceholderSeparatorChar)
                return value;

            return value.Replace(PlaceholderSeparatorChar, Path.DirectorySeparatorChar);
        }
    }
}
