using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using Spark;
using Spark.FileSystem;


namespace MediumTrustHosting
{
    /// <summary>
    /// This installer is invoked in the post-build step of the csproj
    /// </summary>
    [RunInstaller(true)]
    public partial class PostBuildStep : Installer
    {
        public PostBuildStep()
        {
            InitializeComponent();
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            // figure out all paths based on this assembly in the bin dir
            var assemblyPath = GetType().Assembly.Location;
            var targetPath = Path.ChangeExtension(assemblyPath, ".Views.dll");
            var webSitePath = Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath));
            var webBinPath = Path.Combine(webSitePath, "bin");
            var webFileHack = Path.Combine(webSitePath, "web");
            var viewsLocation = Path.Combine(webSitePath, "Views");

            // this hack enables you to open the web.config as if it was an .exe.config
            File.Create(webFileHack).Close();
            var config = ConfigurationManager.OpenExeConfiguration(webFileHack);
            File.Delete(webFileHack);

            // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
            AppDomain.CurrentDomain.AssemblyResolve += ((sender, e) => Assembly.LoadFile(Path.Combine(webBinPath, e.Name + ".dll")));
            var settings = (ISparkSettings)config.GetSection("spark");

            // Finally create an engine with the <spark> settings from the web.config
            var engine = new SparkViewEngine(settings)
                             {
                                 ViewFolder = new FileSystemViewFolder(viewsLocation)
                             };

            // And generate all of the known view/master templates into the target assembly
            engine.BatchCompilation(targetPath, Global.AllKnownDescriptors());
        }

        public override void Commit(IDictionary savedState)
        {

        }
    }
}
