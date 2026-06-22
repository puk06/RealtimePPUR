const fs = require("fs");
const path = require("path");

const REQUIRED_FILES = [
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

const BuildDirectory = process.argv[2];

const FilesList = fs.readdirSync(BuildDirectory);
for (const file of FilesList) {
    if (REQUIRED_FILES.includes(file)) continue;
    
    const filePath = path.join(BuildDirectory, file);
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
