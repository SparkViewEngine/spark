using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using MvcContrib.ViewFactories;
using Spark;


namespace MvcContrib.SparkViewEngine.Install
{
    [RunInstaller(true)]
    public partial class PrecompileInstaller : Installer
    {
        public PrecompileInstaller()
        {
            InitializeComponent();
        }

        public string TargetAssemblyFile { get; set; }

        public event DescribeBatchHandler DescribeBatch;

        public override void Install(IDictionary stateSaver)
        {
            // figure out all paths based on this assembly in the bin dir
            var assemblyPath = Parent.GetType().Assembly.Location;
            var targetPath = Path.ChangeExtension(assemblyPath, ".Views.dll");
            var webSitePath = Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath));
            var webBinPath = Path.Combine(webSitePath, "bin");
            var webFileHack = Path.Combine(webSitePath, "web");
            var viewsLocation = Path.Combine(webSitePath, "Views");

            if (!string.IsNullOrEmpty(TargetAssemblyFile))
                targetPath = Path.Combine(webBinPath, TargetAssemblyFile);

            // this hack enables you to open the web.config as if it was an .exe.config
            File.Create(webFileHack).Close();
            var config = ConfigurationManager.OpenExeConfiguration(webFileHack);
            File.Delete(webFileHack);

            // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
            AppDomain.CurrentDomain.AssemblyResolve += ((sender, e) => Assembly.LoadFile(Path.Combine(webBinPath, e.Name + ".dll")));
            var settings = (ISparkSettings)config.GetSection("spark");


            // Finally create an engine with the <spark> settings from the web.config
            var factory = new SparkViewFactory(settings)
            {
                ViewSourceLoader = new FileSystemViewSourceLoader(viewsLocation)
            };

            // And generate all of the known view/master templates into the target assembly

            var batch = new SparkBatchDescriptor(targetPath);
            if (DescribeBatch != null)
                DescribeBatch(this, new DescribeBatchEventArgs { Batch = batch});
            
            factory.Precompile(batch);

            base.Install(stateSaver);
        }
    }
}
