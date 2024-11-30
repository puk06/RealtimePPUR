![](https://github.com/puk06/RealtimePPUR-old/assets/86549420/5a41b979-3176-443a-91f0-6150d9243cda)

# RealtimePPUR ![License](https://img.shields.io/github/license/puk06/RealtimePPUR?style=flat-square) ![Release](https://img.shields.io/github/v/release/puk06/RealtimePPUR?style=flat-square) ![Language](https://img.shields.io/badge/language-c%23-green?style=flat-square) ![CodeFactor](https://www.codefactor.io/repository/github/puk06/RealtimePPUR/badge)
This software will tell you how much the offset is off, with real-time PP, UR, in the osu game.

Support Discord Server: https://discord.gg/AGNDPsZPya

> [!NOTE]
> This software is currently in the development phase! If you have any bugs, please contact us on Discord!

## ☕ Support Me

もし私の活動を応援していただけるなら、Ko-fiでコーヒーをおごっていただけると嬉しいです！

If you enjoy my work and would like to support my efforts, you can buy me a coffee on Ko-fi!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/pukorufu)

### サポートのお願い / Request for Support
私のプロジェクトが役に立ったり、気に入っていただけた場合、サポートをいただけると今後の開発の励みになります。どうぞよろしくお願いします！

If you find my project useful or like it, your support will encourage future development. Thank you in advance!

# Features
- **PP, UR and SR displayed in real time**
  > * Calculate SR, PP and UR in real time and display them on the software and on the InGameOverlay!

- **InGameOverlay that can display various information such as PP, SR, Hits, etc.**
  > * InGameOverlay (in-game overlay) that can be customized in various ways!
  > * Since it does not use injector, it does not violate the terms of osu!

- **Function to open other people's replay screens to see their PP**!
  > * Function to calculate and display IFFC, SSPP, SR, and PP when you open someone else's result in ranking, etc. **Function to see PP when you open someone else's result in ranking, etc!

- **Complete conversion support**
  > * Complete support for conversion of all Modes, so even converters can use it with confidence!

- **Lightweight!**
  > * Memory optimization has been done so that it is much lighter than the previous RealtimePPUR!

- **All Modes supported**
  > * This software is fully compatible with all Modes!

# How to launch RealtimePPUR?
Simply launch RealtimePPUR.exe, the icon with the blue, PP.

# How to switch to Offset Helper and RealtimePP?
You can change it by right-clicking on the software → Mode.

# How to change the default software settings
Change the Config.cfg file in the folder!

I have written a lot of explanations so it will be easy to understand!


# About IngameOverlay

### Values
1: SR → SR: 〇〇 / 〇〇

2: SSPP → SSPP: 〇〇pp

3: CurrentPP → PP: 〇〇pp

4: CurrentACC → ACC: 〇〇%

5: Hits → Hits: 〇〇/〇〇/〇〇/〇〇

6: IFFCHits → IFFCHits: 〇〇/〇〇/〇〇/〇〇

7: UR → UR: \(CurrentUR\)

8: OffsetHelp → Offset: 〇〇

9: ExpectedManiaScore  → ManiaScore: 〇〇〇〇〇〇

10: AVGOFFSET → AvgOffset: 〇〇

11: PROGRESS → Progress: 〇〇%

12: HealthPercentage → HP: 〇〇%

13: CurrentPosition → Position: #〇

14: HigherScoreDiff → HigherDiff: 〇〇

15: HighestScoreDiff → HighestDiff: 〇〇

16: UserScore → Score: 〇〇

17: CurrentBPM → BPM: 〇〇〇

18: CurrentRank → Rank: 〇 / 〇

19: Remaining Notes → Notes: 〇〇〇〇


### How to save IngameOverlay Config?
You can save the configuration by clicking Save Config from the right-click menu!


### How to change IngameOverlay Priority?
Click Change Priority from the right-click menu to change it on the UI.

Or, change it by looking directly at the INGAMEOVERLAYPRIORITY value in Config.cfg!


# Special Thanks
- **[Vanilla](https://twitter.com/Van1IIa) For Saggesting IngameOverlay Font!!!**

# I want to build RealtimePPUR!!
必要なモノ
1. Node.js
2. .NET8.0 SDK
3. Git

手順
1. [RealtimePPUR-Build.bat](https://raw.githubusercontent.com/puk06/RealtimePPUR/refs/heads/master/RealtimePPUR-Build.bat)このリンクを右クリックして、リンク先を保存からダウンロードしてください。
2. ダウンロードしたファイルを、RealtimePPURを作りたいフォルダに移動して、ダブルクリックで起動します。(RealtimePPUR-Build.batがあるフォルダと同じ場所にRealtimePPURというフォルダが作られます。)
3. ビルドが終わると、自動でビルド先のエクスプローラーが開き、RealtimePPUR.exeを実行します。(ビルド先はRealtimePPUR/RealtimePPUR/bin/buildフォルダです)


# I want to build RealtimePPUR with Local Rulesets for Rework!!
必要なものは上と同じです。

手順
1. [RealtimePPUR-Build.bat](https://raw.githubusercontent.com/puk06/RealtimePPUR/refs/heads/master/RealtimePPUR-Build.bat)このリンクを右クリックして、リンク先を保存からダウンロードしてください。
2. RealtimePPURを作りたいフォルダのパスをコピーし、コマンドプロンプトを開いて
```php
cd "コピーしたパス"
```
と入力し、〇〇>のところがコピーしたところになってればOKです。
次にそのフォルダ内にRealtimePPUR-Build.batがあることを確認して、
```php
RealtimePPUR-Build.bat "リワークのリポジトリ" "コミット、ブランチ、タグなどあればここに"
```
と入力し、ビルドします。
3. ビルドが終わると、自動でビルド先のエクスプローラーが開き、RealtimePPUR.exeを実行します。(ビルド先はRealtimePPUR/RealtimePPUR/bin/buildフォルダです)
