using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Bindings
{
    public interface IBindingProvider
    {
        IEnumerable<Binding> GetBindings(IViewFolder viewFolder, string directoryPath);
    }
}
