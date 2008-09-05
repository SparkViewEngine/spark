using System.ComponentModel;
using System.Configuration.Install;
using PrecompiledViews.Controllers;
using Spark.Web.Mvc.Install;


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

        private void precompileInstaller1_AfterInstall_1(object sender, InstallEventArgs e)
        {

        }
    }
}
