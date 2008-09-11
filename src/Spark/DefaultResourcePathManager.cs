using System;

namespace Spark
{
    public class DefaultResourcePathManager : IResourcePathManager
    {
        private readonly ISparkSettings _settings;

        public DefaultResourcePathManager(ISparkSettings settings)
        {
            _settings = settings;
        }

        public string GetResourcePath(string siteRoot, string path)
        {
            foreach(var mapping in _settings.ResourceMappings)
            {
                if (path.StartsWith(mapping.Match, StringComparison.InvariantCultureIgnoreCase))
                {
                    return mapping.Location + path.Substring(mapping.Match.Length);
                }
            }
            return siteRoot + path;
        }
    }
}
