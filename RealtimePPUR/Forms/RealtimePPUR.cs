using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DiscordRPC;
using osu.Game.Rulesets.Scoring;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using RealtimePPUR.Classes;
using Path = System.IO.Path;
using static RealtimePPUR.Classes.Helper;
using static RealtimePPUR.Classes.CalculatorHelpers;

namespace RealtimePPUR.Forms
{
    public sealed partial class RealtimePpur : Form
    {
        private const string CURRENT_VERSION = "v1.1.9-Release";
        private const string DISCORD_CLIENT_ID = "1237279508239749211";
#if DEBUG
        private const bool DEBUG_MODE = true;
#else
        private const bool DEBUG_MODE = false;
#endif

        private Label currentPp, sr, iffc, good, ok, miss, avgoffset, ur, avgoffsethelp;

        private readonly PrivateFontCollection fontCollection = new();
        private readonly string ingameoverlayPriority;

        private Point mousePoint;
        private string displayFormat;
        private int mode, x, y;
        private bool isosumode;
        private bool nowPlaying;
        private int currentBackgroundImage = 1;
        private bool isDirectoryLoaded;
        private string osuDirectory;
        private string songsPath;
        private string preMapPath;
        private PpCalculator calculator;
        private bool isplaying;
        private bool isResultScreen;
        private double avgOffset;
        private double avgOffsethelp;
        private int urValue;
        private int currentBeatmapGamemode;
        private int currentOsuGamemode;
        private int currentGamemode;
        private int preOsuGamemode;
        private BeatmapData calculatedObject;
        private OsuMemoryStatus currentStatus;
        private static DiscordRpcClient _client;
        private readonly Stopwatch stopwatch = new();
        private HitsResult previousHits = new();
        private string prevErrorMessage;
        private string[] prevModStrings;
        public List<int> UnstableRateArray { get; set; }

        private readonly Dictionary<string, string> configDictionary = new();
        private readonly StructuredOsuMemoryReader sreader = new();
        private readonly OsuBaseAddresses baseAddresses = new();
        private readonly string customSongsFolder;
        private readonly Dictionary<string, int> osuModeValue = new()
        {
            { "left", 0 },
            { "top", 0 }
        };

        public FontFamily GuiFont;
        public FontFamily InGameOverlayFont;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

        [DllImport("kernel32.dll")]
        static extern bool AllocConsole();

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        // Initialize
        private void RealtimePpur_Shown(object sender, EventArgs e)
        {
            TopMost = true;
            Thread updateMemoryThread = new(UpdateMemoryData) { IsBackground = true };
            Thread updatePpDataThread = new(UpdatePpData) { IsBackground = true };
            Thread updateDiscordRichPresenceThread = new(UpdateDiscordRichPresence) { IsBackground = true };
            updateMemoryThread.Start();
            updatePpDataThread.Start();
            updateDiscordRichPresenceThread.Start();
            UpdateLoop();
        }

        public RealtimePpur()
        {
            if (DEBUG_MODE) AllocConsole();

            AddFontFile();
            InitializeComponent();

            DebugLogger("RealtimePPUR Initialized.");

            if (File.Exists("Error.log")) File.Delete("Error.log");

            if (!File.Exists("Config.cfg"))
            {
                ShowInformationMessageBox("Config.cfgがフォルダ内に存在しないため、すべての項目がOffとして設定されます。アップデートチェックのみ行われます。");
                GithubUpdateChecker(CURRENT_VERSION);
                sRToolStripMenuItem.Checked = false;
                sSPPToolStripMenuItem.Checked = false;
                currentPPToolStripMenuItem.Checked = false;
                currentACCToolStripMenuItem.Checked = false;
                hitsToolStripMenuItem.Checked = false;
                uRToolStripMenuItem.Checked = false;
                offsetHelpToolStripMenuItem.Checked = false;
                avgOffsetToolStripMenuItem.Checked = false;
                progressToolStripMenuItem.Checked = false;
                ifFCPPToolStripMenuItem.Checked = false;
                ifFCHitsToolStripMenuItem.Checked = false;
                expectedManiaScoreToolStripMenuItem.Checked = false;
                healthPercentageToolStripMenuItem.Checked = false;
                currentPositionToolStripMenuItem.Checked = false;
                higherScoreToolStripMenuItem.Checked = false;
                highestScoreToolStripMenuItem.Checked = false;
                userScoreToolStripMenuItem.Checked = false;
                currentBPMToolStripMenuItem.Checked = false;
                remainingNotesToolStripMenuItem.Checked = false;
                discordRichPresenceToolStripMenuItem.Checked = false;
                pPLossModeToolStripMenuItem.Checked = false;
                calculateFirstToolStripMenuItem.Checked = false;

                ingameoverlayPriority = "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19";
                inGameValue.Font = new Font(InGameOverlayFont, 19F);
                customSongsFolder = "";
            }
            else
            {
                string[] lines = File.ReadAllLines("Config.cfg");
                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length != 2) continue;

                    string name = parts[0].Trim();
                    string value = parts[1].Trim();
                    configDictionary[name] = value;
                }

                if (CheckConfigDictionaryValue("UPDATECHECK"))
                {
                    GithubUpdateChecker(CURRENT_VERSION);
                }

                var defaultmodeTest = configDictionary.TryGetValue("DEFAULTMODE", out string defaultmodestring);
                if (defaultmodeTest)
                {
                    var defaultModeResult = int.TryParse(defaultmodestring, out int defaultmode);
                    if (!defaultModeResult || defaultmode is not (0 or 1 or 2))
                    {
                        ShowErrorMessageBox("Config.cfgのDEFAULTMODEの値が不正であったため、初期値の0が適用されます。0、1、2のどれかを入力してください。");
                    }
                    else
                    {
                        switch (defaultmode)
                        {
                            case 1:
                                ClientSize = new Size(316, 65);
                                BackgroundImage = Properties.Resources.PP;
                                currentBackgroundImage = 2;
                                RoundCorners();
                                mode = 1;
                                break;

                            case 2:
                                ClientSize = new Size(316, 65);
                                BackgroundImage = Properties.Resources.UR;
                                currentBackgroundImage = 3;
                                RoundCorners();
                                foreach (Control control in Controls)
                                {
                                    if (control.Name == "inGameValue") continue;
                                    control.Location = control.Location with { Y = control.Location.Y - 65 };
                                }

                                mode = 2;
                                break;
                        }
                        ChangeSoftwareMode(mode);
                    }
                }

                // InGameOverlay
                sRToolStripMenuItem.Checked = CheckConfigDictionaryValue("SR");
                sSPPToolStripMenuItem.Checked = CheckConfigDictionaryValue("SSPP");
                currentPPToolStripMenuItem.Checked = CheckConfigDictionaryValue("CURRENTPP");
                currentACCToolStripMenuItem.Checked = CheckConfigDictionaryValue("CURRENTACC");
                hitsToolStripMenuItem.Checked = CheckConfigDictionaryValue("HITS");
                uRToolStripMenuItem.Checked = CheckConfigDictionaryValue("UR");
                offsetHelpToolStripMenuItem.Checked = CheckConfigDictionaryValue("OFFSETHELP");
                avgOffsetToolStripMenuItem.Checked = CheckConfigDictionaryValue("AVGOFFSET");
                progressToolStripMenuItem.Checked = CheckConfigDictionaryValue("PROGRESS");
                ifFCPPToolStripMenuItem.Checked = CheckConfigDictionaryValue("IFFCPP");
                ifFCHitsToolStripMenuItem.Checked = CheckConfigDictionaryValue("IFFCHITS");
                expectedManiaScoreToolStripMenuItem.Checked = CheckConfigDictionaryValue("EXPECTEDMANIASCORE");
                healthPercentageToolStripMenuItem.Checked = CheckConfigDictionaryValue("HEALTHPERCENTAGE");
                currentPositionToolStripMenuItem.Checked = CheckConfigDictionaryValue("CURRENTPOSITION");
                higherScoreToolStripMenuItem.Checked = CheckConfigDictionaryValue("HIGHERSCOREDIFF");
                highestScoreToolStripMenuItem.Checked = CheckConfigDictionaryValue("HIGHESTSCOREDIFF");
                userScoreToolStripMenuItem.Checked = CheckConfigDictionaryValue("USERSCORE");
                currentBPMToolStripMenuItem.Checked = CheckConfigDictionaryValue("CURRENTBPM");
                currentRankToolStripMenuItem.Checked = CheckConfigDictionaryValue("CURRENTRANK");
                remainingNotesToolStripMenuItem.Checked = CheckConfigDictionaryValue("REMAININGNOTES");

