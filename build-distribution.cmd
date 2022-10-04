if "%1"=="" build-distribution 1
:start
REM Assumes nant is installed and in your path 
nant.exe -f:spark.build build package push -D:build.number=%1
pause
goto start
