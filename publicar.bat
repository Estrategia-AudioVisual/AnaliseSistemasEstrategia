@echo off
rem Gera o ESTRATEGIA DIAGNOSTICO pronto para levar a outro PC (nao precisa instalar o .NET la).
rem Saida: ..\EstrategiaDiagnostico-dist\EstrategiaDiagnostico.exe + LEIA-ME.txt + LICENSE.txt
rem        + EstrategiaDiagnostico-Setup.exe (instalador), se o Inno Setup 6 estiver instalado
rem Sempre recompila do zero a partir do codigo atual desta pasta.
cd /d "%~dp0"
echo Fechando o EstrategiaDiagnostico se estiver aberto ...
taskkill /f /im EstrategiaDiagnostico.exe >nul 2>&1
echo Gerando EstrategiaDiagnostico.exe a partir do codigo atual ...
rmdir /s /q obj\Release >nul 2>&1
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o "..\EstrategiaDiagnostico-dist" -nologo -v q
if errorlevel 1 (
    echo.
    echo FALHOU. Veja os erros acima. O .exe antigo NAO foi atualizado.
    pause
    exit /b 1
)
copy /y "LEIA-ME.txt" "..\EstrategiaDiagnostico-dist\LEIA-ME.txt" >nul
rem Licenca MIT do RESET EXPLODE (original): o aviso de copyright precisa ir junto com o .exe
copy /y "LICENSE" "..\EstrategiaDiagnostico-dist\LICENSE.txt" >nul
rem Instalador (opcional): winget install JRSoftware.InnoSetup
set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if not exist "%ISCC%" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if exist "%ISCC%" (
    echo Gerando o instalador EstrategiaDiagnostico-Setup.exe ...
    "%ISCC%" /Q /DDistDir="%~dp0..\EstrategiaDiagnostico-dist" "installer\EstrategiaDiagnostico.iss"
    if errorlevel 1 echo FALHOU ao gerar o instalador. O .exe foi gerado normalmente.
) else (
    echo Inno Setup nao encontrado: gerado so o .exe, sem o instalador.
)
echo.
echo Pronto: %~dp0..\EstrategiaDiagnostico-dist
for %%F in ("..\EstrategiaDiagnostico-dist\EstrategiaDiagnostico.exe") do echo Arquivo gerado em %%~tF (confira se e de agora^)
pause
