using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Castle.MonoRail.Views.Spark.Install;
using Castle.MonoRail.Views.Spark.Tests.Stubs;
using NUnit.Framework;

namespace Castle.MonoRail.Views.Spark.Tests
{
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
