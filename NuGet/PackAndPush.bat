nuget pack Spark\Spark.nuspec
nuget pack Spark.Web.Mvc2\Spark.Web.Mvc2.nuspec
nuget pack Spark.Web.Mvc3\Spark.Web.Mvc3.nuspec
nuget pack Spark.Web.Mvc4\Spark.Web.Mvc4.nuspec
nuget pack Spark.Web.Mvc5\Spark.Web.Mvc5.nuspec

nuget push Spark.1.8.1.0.nupkg
nuget push Spark.Web.Mvc2.1.8.1.0.nupkg
nuget push Spark.Web.Mvc3.1.8.1.0.nupkg
nuget push Spark.Web.Mvc4.1.8.1.0.nupkg
nuget push Spark.Web.Mvc5.1.8.1.0.nupkg

pause