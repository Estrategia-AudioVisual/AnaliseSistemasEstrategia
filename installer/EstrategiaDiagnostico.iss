; Instalador do ESTRATÉGIA DIAGNÓSTICO (Inno Setup 6).
; Empacota o .exe já publicado (arquivo único, com o .NET embutido): o publicar.bat e o GitHub Actions informam a pasta em DistDir.
; Gerar à mão:  ISCC /DDistDir="C:\caminho\EstrategiaDiagnostico-dist" installer\EstrategiaDiagnostico.iss
; O instalador sai na mesma pasta do .exe: EstrategiaDiagnostico-Setup.exe

#ifndef DistDir
  #define DistDir "..\..\EstrategiaDiagnostico-dist"
#endif
#define AppExe "EstrategiaDiagnostico.exe"
#ifndef AppVersion
  #define AppVersion GetVersionNumbersString(AddBackslash(DistDir) + AppExe)
#endif
#define AppName "Estratégia Diagnóstico"
#define RepoUrl "https://github.com/Estrategia-AudioVisual/AnaliseSistemasEstrategia"

[Setup]
; Identifica o app no Windows (atualizar por cima, desinstalar): nunca mude
AppId={{4D3A74AC-8EF1-4836-B6E4-6C8B23EB3E94}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher=Estratégia
AppPublisherURL=https://www.estrategia.com
AppSupportURL={#RepoUrl}
AppUpdatesURL={#RepoUrl}/releases/latest
DefaultDirName={autopf}\Estrategia Diagnostico
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableWelcomePage=no
; O app roda como administrador (sensores de baixo nível): instala para todos, em Arquivos de Programas
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0
OutputDir={#DistDir}
OutputBaseFilename=EstrategiaDiagnostico-Setup
SetupIconFile=..\Assets\app.ico
UninstallDisplayIcon={app}\{#AppExe}
UninstallDisplayName={#AppName}
WizardStyle=modern
WizardImageFile=wizard-large.bmp,wizard-large-2x.bmp
WizardSmallImageFile=wizard-small.bmp,wizard-small-2x.bmp
Compression=lzma2/max
SolidCompression=yes
VersionInfoVersion={#AppVersion}
VersionInfoCompany=Estratégia
VersionInfoProductName={#AppName}
VersionInfoDescription=Instalador do {#AppName}

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#DistDir}\{#AppExe}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\LEIA-ME.txt"; DestDir: "{app}"; Flags: ignoreversion
; Licença MIT do RESET EXPLODE (original): o aviso de copyright vai junto com o programa
Source: "..\LICENSE"; DestDir: "{app}"; DestName: "LICENSE.txt"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Run]
; O instalador já roda como administrador: abre o app direto, sem pedir permissão de novo
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent runascurrentuser
