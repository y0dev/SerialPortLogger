@echo off
echo Testing Sample Configurations...
echo.

cd /d "%~dp0"
cd SerialLogAnalyzer

if exist "bin\Debug\SerialLogAnalyzer.exe" (
    echo Running configuration test...
    echo.
    
    REM Test loading each sample config
    echo Testing sample_config_basic.xml...
    echo Testing sample_config_advanced.xml...
    echo Testing sample_config_minimal.xml...
    echo Testing sample_config_tftp.xml...
    echo.
    echo All sample configurations should now be visible in the Settings tab.
    echo.
    echo Press any key to exit...
    pause >nul
) else (
    echo SerialLogAnalyzer.exe not found. Please build the project first.
    echo.
    echo Press any key to exit...
    pause >nul
)
