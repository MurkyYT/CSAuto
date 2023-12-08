@echo OFF
rm -r Output
@echo Cleaning csauto project...
msbuild src\CSAuto\CSAuto.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto project
@echo Compiling csauto project...
msbuild src\CSAuto\CSAuto.csproj /property:Configuration=Release > nul
@echo Compiled csauto project
ConfuserEx\confuser.cli -n CSAuto.crproj > nul
echo f | xcopy /s /y src\APIKeys\bin\Release\Confused\APIKeys.dll src\CSAuto\bin\Release\APIKeys.dll> nul
@echo confused APIKeys
@echo ------------------------------------------------------------------
@echo Cleaning steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned steamapiserver project
@echo Compiling steamapiserver project...
msbuild src\SteamAPIServer\SteamAPIServer.csproj /property:Configuration=Release > nul
@echo Compiled steamapiserver project
@echo ------------------------------------------------------------------
@echo Cleaning csauto_mobile project...
msbuild src\CSAuto_Mobile\CSAuto_Mobile.csproj /t:Clean /property:Configuration=Release > nul
@echo Cleaned csauto_mobile project
@echo Compiling csauto_mobile project...
msbuild  src\CSAuto_Mobile\CSAuto_Mobile.csproj /verbosity:normal /t:Rebuild /t:PackageForAndroid /t:SignAndroidPackage /p:Configuration=Release > nul
@echo Compiled csauto_mobile project
@echo ------------------------------------------------------------------
set "version="
for /F "skip=80 delims=" %%i in (src\CSAuto\MainApp.xaml.cs) do (if not defined version (set "version=%%i" & goto split))
:split
FOR /f "tokens=1,2 delims='" %%a IN ("%version:"='%") do set "version=%%b" & goto compile
:compile
@echo Compiling the installer...
ISCC.exe installer.iss /DVERSION_NAME=%version%> nul
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