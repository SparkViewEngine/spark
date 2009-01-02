:start
svn update
bin\nant\nant.exe -f:spark.build tools build package
pause
goto start
