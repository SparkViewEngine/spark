using Spark.FileSystem;

namespace Spark.Bindings
{
    public class BindingRequest
    {
        public BindingRequest(IViewFolder viewFolder)
        {
            ViewFolder = viewFolder;
            ViewPath = string.Empty;
        }

        public IViewFolder ViewFolder { get; private set; }
        public string ViewPath { get; set; }
    }
}