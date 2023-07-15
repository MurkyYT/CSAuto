@echo off
ISCC.exe installer.iss
echo f | xcopy /s /y CSAuto_Mobile\bin\Release\com.murky.csauto_mobile.apk Output\CSAuto_Android.apk
@echo Copied csauto_mobile apk
tar -caf Output\CSAuto_Portable.zip -C CSAuto\bin\Release *.dll *.exe
@echo Zipped csauto
@echo ------------------------------------------------------------------
@echo Everything should be in Output folder
@echo ------------------------------------------------------------------
pause