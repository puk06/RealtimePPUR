![](https://github.com/puk06/RealtimePPUR-old/assets/86549420/5a41b979-3176-443a-91f0-6150d9243cda)

# RealtimePPUR ![License](https://img.shields.io/github/license/puk06/RealtimePPUR?style=flat-square) ![Release](https://img.shields.io/github/v/release/puk06/RealtimePPUR?style=flat-square) ![Language](https://img.shields.io/badge/language-c%23-green?style=flat-square) ![CodeFactor](https://www.codefactor.io/repository/github/puk06/RealtimePPUR/badge)
このソフトウェアは、osuのゲーム内でリアルタイムPP、URと、オフセットがどれほどズレているか教えてくれるソフトです。

## ☕ Support Me

もし私の活動を応援していただけるなら、Ko-fiでコーヒーをおごっていただけると嬉しいです！

If you enjoy my work and would like to support my efforts, you can buy me a coffee on Ko-fi!

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/pukorufu)

### サポートのお願い
私のプロジェクトが役に立ったり、気に入っていただけた場合、サポートをいただけると今後の開発の励みになります。どうぞよろしくお願いします！

# Functions
- **リアルタイムで表示されるPPやUR、SR**
  > * リアルタイムでSR、PP、URを計算してソフト上、InGameOverlayに表示します！

- **PPやSR、Hitsなど、様々な情報を表示することができるInGameOverlay**
  > * 様々なカスタマイズが可能なInGameOverlay(ゲーム内オーバーレイ)!
  > * injectorを使わないので、osu!の規約に違反することはありません！

- **他人のリプレイ画面を開くとPPを見ることができる機能**
  > * ランキングなどで他人のリザルトを開くと、IFFC、SSPP、SR、PPを計算して表示します！

- **コンバート完全対応**
  > * 全Modeのコンバートに完全対応しており、コンバート勢でも安心して使うことが出来ます！

- **軽量！**
  > * メモリの最適化が行われているので、従来のRealtimePPURよりずっと軽くなりました！

- **全Mode対応**
  > * コンバートのところでも話しましたが、このソフトは全Modeに完全対応しています！！！

# How to use
青く、PPと書かれたアイコンのRealtimePPUR.exeを起動するだけです。

# How to switch to Offset Helper and RealtimePP?
ソフト上で右クリック→Modeから変更できます。

# How to save Config?
右クリックメニューからSave Configをクリックすることで設定を保存できます。

# How to edit Config?
フォルダ内にあるConfig.cfgを編集することで設定を変更できます。

説明がたくさんあるので、わかりやすいと思います。

# How to use InGameOverlay?

右クリックメニューからosu! modeをクリックすることでオンにできます。
<br>
オンになったらプレイ画面に移った時、自動的にオーバーレイが表示されます。

<br>

※**フルスクリーンの状態では動作しません！！** ボーダーレスかウィンドウの時のみ動作します！

<br>

InGameOverlayに表示するものは右クリックメニューのInGameOverlayから編集できます。
<br>
その他細かい設定(デフォルトでオンにしておく物や位置、フォントサイズなど)はフォルダ内のConfig.cfgを編集することで設定を変更できます。

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
