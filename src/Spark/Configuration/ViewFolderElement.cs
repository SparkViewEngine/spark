using System.Collections.Generic;
using System.Configuration;
using Spark.FileSystem;

namespace Spark.Configuration
{
    public class ViewFolderElement : ConfigurationElement, IViewFolderSettings
    {
        public ViewFolderElement()
        {
            Parameters = new Dictionary<string, string>();
        }

        [ConfigurationProperty("name")]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("folderType")]
        public ViewFolderType FolderType
        {
            get { return (ViewFolderType)this["folderType"]; }
            set { this["folderType"] = value; }
        }

        [ConfigurationProperty("type")]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }

        [ConfigurationProperty("subfolder")]
        public string Subfolder
        {
            get { return (string)this["subfolder"]; }
            set { this["subfolder"] = value; }
        }

        public IDictionary<string, string> Parameters { get; set;}


        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            Parameters.Add(name, value);
            return true;
        }
    }
}