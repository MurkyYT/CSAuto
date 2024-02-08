@echo off
rm -r Output
@echo Cleaning csauto project...
msbuild src\CSAuto\CSAuto.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto project
@echo Compiling csauto project...
msbuild CSAuto.sln /t:CSAuto /p:Configuration="Release"
@echo Compiled csauto project
@echo ------------------------------------------------------------------
@echo Cleaning steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned steamapiserver project
@echo Compiling steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /property:Configuration=Release
@echo Compiled steamapiserver project
@echo ------------------------------------------------------------------
@echo Cleaning csauto_mobile project...
msbuild src\CSAuto_Mobile\CSAuto_Mobile.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto_mobile project
@echo Compiling csauto_mobile project...
msbuild  src\CSAuto_Mobile\CSAuto_Mobile.csproj /verbosity:normal /t:Rebuild /t:PackageForAndroid /t:SignAndroidPackage /p:Configuration=Release
@echo Compiled csauto_mobile project
@echo ------------------------------------------------------------------
@echo Cleaning updater project...
msbuild src\Updater\Updater.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned updater project
@echo Compiling updater project...
msbuild src\Updater\Updater.csproj /property:Configuration=Release
@echo Compiled steamapiserver project
@echo ------------------------------------------------------------------
@echo Compiling the installer...
ISCC.exe installer.iss
@echo Compiled the installer
@echo ------------------------------------------------------------------
@echo Copying the apk...
echo f | xcopy /s /y src\CSAuto_Mobile\bin\Release\net7.0-android33.0\com.murky.csauto-Signed.apk Output\CSAuto_Android.apk > nul
@echo Copied csauto_mobile apk
@echo ------------------------------------------------------------------
@echo Zipping csauto...
tar -caf Output\CSAuto_Portable.zip -C src\CSAuto\bin\Release *.dll *.exe resource
@echo Zipped csauto
@echo ------------------------------------------------------------------
@echo Everything should be in the Output folder
@echo ------------------------------------------------------------------
pause