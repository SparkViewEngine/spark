
::: This go.cmd file assumes you have built the debug version of the xpark project

::: This is an example of piping xml through xpark. Like nunit or nant xml output.
type Spark.Tests.dll-results.xml | ..\xpark testresults.spark >nunit.html

::: This is an example of providing all three template, input, and output names
..\xpark diggstories.spark stories.xml stories.html

::: This is the same example as above that uses the digg api url as input
: ..\xpark diggstories.spark "http://services.digg.com/stories?count=10&appkey=http%3A%2F%2Fsparkviewengine.com%2fxpark" stories.html

::: Now we kick off some tabs in the default browser to view the results
start nunit.html
start stories.html