                pPLossModeToolStripMenuItem.Checked = CheckConfigDictionaryValue("PPLOSSMODE");
                calculateFirstToolStripMenuItem.Checked = CheckConfigDictionaryValue("CALCULATEFIRST");
                discordRichPresenceToolStripMenuItem.Checked = CheckConfigDictionaryValue("DISCORDRICHPRESENCE");
                ingameoverlayPriority = CheckConfigDictionaryString("INGAMEOVERLAYPRIORITY", "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19");
                if (configDictionary.TryGetValue("CUSTOMSONGSFOLDER", out string customSongsFolderValue) && !customSongsFolderValue.Equals("songs", StringComparison.CurrentCultureIgnoreCase))
                {
                    customSongsFolder = customSongsFolderValue;
                }
                else
                {
                    customSongsFolder = "";
                }

                if (CheckConfigDictionaryValue("USECUSTOMFONT"))
                {
                    if (File.Exists("Font"))
                    {
                        var fontDictionary = new Dictionary<string, string>();
                        string[] fontInfo = File.ReadAllLines("Font");
                        foreach (string line in fontInfo)
                        {
                            string[] parts = line.Split('=');
                            if (parts.Length != 2) continue;
                            string name = parts[0].Trim();
                            string value = parts[1].Trim();
                            fontDictionary[name] = value;
                        }

                        var fontName = fontDictionary.TryGetValue("FONTNAME", out string fontNameValue);
                        var fontSize = fontDictionary.TryGetValue("FONTSIZE", out string fontSizeValue);
                        var fontStyle = fontDictionary.TryGetValue("FONTSTYLE", out string fontStyleValue);

                        if (fontDictionary.Count == 3 && fontName && fontNameValue != "" && fontSize &&
                            fontSizeValue != "" && fontStyle && fontStyleValue != "")
                        {
                            try
                            {
                                inGameValue.Font = new Font(fontNameValue, float.Parse(fontSizeValue),
                                    (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleValue));
                            }
                            catch
                            {
                                ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。");
                                var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                                if (!fontsizeResult)
                                {
                                    ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                                    inGameValue.Font = new Font(InGameOverlayFont, 19F);
                                }
                                else
                                {
                                    var result = float.TryParse(fontsizeValue, out float fontsize);
                                    if (!result)
                                    {
                                        ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                                        inGameValue.Font = new Font(InGameOverlayFont, 19F);
                                    }
                                    else
                                    {
                                        inGameValue.Font = new Font(InGameOverlayFont, fontsize);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。");
                            var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                            if (!fontsizeResult)
                            {
                                ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                                inGameValue.Font = new Font(InGameOverlayFont, 19F);
                            }
                            else
                            {
                                var result = float.TryParse(fontsizeValue, out float fontsize);
                                if (!result)
                                {
                                    ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                                    inGameValue.Font = new Font(InGameOverlayFont, 19F);
                                }
                                else
                                {
                                    inGameValue.Font = new Font(InGameOverlayFont, fontsize);
                                }
                            }
                        }
                    }
                    else
                    {
                        var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                        if (!fontsizeResult)
                        {
                            ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                            inGameValue.Font = new Font(InGameOverlayFont, 19F);
                        }
                        else
                        {
                            var result = float.TryParse(fontsizeValue, out float fontsize);
                            if (!result)
                            {
                                ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                                inGameValue.Font = new Font(InGameOverlayFont, 19F);
                            }
                            else
                            {
                                inGameValue.Font = new Font(InGameOverlayFont, fontsize);
                            }
                        }
                    }
                }
                else
                {
                    var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                    if (!fontsizeResult)
                    {
                        ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                        inGameValue.Font = new Font(InGameOverlayFont, 19F);
                    }
                    else
                    {
                        var result = float.TryParse(fontsizeValue, out float fontsize);
                        if (!result)
                        {
                            ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                            inGameValue.Font = new Font(InGameOverlayFont, 19F);
                        }
                        else
                        {
                            inGameValue.Font = new Font(InGameOverlayFont, fontsize);
                        }
                    }
                }
            }
        }

        private void AddFontFile()
        {
            DebugLogger("Loading fonts...");
            fontCollection.AddFontFile("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf");
            fontCollection.AddFontFile("./src/Fonts/Nexa Light.otf");

            foreach (FontFamily font in fontCollection.Families)
            {
                switch (font.Name)
                {
                    case "Rounded Mplus 1c ExtraBold":
                        GuiFont = font;
                        break;
                    case "Nexa Light":
                        InGameOverlayFont = font;
                        break;
                }
            }
            DebugLogger("Fonts loaded.");
        }

        // Loop
        private async void UpdateLoop()
        {
            while (true)
            {
                await Task.Delay(15);
                try
                {
                    if (!TopMost) TopMost = true;
                    if (Process.GetProcessesByName("osu!").Length == 0) throw new Exception("osu! is not running.");
                    bool isPlayingBool = isplaying;
                    bool isResultScreenBool = isResultScreen;
                    int currentGamemodeValue = currentGamemode;
                    OsuMemoryStatus status = currentStatus;

                    HitsResult hits = new()
                    {
                        HitGeki = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitGeki,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitGeki,
                            _ => 0
                        },
                        Hit300 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit300,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit300,
                            _ => 0
                        },
                        HitKatu = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitKatu,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitKatu,
                            _ => 0
                        },
                        Hit100 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit100,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit100,
                            _ => 0
                        },
                        Hit50 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit50,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit50,
                            _ => 0
                        },
                        HitMiss = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitMiss,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitMiss,
                            _ => 0
                        },
                        Combo = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.MaxCombo,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.MaxCombo,
                            _ => 0
                        },
                        Score = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Score,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Score,
                            _ => 0
                        }
                    };

                    if (isPlayingBool)
                    {
                        hits.HitGeki = baseAddresses.Player.HitGeki;
                        hits.Hit300 = baseAddresses.Player.Hit300;
                        hits.HitKatu = baseAddresses.Player.HitKatu;
                        hits.Hit100 = baseAddresses.Player.Hit100;
                        hits.Hit50 = baseAddresses.Player.Hit50;
                        hits.HitMiss = baseAddresses.Player.HitMiss;
                        hits.Combo = baseAddresses.Player.MaxCombo;
                        hits.Score = baseAddresses.Player.Score;
                    }

                    if (calculatedObject == null) continue;

                    double starRatingValue = IsNaNWithNum(Math.Round(calculatedObject.CurrentDifficultyAttributes.StarRating, 2));
                    double ssppValue = IsNaNWithNum(calculatedObject.PerformanceAttributes.Total);
                    double currentPpValue = IsNaNWithNum(calculatedObject.CurrentPerformanceAttributes.Total);
                    if (pPLossModeToolStripMenuItem.Checked && !isResultScreenBool && (currentGamemodeValue is 1 or 3)) currentPpValue = IsNaNWithNum(calculatedObject.PerformanceAttributesLossMode.Total);
                    double ifFcPpValue = IsNaNWithNum(calculatedObject.PerformanceAttributesIffc.Total);
                    double lossPpValue = IsNaNWithNum(calculatedObject.PerformanceAttributesLossMode.Total);

                    avgoffset.Text = Math.Round(avgOffset, 2) + "ms";
                    avgoffset.Width = TextRenderer.MeasureText(avgoffset.Text, avgoffset.Font).Width;

                    ur.Text = urValue.ToString(CultureInfo.CurrentCulture);
                    ur.Width = TextRenderer.MeasureText(ur.Text, ur.Font).Width;

                    avgoffsethelp.Text = avgOffsethelp.ToString(CultureInfo.CurrentCulture);
                    avgoffsethelp.Width = TextRenderer.MeasureText(avgoffsethelp.Text, avgoffsethelp.Font).Width;

                    sr.Text = starRatingValue.ToString(CultureInfo.CurrentCulture);
                    sr.Width = TextRenderer.MeasureText(sr.Text, sr.Font).Width;

                    if (isPlayingBool)
                    {
                        if (currentGamemodeValue is 1 or 3)
                        {
                            if (pPLossModeToolStripMenuItem.Checked)
                            {
                                iffc.Text = Math.Round(ifFcPpValue) + " / " + Math.Round(ssppValue);
                            }
                            else
                            {
                                iffc.Text = Math.Round(lossPpValue) + " / " + Math.Round(ssppValue);
                            }
                        }
                        else
                        {
                            iffc.Text = Math.Round(ifFcPpValue) + " / " + Math.Round(ssppValue);
                        }
                    }
                    else if (isResultScreenBool)
                    {
                        if (currentGamemodeValue != 3)
                        {
                            iffc.Text = Math.Round(ifFcPpValue) + " / " + Math.Round(ssppValue);
                        }
                        else
                        {
                            iffc.Text = Math.Round(ssppValue).ToString(CultureInfo.CurrentCulture);
                        }
                    }
                    else
                    {
                        iffc.Text = Math.Round(ssppValue).ToString(CultureInfo.CurrentCulture);
                    }

                    iffc.Width = TextRenderer.MeasureText(iffc.Text, iffc.Font).Width;

                    currentPp.Text = Math.Round(currentPpValue).ToString(CultureInfo.CurrentCulture);
                    currentPp.Width = TextRenderer.MeasureText(currentPp.Text, currentPp.Font).Width;
                    currentPp.Left = ClientSize.Width - currentPp.Width - 35;

                    switch (currentGamemodeValue)
                    {
                        case 0:
                            good.Text = hits.Hit300.ToString();
                            ok.Text = (hits.Hit100 + hits.Hit50).ToString();
                            miss.Text = hits.HitMiss.ToString();
                            break;

                        case 1:
                            good.Text = hits.Hit300.ToString();
                            ok.Text = hits.Hit100.ToString();
                            miss.Text = hits.HitMiss.ToString();
                            break;

                        case 2:
                            good.Text = hits.Hit300.ToString();
                            ok.Text = (hits.Hit100 + hits.Hit50).ToString();
                            miss.Text = hits.HitMiss.ToString();
                            break;

                        case 3:
                            good.Text = (hits.Hit300 + hits.HitGeki).ToString();
                            ok.Text = (hits.HitKatu + hits.Hit100 + hits.Hit50).ToString();
                            miss.Text = hits.HitMiss.ToString();
                            break;
                    }

                    good.Width = TextRenderer.MeasureText(good.Text, good.Font).Width;
                    good.Left = ((ClientSize.Width - good.Width) / 2) - 120;

                    ok.Width = TextRenderer.MeasureText(ok.Text, ok.Font).Width;
                    ok.Left = ((ClientSize.Width - ok.Width) / 2) - 61;

                    miss.Width = TextRenderer.MeasureText(miss.Text, miss.Font).Width;
                    miss.Left = ((ClientSize.Width - miss.Width) / 2) - 3;

                    RenderIngameOverlay(hits, calculatedObject, currentGamemodeValue);
                }
                catch (Exception e)
                {
                    ErrorLogger(e);
                    if (!nowPlaying) inGameValue.Text = "";
                    sr.Text = "0";
                    iffc.Text = "0";
                    currentPp.Text = "0";
                    good.Text = "0";
                    ok.Text = "0";
                    miss.Text = "0";
                    avgoffset.Text = "0ms";
                    ur.Text = "0";
                    avgoffsethelp.Text = "0";
                }
            }
        }

        private void UpdateMemoryData()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(15);
                    if (Process.GetProcessesByName("osu!").Length == 0) throw new Exception("osu! is not running.");
                    if (!isDirectoryLoaded)
                    {
                        Process osuProcess = Process.GetProcessesByName("osu!")[0];
                        string tempOsuDirectory = Path.GetDirectoryName(osuProcess.MainModule.FileName);
                        DebugLogger($"osu! directory: {tempOsuDirectory}");
                        if (string.IsNullOrEmpty(tempOsuDirectory) || !Directory.Exists(tempOsuDirectory))
                            throw new Exception("osu! directory not found.");

                        osuDirectory = tempOsuDirectory;
                        songsPath = GetSongsFolderLocation(osuDirectory, customSongsFolder);
                        isDirectoryLoaded = true;
                        DebugLogger($"Songs folder: {songsPath}");
                        DebugLogger("Directory Data initialized.");
                    }

                    if (!isDirectoryLoaded) throw new Exception("osu! directory not found.");

                    if (!sreader.CanRead) throw new Exception("Memory reader is not initialized.");

                    sreader.TryRead(baseAddresses.Beatmap);
                    sreader.TryRead(baseAddresses.Player);
                    sreader.TryRead(baseAddresses.GeneralData);
                    sreader.TryRead(baseAddresses.LeaderBoard);
                    sreader.TryRead(baseAddresses.ResultsScreen);
                    sreader.TryRead(baseAddresses.BanchoUser);

                    currentStatus = baseAddresses.GeneralData.OsuStatus;

                    if (currentStatus == OsuMemoryStatus.Playing) isplaying = true;
                    else if (currentStatus != OsuMemoryStatus.ResultsScreen) isplaying = false;

                    if (currentStatus == OsuMemoryStatus.Playing && !baseAddresses.Player.IsReplay) stopwatch.Start();
                    else stopwatch.Reset();
                    isResultScreen = currentStatus == OsuMemoryStatus.ResultsScreen;
                    currentOsuGamemode = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => baseAddresses.Player.Mode,
                        OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Mode,
                        _ => baseAddresses.GeneralData.GameMode
                    };

                    //Hit Error
                    UnstableRateArray = baseAddresses.Player.HitErrors;
                }
                catch (Exception e)
                {
                    ErrorLogger(e);
                }
            }
        }

        private async void UpdatePpData()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(15);

                    if (Process.GetProcessesByName("osu!").Length == 0) throw new Exception("osu! is not running.");
                    if (!isDirectoryLoaded) throw new Exception("Directory not loaded. Skipping...");

                    bool playing = isplaying;
                    bool resultScreen = isResultScreen;
                    string currentOsuFileName = baseAddresses.Beatmap.OsuFileName;
                    string osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "",
                        currentOsuFileName ?? "");

                    if (osuBeatmapPath == songsPath) continue;

                    OsuMemoryStatus status = currentStatus;

                    if (status == OsuMemoryStatus.Playing)
                    {
                        double currentUr =
                            baseAddresses.Player.HitErrors == null || baseAddresses.Player.HitErrors.Count == 0
                                ? 0
                                : CalculateUnstableRate(baseAddresses.Player.HitErrors);
                        double currentAvgOffset = CalculateAverage(baseAddresses.Player.HitErrors);
                        if (!double.IsNaN(currentUr)) urValue = (int)Math.Round(currentUr);
                        if (!double.IsNaN(currentAvgOffset))
                            avgOffset = baseAddresses.Player.HitErrors == null ||
                                        baseAddresses.Player.HitErrors.Count == 0
                                ? 0
                                : -Math.Round(currentAvgOffset, 2);
                        avgOffsethelp = (int)Math.Round(-avgOffset);
                    }

                    if (preMapPath != osuBeatmapPath)
                    {
                        preMapPath = osuBeatmapPath;
                        DebugLogger("Map change detected.");
                        DebugLogger($"Current beatmap path: {osuBeatmapPath}");

                        if (!File.Exists(osuBeatmapPath))
                        {
                            // Fix the beatmap path(idk why)
                            DebugLogger("Beatmap file not found. Trying to fix the path... (Attempting 1)");
                            osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName?.Trim() ?? "", currentOsuFileName ?? "");
                            DebugLogger($"Current beatmap path: {osuBeatmapPath}");

                            if (!File.Exists(osuBeatmapPath))
                            {
                                DebugLogger("Beatmap file not found. Trying to fix the path again... (Attempting 2)");
                                osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "", currentOsuFileName?.Trim() ?? "");
                                DebugLogger($"Current beatmap path: {osuBeatmapPath}");
                            }

                            if (!File.Exists(osuBeatmapPath))
                            {
                                DebugLogger("Beatmap file not found. Trying to fix the path again... (Attempting 3)");
                                osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName?.Trim() ?? "", currentOsuFileName?.Trim() ?? "");
                                DebugLogger($"Current beatmap path: {osuBeatmapPath}");
                            }

                            if (File.Exists(osuBeatmapPath)) DebugLogger("Beatmap file found.");
                        }

                        if (!File.Exists(osuBeatmapPath)) throw new Exception("Beatmap file not found.");

                        int currentBeatmapGamemodeTemp = await GetMapMode(osuBeatmapPath);
                        if (currentBeatmapGamemodeTemp is -1 or not (0 or 1 or 2 or 3))
                            throw new Exception("Invalid gamemode.");
                        DebugLogger($"Current beatmap gamemode: {currentBeatmapGamemodeTemp}");

                        currentBeatmapGamemode = currentBeatmapGamemodeTemp;
                        currentGamemode = currentBeatmapGamemode == 0 ? currentOsuGamemode : currentBeatmapGamemode;

                        if (calculator == null)
                        {
                            calculator = new PpCalculator(osuBeatmapPath, currentGamemode);
                            DebugLogger("Calculator initialized.");
                        }
                        else
                        {
                            calculator.SetMap(osuBeatmapPath, currentGamemode);
                            DebugLogger("Calculator updated.");
                        }
                    }

                    if (currentOsuGamemode != preOsuGamemode)
                    {
                        if (calculator == null) continue;
                        if (currentBeatmapGamemode == 0 && currentOsuGamemode is 0 or 1 or 2 or 3)
                        {
                            calculator.SetMode(currentOsuGamemode);
                            currentGamemode = currentOsuGamemode;
                            DebugLogger($"Gamemode changed to {currentOsuGamemode}");
                        }

                        preOsuGamemode = currentOsuGamemode;
                    }

                    if (status == OsuMemoryStatus.EditingMap) currentGamemode = currentBeatmapGamemode;

                    HitsResult hits = new()
                    {
                        HitGeki = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitGeki,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitGeki,
                            _ => 0
                        },
                        Hit300 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit300,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit300,
                            _ => 0
                        },
                        HitKatu = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitKatu,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitKatu,
                            _ => 0
                        },
                        Hit100 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit100,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit100,
                            _ => 0
                        },
                        Hit50 = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Hit50,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Hit50,
                            _ => 0
                        },
                        HitMiss = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.HitMiss,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.HitMiss,
                            _ => 0
                        },
                        Combo = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.MaxCombo,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.MaxCombo,
                            _ => 0
                        },
                        Score = status switch
                        {
                            OsuMemoryStatus.Playing => baseAddresses.Player.Score,
                            OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Score,
                            _ => 0
                        }
                    };

                    if (playing)
                    {
                        hits.HitGeki = baseAddresses.Player.HitGeki;
                        hits.Hit300 = baseAddresses.Player.Hit300;
                        hits.HitKatu = baseAddresses.Player.HitKatu;
                        hits.Hit100 = baseAddresses.Player.Hit100;
                        hits.Hit50 = baseAddresses.Player.Hit50;
                        hits.HitMiss = baseAddresses.Player.HitMiss;
                        hits.Combo = baseAddresses.Player.MaxCombo;
                        hits.Score = baseAddresses.Player.Score;
                    }

                    if (hits.Equals(previousHits) && status is OsuMemoryStatus.Playing && !hits.IsEmpty()) continue;
                    if (status is OsuMemoryStatus.Playing) previousHits = hits.Clone();

                    string[] mods = status switch
                    {
                        OsuMemoryStatus.Playing => ParseMods(baseAddresses.Player.Mods.Value).Calculate,
                        OsuMemoryStatus.ResultsScreen => ParseMods(baseAddresses.ResultsScreen.Mods.Value).Calculate,
                        OsuMemoryStatus.MainMenu => ParseMods(baseAddresses.GeneralData.Mods).Calculate,
                        _ => ParseMods(baseAddresses.GeneralData.Mods).Calculate
                    };

                    if (playing) mods = ParseMods(baseAddresses.Player.Mods.Value).Calculate;
                    prevModStrings = mods;

                    double acc = CalculateAcc(hits, currentGamemode);

                    var calcArgs = new CalculateArgs
                    {
                        Accuracy = acc,
                        Combo = hits.Combo,
                        Score = hits.Score,
                        Mods = mods,
                        Time = baseAddresses.GeneralData.AudioTime,
                        CalculateBeforePlaying = calculateFirstToolStripMenuItem.Checked
                    };
                    var result = calculator?.Calculate(calcArgs, playing,
                        resultScreen && !playing, hits);
                    if (result?.DifficultyAttributes == null || result.PerformanceAttributes == null ||
                        result.CurrentDifficultyAttributes == null ||
                        result.CurrentPerformanceAttributes == null) continue;
                    calculatedObject = result;
                }
                catch (Exception e)
                {
                    ErrorLogger(e);
                }
            }
        }

        private void UpdateDiscordRichPresence()
        {
            bool isConnectedToDiscord = false;
            bool hasClearedPresence = false;

            while (!isConnectedToDiscord)
            {
                try
                {
                    _client = new DiscordRpcClient(DISCORD_CLIENT_ID);
                    _client.Initialize();
                    isConnectedToDiscord = true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(5000);
                    ErrorLogger(e);
                }
            }

            DebugLogger("Connected to Discord.");

            while (true)
            {
                try
                {
                    Thread.Sleep(2000);

                    if (Process.GetProcessesByName("osu!").Length == 0)
                    {
                        if (hasClearedPresence) continue;
                        DebugLogger("Discord Rich Presence Cleared.");
                        _client.ClearPresence();
                        hasClearedPresence = true;
                        continue;
                    }

                    if (!discordRichPresenceToolStripMenuItem.Checked)
                    {
                        if (hasClearedPresence) continue;
                        hasClearedPresence = true;
                        _client.ClearPresence();
                        continue;
                    }

                    hasClearedPresence = false;

                    HitsResult hits = new()
                    {
                        HitGeki = baseAddresses.Player.HitGeki,
                        Hit300 = baseAddresses.Player.Hit300,
                        HitKatu = baseAddresses.Player.HitKatu,
                        Hit100 = baseAddresses.Player.Hit100,
                        Hit50 = baseAddresses.Player.Hit50,
                        HitMiss = baseAddresses.Player.HitMiss,
                        Combo = baseAddresses.Player.MaxCombo,
                        Score = baseAddresses.Player.Score
                    };

                    if (calculatedObject == null) continue;

                    switch (baseAddresses.GeneralData.OsuStatus)
                    {
                        case OsuMemoryStatus.Playing when !baseAddresses.Player.IsReplay:
                            _client.SetPresence(new RichPresence
                            {
                                Details = RichPresenceStringChecker(baseAddresses.BanchoUser.Username +
                                                                    ConvertStatus(baseAddresses.GeneralData.OsuStatus)),
                                State = RichPresenceStringChecker(baseAddresses.Beatmap.MapString),
                                Timestamps = new Timestamps()
                                {
                                    Start = DateTime.UtcNow - stopwatch.Elapsed
                                },
                                Assets = new Assets()
                                {
                                    LargeImageKey = "osu_icon",
                                    LargeImageText =
                                        $"RealtimePPUR ({CURRENT_VERSION})",
                                    SmallImageKey = "osu_playing",
                                    SmallImageText =
                                        $"{Math.Round(IsNaNWithNum(calculatedObject.CurrentPerformanceAttributes.Total), 2)}pp  +{string.Join("", ParseMods(baseAddresses.Player.Mods.Value).Show)}  {baseAddresses.Player.Combo}x  {ConvertHits(baseAddresses.Player.Mode, hits)}"
                                }
                            });
                            break;

                        case OsuMemoryStatus.Playing when
                            baseAddresses.Player.IsReplay:
                            _client.SetPresence(new RichPresence
                            {
                                Details = RichPresenceStringChecker(
                                    $"{baseAddresses.BanchoUser.Username} is Watching {baseAddresses.Player.Username}'s play"),
                                State = RichPresenceStringChecker(baseAddresses.Beatmap.MapString),
                                Assets = new Assets()
                                {
                                    LargeImageKey = "osu_icon",
                                    LargeImageText =
                                        $"RealtimePPUR ({CURRENT_VERSION})",
                                    SmallImageKey = "osu_playing",
                                    SmallImageText =
                                        $"{Math.Round(IsNaNWithNum(calculatedObject.CurrentPerformanceAttributes.Total), 2)}pp  +{string.Join("", ParseMods(baseAddresses.Player.Mods.Value).Show)}  {baseAddresses.Player.Combo}x  {ConvertHits(baseAddresses.Player.Mode, hits)}"
                                }
                            });
                            break;

                        default:
                            _client.SetPresence(new RichPresence
                            {
                                Details = RichPresenceStringChecker(baseAddresses.BanchoUser.Username +
                                                                    ConvertStatus(baseAddresses.GeneralData.OsuStatus)),
                                State = RichPresenceStringChecker(baseAddresses.Beatmap.MapString),
                                Assets = new Assets()
                                {
                                    LargeImageKey = "osu_icon",
                                    LargeImageText =
                                        $"RealtimePPUR ({CURRENT_VERSION})"
                                }
                            });
                            break;
                    }
                }
                catch (Exception e)
                {
                    ErrorLogger(e);
                }
            }
        }

        // Mode
        private void RealtimePPURToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == 0) return;
            ClientSize = new Size(316, 130);
            BackgroundImage = Properties.Resources.PPUR;
            currentBackgroundImage = 1;
            RoundCorners();
            if (mode == 2)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y + 65 };
                }
            }

            DebugLogger("RealtimePPUR mode enabled.");

            mode = 0;
            ChangeSoftwareMode(mode);
        }

        private void RealtimePPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == 1) return;
            ClientSize = new Size(316, 65);
            BackgroundImage = Properties.Resources.PP;
            currentBackgroundImage = 2;
            RoundCorners();
            if (mode == 2)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y + 65 };
                }
            }

            DebugLogger("RealtimePP mode enabled.");

            mode = 1;
            ChangeSoftwareMode(mode);
        }

        private void OffsetHelperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mode == 2) return;
            ClientSize = new Size(316, 65);
            BackgroundImage = Properties.Resources.UR;
            currentBackgroundImage = 3;
            RoundCorners();
            if (mode is 0 or 1)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y - 65 };
                }
            }

            DebugLogger("Offset Helper mode enabled.");

            mode = 2;
            ChangeSoftwareMode(mode);
        }

        // Font
        private void ChangeFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog font = new();
            try
            {
                if (font.ShowDialog() == DialogResult.Cancel) return;
                DebugLogger($"Font changed to {font.Font.Name} {font.Font.Size} {font.Font.Style}");
                inGameValue.Font = font.Font;
                DialogResult fontfDialogResult = MessageBox.Show("このフォントを保存しますか？", "情報", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (fontfDialogResult == DialogResult.No) return;

                try
                {
                    if (File.Exists("Font"))
                    {
                        const string filePath = "Font";
                        StreamWriter sw = new(filePath, false);
                        string fontInfo =
                            $"※絶対にこのファイルを自分で編集しないでください！\n※フォント名などを編集してしまうとフォントが見つからず、Windows標準のフォントが割り当てられてしまいます。\n※もし編集してしまった場合はこのファイルを削除することをお勧めします。\nFONTNAME={font.Font.Name}\nFONTSIZE={font.Font.Size}\nFONTSTYLE={font.Font.Style}";
                        sw.WriteLine(fontInfo);
                        sw.Close();
                        ShowInformationMessageBox("フォントの保存に成功しました。Config.cfgのUSECUSTOMFONTをtrueにすることで起動時から保存されたフォントを使用できます。右クリック→Load Fontからでも読み込むことが可能です！");
                    }
                    else
                    {
                        FileStream fs = File.Create("Font");
                        string fontInfo =
                            $"※絶対にこのファイルを自分で編集しないでください！\n※フォント名などを編集してしまうとフォントが見つからず、Windows標準のフォントが割り当てられてしまいます。\n※もし編集してしまった場合はこのファイルを削除することをお勧めします。\nFONTNAME={font.Font.Name}\nFONTSIZE={font.Font.Size}\nFONTSTYLE={font.Font.Style}";
                        byte[] fontInfoByte = System.Text.Encoding.UTF8.GetBytes(fontInfo);
                        fs.Write(fontInfoByte, 0, fontInfoByte.Length);
                        fs.Close();
                        ShowInformationMessageBox("フォントの保存に成功しました。Config.cfgのUSECUSTOMFONTをtrueにすることで起動時から保存されたフォントを使用できます。右クリック→Load Fontからでも読み込むことが可能です！");
                    }
                }
                catch
                {
                    ShowErrorMessageBox("フォントの保存に失敗しました。もしFontファイルが作成されていたら削除することをお勧めします。");
                }
            }
            catch
            {
                ShowErrorMessageBox("フォントの変更に失敗しました。対応していないフォントです。");
            }
        }

        private void LoadFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists("Font"))
            {
                var fontDictionaryLoad = new Dictionary<string, string>();
                string[] fontInfo = File.ReadAllLines("Font");
                foreach (string line in fontInfo)
                {
                    string[] parts = line.Split('=');
                    if (parts.Length != 2) continue;
                    string name = parts[0].Trim();
                    string value = parts[1].Trim();
                    fontDictionaryLoad[name] = value;
                }

                var fontName = fontDictionaryLoad.TryGetValue("FONTNAME", out string fontNameValue);
                var fontSize = fontDictionaryLoad.TryGetValue("FONTSIZE", out string fontSizeValue);
                var fontStyle = fontDictionaryLoad.TryGetValue("FONTSTYLE", out string fontStyleValue);

                if (fontDictionaryLoad.Count == 3 && fontName && fontNameValue != "" && fontSize &&
                    fontSizeValue != "" && fontStyle && fontStyleValue != "")
                {
                    try
                    {
                        DebugLogger($"Font loaded: {fontNameValue} {fontSizeValue} {fontStyleValue}");
                        inGameValue.Font = new Font(fontNameValue, float.Parse(fontSizeValue),
                            (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleValue));
                        MessageBox.Show(
                            $"フォントの読み込みに成功しました。\n\nフォント名: {fontNameValue}\nサイズ: {fontSizeValue}\nスタイル: {fontStyleValue}",
                            "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        ShowErrorMessageBox("Fontファイルのフォント情報が不正、もしくは非対応であったため読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。");
                    }
                }
                else
                {
                    ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。");
                }
            }
            else
            {
                ShowErrorMessageBox("Fontファイルが存在しません。一度Change Fontでフォントを保存してください。");
            }
        }

        private void ResetFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
            if (!fontsizeResult)
            {
                ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                inGameValue.Font = new Font(InGameOverlayFont, 19F);
            }
            else
            {
                var result = float.TryParse(fontsizeValue, out float fontsize);
                if (!result)
                {
                    ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                    inGameValue.Font = new Font(InGameOverlayFont, 19F);
                }
                else
                {
                    inGameValue.Font = new Font(InGameOverlayFont, fontsize);
                }
                ShowInformationMessageBox("フォントのリセットが完了しました！");
            }
            DebugLogger("Font reset.");
        }

        // Move Window
        private void RealtimePPUR_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) mousePoint = new Point(e.X, e.Y);
        }

        private void RealtimePPUR_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;
            Left += e.X - mousePoint.X;
            Top += e.Y - mousePoint.Y;
        }

        // Close
        private void CloseToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void RealtimePPUR_Closed(object sender, EventArgs e) => Application.Exit();

        // osu! Mode
        private void OsuModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lefttest = configDictionary.TryGetValue("LEFT", out string leftvalue);
            var toptest = configDictionary.TryGetValue("TOP", out string topvalue);
            if (!lefttest || !toptest)
            {
                ShowErrorMessageBox("Config.cfgにLEFTまたはTOPの値が存在しなかったため、osu! Modeの起動に失敗しました。");
                return;
            }

            var leftResult = int.TryParse(leftvalue, out int left);
            var topResult = int.TryParse(topvalue, out int top);
            if ((!leftResult || !topResult) && !isosumode)
            {
                ShowErrorMessageBox("Config.cfgのLEFT、またはTOPの値が不正であったため、osu! Modeの起動に失敗しました。LEFT、TOPには数値以外入力しないでください。");
                return;
            }

            osuModeValue["left"] = left;
            osuModeValue["top"] = top;
            isosumode = !isosumode;
            osuModeToolStripMenuItem.Checked = isosumode;
        }

        private void ChangePriorityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form priorityForm = new ChangePriorityForm();
            priorityForm.Show();
        }

        private void RenderIngameOverlay(HitsResult hits, BeatmapData calculatedData, int currentGamemodeValue)
        {
            inGameValue.Text = SetIngameValue(calculatedData, hits, currentGamemodeValue);
            CheckOsuMode();
        }

        private void CheckOsuMode()
        {
            if (isosumode)
            {
                var processes = Process.GetProcessesByName("osu!");
                if (processes.Length > 0)
                {
                    Process osuProcess = processes[0];
                    IntPtr osuMainWindowHandle = osuProcess.MainWindowHandle;
                    if (GetWindowRect(osuMainWindowHandle, out Rect rect) &&
                        baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing &&
                        GetForegroundWindow() == osuMainWindowHandle && osuMainWindowHandle != IntPtr.Zero)
                    {
                        if (!nowPlaying)
                        {
                            x = Location.X;
                            y = Location.Y;
                            nowPlaying = true;
                        }

                        BackgroundImage = null;
                        currentBackgroundImage = 0;
                        inGameValue.Visible = true;
                        avgoffsethelp.Visible = false;
                        sr.Visible = false;
                        iffc.Visible = false;
                        currentPp.Visible = false;
                        good.Visible = false;
                        ok.Visible = false;
                        miss.Visible = false;
                        avgoffset.Visible = false;
                        ur.Visible = false;
                        Region = null;
                        Size = new Size(inGameValue.Width, inGameValue.Height);
                        Location = new Point(rect.Left + osuModeValue["left"] + 2,
                            rect.Top + osuModeValue["top"]);
                    }
                    else if (nowPlaying)
                    {
                        switch (mode)
                        {
                            case 0:
                                if (currentBackgroundImage != 1)
                                {
                                    ClientSize = new Size(316, 130);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.PPUR;
                                    currentBackgroundImage = 1;
                                }

                                break;

                            case 1:
                                if (currentBackgroundImage != 2)
                                {
                                    ClientSize = new Size(316, 65);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.PP;
                                    currentBackgroundImage = 2;
                                }

                                break;

                            case 2:
                                if (currentBackgroundImage != 3)
                                {
                                    ClientSize = new Size(316, 65);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.UR;
                                    currentBackgroundImage = 3;
                                }

                                break;
                        }

                        if (nowPlaying)
                        {
                            Location = new Point(x, y);
                            nowPlaying = false;
                        }

                        inGameValue.Visible = false;
                        sr.Visible = true;
                        iffc.Visible = true;
                        currentPp.Visible = true;
                        good.Visible = true;
                        ok.Visible = true;
                        miss.Visible = true;
                        avgoffset.Visible = true;
                        ur.Visible = true;
                        avgoffsethelp.Visible = true;
                    }
                }
                else if (nowPlaying)
                {
                    switch (mode)
                    {
                        case 0:
                            if (currentBackgroundImage != 1)
                            {
                                ClientSize = new Size(316, 130);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.PPUR;
                                currentBackgroundImage = 1;
                            }

                            break;

                        case 1:
                            if (currentBackgroundImage != 2)
                            {
                                ClientSize = new Size(316, 65);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.PP;
                                currentBackgroundImage = 2;
                            }

                            break;

                        case 2:
                            if (currentBackgroundImage != 3)
                            {
                                ClientSize = new Size(316, 65);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.UR;
                                currentBackgroundImage = 3;
                            }

                            break;
                    }

                    if (nowPlaying)
                    {
                        Location = new Point(x, y);
                        nowPlaying = false;
                    }

                    inGameValue.Visible = false;
                    sr.Visible = true;
                    iffc.Visible = true;
                    currentPp.Visible = true;
                    good.Visible = true;
                    ok.Visible = true;
                    miss.Visible = true;
                    avgoffset.Visible = true;
                    ur.Visible = true;
                    avgoffsethelp.Visible = true;
                }
            }
            else if (nowPlaying)
            {
                switch (mode)
                {
                    case 0:
                        if (currentBackgroundImage != 1)
                        {
                            ClientSize = new Size(316, 130);
                            RoundCorners();
                            BackgroundImage = Properties.Resources.PPUR;
                            currentBackgroundImage = 1;
                        }

                        break;

                    case 1:
                        if (currentBackgroundImage != 2)
                        {
                            ClientSize = new Size(316, 65);
                            RoundCorners();
                            BackgroundImage = Properties.Resources.PP;
                            currentBackgroundImage = 2;
                        }

                        break;

                    case 2:
                        if (currentBackgroundImage != 3)
                        {
                            ClientSize = new Size(316, 65);
                            RoundCorners();
                            BackgroundImage = Properties.Resources.UR;
                            currentBackgroundImage = 3;
                        }

                        break;
                }

                if (nowPlaying)
                {
                    Location = new Point(x, y);
                    nowPlaying = false;
                }

                inGameValue.Visible = false;
                sr.Visible = true;
                iffc.Visible = true;
                currentPp.Visible = true;
                good.Visible = true;
                ok.Visible = true;
                miss.Visible = true;
                avgoffset.Visible = true;
                ur.Visible = true;
                avgoffsethelp.Visible = true;
            }
        }

        private void ChangeSoftwareMode(int softwareMode)
        {
            switch (softwareMode)
            {
                case 0:
                    realtimePPURToolStripMenuItem.Checked = true;
                    realtimePPToolStripMenuItem.Checked = false;
                    offsetHelperToolStripMenuItem.Checked = false;
                    break;

                case 1:
                    realtimePPURToolStripMenuItem.Checked = false;
                    realtimePPToolStripMenuItem.Checked = true;
                    offsetHelperToolStripMenuItem.Checked = false;
                    break;

                case 2:
                    realtimePPURToolStripMenuItem.Checked = false;
                    realtimePPToolStripMenuItem.Checked = false;
                    offsetHelperToolStripMenuItem.Checked = true;
                    break;
            }
        }

        private string SetIngameValue(BeatmapData calculatedData, HitsResult hits, int currentGamemodeValue)
        {
            double starRatingValue = IsNaNWithNum(Math.Round(calculatedData.CurrentDifficultyAttributes.StarRating, 2));
            double fullSrValue = IsNaNWithNum(Math.Round(calculatedData.DifficultyAttributes.StarRating, 2));
            double ssppValue = IsNaNWithNum(calculatedData.PerformanceAttributes.Total);
            double currentPpValue = IsNaNWithNum(calculatedData.CurrentPerformanceAttributes.Total);
            double ifFcPpValue = IsNaNWithNum(calculatedData.PerformanceAttributesIffc.Total);
            double lossModePpValue = IsNaNWithNum(calculatedData.PerformanceAttributesLossMode.Total);

            var leaderBoardData = GetLeaderBoard(baseAddresses.LeaderBoard, baseAddresses.Player.Score);
            double healthPercentage = IsNaNWithNum(Math.Round(baseAddresses.Player.HP / 2, 1));
            int userScore = hits.Score;

            int currentPosition = leaderBoardData["currentPosition"];
            int higherScore = leaderBoardData["higherScore"];
            int highestScore = leaderBoardData["highestScore"];

            var ingameoverlayPriorityArray = ingameoverlayPriority.Replace(" ", "").Split('/');

            displayFormat = "";
            foreach (var priorityValue in ingameoverlayPriorityArray)
            {
                var priorityValueResult = int.TryParse(priorityValue, out int priorityValueInt);
                if (!priorityValueResult) continue;
                switch (priorityValueInt)
                {
                    case 1:
                        if (sRToolStripMenuItem.Checked)
                        {
                            displayFormat += "SR: " + starRatingValue + " / " + fullSrValue + "\n";
                        }

                        break;

                    case 2:
                        if (sSPPToolStripMenuItem.Checked)
                        {
                            displayFormat += "SSPP: " + Math.Round(ssppValue) + "pp\n";
                        }

                        break;

                    case 3:
                        if (currentPPToolStripMenuItem.Checked)
                        {
                            if (ifFCPPToolStripMenuItem.Checked)
                            {
                                if (currentGamemodeValue is 1 or 3)
                                {
                                    if (pPLossModeToolStripMenuItem.Checked)
                                    {
                                        displayFormat += "PP: " + Math.Round(lossModePpValue) + " / " + Math.Round(ifFcPpValue) + "pp\n";
                                    }
                                    else
                                    {
                                        displayFormat += "PP: " + Math.Round(currentPpValue) + " / " + Math.Round(lossModePpValue) + "pp\n";
                                    }
                                }
                                else
                                {
                                    displayFormat += "PP: " + Math.Round(currentPpValue) + " / " + Math.Round(ifFcPpValue) + "pp\n";
                                }
                            }
                            else if (currentGamemodeValue is 1 or 3 && pPLossModeToolStripMenuItem.Checked)
                            {
                                displayFormat += "PP: " + Math.Round(lossModePpValue) + "pp\n";
                            }
                            else
                            {
                                displayFormat += "PP: " + Math.Round(currentPpValue) + "pp\n";
                            }
                        }

                        break;

                    case 4:
                        if (currentACCToolStripMenuItem.Checked)
                        {
                            if (currentGamemode is 1 or 3)
                            {
                                displayFormat += "ACC: " + Math.Round(baseAddresses.Player.Accuracy, 2) + " / " + Math.Round((GetAccuracy(calculatedData.HitResultLossMode, currentGamemode) * 100), 2) + "%\n";
                            }
                            else
                            {
                                displayFormat += "ACC: " + Math.Round(baseAddresses.Player.Accuracy, 2) + "%\n";
                            }
                        }

                        break;

                    case 5:
                        if (hitsToolStripMenuItem.Checked)
                        {
                            switch (currentGamemodeValue)
                            {
                                case 0:
                                    displayFormat += $"Hits: {hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}\n";
                                    break;

                                case 1:
                                    {
                                        displayFormat += $"Hits: {hits.Hit300}/{hits.Hit100}/{hits.HitMiss}\n";
                                        break;
                                    }


                                case 2:
                                    displayFormat += $"Hits: {hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}\n";
                                    break;

                                case 3:
                                    displayFormat += $"Hits: {hits.HitGeki}/{hits.Hit300}/{hits.HitKatu}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}\n";
                                    break;
                            }
                        }

                        break;

                    case 6:
                        if (ifFCHitsToolStripMenuItem.Checked)
                        {
                            int ifFcGood = calculatedData.IfFcHitResult[HitResult.Great];
                            int ifFcOk = currentGamemodeValue == 2
                                ? calculatedData.IfFcHitResult[HitResult.LargeTickHit]
                                : calculatedData.IfFcHitResult[HitResult.Ok];
                            int ifFcBad = currentGamemodeValue switch
                            {
                                0 => calculatedData.IfFcHitResult[HitResult.Meh],
                                1 => 0,
                                2 => calculatedData.IfFcHitResult[HitResult.SmallTickHit],
                                _ => 0
                            };
                            const int ifFcMiss = 0;

                            switch (currentGamemodeValue)
                            {
                                case 0:
                                    displayFormat += $"IFFCHits: {ifFcGood}/{ifFcOk}/{ifFcBad}/{ifFcMiss}\n";
                                    break;

                                case 1:
                                    displayFormat += $"IFFCHits: {ifFcGood}/{ifFcOk}/{ifFcMiss}\n";
                                    break;

                                case 2:
                                    displayFormat += $"IFFCHits: {ifFcGood}/{ifFcOk}/{ifFcBad}/{ifFcMiss}\n";
                                    break;
                            }
                        }

                        break;

                    case 7:
                        if (uRToolStripMenuItem.Checked)
                        {
                            displayFormat += "UR: " + urValue + "\n";
                        }

                        break;

                    case 8:
                        if (offsetHelpToolStripMenuItem.Checked)
                        {
                            displayFormat += "Offset: " + Math.Round(avgOffsethelp) + "\n";
                        }

                        break;

                    case 9:
                        if (expectedManiaScoreToolStripMenuItem.Checked && currentGamemodeValue == 3)
                        {
                            displayFormat += "ManiaScore: " + calculatedData.ExpectedManiaScore + "\n";
                        }

                        break;

                    case 10:
                        if (avgOffsetToolStripMenuItem.Checked)
                        {
                            displayFormat += "AvgOffset: " + avgOffset + "\n";
                        }

                        break;

                    case 11:
                        if (progressToolStripMenuItem.Checked)
                        {
                            displayFormat += "Progress: " + Math.Round(baseAddresses.GeneralData.AudioTime /
                                baseAddresses.GeneralData.TotalAudioTime * 100) + "%\n";
                        }

                        break;

                    case 12:
                        if (healthPercentageToolStripMenuItem.Checked)
                        {
                            displayFormat += "HP: " + healthPercentage + "%\n";
                        }

                        break;

                    case 13:
                        if (currentPositionToolStripMenuItem.Checked && currentPosition != 0)
                        {
                            if (currentPosition > 50)
                            {
                                displayFormat += "Position: >#50" + "\n";
                            }
                            else
                            {
                                displayFormat += "Position: #" + currentPosition + "\n";
                            }
                        }

                        break;

                    case 14:
                        if (higherScoreToolStripMenuItem.Checked && higherScore != 0)
                        {
                            displayFormat += "HigherDiff: " + (higherScore - userScore) + "\n";
                        }

                        break;

                    case 15:
                        switch (highestScoreToolStripMenuItem.Checked)
                        {
                            case true when highestScore != 0 && currentPosition == 1:
                                displayFormat += "HighestDiff: Top!" + "\n";
                                break;

                            case true when highestScore != 0:
                                displayFormat += "HighestDiff: " + (highestScore - userScore) + "\n";
                                break;
                        }

                        break;

                    case 16:
                        if (userScoreToolStripMenuItem.Checked)
                        {
                            displayFormat += "Score: " + userScore + "\n";
                        }

                        break;

                    case 17:
                        if (currentBPMToolStripMenuItem.Checked)
                        {
                            var currentBpm = calculatedData.CurrentBpm;

                            if (prevModStrings.Contains("dt") || prevModStrings.Contains("nc"))
                            {
                                currentBpm *= 1.5;
                            }
                            else if (prevModStrings.Contains("ht"))
                            {
                                currentBpm *= 0.75;
                            }

                            currentBpm = Math.Round(currentBpm, 1);

                            displayFormat += "BPM: " + currentBpm + "\n";
                        }

                        break;

                    case 18:
                        if (currentRankToolStripMenuItem.Checked)
                        {
                            var mods = ParseMods(baseAddresses.Player.Mods.Value).Show;
                            var currentRank = GetCurrentRank(calculatedData.HitResults, currentGamemodeValue, mods);
                            var currentRankLossMode = GetCurrentRank(calculatedData.HitResultLossMode, currentGamemodeValue, mods);
                            displayFormat += "Rank: " + currentRank + " / " + currentRankLossMode + "\n";
                        }

                        break;

                    case 19:
                        if (remainingNotesToolStripMenuItem.Checked)
                        {
                            var totalNotes = calculatedData.TotalHitObjectCount;

                            int currentNotes = currentGamemodeValue switch
                            {
                                0 => hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitMiss,
                                1 => hits.Hit300 + hits.Hit100 + hits.HitMiss,
                                2 => hits.Hit300 + hits.Hit100 + hits.HitMiss,
                                3 => hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 +
                                     hits.HitMiss,
                                _ => throw new NotImplementedException()
                            };

                            var remainingNotes = totalNotes - currentNotes;
                            displayFormat += "Notes: " + remainingNotes + "\n";
                        }

                        break;
                }
            }

            return displayFormat;
        }

        // Check Config
        private bool CheckConfigDictionaryValue(string key)
        {
            return configDictionary.TryGetValue(key, out string test) && test == "true";
        }

        private string CheckConfigDictionaryString(string key, string value)
        {
            return configDictionary.TryGetValue(key, out string test) ? test : value;
        }

        // Save Config
        private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                const string filePath = "Config.cfg";
                if (!File.Exists(filePath))
                {
                    ShowErrorMessageBox("Config.cfgが見つかりませんでした。RealtimePPURをダウンロードし直してください。");
                    return;
                }

                var parameters = new Dictionary<string, string>
                {
                    { "SR", ConfigValueToString(sRToolStripMenuItem.Checked) },
                    { "SSPP", ConfigValueToString(sSPPToolStripMenuItem.Checked) },
                    { "CURRENTPP", ConfigValueToString(currentPPToolStripMenuItem.Checked ) },
                    { "CURRENTACC", ConfigValueToString(currentACCToolStripMenuItem.Checked) },
                    { "HITS", ConfigValueToString(hitsToolStripMenuItem.Checked) },
                    { "IFFCHITS", ConfigValueToString(ifFCHitsToolStripMenuItem.Checked) },
                    { "UR", ConfigValueToString(uRToolStripMenuItem.Checked) },
                    { "OFFSETHELP", ConfigValueToString(offsetHelpToolStripMenuItem.Checked) },
                    { "EXPECTEDMANIASCORE", ConfigValueToString(expectedManiaScoreToolStripMenuItem.Checked) },
                    { "AVGOFFSET", ConfigValueToString(avgOffsetToolStripMenuItem.Checked) },
                    { "PROGRESS", ConfigValueToString(progressToolStripMenuItem.Checked) },
                    { "IFFCPP", ConfigValueToString(ifFCPPToolStripMenuItem.Checked) },
                    { "HEALTHPERCENTAGE", ConfigValueToString(healthPercentageToolStripMenuItem.Checked) },
                    { "CURRENTPOSITION", ConfigValueToString(currentPositionToolStripMenuItem.Checked) },
                    { "HIGHERSCOREDIFF", ConfigValueToString(higherScoreToolStripMenuItem.Checked) },
                    { "USERSCORE", ConfigValueToString(userScoreToolStripMenuItem.Checked) },
                    { "CURRENTBPM", ConfigValueToString(currentBPMToolStripMenuItem.Checked) },
                    { "CURRENTRANK", ConfigValueToString(currentRankToolStripMenuItem.Checked) },
                    { "REMAININGNOTES", ConfigValueToString(remainingNotesToolStripMenuItem.Checked) },
                    { "PPLOSSMODE", ConfigValueToString(pPLossModeToolStripMenuItem.Checked) },
                    { "CALCULATEFIRST", ConfigValueToString(calculateFirstToolStripMenuItem.Checked) },
                    { "DISCORDRICHPRESENCE", ConfigValueToString(discordRichPresenceToolStripMenuItem.Checked) }
                };

                WriteConfigFile(filePath, parameters);
                ShowInformationMessageBox("Config.cfgの保存が完了しました！");
            }
            catch (Exception error)
            {
                ErrorLogger(error);
                ShowErrorMessageBox("Config.cfgの保存に失敗しました。");
            }
        }

        // Error Logger
        private void ErrorLogger(Exception error)
        {
            try
            {
                if (error.Message == prevErrorMessage) return;
                DebugLogger("Error: " + error.Message, true);
                prevErrorMessage = error.Message;
                const string filePath = "Error.log";
                StreamWriter sw = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
                var currentDateString = DebugDateGenerator();
                sw.WriteLine("[" + currentDateString + "]");
                sw.WriteLine(error);
                sw.WriteLine();
                sw.Close();
            }
            catch
            {
                DebugLogger("Error Logger Failed");
            }
        }

        // Round Corners
        private void RoundCorners()
        {
            const int radius = 11;
            const int diameter = radius * 2;
            GraphicsPath gp = new();
            gp.AddPie(0, 0, diameter, diameter, 180, 90);
            gp.AddPie(Width - diameter, 0, diameter, diameter, 270, 90);
            gp.AddPie(0, Height - diameter, diameter, diameter, 90, 90);
            gp.AddPie(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
            gp.AddRectangle(new Rectangle(radius, 0, Width - diameter, Height));
            gp.AddRectangle(new Rectangle(0, radius, radius, Height - diameter));
            gp.AddRectangle(new Rectangle(Width - radius, radius, radius, Height - diameter));
            Region = new Region(gp);
        }

        private void uRGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnstableRateGraph unstableRateGraph = new(this);
            unstableRateGraph.Show();
        }
    }
}
