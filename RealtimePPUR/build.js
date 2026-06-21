const fs = require("fs");
const path = require("path");

const REQUIRED_LIBRARIES = [
    "Avalonia.Markup.dll",
    "Avalonia.Markup.Xaml.dll",
    "Avalonia.Metal.dll",
    "Avalonia.MicroCom.dll",
    "Avalonia.OpenGL.dll",
    "Avalonia.Skia.dll",
    "Avalonia.Themes.Fluent.dll",
    "Avalonia.Vulkan.dll",
    "Avalonia.Win32.Automation.dll",
    "Avalonia.Win32.dll",
    "HarfBuzzSharp.dll",
    "libHarfBuzzSharp.dll",
    "libSkiaSharp.dll",
    "MessagePack.Annotations.dll",
    "MicroCom.Runtime.dll",
    "Newtonsoft.Json.dll",
    "nunit.framework.dll",
    "osu.Framework.dll",
    "osu.Game.dll",
    "osu.Game.Rulesets.Catch.dll",
    "osu.Game.Rulesets.Mania.dll",
    "osu.Game.Rulesets.Osu.dll",
    "osu.Game.Rulesets.Taiko.dll",
    "OsuMemoryDataProvider.dll",
    "osuTK.dll",
    "ppy.ManagedBass.dll",
    "ProcessMemoryDataFinder.dll",
    "Realm.dll",
    "RealtimePPUR.dll",
    "RealtimePPUR.exe",
    "SkiaSharp.dll",
    "RealtimePPUR.runtimeconfig.json",
    "AutoMapper.dll",
    "av_libglesv2.dll",
    "Avalonia.Base.dll",
    "Avalonia.Controls.dll",
    "Avalonia.Desktop.dll",
    "Avalonia.Dialogs.dll",
    "Avalonia.Fonts.Inter.dll"
];

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
