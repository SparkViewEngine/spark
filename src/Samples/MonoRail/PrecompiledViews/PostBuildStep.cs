using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;


namespace PrecompiledViews
{
    [RunInstaller(true)]
    public partial class PostBuildStep : Installer
    {
        public PostBuildStep()
        {
            InitializeComponent();
        }

        private void precompileInstaller1_DescribeBatch(object sender, Castle.MonoRail.Views.Spark.Install.DescribeBatchEventArgs e)
        {
            Global.FillBatchDescriptor(e.Batch);
        }
    }
}
