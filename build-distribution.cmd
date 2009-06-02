if "%1"=="" build-distribution 1
:start
bin\nant\nant.exe -f:spark.build tools build package -D:build.number=%1
pause
goto start
