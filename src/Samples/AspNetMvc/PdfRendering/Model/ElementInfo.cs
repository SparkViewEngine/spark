using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PdfRendering.Model
{
    public class ElementInfo
    {
        public string Name { get; set; }
        public string[] Attributes { get; set; }

        public ElementInfo(string name, params string[] attributes)
        {
            Name = name;
            Attributes = attributes;
        }
    }
    

    public static class ElementInfoExtensions
    {
        public static ICollection<ElementInfo> Add(this ICollection<ElementInfo> list, string name, params string[] attributes)
        {
            list.Add(new ElementInfo(name, attributes));
            return list;
        }
    }
}
