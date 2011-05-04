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
        public override IEnumerable<Binding> GetBindings(IViewFolder viewFolder, string viewPath)
        {
            if (viewFolder.HasView("bindings.xml") == false)
                return new Binding[0];

            var file = viewFolder.GetViewSource("bindings.xml");
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
