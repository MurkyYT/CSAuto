@echo off
@echo Cleaning csauto project...
msbuild CSAuto\CSAuto.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto project
@echo Compiling csauto project...
msbuild CSAuto\CSAuto.csproj /property:Configuration=Release > nul
@echo Compiled csauto project
ConfuserEx\confuser.cli -n CSAuto.crproj > nul
echo f | xcopy /s /y CSAuto\bin\Release\Confused\CSAuto.exe CSAuto\bin\Release\CSAuto.exe > nul
@echo confused CSAuto.exe
@echo ------------------------------------------------------------------
@echo Cleaning steamapiserver project...
msbuild SteamAPIServer\SteamAPIServer.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned steamapiserver project
@echo Compiling steamapiserver project...
msbuild SteamAPIServer\SteamAPIServer.csproj /property:Configuration=Release > nul
@echo Compiled steamapiserver project
@echo ------------------------------------------------------------------
@echo Cleaning csauto_mobile project...
msbuild CSAuto_Mobile\CSAuto_Mobile.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto_mobile project
@echo Compiling csauto_mobile project...
msbuild  CSAuto_Mobile\CSAuto_Mobile.csproj /verbosity:normal /t:Rebuild /t:PackageForAndroid /t:SignAndroidPackage /p:Configuration=Release > nul
@echo Compiled csauto_mobile project
@echo ------------------------------------------------------------------
@echo Compiling the installer...
ISCC.exe installer.iss > nul
@echo Compiled the installer
@echo ------------------------------------------------------------------
@echo Copying the apk...
echo f | xcopy /s /y CSAuto_Mobile\bin\Release\com.murky.csauto_mobile-Signed.apk Output\CSAuto_Android.apk > nul
@echo Copied csauto_mobile apk
@echo ------------------------------------------------------------------
@echo Zipping csauto...
tar -caf Output\CSAuto_Portable.zip -C CSAuto\bin\Release *.dll *.exe > nul
@echo Zipped csauto
@echo ------------------------------------------------------------------
@echo Everything should be in the Output folder
@echo ------------------------------------------------------------------
pause