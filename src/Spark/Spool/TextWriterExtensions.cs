using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Spark.Spool
{
    public static class TextWriterExtensions 
    {
        public static void WriteTo(this TextWriter source, TextWriter target)
        {
            if (source is SpoolWriter)
            {
                if (target is SpoolWriter)
                {
                    ((SpoolWriter)source).SendToSpoolWriter((SpoolWriter)target);
                }
                else
                {
                    ((SpoolWriter)source).SendToTextWriter(target);
                }
            }
            else
            {
                target.Write(source);
            }
        }
    }
}
