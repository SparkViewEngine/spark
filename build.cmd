:start
bin\nant\nant.exe -f:spark.build build package
:%systemroot%\microsoft.net\framework\v3.5\msbuild.exe /t:Rebuild /property:Configuration=Debug;OutDir=../../build/tests/ src/Spark.sln
:%systemroot%\microsoft.net\framework\v3.5\msbuild.exe /t:Rebuild /property:Configuration=Release;OutDir=../../build/spark/ src/Spark/Spark.csproj
:%systemroot%\microsoft.net\framework\v3.5\msbuild.exe /t:Rebuild /property:Configuration=Release;OutDir=../../build/castle/ src/Castle.MonoRail.Views.Spark/Castle.MonoRail.Views.Spark.csproj
:%systemroot%\microsoft.net\framework\v3.5\msbuild.exe /t:Rebuild /property:Configuration=Release;OutDir=../../build/aspnetmvc/ src/MvcContrib.SparkViewEngine/MvcContrib.SparkViewEngine.csproj
:%systemroot%\microsoft.net\framework\v3.5\msbuild.exe /t:Rebuild;_CopyWebApplication /property:Configuration=Release;OutDir=../../build/samples/NorthwindDemo/bin/;WebProjectOutputDir=../../build/samples/NorthwindDemo/ src/NorthwindDemo/NorthwindDemo.csproj
pause
:goto start
