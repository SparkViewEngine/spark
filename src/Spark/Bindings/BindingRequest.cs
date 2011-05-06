using System;
using Spark.FileSystem;

namespace Spark.Bindings
{
    public class BindingRequest : IEquatable<BindingRequest>
    {
        public BindingRequest(IViewFolder viewFolder)
        {
            ViewFolder = viewFolder;
            ViewPath = string.Empty;
        }

        public IViewFolder ViewFolder { get; private set; }
        public string ViewPath { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (BindingRequest)) return false;
            return Equals((BindingRequest) obj);
        }

        public bool Equals(BindingRequest other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.ViewFolder, ViewFolder) && Equals(other.ViewPath, ViewPath);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ViewFolder != null ? ViewFolder.GetHashCode() : 0)*397) ^ (ViewPath != null ? ViewPath.GetHashCode() : 0);
            }
        }
    }
}