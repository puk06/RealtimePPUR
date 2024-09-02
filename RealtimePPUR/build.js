const fs = require("fs");
const path = require("path");

const OSU_LIBRARY = [
    "osu.Game.dll",
    "osu.Game.Rulesets.Osu.dll",
    "osu.Game.Rulesets.Taiko.dll",
    "osu.Game.Rulesets.Catch.dll",
    "osu.Game.Rulesets.Mania.dll",
    "osu.Framework.dll",
    "AutoMapper.dll",
    "osuTK.dll",
    "Realm.dll"
];

const SOFTWARE_LIBRARY = [
    "DiscordRPC.dll",
    "Newtonsoft.Json.dll",
    "Octokit.dll",
    "OsuMemoryDataProvider.dll",
    "ProcessMemoryDataFinder.dll"
];

const REALTIMEPPUR_FILES = [
    "RealtimePPUR.deps.json",
    "RealtimePPUR.dll",
    "RealtimePPUR.exe",
    "RealtimePPUR.runtimeconfig.json"
];

const BUILD_FOLDER = "./build";

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

for (const file of OSU_LIBRARY) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

for (const file of SOFTWARE_LIBRARY) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

for (const file of REALTIMEPPUR_FILES) {
    const filePath = path.join(BUILD_FOLDER, file);
    console.log(`Copying ${file} to ${filePath}`);
    fs.copyFileSync(file, filePath);
    fs.unlinkSync(file);
}

const files2 = fs.readdirSync(".");
for (const file of files2) {
    if (file !== "build" && file !== "build.js") {
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
}

console.log("Build completed.");
