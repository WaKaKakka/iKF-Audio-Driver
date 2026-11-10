#define MyAppName "iKF Audio"
#define MyAppVersion "1.1.10.1-fixed"
#define MyAppPublisher "iKF Audio"
#ifndef PayloadDir
#define PayloadDir "payload\app"
#endif

[Setup]
AppId={{9D97C64D-4B87-4E16-8C44-1A2B3C4D5E6F}}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\iKF Audio
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir=..\dist
OutputBaseFilename=iKF_Audio_修正版_安装包
SetupIconFile=..\src\app.ico
UninstallDisplayIcon={app}\UI\XearONE.exe
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
VersionInfoVersion=1.1.10.1
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=iKF Audio driver and XearONE UI (device-scan startup fix)

[Files]
; Payload is the complete, legally obtained runtime tree (vendor DLLs, APO driver SYS/CAT/INF,
; resources and SDK) with XearONE.exe replaced by the fixed build. It lives in installer\payload\app.
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\UI\XearONE.exe"; WorkingDir: "{app}\UI"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\UI\XearONE.exe"; WorkingDir: "{app}\UI"

[Run]
Filename: "{app}\ApoDriver\CmApoDriverSetup.exe"; Parameters: "/install"; WorkingDir: "{app}\ApoDriver"; Flags: runhidden waituntilterminated
Filename: "{app}\Program\CmDeviceCheck.exe"; WorkingDir: "{app}\Program"; Flags: runhidden waituntilterminated
Filename: "{app}\UI\XearONE.exe"; WorkingDir: "{app}\UI"; Description: "启动 iKF Audio"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{app}\ApoDriver\CmApoDriverSetup.exe"; Parameters: "/uninstall"; WorkingDir: "{app}\ApoDriver"; Flags: runhidden waituntilterminated; RunOnceId: "RemoveApoDriver"
