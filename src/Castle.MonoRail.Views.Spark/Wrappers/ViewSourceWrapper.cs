using System.IO;
using Castle.MonoRail.Framework;
using Spark.FileSystem;

namespace Castle.MonoRail.Views.Spark.Wrappers
{
    public class ViewSourceWrapper : IViewFile
    {
        private readonly IViewSource _source;

        public ViewSourceWrapper(IViewSource source)
        {
            _source = source;
        }

        public long LastModified
        {
            get { return _source.LastModified; }
        }

        public Stream OpenViewStream()
        {
            return _source.OpenViewStream();
        }
    }
}