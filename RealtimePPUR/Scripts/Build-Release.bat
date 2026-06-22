set ProjectDirectory=%~dp0../

@REM # BUILD
dotnet publish "%ProjectDirectory%RealtimePPUR.csproj" -c Release -r win-x64 -o "%ProjectDirectory%bin/build"

node RemoveUnnecessaryFiles.js "%ProjectDirectory%bin/build"
