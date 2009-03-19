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

namespace Castle.MonoRail.Views.Spark.Tests
{
    using System;
    using System.Collections;
    using System.Configuration.Install;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Castle.MonoRail.Views.Spark.Install;
    using Castle.MonoRail.Views.Spark.Tests.Stubs;
    using NUnit.Framework;

    [TestFixture]
    public class PrecompileInstallerTests
    {
        [Test]
        public void RunPrecompiler()
        {
            var appBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            var targetFile = Path.Combine(appBase, "RunPrecompiler.dll");
            File.Delete(targetFile);

            var parent = new ParentInstaller();
            var precompile = new PrecompileInstaller();

            precompile.TargetAssemblyFile = targetFile;
            precompile.ViewPath = "MonoRail.Tests.Views";
            precompile.DescribeBatch += ((sender, e) => e.Batch.For<StubController>().Include("*").Include("_*"));

            var context = new InstallContext();
            var state = new Hashtable();

            parent.Installers.Add(precompile);
            parent.Install(state);
            parent.Commit(state);

            Assert.That(File.Exists(targetFile), "File exists");

            var result = Assembly.LoadFrom(targetFile);
            Assert.AreEqual(3, result.GetTypes().Count());
        }

        public class ParentInstaller : Installer
        {

        }
    }
}
