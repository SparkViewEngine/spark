nuget pack Spark\Spark.nuspec
nuget pack Spark.Web.Mvc2\Spark.Web.Mvc2.nuspec
nuget pack Spark.Web.Mvc3\Spark.Web.Mvc3.nuspec

nuget push Spark.1.7.2.0.nupkg
nuget push Spark.Web.Mvc2.1.7.2.0.nupkg
nuget push Spark.Web.Mvc3.1.7.2.0.nupkg

pause