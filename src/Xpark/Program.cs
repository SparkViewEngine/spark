using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Spark;
using Spark.Bindings;
using Spark.Compiler;
using Spark.FileSystem;
using Spark.Parser.Syntax;

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

            // Find the full path to the template file, 
            // using current directory if argument isn't fully qualified
            var templatePath = Path.Combine(Environment.CurrentDirectory, args[0]);
            var templateName = Path.GetFileName(templatePath);
            var templateDirPath = Path.GetDirectoryName(templatePath);

            // Look for views in the root and shared location
            var viewFolder = new FileSystemViewFolder(templateDirPath);
            viewFolder.Append(new SubViewFolder(viewFolder, "Shared"));

            var settings = new SparkSettings()
                .SetBaseClassTypeName(typeof(SparkView));

            var partialProvider = new DefaultPartialProvider();

            var batchCompiler = new RoslynBatchCompiler();

            var engine = new SparkViewEngine(
                settings,
                new DefaultSyntaxProvider(settings),
                new DefaultViewActivator(),
                new DefaultLanguageFactory(batchCompiler, settings),
                new CompiledViewHolder(),
                viewFolder,
                batchCompiler,
                partialProvider,
                new DefaultPartialReferenceProvider(partialProvider),
                new DefaultBindingProvider(),
                null);

            // compile and instantiate the template
            var view =
                (SparkView)engine.CreateInstance(
                    new SparkViewDescriptor()
                        .AddTemplate(templateName));

            // load the second argument, or default to reading stdin
            if (args.Length >= 2)
            {
                view.Model = XDocument.Load(args[1]);
            }
            else
            {
                view.Model = XDocument.Load(XmlReader.Create(Console.OpenStandardInput()));
            }

            // write out to the third argument, or default to writing stdout
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
    }
}
