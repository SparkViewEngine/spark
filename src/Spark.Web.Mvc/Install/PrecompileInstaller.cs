// Copyright 2008-2009 Louis DeJardin - http://whereslou.com
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
using Spark.FileSystem;

namespace Spark.Web.Mvc.Install
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

        public Func<ISparkSettings> SettingsInstantiator { get; set; }

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

            ISparkSettings settings;
            if (SettingsInstantiator != null)
            {
                settings = SettingsInstantiator();
            }
            else
            {
                // this hack enables you to open the web.config as if it was an .exe.config
                File.Create(webFileHack).Close();
                var config = ConfigurationManager.OpenExeConfiguration(webFileHack);
                File.Delete(webFileHack);

                // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
                AppDomain.CurrentDomain.AssemblyResolve +=
                    ((sender, e) => Assembly.LoadFile(Path.Combine(webBinPath, e.Name + ".dll")));
                settings = (ISparkSettings) config.GetSection("spark");
            }

            // Finally create an engine with the <spark> settings from the web.config
            var factory = new SparkViewFactory(settings)
                              {
                                  ViewFolder = new FileSystemViewFolder(viewsLocation)
                              };

            // And generate all of the known view/master templates into the target assembly
            var batch = new SparkBatchDescriptor(targetPath);
            
            // create entries for controller attributes in the parent installer's assembly
            batch.FromAssembly(Parent.GetType().Assembly);

            // and give the containing installer a change to add entries
            if (DescribeBatch != null)
                DescribeBatch(this, new DescribeBatchEventArgs {Batch = batch});

            factory.Precompile(batch);

            base.Install(stateSaver);
        }
    }
}