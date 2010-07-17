using System.IO;
using System;

namespace SparkSense.Parsing
{
    public static class ParsingExtensions
    {
        public static bool IsNonPartialSparkFile(this string filePath)
        {
            return filePath.EndsWith(".spark") && !Path.GetFileName(filePath).StartsWith("_");
        }
    }
}
