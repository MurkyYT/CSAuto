@echo off
rm -r Output
@echo Cleaning csauto project...
msbuild src\CSAuto\CSAuto.csproj /t:Clean /property:Configuration=Release
@echo Cleaned csauto project
@echo Compiling csauto project...
msbuild CSAuto.sln /t:CSAuto /p:Configuration="Release"
@echo Compiled csauto project
@echo ------------------------------------------------------------------
@echo Cleaning steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /t:Clean /property:Configuration=Release
@echo Cleaned steamapiserver project
@echo Compiling steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /property:Configuration=Release
@echo Compiled steamapiserver project
@echo ------------------------------------------------------------------
@echo Cleaning csauto_mobile project...
msbuild src\CSAuto_Mobile\CSAuto_Mobile.csproj /t:Clean /property:Configuration=Release
@echo Cleaned csauto_mobile project
@echo Compiling csauto_mobile project...
msbuild  src\CSAuto_Mobile\CSAuto_Mobile.csproj /verbosity:normal /t:Rebuild /t:PackageForAndroid /t:SignAndroidPackage /p:Configuration=Release
@echo Compiled csauto_mobile project
@echo ------------------------------------------------------------------
set "version="
for /F "skip=80 delims=" %%i in (src\CSAuto\MainApp.xaml.cs) do (if not defined version (set "version=%%i" & goto split))
:split
FOR /f "tokens=1,2 delims='" %%a IN ("%version:"='%") do set "version=%%b" & goto compile
:compile
@echo Compiling the installer...
ISCC.exe installer.iss /DVERSION_NAME=%version%
@echo Compiled the installer
@echo ------------------------------------------------------------------
@echo Copying the apk...
echo f | xcopy /s /y src\CSAuto_Mobile\bin\Release\com.murky.csauto_mobile-Signed.apk Output\CSAuto_Android.apk > nul
@echo Copied csauto_mobile apk
@echo ------------------------------------------------------------------
@echo Zipping csauto...
tar -caf Output\CSAuto_Portable.zip -C src\CSAuto\bin\Release *.dll *.exe resource  > nul
@echo Zipped csauto
@echo ------------------------------------------------------------------
@echo Everything should be in the Output folder
@echo ------------------------------------------------------------------
pause