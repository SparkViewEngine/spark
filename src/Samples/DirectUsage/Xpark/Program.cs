using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Spark;
using Spark.FileSystem;

namespace Xpark
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Write(@"
Transforms Xml using a Spark template

XPARK templatefile [inputfile [outputfile]]

  templatefile  Path to a .spark file.
  inputfile     Source xml. Path to an file or url for http GET.
  outputfile    Target file to receive template output.

If inputfile or outputfile are not provided stdin and stdout are used.

The Model in the template is an XDocument loaded from the source.

The templatefile location may also contain _partial.spark files, and
a _global.spark file with common namespaces, macros, etc.
");
                return;
            }
            var templatePath = Path.Combine(Environment.CurrentDirectory, args[0]);
            var templateName = Path.GetFileName(templatePath);
            var templateDirPath = Path.GetDirectoryName(templatePath);

            var viewFolder = new FileSystemViewFolder(templateDirPath);

            var engine = new SparkViewEngine
                             {
                                 DefaultPageBaseType = typeof(SparkView).FullName,
                                 ViewFolder = viewFolder.Append(new SubViewFolder(viewFolder, "Shared"))
                             };

            try
            {
                var view = (SparkView)engine.CreateInstance(
                                           new SparkViewDescriptor()
                                               .AddTemplate(templateName));

                if (args.Length >= 2)
                    view.Model = XDocument.Load(args[1]);
                else
                    view.Model = XDocument.Load(XmlReader.Create(Console.OpenStandardInput()));

                if (args.Length >= 3)
                {
                    using (var writer = new StreamWriter(new FileStream(args[2], FileMode.Create), Encoding.UTF8))
                    {
                        view.RenderView(writer);
                    }
                }
                else
                {
                    using (var writer = new StreamWriter(Console.OpenStandardOutput(), Encoding.UTF8))
                    {
                        view.RenderView(writer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
            }
        }
    }
}
