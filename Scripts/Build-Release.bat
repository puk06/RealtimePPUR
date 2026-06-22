set ScriptDirectory=%~dp0
set ProjectRootDirectory=%ScriptDirectory%..\
set ProjectDirectory=%ProjectRootDirectory%RealtimePPUR\
set ProjectFile=%ProjectDirectory%RealtimePPUR.csproj
set BuildTargetDirectory=%ProjectDirectory%bin\build

@REM Remove built files in target directory
rmdir "%BuildTargetDirectory%" /s /q

@REM Build
dotnet publish "%ProjectFile%" -c Release -r win-x64 -o "%BuildTargetDirectory%"

@REM Remove Unnecessary Files
node "%ScriptDirectory%RemoveUnnecessaryFiles.js" "%BuildTargetDirectory%"

@REM Copy Licenses
copy "%ProjectRootDirectory%LICENSE" "%BuildTargetDirectory%\LICENSE"
copy "%ProjectRootDirectory%THIRD_PARTY_LICENSES" "%BuildTargetDirectory%\THIRD_PARTY_LICENSES"
