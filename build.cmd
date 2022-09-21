:start
REM Assumes nant is installed and in your path 
nant.exe -f:spark.build build package
pause
:goto start
