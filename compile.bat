@echo off
setlocal
call :setESC
title Compiling CSAuto...
del /f /s /q Output
rmdir Output

@echo Cleaning steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned steamapiserver project
@echo Compiling steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /property:Configuration=Release /maxcpucount 
@if errorlevel 1 echo %ESC%[91mERROR: Error when building SteamAPI%ESC%[0m && pause && exit
@echo %ESC%[92mSUCCESS: Compiled SteamAPI project%ESC%[0m
@echo ------------------------------------------------------------------
@echo Cleaning csauto_mobile project...
msbuild src\CSAuto_Mobile\CSAuto_Mobile.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto_mobile project
@echo Compiling csauto_mobile project...
msbuild  src\CSAuto_Mobile\CSAuto_Mobile.csproj /verbosity:normal /t:Rebuild /t:PackageForAndroid /t:SignAndroidPackage /p:Configuration=Release /maxcpucount 
@if errorlevel 1 echo %ESC%[91mERROR: Error when building CSAuto_Mobile%ESC%[0m && pause && exit
@echo %ESC%[92mSUCCESS: Compiled CSAuto_Mobile project%ESC%[0m
@echo ------------------------------------------------------------------
@echo Cleaning updater project...
msbuild src\Updater\Updater.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned updater project
@echo Compiling updater project...
msbuild src\Updater\Updater.csproj /property:Configuration=Release /maxcpucount 
@if errorlevel 1 echo %ESC%[91mERROR: Error when building Updater%ESC%[0m && pause && exit
@echo %ESC%[92mSUCCESS: Compiled Updater project%ESC%[0m
@echo ------------------------------------------------------------------
@echo Cleaning csauto project...
msbuild src\CSAuto\CSAuto.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto project
@echo Compiling csauto project...
msbuild CSAuto.sln /t:CSAuto /p:Configuration="Release" /maxcpucount 
@if errorlevel 1 echo %ESC%[91mERROR: Error when building CSAuto%ESC%[0m && pause && exit
@echo %ESC%[92mSUCCESS: Compiled CSAuto project%ESC%[0m
@echo ------------------------------------------------------------------
@echo Compiling the installer...
ISCC.exe installer.iss
@if errorlevel 1 echo %ESC%[91mERROR: Error when compiling installer%ESC%[0m && pause && exit
@echo %ESC%[92mSUCCESS: Compiled the installer%ESC%[0m
@echo ------------------------------------------------------------------
@echo Copying the apk...
echo f | xcopy /s /y src\CSAuto_Mobile\bin\Release\net8.0-android34.0\com.murky.csauto-Signed.apk Output\CSAuto_Android.apk > nul
@echo Copied csauto_mobile apk
@echo ------------------------------------------------------------------
@echo Zipping csauto...
tar -caf Output\CSAuto_Portable.zip -C src\CSAuto\bin\Release *.exe resource bin
@echo Zipped csauto
@echo %ESC%[92m------------------------------------------------------------------
@echo Everything should be in the Output folder
@echo ------------------------------------------------------------------%ESC%[0m
pause

:setESC
for /F "tokens=1,2 delims=#" %%a in ('"prompt #$H#$E# & echo on & for %%b in (1) do rem"') do (
  set ESC=%%b
  exit /B 0
)