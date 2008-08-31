using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler
{
    public static class CollectionUtility
    {
        public  static int Count<T>(IEnumerable<T> enumerable)
        {
            return enumerable.Count();
        }

        public static int Count(IEnumerable enumerable)
        {
            var count = 0;
            var enumerator = enumerable.GetEnumerator();
            while(enumerator.MoveNext())
                ++count;
            return count;
        }
    }
}
