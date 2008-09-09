using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler.NodeVisitors
{
    static class NameUtility
    {
        public static string RemovePrefix(string name)
        {
            var colonIndex = name.IndexOf(':');
            if (colonIndex < 0)
                return name;
            return name.Substring(colonIndex + 1);
        }
        public static bool IsMatch(string matchName, NamespacesType type, string name, string ns)
        {
            if (type == NamespacesType.Unqualified)
                return name == matchName;
            if (ns != Constants.Namespace)
                return false;
            return RemovePrefix(name) == matchName;
        }
    }
}
