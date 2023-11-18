!define APP_NAME "jammer"
!define EXE_NAME "jammer.exe"

; Request administrative privileges
RequestExecutionLevel admin

; Set the output folder for the installer
Outfile "jammer-Setup.exe"

; Default section
Section

; Set output path to install "jammer" folder
SetOutPath "$PROGRAMFILES\${APP_NAME}"

; Create "jammer" folder
CreateDirectory "$PROGRAMFILES\${APP_NAME}"

; Copy files to the installation directory
File /r "nsis-folder\*.*"

; Copy the icon file to the installation directory
File "jammer_1024px.ico"

; Write uninstaller
WriteUninstaller "$PROGRAMFILES\${APP_NAME}\Uninstall.exe"

; Create shortcut to sendto folder in AppData\Roaming\Microsoft\Windows\SendTo
CreateShortCut "$SENDTO\${APP_NAME}.lnk" "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}" "" "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico" 0

; CREATE SHORTCUT TO DESKTOP
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}" "" "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico" 0

SectionEnd

Section "Uninstall"

    ; Remove files
    Delete "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}"
    Delete "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico"
    Delete "$PROGRAMFILES\${APP_NAME}\jammer.ico"
    Delete "$PROGRAMFILES\${APP_NAME}\Uninstall.exe"
    Delete "$PROGRAMFILES\${APP_NAME}\selfdestruct.bat"

    ; Remove shortcut
    Delete "$SENDTO\${APP_NAME}.lnk"
    Delete "$DESKTOP\${APP_NAME}.lnk"

    ; Remove directories
    RMDir "$PROGRAMFILES\${APP_NAME}"

SectionEnd