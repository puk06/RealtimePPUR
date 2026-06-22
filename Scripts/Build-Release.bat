set ProjectDirectory=%~dp0../RealtimePPUR/
set ProjectFile=%ProjectDirectory%RealtimePPUR.csproj
set BuildTargetDirectory=%ProjectDirectory%bin/build

@REM Remove built files in target directory
rmdir "%BuildTargetDirectory%" /s /q

@REM Build
dotnet publish "%ProjectFile%" -c Release -r win-x64 -o "%BuildTargetDirectory%"

@REM Remove Unnecessary Files
node RemoveUnnecessaryFiles.js "%BuildTargetDirectory%"
