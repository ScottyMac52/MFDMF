; ==================== installer/installer.iss ====================
; Build with:
;   ISCC.exe ^
;     "/DPublishDir=<dotnet publish output>" ^
;     "/DSetupAux=installer" ^
;     "/DAppVersion=<tag or version>" ^
;     "installer\installer.iss"

#ifndef PublishDir
  #error PublishDir not defined. Call ISCC with /DPublishDir=...
#endif
#ifndef SetupAux
  #define SetupAux "installer"
#endif
#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

[Setup]
AppName=MFDMF
AppVersion={#AppVersion}
DefaultDirName={autopf}\Vyper Industries\MFDMF
DefaultGroupName=MFDMF
PrivilegesRequired=admin
OutputBaseFilename=MFDMF-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
DisableDirPage=no
DisableProgramGroupPage=yes

[Files]
; Main payload from dotnet publish
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Always deploy appsettings.json to the user config folder
Source: "{#SetupAux}\appsettings.json"; DestDir: "{code:GetConfigDir}"; Flags: ignoreversion

; Modules JSON:
; - normal case (no /skipModules): allow overwrite
Source: "{#SetupAux}\Modules\*.json"; DestDir: "{app}\Modules"; Flags: ignoreversion recursesubdirs; Check: ShouldInstallModulesRegular
; - with /skipModules: only install files that don't exist yet
Source: "{#SetupAux}\Modules\*.json"; DestDir: "{app}\Modules"; Flags: ignoreversion recursesubdirs onlyifdoesntexist; Check: ShouldInstallModulesOnlyIfMissing

[Dirs]
; Ensure Modules exists even if no JSONs are shipped
Name: "{app}\Modules"; Flags: uninsalwaysuninstall

[Icons]
Name: "{group}\MFDMF"; Filename: "{app}\MFDMFApp.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\MFDMF"; Filename: "{app}\MFDMFApp.exe"; Tasks: desktopicon; WorkingDir: "{app}"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Code]
var
  GSkipModules: Boolean;
  GDetectPath: string;
  GDetectFound: Boolean;

function BoolToText(B: Boolean): string;
begin
  if B then Result := 'true' else Result := 'false';
end;

function HasSwitch(const Switch: string): Boolean;
var s: string;
begin
  s := ' ' + LowerCase(GetCmdTail) + ' ';
  Result := (Pos(' /' + LowerCase(Switch) + ' ', s) > 0) or
            (Pos(' -' + LowerCase(Switch) + ' ', s) > 0);
end;

function GetParamValue(const Name, DefaultValue: string): string;
begin
  Result := ExpandConstant('{param:' + Name + '|' + DefaultValue + '}');
end;

function GetConfigDir(Param: string): string;
begin
  // No WinAPI—simple, reliable path on Win10/11:
  Result := ExpandConstant('{userprofile}\Saved Games\Vyper Industries\MFDMF\Config');
end;

function ShouldInstallModulesRegular(): Boolean;
begin
  Result := not GSkipModules;
end;

function ShouldInstallModulesOnlyIfMissing(): Boolean;
begin
  Result := GSkipModules;
end;

procedure InitializeDefaults();
begin
  GSkipModules := HasSwitch('skipModules');

  GDetectPath := GetParamValue('detect', '');
  if GDetectPath <> '' then
    GDetectFound := FileExists(GDetectPath)
  else
    GDetectFound := False;

  if GDetectPath <> '' then
    Log('Detect file check: ' + GDetectPath + '  exists=' + BoolToText(GDetectFound))
  else
    Log('Detect file check: /detect not supplied (skipping)');
end;

function InitializeSetup(): Boolean;
begin
  InitializeDefaults();
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssInstall then
    ForceDirectories(GetConfigDir(''));
end;
// ==================== end ====================


