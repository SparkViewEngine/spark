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
using System.Configuration;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using Castle.MonoRail.Framework;
using Castle.MonoRail.Framework.Test;
using Spark;


namespace Castle.MonoRail.Views.Spark.Install
{
    public partial class PrecompileInstaller : Installer
    {
        public PrecompileInstaller()
        {
            InitializeComponent();
        }

        public string TargetAssemblyFile { get; set; }
        public string ViewPath { get; set; }

        public event DescribeBatchEventHandler DescribeBatch;

        public override void Install(IDictionary stateSaver)
        {
            // figure out all paths based on this assembly in the bin dir

            var assemblyPath = Parent.GetType().Assembly.CodeBase.Replace("file:///", "");
            var targetPath = Path.ChangeExtension(assemblyPath, ".Views.dll");

            var appBinPath = Path.GetDirectoryName(assemblyPath);
            var appBasePath = Path.GetDirectoryName(appBinPath);

            var viewPath = ViewPath;
            if (string.IsNullOrEmpty(viewPath))
                viewPath = "Views";

            if (!Directory.Exists(Path.Combine(appBasePath, viewPath)) &&
                Directory.Exists(Path.Combine(appBinPath, viewPath)))
            {
                appBasePath = appBinPath;
            }

            var webFileHack = Path.Combine(appBasePath, "web");
            var viewsLocation = Path.Combine(appBasePath, viewPath);

            if (!string.IsNullOrEmpty(TargetAssemblyFile))
                targetPath = Path.Combine(appBinPath, TargetAssemblyFile);

            // this hack enables you to open the web.config as if it was an .exe.config
            File.Create(webFileHack).Close();
            var config = ConfigurationManager.OpenExeConfiguration(webFileHack);
            File.Delete(webFileHack);

            // GetSection will try to resolve the "Spark" assembly, which the installutil appdomain needs help finding
            AppDomain.CurrentDomain.AssemblyResolve += ((sender, e) => Assembly.LoadFile(Path.Combine(appBinPath, e.Name + ".dll")));
            var settings = (ISparkSettings)config.GetSection("spark");

            var services = new StubMonoRailServices();
            services.AddService(typeof(IViewSourceLoader), new FileAssemblyViewSourceLoader(viewsLocation));
            services.AddService(typeof(ISparkViewEngine), new SparkViewEngine(settings));
            services.AddService(typeof(IControllerDescriptorProvider), services.ControllerDescriptorProvider);

            var factory = new SparkViewFactory();
            factory.Service(services);

            // And generate all of the known view/master templates into the target assembly
            var batch = new SparkBatchDescriptor(targetPath);

            // create entries for controller attributes in the parent installer's assembly
            batch.FromAssembly(Parent.GetType().Assembly);

            // and give the containing installer a change to add entries
            if (DescribeBatch != null)
                DescribeBatch(this, new DescribeBatchEventArgs { Batch = batch });

            factory.Precompile(batch);

            base.Install(stateSaver);
        }
    }
}
