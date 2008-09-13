using System;
using System.Linq;
using Castle.MonoRail.Framework;

namespace Castle.MonoRail.Views.Spark
{
    public class ViewComponentInfo
    {
        public ViewComponentInfo()
        {

        }
        public ViewComponentInfo(Type type)
        {
            Type = type;
            Details = type.GetCustomAttributes(typeof(ViewComponentDetailsAttribute), false).OfType<ViewComponentDetailsAttribute>().FirstOrDefault();
            if (Details == null)
                Instance = (ViewComponent)Activator.CreateInstance(type);
        }

        public Type Type { get; set; }
        public ViewComponentDetailsAttribute Details { get; set; }
        public ViewComponent Instance { get; set; }

        public bool SupportsSection(string sectionName)
        {
            if (Details != null)
                return Details.SupportsSection(sectionName);
            
            if (Instance != null)
            {
                // if a component doesn't provide an implementation the default may throw an exception
                try { return Instance.SupportsSection(sectionName); }
                catch (NullReferenceException) { }
            }

            return false;
        }
    }
}