using System.Configuration;

namespace Spark.Configuration
{
    public class ViewFolderElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ViewFolderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ViewFolderElement)element).Name;
        }

        public void Add(string name)
        {
            base.BaseAdd(new ViewFolderElement { Name = name });
        }
    }
}