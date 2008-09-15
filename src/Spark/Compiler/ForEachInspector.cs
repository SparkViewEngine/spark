using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Compiler
{
    public class ForEachInspector
    {
        public ForEachInspector(string code)
        {
            var terms = code.Split(' ', '\r', '\n', '\t').ToList();
            var inIndex = terms.IndexOf("in");
            if (inIndex >= 2)
            {
                Recognized = true;
                VariableType = string.Join(" ", terms.ToArray(), 0, inIndex - 1);
                VariableName = terms[inIndex - 1];
                CollectionCode = string.Join(" ", terms.ToArray(), inIndex + 1, terms.Count - inIndex - 1);
            }
        }

        public bool Recognized { get; set; }
        public string VariableType { get; set; }
        public string VariableName { get; set; }
        public string CollectionCode { get; set; }
    }
}
