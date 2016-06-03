@echo off
rem Windows XP
if exist %SystemRoot%\Microsoft.NET\Framework\v2.0.50727\csc.exe set net=2
rem Windows 2003
if exist %SystemRoot%\Microsoft.NET\Framework\v3.5\csc.exe set net=3
rem Windows 7+
if exist %SystemRoot%\Microsoft.NET\Framework\v4.0.30319\csc.exe set net=4

rem Replace *** with your domain and GUID of your domain policy
set fld=\\***\SYSVOL\***\Policies\{***}\Machine\Scripts\Startup
set src=%fld%\net%net%\PingDown.exe

if not exist %windir%\system32\PingDown.exe call :pinginstall
call :pingcompare %windir%\system32\PingDown.exe %src%
goto :eof

:pingcompare
if "%~t1"=="%~t2" goto cfg
call :pingremove
call :pinginstall
:cfg
if "%~t1.config"=="%~t2.config" goto eof
call :cfgupdate
goto :eof

:pingremove
net stop PingDown
%windir%\system32\PingDown.exe -u
del %windir%\system32\PingDown.exe
del %windir%\system32\PingDown.exe.config
goto :eof

:pinginstall
copy %src% %windir%\system32
copy %src%.config %windir%\system32
%windir%\system32\PingDown.exe -i
net start PingDown
goto :eof

:cfgupdate
net stop PingDown
copy %src%.config %windir%\system32 >nul
net start PingDown >nul
goto :eof
