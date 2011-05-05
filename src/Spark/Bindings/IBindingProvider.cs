using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark.Bindings
{
    public interface IBindingProvider
    {
        IEnumerable<Binding> GetBindings(BindingRequest bindingRequest);
    }
}
