using System;

namespace Spark.Web
{
    public class ApplicationBaseSparkSettings : SparkSettings
    {
        public override string RootPath => AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
    }
}
