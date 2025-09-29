; ==================== installer/installer.iss ====================

; These are supplied at build time by ISCC command line:
;   /DPublishDir=...   → the dotnet publish output folder
;   /DSetupAux=...     → repo folder containing appsettings.json and Modules (usually "installer")
;   /DAppVersion=...   → tag or version string to show and to name the .exe
#ifndef PublishDir
  #error PublishDir not defined. Call ISCC with /DPublishDir=...
#endif
#ifndef SetupAux
  #define SetupAux "."
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
; Main payload: everything that dotnet publish produced
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

; Always place appsettings.json in the user-writable Saved Games config directory
Source: "{#SetupAux}\appsettings.json"; DestDir: "{code:GetConfigDir}"; Flags: ignoreversion

; Modules JSON rules:
;  - normal case (no /skipModules): allow overwrite
Source: "{#SetupAux}\Modules\*.json"; DestDir: "{app}\Modules"; Flags: ignoreversion recursesubdirs; Check: ShouldInstallModulesRegular
;  - /skipModules case: only install JSON files that don't exist yet
Source: "{#SetupAux}\Modules\*.json"; DestDir: "{app}\Modules"; Flags: ignoreversion recursesubdirs onlyifdoesntexist; Check: ShouldInstallModulesOnlyIfMissing

[Dirs]
; Ensure Modules folder exists even if repo doesn't include any .json files
Name: "{app}\Modules"; Flags: uninsalwaysuninstall

[Icons]
Name: "{group}\MFDMF"; Filename: "{app}\MFDMFApp.exe"; WorkingDir: "{app}"
Name: "{commondesktop}\MFDMF"; Filename: "{app}\MFDMFApp.exe"; Tasks: desktopicon; WorkingDir: "{app}"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: unchecked

[Code]
const
  KF_FLAG_DEFAULT = $00000000;
  // FOLDERID_SavedGames ({4C5C32FF-BB9D-43B0-B5B4-BD0E116F75A8})
  FOLDERID_SavedGames_S = '{4C5C32FF-BB9D-43B0-B5B4-BD0E116F75A8}';

type
  THandle = LongWord;
  HRESULT = LongInt;
  PPWideChar = ^PWideChar;

function SHGetKnownFolderPath(const rfid: TGUID; dwFlags: DWORD; hToken: THandle; var ppszPath: PWideChar): HRESULT;
  external 'SHGetKnownFolderPath@Shell32.dll stdcall';
procedure CoTaskMemFree(pv: Pointer);
  external 'CoTaskMemFree@ole32.dll stdcall';

var
  GSkipModules: Boolean;
  GDetectPath: string;
  GDetectFound: Boolean;

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

function GetSavedGamesFolder(): string;
var
  p: PWideChar;
  guid: TGUID;
begin
  p := nil;
  try
    guid := StringToGUID(FOLDERID_SavedGames_S);
    if SHGetKnownFolderPath(guid, KF_FLAG_DEFAULT, 0, p) = 0 then
    begin
      Result := p;
      CoTaskMemFree(p);
    end
    else
      Result := ExpandConstant('{userprofile}\Saved Games');
  except
    // safety fallback if StringToGUID or the API throws
    Result := ExpandConstant('{userprofile}\Saved Games');
  end;
end;

function GetConfigDir(Param: string): string;
begin
  Result := AddBackslash(GetSavedGamesFolder) + 'Vyper Industries\MFDMF\Config';
end;

function ShouldInstallModulesRegular(): Boolean; begin Result := not GSkipModules; end;
function ShouldInstallModulesOnlyIfMissing(): Boolean; begin Result := GSkipModules; end;

procedure InitializeDefaults();
begin
  GSkipModules := HasSwitch('skipModules');
  GDetectPath := GetParamValue('detect', '');
  if GDetectPath <> '' then
    GDetectFound := FileExists(GDetectPath)
  else
    GDetectFound := False;

  if GDetectPath <> '' then
    Log(Format('Detect file check: %s  exists=%s', [GDetectPath, GetBOOLStr(GDetectFound)]))
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
; ==================== end installer/installer.iss ====================

