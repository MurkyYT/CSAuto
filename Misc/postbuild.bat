@echo off
setlocal EnableDelayedExpansion

if "%~1"=="" (
    exit /b 1
)

set "SEARCH_PATH=%~1"
shift

set "SKIP_LIST="
:build_skip
if "%~1"=="" goto start
set "SKIP_LIST=!SKIP_LIST! %~1"
shift
goto build_skip

:start

for %%F in ("%SEARCH_PATH%\*.dll") do (
    set "SKIP=0"
    for %%S in (!SKIP_LIST!) do (
        if /I "%%~nxF"=="%%S" set "SKIP=1"
    )

    if "!SKIP!"=="1" (
        echo [KEEP]   %%~nxF
    ) else (
        echo [DELETE] %%~nxF
        del /F /Q "%%F"
    )
)

endlocal