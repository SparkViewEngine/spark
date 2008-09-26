// Copyright 2008 Louis DeJardin - http://whereslou.com
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
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

        public override void Install(IDictionary stateSaver)
        {
            // figure out all paths based on this assembly in the bin dir
            string assemblyPath = GetType().Assembly.Location;
            string targetPath = Path.ChangeExtension(assemblyPath, ".Views.dll");
            string webSitePath = Path.GetDirectoryName(Path.GetDirectoryName(assemblyPath));
            string webBinPath = Path.Combine(webSitePath, "bin");
            string webFileHack = Path.Combine(webSitePath, "web");
            string viewsLocation = Path.Combine(webSitePath, "Views");

            // this hack enables you to open the web.config as if it was an .exe.config
            File.Create(webFileHack).Close();
            Configuration config = ConfigurationManager.OpenExeConfiguration(webFileHack);
            File.Delete(webFileHack);

            // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
            AppDomain.CurrentDomain.AssemblyResolve +=
                ((sender, e) => Assembly.LoadFile(Path.Combine(webBinPath, e.Name + ".dll")));
            var settings = (ISparkSettings) config.GetSection("spark");

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