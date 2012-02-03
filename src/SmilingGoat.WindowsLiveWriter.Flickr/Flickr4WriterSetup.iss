[Setup]
AppName=Flickr4Writer
AppVerName=Flickr4Writer
AppPublisher=flickr4writer.com
AppPublisherURL=http://www.flickr4writer.com
AppSupportURL=http://www.timheuer.com
AppUpdatesURL=http://www.flickr4writer.com
OutputDir=C:\Users\timheuer\Documents\Visual Studio 2005\Projects\SmilingGoat.WindowsLiveWriter.Flickr\Flickr4WriterSetup
OutputBaseFilename=Flickr4WriterSetup
SolidCompression=true
MinVersion=0,5.01.2600sp2
AppVersion=1.1.60817.1
InfoAfterFile=C:\Users\timheuer\Documents\Visual Studio 2005\Projects\SmilingGoat.WindowsLiveWriter.Flickr\Flickr4WriterSetup\readme.rtf
LicenseFile=C:\Users\timheuer\Documents\Visual Studio 2005\Projects\SmilingGoat.WindowsLiveWriter.Flickr\Flickr4WriterSetup\license.rtf
ShowLanguageDialog=no
AppContact=tim@timheuer.com
UninstallDisplayName=Flickr4Writer - a Windows Live Writer Plugin
AllowUNCPath=false
CreateAppDir=true
AppCopyright=Copyright © 2006 timheuer.com
UsePreviousAppDir=false
UsePreviousGroup=false
AppendDefaultDirName=false
AppendDefaultGroupName=false
DefaultDirName={pf}\Windows Live Writer\Plugins
EnableDirDoesntExistWarning=true
DirExistsWarning=no
DisableProgramGroupPage=true

[Languages]
Name: en; MessagesFile: compiler:Default.isl; LicenseFile: C:\Users\timheuer\Documents\Visual Studio 2005\Projects\SmilingGoat.WindowsLiveWriter.Flickr\Flickr4WriterSetup\license.rtf; InfoAfterFile: C:\Users\timheuer\Documents\Visual Studio 2005\Projects\SmilingGoat.WindowsLiveWriter.Flickr\Flickr4WriterSetup\readme.rtf

[Files]
Source: bin\Release\FlickrNet.dll; DestDir: {pf}\Windows Live Writer\Plugins; Flags: confirmoverwrite
Source: bin\Release\SmilingGoat.WindowsLiveWriter.Flickr.dll; DestDir: {pf}\Windows Live Writer\Plugins; Flags: confirmoverwrite
Source: ..\..\..\..\..\..\..\Program Files\Download DLL\isxdl.dll; Flags: dontcopy
Source: ..\..\..\..\..\..\..\Program Files\Windows Live Writer\WindowsLiveWriter.exe.config; DestDir: {pf}\Windows Live Writer; Flags: onlyifdoesntexist
[Code]
var
  dotnetRedistPath: string;
  downloadNeeded: boolean;
  dotNetNeeded: boolean;
  memoDependenciesNeeded: string;

procedure isxdl_AddFile(URL, Filename: PChar);
external 'isxdl_AddFile@files:isxdl.dll stdcall';
function isxdl_DownloadFiles(hWnd: Integer): Integer;
external 'isxdl_DownloadFiles@files:isxdl.dll stdcall';
function isxdl_SetOption(Option, Value: PChar): Integer;
external 'isxdl_SetOption@files:isxdl.dll stdcall';


const
  dotnetRedistURL = 'http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe';

function InitializeSetup(): Boolean;

begin
  Result := true;
  dotNetNeeded := false;

  // Check for required netfx installation
  if (not RegKeyExists(HKLM, 'Software\Microsoft\.NETFramework\policy\v2.0')) then begin
    dotNetNeeded := true;
    if (not IsAdminLoggedOn()) then begin
      MsgBox('This plugin requires the .NET Framework 2.0 to be installed on the machine which requires an Administrator.', mbInformation, MB_OK);
      Result := false;
    end else begin
      memoDependenciesNeeded := memoDependenciesNeeded + '      .NET Framework 2.0' #13;
      dotnetRedistPath := ExpandConstant('{src}\dotnetfx.exe');
      if not FileExists(dotnetRedistPath) then begin
        dotnetRedistPath := ExpandConstant('{tmp}\dotnetfx.exe');
        if not FileExists(dotnetRedistPath) then begin
          isxdl_AddFile(dotnetRedistURL, dotnetRedistPath);
          downloadNeeded := true;
        end;
      end;
      SetIniString('install', 'dotnetRedist', dotnetRedistPath, ExpandConstant('{tmp}\dep.ini'));
    end;
  end;

end;

function NextButtonClick(CurPage: Integer): Boolean;
var
  hWnd: Integer;
  ResultCode: Integer;

begin
  Result := true;

  if CurPage = wpReady then begin

    hWnd := StrToInt(ExpandConstant('{wizardhwnd}'));

    // don't try to init isxdl if it's not needed because it will error on < ie 3
    if downloadNeeded then begin

      isxdl_SetOption('label', 'Downloading Microsoft .NET Framework');
      isxdl_SetOption('description', 'MyApp needs to install the Microsoft .NET Framework. Please wait while Setup is downloading extra files to your computer.');
      if isxdl_DownloadFiles(hWnd) = 0 then Result := false;
    end;
    if (Result = true) and (dotNetNeeded = true) then begin
      if Exec(ExpandConstant(dotnetRedistPath), '', '', SW_SHOW, ewWaitUntilTerminated, ResultCode) then begin
         // handle success if necessary; ResultCode contains the exit code
         if not (ResultCode = 0) then begin
           Result := false;
         end;
      end else begin
         // handle failure if necessary; ResultCode contains the error code
         Result := false;
      end;
    end;
  end;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  s: string;

begin
  if memoDependenciesNeeded <> '' then s := s + 'The following dependencies are required to install and will be downloaded/installed.' + NewLine + NewLine + 'Click cancel to quit the installation:' + NewLine + memoDependenciesNeeded + NewLine;
  s := s + MemoDirInfo + NewLine + NewLine;

  Result := s
end;
[Messages]
SelectDirLabel3=Setup will install [name] into the following folder.%n%nNOTE: The default location is below.  If you installed Windows Live Writer in the default location, do not change this setting.
WelcomeLabel2=This will install [name] - a plugin for Windows Live Writer on your computer.%n%nIt is recommended that you close all other applications before continuing.  Please ensure Windows Live Writer is closed before continuing.
