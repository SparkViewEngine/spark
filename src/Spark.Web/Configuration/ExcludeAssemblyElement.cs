using System.Configuration;

namespace Spark.Configuration
{
    public class ExcludeAssemblyElement : ConfigurationElement
    {
        [ConfigurationProperty("excludeAssembly")]
        public string Assembly
        {
            get => (string)this["excludeAssembly"];
            set => this["excludeAssembly"] = value;
        }
    }
}