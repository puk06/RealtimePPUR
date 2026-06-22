set ProjectDirectory=%~dp0../RealtimePPUR/
set ProjectFile=%ProjectDirectory%RealtimePPUR.csproj
set BuildTargetDirectory=%ProjectDirectory%bin/build

@REM # BUILD
dotnet publish "%ProjectFile%" -c Release -r win-x64 -o "%BuildTargetDirectory%"

node RemoveUnnecessaryFiles.js "%BuildTargetDirectory%"
