@echo off
set BAT=%~dp0
set IRONRUBY_ROOT=%BAT:~0,-4%
set BAT=

%IRONRUBY_ROOT%\bin\ir.exe -AI "%IRONRUBY_ROOT%\lib\IronRuby";"%IRONRUBY_ROOT%\lib\ruby\1.8" %*


