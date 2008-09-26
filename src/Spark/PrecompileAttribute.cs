using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class PrecompileAttribute : Attribute
    {
        public PrecompileAttribute()
        {
            
        }
        public PrecompileAttribute(string include)
        {
            Include = include;
        }
        public PrecompileAttribute(string include, string layout)
        {
            Include = include;
            Layout = layout;
        }
        public string Include { get; set; }
        public string Exclude { get; set; }
        public string Layout { get; set; }
    }
}
