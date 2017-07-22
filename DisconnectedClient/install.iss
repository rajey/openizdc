; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "OpenIZ Disconnected Client"
#define MyAppVersion "0.9.7.2"
#define MyAppPublisher "Mohawk College of Applied Arts and Technology"
#define MyAppURL "http://openiz.org"
#define MyAppExeName "DisconnectedClient.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{6AB504BB-8E44-43E6-A5C7-E033DC8D819B}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={pf}\Mohawk College\OpenIZ\Disconnected Client
DisableProgramGroupPage=yes
LicenseFile=.\License.rtf
OutputDir=.\dist
OutputBaseFilename=openiz-dc-{#MyAppVersion}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: ".\bin\x86\SignedRelease\DisconnectedClient.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\CefSharp.BrowserSubprocess.exe"; DestDir: "{app}"; Flags: ignoreversion
;Source: ".\bin\x86\SignedRelease\sqlite3.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\Applets\org.openiz.core.pak"; DestDir: "{app}\applets"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\fr\OpenIZ.Mobile.Core.resources.dll"; DestDir: "{app}\fr"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\locales\*"; DestDir: "{app}\locales"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: ".\bin\x86\SignedRelease\sw\OpenIZ.Mobile.Core.Xamarin.resources.dll"; DestDir: "{app}\sw"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\Antlr3.Runtime.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\cef.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\cef_100_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\cef_200_percent.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\cef_extensions.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\CefSharp.BrowserSubprocess.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\CefSharp.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\CefSharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\CefSharp.WinForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\chrome_elf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\d3dcompiler_47.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\devtools_resources.pak"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\ExpressionEvaluator.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\icudtl.dat"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\jint.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\libcef.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\libEGL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\libGLESv2.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\MARC.HI.EHRS.SVC.Auditing.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\MohawkCollege.Util.Console.Parameters.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\Mono.Data.Sqlite.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\natives_blob.bin"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.BusinessRules.JavaScript.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Alert.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Applets.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Model.AMI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Model.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Model.RISI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.Model.ViewModelSerializers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Core.PCL.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Messaging.AMI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Messaging.IMSI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Messaging.RISI.Client.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Mobile.Core.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Mobile.Core.Xamarin.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Mobile.Reporting.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\OpenIZ.Protocol.Xml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\snapshot_blob.bin"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\SQLite.Net.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\SQLite.Net.Platform.Generic.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\sqlite3.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\System.Data.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\System.Transactions.Portable.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\widevinecdmadapter.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\sqlcipher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\SQLite.Net.Platform.SqlCipher.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\bin\x86\SignedRelease\libeay32.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\installsupp\vcredist_x86.exe"; DestDir: "{tmp}"; Flags: dontcopy;
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{commonprograms}\Open Immunize\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commonprograms}\Open Immunize\Reset {#MyAppName} Configuration"; Parameters:"--reset"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

[Code]

function PrepareToInstall(var needsRestart:Boolean): String;
var
  hWnd: Integer;
  ResultCode : integer;
  uninstallString : string;
begin
    EnableFsRedirection(true);
    ExtractTemporaryFile('vcredist_x86.exe');
    Exec(ExpandConstant('{tmp}\vcredist_x86.exe'), '/install /passive', '', SW_SHOW, ewWaitUntilTerminated, ResultCode);
end;
