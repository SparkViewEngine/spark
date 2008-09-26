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

    }
}
