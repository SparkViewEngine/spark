using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using MvcContrib.SparkViewEngine.Install;
using PrecompiledViews.Controllers;
using Spark;


namespace PrecompiledViews
{
    [RunInstaller(true)]
    public partial class PostBuildStep : Installer
    {
        public PostBuildStep()
        {
            InitializeComponent();
        }

        private void precompileInstaller1_DescribeBatch(object sender, DescribeBatchEventArgs e)
        {
            e.Batch
                .For<HomeController>()
                .For<HomeController>().Layout("Ajax").Include("_Notification")
                .For<AccountController>();
        }

        private void precompileInstaller1_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
