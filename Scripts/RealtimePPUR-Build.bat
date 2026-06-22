@echo off
REM Clone the repository with submodules
git clone --recursive "https://github.com/puk06/RealtimePPUR.git"
cd RealtimePPUR

REM Add new osu submodule
if "%1" == "" goto skip
git submodule deinit -f osu
git rm -f osu
rmdir /s /q .git\modules\osu
git submodule add %1

if "%2" == "" goto skip
cd osu
git checkout %2
cd ..

:skip
REM Build the project
Scripts/Build-Release.bat
