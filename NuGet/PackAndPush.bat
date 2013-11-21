nuget pack Spark\Spark.nuspec
nuget pack Spark.Web.Mvc2\Spark.Web.Mvc2.nuspec
nuget pack Spark.Web.Mvc3\Spark.Web.Mvc3.nuspec
nuget pack Spark.Web.Mvc4\Spark.Web.Mvc4.nuspec

nuget push Spark.1.7.5.3.nupkg
nuget push Spark.Web.Mvc2.1.7.5.3.nupkg
nuget push Spark.Web.Mvc3.1.7.5.3.nupkg
nuget push Spark.Web.Mvc4.1.7.5.3.nupkg

pause