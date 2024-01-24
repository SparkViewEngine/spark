using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Spark.FileSystem;

namespace Spark.Bindings
{
    public class DefaultBindingProvider : BindingProvider
    {
        public override IEnumerable<Binding> GetBindings(BindingRequest bindingRequest)
        {
            if (bindingRequest.ViewFolder.HasView("bindings.xml") == false)
                return Array.Empty<Binding>();

            var file = bindingRequest.ViewFolder.GetViewSource("bindings.xml");
            using (var stream = file.OpenViewStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    return LoadStandardMarkup(reader);
                }
            }
        }
    }
}
