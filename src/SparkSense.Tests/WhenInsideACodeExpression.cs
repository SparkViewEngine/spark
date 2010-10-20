using NUnit.Framework;
using System;
using EnvDTE;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Interop;
using System.ComponentModel.Design;

namespace SparkSense.Tests
{
    public class WhenInsideACodeExpression : TestingContext
    {
        private ITypeDiscoveryService _discovery;
        private ISparkServiceProvider _services = new SparkServiceProvider();
        private IVsHierarchy _hier;

        public WhenInsideACodeExpression()
        {

        }

        [Test]
        public void blah()
        {
            var testDTE = (DTE)Marshal.GetActiveObject("VisualStudio.DTE.10.0");
            if (_hier == null)
            {
                var sln = testDTE.Solution as IVsSolution;
                string projectName = testDTE.ActiveDocument.ProjectItem.ContainingProject.UniqueName;
                //sln.GetProjectOfUniqueName(projectName, out _hier);
            }

            _discovery = SparkServiceProvider.TypeService.GetTypeDiscoveryService(_hier);
        }
    }
}
