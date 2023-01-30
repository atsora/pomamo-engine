; Properties
!define PACKAGE "PulseTranslator"
!define VERSION "5.0.0.0"
; For PulseBirtReports_2.5.0.0-8

; The name of the installer
Name "${PACKAGE}"

; The file to write
OutFile "${PACKAGE}_${VERSION}.exe"

; The default installation directory
InstallDir $PROGRAMFILES\PulseTranslator

; Registry key to check for directory (so if you install again, it will
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\Lemoine\${PACKAGE}" "InstallDir"

; Directory
DirText "Please choose a directory to which you'd like to install P.U.L.S.E translator"

; Request application privileges for Windows Vista
;RequestExecutionLevel admin

; Use LZMA solid to compress
SetCompressor /SOLID lzma


;-------------------------------

; Pages

;Page instfiles

;UninstPage instfiles

;-------------------------------

; The install section
Section "Install"

  ; Set output path to the installation directory.
  SetOutPath "$INSTDIR"

  ; Put file there
  File "$%PULSE_EXE%\Lem_Translator.exe"
  File "$%PULSE_EXE%\Lemoine.Core.dll"
  File "$%PULSE_EXE%\Lem_Infos.exe"
  #File "$%PULSE_EXE%\Lem_GacInstaller.exe"
  File "$%PULSE_EXE%\PULSEi18n.dll"
  File "..\..\pulse_default.txt"
  File "..\..\..\pulsebirt\pulsereports\pulsereportsi18n.properties"
  File "..\3rdParty\log4net\log4net.dll"
  File "..\3rdParty\DotNetZip\Ionic.Zip.dll"
  File "..\PULSEi18nKey.snk"
  File "..\..\LICENSE.txt"

  ; Registry
  WriteRegStr HKLM "Software\Lemoine\${PACKAGE}" "InstallDir" "$INSTDIR"
  WriteRegStr HKLM "Software\Lemoine\${PACKAGE}" "${PACKAGE}Version" "${VERSION}"

  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PACKAGE}" "DisplayName" "P.U.L.S.E Translator"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PACKAGE}" "UninstallString" '"$INSTDIR\${PACKAGE}Uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PACKAGE}" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PACKAGE}" "No Repair" 1
  WriteUninstaller "${PACKAGE}Uninstall.exe"

  ; Create the short cuts
  CreateDirectory "$SMPROGRAMS\P.U.L.S.E Translator"
  CreateShortCut "$SMPROGRAMS\P.U.L.S.E Translator\Translator.lnk" "$INSTDIR\Lem_Translator.exe" "-r -s"
  CreateShortCut "$SMPROGRAMS\P.U.L.S.E Translator\Uninstall.lnk" "$INSTDIR\${PACKAGE}Uninstall.exe"

SectionEnd

; The uninstaller section
Section "Uninstall"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PACKAGE}"
  
  ; Remove the uninstaller
  Delete "$INSTDIR\${PACKAGE}Uninstall.exe"

  ; Remove the files and directories
  Delete "$INSTDIR\"
  Delete "$INSTDIR\Lem_Translator.exe"
  Delete "$INSTDIR\Lemoine.Core.dll"
  Delete "$INSTDIR\Lemoine.I18N.Resources.dll"
  Delete "$INSTDIR\Lem_Infos.exe"
  Delete "$INSTDIR\Lem_GacInstaller.exe"
  Delete "$INSTDIR\PULSEi18n.dll"
  Delete "$INSTDIR\pulse_default.txt"
  Delete "$INSTDIR\pulsereportsi18n.properties"
  Delete "$INSTDIR\log4net.dll"
  Delete "$INSTDIR\Ionic.Zip.dll"
  Delete "$INSTDIR\PULSEi18nKey.snk"
  Delete "$INSTDIR\LICENSE.txt"
  RMDir "$INSTDIR"

  ; Remove the shortcuts
  Delete "$SMPROGRAMS\P.U.L.S.E Translator\Translator.lnk"
  Delete "$SMPROGRAMS\P.U.L.S.E Translator\Uninstall.lnk"
  RMDir "$SMPROGRAMS\P.U.L.S.E Translator"

  ; Clean the registry
  DeleteRegValue HKLM "Software\Lemoine\${PACKAGE}" "${PACKAGE}Version"
  DeleteRegKey HKLM "Software\Lemoine\${PACKAGE}\InstallDir"
  DeleteRegKey HKLM "Software\Lemoine\${PACKAGE}"

SectionEnd

