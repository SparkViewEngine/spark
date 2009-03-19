namespace PrecompiledViews
{
    partial class PostBuildStep
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.precompileInstaller1 = new Castle.MonoRail.Views.Spark.Install.PrecompileInstaller();
            // 
            // precompileInstaller1
            // 
            this.precompileInstaller1.TargetAssemblyFile = null;
            this.precompileInstaller1.ViewPath = null;

            // 
            // PostBuildStep
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.precompileInstaller1});

        }

        #endregion

        private Castle.MonoRail.Views.Spark.Install.PrecompileInstaller precompileInstaller1;
    }
}