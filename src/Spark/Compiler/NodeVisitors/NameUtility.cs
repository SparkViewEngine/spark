// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler.NodeVisitors
{
    static class NameUtility
    {

        public static string GetPrefix(string name)
        {
            var colonIndex = name.IndexOf(':');
            return colonIndex <= 0 ? "" : name.Substring(0, colonIndex);
        }

        public static string GetName(string name)
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
            return GetName(name) == matchName;
        }

        public static bool IsMatch(string nameA, string namespaceA, string nameB, string namespaceB, NamespacesType type)
        {
            if (type == NamespacesType.Unqualified)
                return nameA == nameB;

            return namespaceA == namespaceB &&
                   GetName(nameA) == GetName(nameB);

        }
    }
}
