using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.VisualStudio.Shell;

namespace SparkVsPackage
{
    public static class Constants
    {
        public const string packageGuidString = "9c109d61-07b1-4d23-a236-a46c699c387e";
        public const string languageGuidString = "1085ff60-19e5-4af2-b454-f10ba0db39e9";
    }

    [ComVisible(true)]
    [ProvideLoadKey("Standard", "1.0", "Spark", "Spark View Engine", 1)]
    [Guid(Constants.packageGuidString)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [DefaultRegistryRoot("Software\\Microsoft\\VisualStudio\\9.0")]
    [ProvideService(typeof(SparkLanguageService), ServiceName = "Spark Language Service")]
    [ProvideLanguageService(typeof(SparkLanguageService), "Spark", 100,
        RequestStockColors = false)]
    [ProvideLanguageExtension(typeof(SparkLanguageService), ".spark")]
    public class SparkPackage : Package
    {
        protected override void Initialize()
        {
            base.Initialize();

            var language = new SparkLanguageService();
            language.SetSite(this);

            var container = (IServiceContainer) this;
            container.AddService(typeof(SparkLanguageService), language, true);
        }
    }
}
