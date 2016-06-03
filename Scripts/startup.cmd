@echo off
rem goto common

rem Place here some other domain startup works

:common
call "%~dp0PingDownUpdate.cmd"
if exist "%~dp0%computername%.cmd" call "%~dp0%computername%.cmd"
goto :eof
