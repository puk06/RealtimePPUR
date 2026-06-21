const fs = require("fs");
const path = require("path");

const REQUIRED_LIBRARIES = [
    "Avalonia.Controls.dll",
    "Avalonia.Base.dll",
    "Avalonia.Desktop.dll",
    "Avalonia.Markup.Xaml.dll",
    "Avalonia.Themes.Fluent.dll",
    "Avalonia.Dialogs.dll",
    "Avalonia.Win32.Automation.dll",
    "Avalonia.Markup.dll",
    "Avalonia.Fonts.Inter.dll",
    "Avalonia.HarfBuzz.dll",
    "Avalonia.Win32.dll",
    "MicroCom.Runtime.dll",
    "Avalonia.Skia.dll",
    "Avalonia.OpenGL.dll",
    "Avalonia.Vulkan.dll",
    "Avalonia.MicroCom.dll",
    "Avalonia.Metal.dll",
    "osu.Game.dll",
    "osu.Framework.dll",
    "osu.Game.Rulesets.Catch.dll",
    "osu.Game.Rulesets.Mania.dll",
    "osu.Game.Rulesets.Osu.dll",
    "osu.Game.Rulesets.Taiko.dll",
    "Realm.dll",
    "OsuMemoryDataProvider.dll",
    "ProcessMemoryDataFinder.dll",
    "HarfBuzzSharp.dll",
    "osuTK.dll",
    "Newtonsoft.Json.dll",
    "ppy.ManagedBass.dll",
    "AutoMapper.dll",
    "MessagePack.Annotations.dll",
    "nunit.framework.dll",
    "RealtimePPUR.deps.json",
    "RealtimePPUR.dll",
    "RealtimePPUR.exe",
    "RealtimePPUR.runtimeconfig.json"
]

const BUILD_FOLDER = "../../build";

if (!fs.existsSync(BUILD_FOLDER)) {
    fs.mkdirSync(BUILD_FOLDER);
} else {
    const BUILD_FOLDER_CONTENTS = fs.readdirSync(BUILD_FOLDER);
    for (const file of BUILD_FOLDER_CONTENTS) {
        const filePath = path.join(BUILD_FOLDER, file);
        console.log(`Deleting ${filePath}`);
        const buildFileStat = fs.statSync(filePath);
        if (buildFileStat.isDirectory()) {
            fs.rmSync(filePath, { recursive: true, force: true });
        } else {
            fs.unlinkSync(filePath);
        }
    }
}

for (const file of REQUIRED_LIBRARIES) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

const fileList = fs.readdirSync(".");
for (const file of fileList) {
    const filePath = path.join(".", file);
    const stat = fs.statSync(filePath);

    if (stat.isDirectory()) {
        console.log(`Deleting folder ${filePath}`);
        fs.rmSync(filePath, { recursive: true, force: true });
    } else {
        console.log(`Deleting file ${filePath}`);
        fs.unlinkSync(filePath);
    }
}

console.log("Build completed.");
