using System.Configuration;

namespace Spark.Configuration
{
    public class ExcludeAssemblyElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AssemblyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ExcludeAssemblyElement)element).Assembly;
        }

        public void Add(string assembly)
        {
            this.BaseAdd(new ExcludeAssemblyElement { Assembly = assembly });
        }
    }
}