using DiscordRPC;
using osu.Game.Rulesets.Scoring;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using RealtimePPUR.Models;
using RealtimePPUR.PPCalculation;
using RealtimePPUR.Utils;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace RealtimePPUR.Forms;

public sealed partial class RealtimePpur : Form
{
    private const string CURRENT_VERSION = "v1.2.0-Release";
    private const string DISCORD_CLIENT_ID = "1237279508239749211";

    private readonly PrivateFontCollection fontCollection = new();
    private readonly Stopwatch stopwatch = new();
    private string ingameoverlayPriority = "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19";
    private readonly int calculateInterval = 15;

    private Point mousePoint = Point.Empty;
    private Point windowLocation = Point.Empty;

    private int softwareMode = 0;
    private int currentBackgroundImage = 1;

    private bool isDirectoryLoaded;
    private bool obsNoticed;

    private bool overlayEnabled;
    private string osuDirectory = string.Empty;
    private string songsPath = string.Empty;

    private PpCalculator? calculator;

    private string preMapPath = string.Empty;
    private string[] prevModStrings = [];

    private bool isPlaying;
    private bool isResultScreen;

    private double avgOffset;
    private double avgOffsethelp;
    private int urValue;

    private int currentBeatmapGamemode;
    private int currentOsuGamemode;
    private int currentGamemode;

    private int preOsuGamemode;

    private BeatmapData? calculatedObject;
    private OsuMemoryStatus currentStatus;

    private static readonly DiscordRpcClient _client = new(DISCORD_CLIENT_ID);
    private HitsResult previousHits = new();
    private string prevErrorMessage = string.Empty;
    public List<int> UnstableRateArray { get; set; } = [];

    private readonly Dictionary<string, string> configDictionary = [];

    private readonly StructuredOsuMemoryReader sreader = StructuredOsuMemoryReader.GetInstance(new("osu!"));
    private readonly OsuBaseAddresses baseAddresses = new();

    private readonly string customSongsFolder;
    private (int left, int top) osuModeValue = new();

    public FontFamily GuiFont = new("Yu Gothic UI");
    public FontFamily InGameOverlayFont = new("Yu Gothic UI");

    private StrainGraph? strainGraph;
    //private UnstableRateGraph? unstableRateGraph;

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out Rect rect);

#if DEBUG
    [LibraryImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();
#endif

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left, Top, Right, Bottom;
    }

    private bool _isOsuRunning = false;
    private bool _isObsRunning = false;
    private Process? _osuProcess = null;

    public RealtimePpur()
    {
#if DEBUG
        AllocConsole();
#endif

        AddFontFile();

        InitializeComponent();
        AdditionalInitialize();

        LogUtils.DebugLogger("RealtimePPUR Initialized.");

        if (File.Exists("Error.log"))
        {
            File.Delete("Error.log");
        }

        if (!File.Exists("Config.cfg"))
        {
            FormUtils.ShowInformationMessageBox("Config.cfgがフォルダ内に存在しないため、すべての項目がOffとして設定されます。アップデートチェックのみ行われます。");
            GithubUtils.CheckUpdate(CURRENT_VERSION);
            inGameValue.Font = new Font(InGameOverlayFont, 19F);
            customSongsFolder = "";
        }
        else
        {
            configDictionary = ConfigUtils.ReadConfigFile("Config.cfg");

            if (CheckConfigDictionary("UPDATECHECK"))
            {
                GithubUtils.CheckUpdate(CURRENT_VERSION);
            }

            var defaultmodeTest = configDictionary.TryGetValue("DEFAULTMODE", out string? defaultmodestring);
            if (defaultmodeTest)
            {
                var defaultModeResult = int.TryParse(defaultmodestring, out int defaultmode);
                if (!defaultModeResult || defaultmode is not (0 or 1 or 2))
                {
                    FormUtils.ShowErrorMessageBox("Config.cfgのDEFAULTMODEの値が不正であったため、初期値の0が適用されます。0、1、2のどれかを入力してください。");
                }
                else if (defaultmode is 1 or 2)
                {
                    ClientSize = new Size(316, 65);
                    currentBackgroundImage = defaultmode + 1;
                    BackgroundImage = defaultmode switch
                    {
                        1 => Properties.Resources.PP,
                        2 => Properties.Resources.UR,
                        _ => BackgroundImage
                    };
                    RoundCorners();

                    if (defaultmode == 2)
                    {
                        foreach (Control control in Controls)
                        {
                            if (control.Name == "inGameValue") continue;
                            control.Location = control.Location with { Y = control.Location.Y - 65 };
                        }
                    }

                    softwareMode = defaultmode;

                    ChangeSoftwareMode(softwareMode);
                }
            }

            // InGameOverlay
            sRToolStripMenuItem.Checked = CheckConfigDictionary("SR");
            sSPPToolStripMenuItem.Checked = CheckConfigDictionary("SSPP");
            currentPPToolStripMenuItem.Checked = CheckConfigDictionary("CURRENTPP");
            currentACCToolStripMenuItem.Checked = CheckConfigDictionary("CURRENTACC");
            hitsToolStripMenuItem.Checked = CheckConfigDictionary("HITS");
            uRToolStripMenuItem.Checked = CheckConfigDictionary("UR");
            offsetHelpToolStripMenuItem.Checked = CheckConfigDictionary("OFFSETHELP");
            avgOffsetToolStripMenuItem.Checked = CheckConfigDictionary("AVGOFFSET");
            progressToolStripMenuItem.Checked = CheckConfigDictionary("PROGRESS");
            ifFCPPToolStripMenuItem.Checked = CheckConfigDictionary("IFFCPP");
            ifFCHitsToolStripMenuItem.Checked = CheckConfigDictionary("IFFCHITS");
            expectedManiaScoreToolStripMenuItem.Checked = CheckConfigDictionary("EXPECTEDMANIASCORE");
            healthPercentageToolStripMenuItem.Checked = CheckConfigDictionary("HEALTHPERCENTAGE");
            currentPositionToolStripMenuItem.Checked = CheckConfigDictionary("CURRENTPOSITION");
            higherScoreToolStripMenuItem.Checked = CheckConfigDictionary("HIGHERSCOREDIFF");
            highestScoreToolStripMenuItem.Checked = CheckConfigDictionary("HIGHESTSCOREDIFF");
            userScoreToolStripMenuItem.Checked = CheckConfigDictionary("USERSCORE");
            currentBPMToolStripMenuItem.Checked = CheckConfigDictionary("CURRENTBPM");
            currentRankToolStripMenuItem.Checked = CheckConfigDictionary("CURRENTRANK");
            remainingNotesToolStripMenuItem.Checked = CheckConfigDictionary("REMAININGNOTES");

            pPLossModeToolStripMenuItem.Checked = CheckConfigDictionary("PPLOSSMODE");
            calculateFirstToolStripMenuItem.Checked = CheckConfigDictionary("CALCULATEFIRST");
            discordRichPresenceToolStripMenuItem.Checked = CheckConfigDictionary("DISCORDRICHPRESENCE");
            ingameoverlayPriority = CheckConfigDictionary("INGAMEOVERLAYPRIORITY", "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16/17/18/19");
            calculateInterval = CheckConfigDictionary("CALCULATEINTERVAL", 15);
            if (configDictionary.TryGetValue("CUSTOMSONGSFOLDER", out string? customSongsFolderValue) && !customSongsFolderValue.Equals("songs", StringComparison.CurrentCultureIgnoreCase))
            {
                customSongsFolder = customSongsFolderValue;
            }
            else
            {
                customSongsFolder = "";
            }

            if (CheckConfigDictionary("USECUSTOMFONT"))
            {
                if (File.Exists("Font"))
                {
                    var fontDictionary = ConfigUtils.ReadConfigFile("Font");

                    var fontName = fontDictionary.TryGetValue("FONTNAME", out string? fontNameValue);
                    var fontSize = fontDictionary.TryGetValue("FONTSIZE", out string? fontSizeValue);
                    var fontStyle = fontDictionary.TryGetValue("FONTSTYLE", out string? fontStyleValue);

                    fontNameValue ??= string.Empty;
                    fontSizeValue ??= string.Empty;
                    fontStyleValue ??= string.Empty;

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
                            FormUtils.ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。");
                            var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string? fontsizeValue);
                            if (!fontsizeResult)
                            {
                                FormUtils.ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                                inGameValue.Font = new Font(InGameOverlayFont, 19F);
                            }
                            else
                            {
                                var result = float.TryParse(fontsizeValue, out float fontsize);
                                if (!result)
                                {
                                    FormUtils.ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
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
                        FormUtils.ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。");
                        var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string? fontsizeValue);
                        if (!fontsizeResult)
                        {
                            FormUtils.ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                            inGameValue.Font = new Font(InGameOverlayFont, 19F);
                        }
                        else
                        {
                            var result = float.TryParse(fontsizeValue, out float fontsize);
                            if (!result)
                            {
                                FormUtils.ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
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
                    var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string? fontsizeValue);
                    if (!fontsizeResult)
                    {
                        FormUtils.ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                        inGameValue.Font = new Font(InGameOverlayFont, 19F);
                    }
                    else
                    {
                        var result = float.TryParse(fontsizeValue, out float fontsize);
                        if (!result)
                        {
                            FormUtils.ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
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
                var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string? fontsizeValue);
                if (!fontsizeResult)
                {
                    FormUtils.ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
                    inGameValue.Font = new Font(InGameOverlayFont, 19F);
                }
                else
                {
                    var result = float.TryParse(fontsizeValue, out float fontsize);
                    if (!result)
                    {
                        FormUtils.ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
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

    #region Initialize
    private void RealtimePpur_Shown(object sender, EventArgs e)
    {
        TopMost = true;

        Thread updateMemoryThread = new(UpdateMemoryData) { IsBackground = true };
        Thread updatePpDataThread = new(UpdatePpData) { IsBackground = true };
        Thread updateDiscordRichPresenceThread = new(UpdateDiscordRichPresence) { IsBackground = true };
        Thread updateProcessStatusThread = new(UpdateProcessStatus) { IsBackground = true };

        updateMemoryThread.Start();
        updatePpDataThread.Start();
        updateDiscordRichPresenceThread.Start();
        updateProcessStatusThread.Start();

        UpdateLoop();
    }

    private void AdditionalInitialize()
    {
            sRToolStripMenuItem.Click += FormUtils.ToggleChecked;
            sSPPToolStripMenuItem.Click += FormUtils.ToggleChecked;
            currentPPToolStripMenuItem.Click += FormUtils.ToggleChecked;
            currentACCToolStripMenuItem.Click += FormUtils.ToggleChecked;
            hitsToolStripMenuItem.Click += FormUtils.ToggleChecked;
            ifFCHitsToolStripMenuItem.Click += FormUtils.ToggleChecked;
            uRToolStripMenuItem.Click += FormUtils.ToggleChecked;
            offsetHelpToolStripMenuItem.Click += FormUtils.ToggleChecked;
            expectedManiaScoreToolStripMenuItem.Click += FormUtils.ToggleChecked;
            avgOffsetToolStripMenuItem.Click += FormUtils.ToggleChecked;
            progressToolStripMenuItem.Click += FormUtils.ToggleChecked;
            ifFCPPToolStripMenuItem.Click += FormUtils.ToggleChecked;
            healthPercentageToolStripMenuItem.Click += FormUtils.ToggleChecked;
            currentPositionToolStripMenuItem.Click += FormUtils.ToggleChecked;
            higherScoreToolStripMenuItem.Click += FormUtils.ToggleChecked;
            highestScoreToolStripMenuItem.Click += FormUtils.ToggleChecked;
            userScoreToolStripMenuItem.Click += FormUtils.ToggleChecked;
            currentBPMToolStripMenuItem.Click += FormUtils.ToggleChecked;
            currentRankToolStripMenuItem.Click += FormUtils.ToggleChecked;
            remainingNotesToolStripMenuItem.Click += FormUtils.ToggleChecked;
            pPLossModeToolStripMenuItem.Click += FormUtils.ToggleChecked;
            calculateFirstToolStripMenuItem.Click += FormUtils.ToggleChecked;
            discordRichPresenceToolStripMenuItem.Click += FormUtils.ToggleChecked;

            avgoffsethelp.Font = new Font(GuiFont, 20F, FontStyle.Bold);
            ur.Font = new Font(GuiFont, 25F, FontStyle.Bold);
            avgoffset.Font = new Font(GuiFont, 13F, FontStyle.Bold);
            miss.Font = new Font(GuiFont, 15F, FontStyle.Bold);
            ok.Font = new Font(GuiFont, 15F, FontStyle.Bold);
            good.Font = new Font(GuiFont, 15F, FontStyle.Bold);
            iffc.Font = new Font(GuiFont, 13F, FontStyle.Bold);
            sr.Font = new Font(GuiFont, 13F, FontStyle.Bold);
            currentPp.Font = new Font(GuiFont, 20F, FontStyle.Bold);
            RoundCorners();
    }

    private void AddFontFile()
    {
        LogUtils.DebugLogger("Loading fonts...");
        fontCollection.AddFontFile("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf");
        fontCollection.AddFontFile("./src/Fonts/IBMPlexSans-Light.ttf");

        foreach (FontFamily font in fontCollection.Families)
        {
            switch (font.Name)
            {
                case "Rounded Mplus 1c ExtraBold":
                    GuiFont = font;
                    break;
                case "IBM Plex Sans Light":
                    InGameOverlayFont = font;
                    break;
            }

            LogUtils.DebugLogger($"Font found: {font.Name}");
        }

        LogUtils.DebugLogger("Fonts loaded.");
    }
    #endregion

    #region Loop
    private async void UpdateLoop()
    {
        while (true)
        {
            try
            {
                await Task.Delay(calculateInterval);

                if (!_isOsuRunning) throw new Exception("osu! is not running.");

                if (!obsNoticed && _isObsRunning)
                {
                    obsNoticed = true;
                    MessageBox.Show("RealtimePPURを録画する際、OBSのウィンドウキャプチャでキャプチャ方法をWindows10 (1903以降)を有効にすると四隅の白い部分が削除されます。\n\nウィンドウキャプチャの[詳細]を開き、[キャプチャ方法]を[Windows10 (1903以降)]に設定してください。", "OBSを検知しました！", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                if (!TopMost) TopMost = true;

                HitsResult hits = new();
                hits.SetValueFromMemory(currentStatus, baseAddresses, isPlaying);

                if (calculatedObject == null) continue;

                double starRatingValue = MathUtils.IsNaNWithNum(Math.Round(calculatedObject.CurrentDifficultyAttributes?.StarRating ?? 0, 2));
                double ssppValue = MathUtils.IsNaNWithNum(calculatedObject.PerformanceAttributes?.Total);

                double currentPpValue = MathUtils.IsNaNWithNum(calculatedObject.CurrentPerformanceAttributes?.Total);
                if (pPLossModeToolStripMenuItem.Checked && !isResultScreen && (currentGamemode is 1 or 3)) currentPpValue = MathUtils.IsNaNWithNum(calculatedObject?.PerformanceAttributesLossMode?.Total);
                
                double ifFcPpValue = MathUtils.IsNaNWithNum(calculatedObject?.PerformanceAttributesIffc?.Total);
                double lossPpValue = MathUtils.IsNaNWithNum(calculatedObject?.PerformanceAttributesLossMode?.Total);

                avgoffset.Text = Math.Round(avgOffset, 2).ToString(CultureInfo.CurrentCulture) + "ms";
                avgoffset.Width = TextRenderer.MeasureText(avgoffset.Text, avgoffset.Font).Width;

                ur.Text = urValue.ToString(CultureInfo.CurrentCulture);
                ur.Width = TextRenderer.MeasureText(ur.Text, ur.Font).Width;

                avgoffsethelp.Text = avgOffsethelp.ToString(CultureInfo.CurrentCulture);
                avgoffsethelp.Width = TextRenderer.MeasureText(avgoffsethelp.Text, avgoffsethelp.Font).Width;

                sr.Text = starRatingValue.ToString(CultureInfo.CurrentCulture);
                sr.Width = TextRenderer.MeasureText(sr.Text, sr.Font).Width;

                if (isPlaying)
                {
                    if (currentGamemode is 1 or 3)
                    {
                        if (pPLossModeToolStripMenuItem.Checked || isResultScreen)
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
                else if (isResultScreen)
                {
                    iffc.Text = Math.Round(ifFcPpValue) + " / " + Math.Round(ssppValue);
                }
                else
                {
                    iffc.Text = Math.Round(ssppValue).ToString();
                }

                iffc.Width = TextRenderer.MeasureText(iffc.Text, iffc.Font).Width;

                currentPp.Text = Math.Round(currentPpValue).ToString();
                currentPp.Width = TextRenderer.MeasureText(currentPp.Text, currentPp.Font).Width;
                currentPp.Left = ClientSize.Width - currentPp.Width - 35;

                HitsResult simplifiedHits = hits.GetSimplifiedHits(currentOsuGamemode);

                good.Text = simplifiedHits.Hit300.ToString();
                good.Width = TextRenderer.MeasureText(good.Text, good.Font).Width;
                good.Left = ((ClientSize.Width - good.Width) / 2) - 120;

                ok.Text = simplifiedHits.Hit100.ToString();
                ok.Width = TextRenderer.MeasureText(ok.Text, ok.Font).Width;
                ok.Left = ((ClientSize.Width - ok.Width) / 2) - 61;

                miss.Text = simplifiedHits.HitMiss.ToString();
                miss.Width = TextRenderer.MeasureText(miss.Text, miss.Font).Width;
                miss.Left = ((ClientSize.Width - miss.Width) / 2) - 3;

                RenderIngameOverlay(hits, calculatedObject, currentGamemode);
            }
            catch (Exception e)
            {
                ErrorLogger(e);

                if (!overlayEnabled)
                {
                    inGameValue.Image?.Dispose();
                    inGameValue.Image = null;
                }

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
                Thread.Sleep(calculateInterval);

                if (!_isOsuRunning) throw new Exception("osu! is not running.");

                if (!isDirectoryLoaded)
                {
                    var (running, path) = ProcessUtils.GetOsuProcess();
                    string tempOsuDirectory = path;

                    LogUtils.DebugLogger($"osu! directory: {tempOsuDirectory}");
                    if (!string.IsNullOrEmpty(tempOsuDirectory) && Directory.Exists(tempOsuDirectory))
                    {
                        osuDirectory = tempOsuDirectory;
                        songsPath = OsuUtils.GetSongsFolderLocation(osuDirectory, customSongsFolder);
                        isDirectoryLoaded = true;
                        LogUtils.DebugLogger($"Songs folder: {songsPath}");
                        LogUtils.DebugLogger("Directory Data initialized.");
                    }
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

                if (currentStatus == OsuMemoryStatus.Playing)
                {
                    isPlaying = true;

                    if (!baseAddresses.Player.IsReplay)
                    {
                        stopwatch.Start();
                    }
                }
                else if (currentStatus != OsuMemoryStatus.ResultsScreen)
                {
                    isPlaying = false;
                    stopwatch.Reset();
                }
                else
                {
                    stopwatch.Reset();
                }

                isResultScreen = currentStatus == OsuMemoryStatus.ResultsScreen;

                currentOsuGamemode = currentStatus switch
                {
                    OsuMemoryStatus.Playing => baseAddresses.Player.Mode,
                    OsuMemoryStatus.ResultsScreen => baseAddresses.ResultsScreen.Mode,
                    _ => baseAddresses.GeneralData.GameMode
                };

                //UnstableRateArray = baseAddresses.Player.HitErrors;
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
                Thread.Sleep(calculateInterval);

                if (Process.GetProcessesByName("osu!").Length == 0) throw new Exception("osu! is not running.");
                if (!isDirectoryLoaded) throw new Exception("Directory not loaded. Skipping...");

                bool isReplay = baseAddresses.Player.IsReplay;
                string currentOsuFileName = baseAddresses.Beatmap.OsuFileName;
                string osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "", currentOsuFileName ?? "");

                if (osuBeatmapPath == songsPath) continue;

                OsuMemoryStatus status = currentStatus;

                if (status == OsuMemoryStatus.Playing)
                {
                    double currentUr = baseAddresses.Player.HitErrors == null || baseAddresses.Player.HitErrors.Count == 0 ? 0 : OsuUtils.CalculateUnstableRate(baseAddresses.Player.HitErrors);
                    double currentAvgOffset = OsuUtils.CalculateAverage(baseAddresses.Player.HitErrors);

                    if (!double.IsNaN(currentUr)) urValue = (int)Math.Round(currentUr);
                    if (!double.IsNaN(currentAvgOffset)) avgOffset = baseAddresses.Player.HitErrors == null || baseAddresses.Player.HitErrors.Count == 0 ? 0 : -Math.Round(currentAvgOffset, 2);
                    avgOffsethelp = (int)Math.Round(-avgOffset);
                }

                if (preMapPath != osuBeatmapPath)
                {
                    preMapPath = osuBeatmapPath;
                    LogUtils.DebugLogger("Map change detected.");
                    LogUtils.DebugLogger($"Current beatmap path: {osuBeatmapPath}");

                    if (!File.Exists(osuBeatmapPath))
                    {
                        // Fix the beatmap path(idk why)
                        LogUtils.DebugLogger("Beatmap file not found. Trying to fix the path... (Attempting 1)");
                        osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName?.Trim() ?? "", currentOsuFileName ?? "");
                        LogUtils.DebugLogger($"Current beatmap path: {osuBeatmapPath}");

                        if (!File.Exists(osuBeatmapPath))
                        {
                            LogUtils.DebugLogger("Beatmap file not found. Trying to fix the path again... (Attempting 2)");
                            osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "", currentOsuFileName?.Trim() ?? "");
                            LogUtils.DebugLogger($"Current beatmap path: {osuBeatmapPath}");
                        }

                        if (!File.Exists(osuBeatmapPath))
                        {
                            LogUtils.DebugLogger("Beatmap file not found. Trying to fix the path again... (Attempting 3)");
                            osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName?.Trim() ?? "", currentOsuFileName?.Trim() ?? "");
                            LogUtils.DebugLogger($"Current beatmap path: {osuBeatmapPath}");
                        }

                        if (File.Exists(osuBeatmapPath)) LogUtils.DebugLogger("Beatmap file found.");
                    }

                    if (!File.Exists(osuBeatmapPath)) throw new Exception("Beatmap file not found.");

                    int currentBeatmapGamemodeTemp = await OsuUtils.GetMapMode(osuBeatmapPath);
                    if (currentBeatmapGamemodeTemp is -1 or not (0 or 1 or 2 or 3)) throw new Exception("Invalid gamemode.");
                    LogUtils.DebugLogger($"Current beatmap gamemode: {currentBeatmapGamemodeTemp}");

                    currentBeatmapGamemode = currentBeatmapGamemodeTemp;
                    currentGamemode = currentBeatmapGamemode == 0 ? currentOsuGamemode : currentBeatmapGamemode;

                    if (calculator == null)
                    {
                        calculator = new PpCalculator(osuBeatmapPath, currentGamemode);
                        LogUtils.DebugLogger("Calculator initialized.");

                        if (strainGraph != null && !strainGraph.IsDisposed)
                        {
                            var strainsData = calculator.GetStrainLists();
                            strainGraph.SetValues(strainsData.Strains, strainsData.SkillNames, calculator.GetFirstObjectTime());
                        }
                    }
                    else
                    {
                        calculator.SetMap(osuBeatmapPath, currentGamemode);

                        LogUtils.DebugLogger("Calculator updated.");

                        if (strainGraph != null && !strainGraph.IsDisposed)
                        {
                            var strainsData = calculator.GetStrainLists();
                            strainGraph.SetValues(strainsData.Strains, strainsData.SkillNames, calculator.GetFirstObjectTime());
                        }
                    }
                }

                if (currentOsuGamemode != preOsuGamemode)
                {
                    if (calculator == null) continue;
                    if (currentBeatmapGamemode == 0 && currentOsuGamemode is 0 or 1 or 2 or 3)
                    {
                        calculator.SetMode(currentOsuGamemode);
                        currentGamemode = currentOsuGamemode;
                        LogUtils.DebugLogger($"Gamemode changed to {currentOsuGamemode}");

                        if (strainGraph != null && !strainGraph.IsDisposed)
                        {
                            var strainsData = calculator.GetStrainLists();
                            strainGraph.SetValues(strainsData.Strains, strainsData.SkillNames, calculator.GetFirstObjectTime());
                        }
                    }

                    preOsuGamemode = currentOsuGamemode;
                }

                if (status == OsuMemoryStatus.EditingMap) currentGamemode = currentBeatmapGamemode;

                HitsResult hits = new();
                hits.SetValueFromMemory(status, baseAddresses, isPlaying);

                if (strainGraph != null && !strainGraph.IsDisposed)
                    strainGraph.UpdateSongProgress(baseAddresses.GeneralData.AudioTime);

                if (hits.Equals(previousHits) && status is OsuMemoryStatus.Playing && !hits.IsEmpty()) continue;
                if (status is OsuMemoryStatus.Playing) previousHits = hits.Clone();

                string[] mods = status switch
                {
                    OsuMemoryStatus.Playing => OsuUtils.ParseMods(baseAddresses.Player.Mods.Value).Calculation,
                    OsuMemoryStatus.ResultsScreen => OsuUtils.ParseMods(baseAddresses.ResultsScreen.Mods.Value).Calculation,
                    OsuMemoryStatus.MainMenu => OsuUtils.ParseMods(baseAddresses.GeneralData.Mods).Calculation,
                    _ => OsuUtils.ParseMods(baseAddresses.GeneralData.Mods).Calculation
                };

                if (isPlaying) mods = OsuUtils.ParseMods(baseAddresses.Player.Mods.Value).Calculation;
                prevModStrings = mods;

                double acc = OsuUtils.CalculateAccuracy(hits, currentGamemode);

                var calcArgs = new CalculateArgs
                {
                    Accuracy = acc,
                    Combo = hits.Combo,
                    Score = hits.Score,
                    Mods = mods,
                    Time = baseAddresses.GeneralData.AudioTime,
                    CalculateBeforePlaying = calculateFirstToolStripMenuItem.Checked
                };

                var result = calculator?.Calculate(calcArgs, isPlaying, isResultScreen && !isPlaying, hits);
                if (result == null) continue;
                calculatedObject = result;
            }
            catch (Exception e)
            {
                ErrorLogger(e);
            }
        }
    }

    private void UpdateProcessStatus()
    {
        while (true)
        {
            try
            {
                var osuProcesses = ProcessUtils.GetProcesses("osu!");
                _isOsuRunning = osuProcesses.Length != 0;
                _osuProcess = osuProcesses.FirstOrDefault();

                if (!obsNoticed) _isObsRunning = ProcessUtils.GetProcesses("obs64").Length != 0;
            }
            catch
            {
                // ignored
            }
            finally
            {
                Thread.Sleep(3000);
            }
        }
    }

    private void UpdateDiscordRichPresence()
    {
        bool hasConnectedToDiscord = false;
        bool hasClearedPresence = false;

        while (!hasConnectedToDiscord)
        {
            try
            {
                var result = _client.Initialize();

                if (result)
                {
                    hasConnectedToDiscord = true;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            catch (Exception e)
            {
                ErrorLogger(e);
                Thread.Sleep(5000);
            }
        }

        LogUtils.DebugLogger("Connected to Discord.");

        while (true)
        {
            try
            {
                Thread.Sleep(2000);

                if (!_isOsuRunning)
                {
                    if (hasClearedPresence) continue;
                    LogUtils.DebugLogger("Discord Rich Presence Cleared.");
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

                HitsResult hits = new();
                hits.SetValueFromMemory(OsuMemoryStatus.Playing, baseAddresses);

                if (calculatedObject == null) continue;

                var richPresence = new RichPresence
                {
                    State = DiscordRichPresenceUtils.CheckString(baseAddresses.Beatmap.MapString),
                    Assets = new Assets()
                    {
                        LargeImageKey = "osu_icon",
                        LargeImageText = $"RealtimePPUR ({CURRENT_VERSION})"
                    }
                };

                if (currentStatus == OsuMemoryStatus.Playing)
                {
                    if (!baseAddresses.Player.IsReplay)
                    {
                        richPresence.Details = DiscordRichPresenceUtils.CheckString(baseAddresses.BanchoUser.Username + OsuUtils.ConvertStatus(baseAddresses.GeneralData.OsuStatus));
                        richPresence.Timestamps = new Timestamps()
                        {
                            Start = DateTime.UtcNow - stopwatch.Elapsed
                        };
                    }
                    else
                    {
                        richPresence.Details = DiscordRichPresenceUtils.CheckString($"{baseAddresses.BanchoUser.Username} is Watching {baseAddresses.Player.Username}'s play");
                    }

                    richPresence.Assets.SmallImageKey = "osu_playing";
                    richPresence.Assets.SmallImageText =
                        $"{Math.Round(MathUtils.IsNaNWithNum(calculatedObject?.CurrentPerformanceAttributes?.Total), 2)}pp  " +
                        $"+{string.Join("", OsuUtils.ParseMods(baseAddresses.Player.Mods.Value).Display)}  " +
                        $"{baseAddresses.Player.Combo}x  " +
                        $"[{OsuUtils.ConvertHits(baseAddresses.Player.Mode, hits)}]";
                }
                else
                {
                    richPresence.Details = DiscordRichPresenceUtils.CheckString(baseAddresses.BanchoUser.Username + OsuUtils.ConvertStatus(baseAddresses.GeneralData.OsuStatus));
                }

                _client.SetPresence(richPresence);
            }
            catch (Exception e)
            {
                ErrorLogger(e);
            }
        }
    }
    #endregion

    #region Software Mode
    private void RealtimePPURToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (softwareMode == 0) return;

        ClientSize = new Size(316, 130);
        BackgroundImage = Properties.Resources.PPUR;
        currentBackgroundImage = 1;
        RoundCorners();

        if (softwareMode == 2)
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "inGameValue") continue;
                control.Location = control.Location with { Y = control.Location.Y + 65 };
            }
        }

        LogUtils.DebugLogger("RealtimePPUR mode enabled.");

        softwareMode = 0;
        ChangeSoftwareMode(softwareMode);
    }

    private void RealtimePPToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (softwareMode == 1) return;

        ClientSize = new Size(316, 65);
        BackgroundImage = Properties.Resources.PP;
        currentBackgroundImage = 2;
        RoundCorners();

        if (softwareMode == 2)
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "inGameValue") continue;
                control.Location = control.Location with { Y = control.Location.Y + 65 };
            }
        }

        LogUtils.DebugLogger("RealtimePP mode enabled.");

        softwareMode = 1;
        ChangeSoftwareMode(softwareMode);
    }

    private void OffsetHelperToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (softwareMode == 2) return;

        ClientSize = new Size(316, 65);
        BackgroundImage = Properties.Resources.UR;
        currentBackgroundImage = 3;
        RoundCorners();

        if (softwareMode is 0 or 1)
        {
            foreach (Control control in Controls)
            {
                if (control.Name == "inGameValue") continue;
                control.Location = control.Location with { Y = control.Location.Y - 65 };
            }
        }

        LogUtils.DebugLogger("Offset Helper mode enabled.");

        softwareMode = 2;
        ChangeSoftwareMode(softwareMode);
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
    #endregion

    #region Font
    private void ChangeFontToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            FontDialog font = new();
            if (font.ShowDialog() != DialogResult.OK) return;

            var selectedFont = font.Font;

            LogUtils.DebugLogger($"Font changed to {selectedFont.Name} {selectedFont.Size} {selectedFont.Style}");

            inGameValue.Font = font.Font;
            DialogResult fontfDialogResult = MessageBox.Show("このフォントを保存しますか？", "情報", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (fontfDialogResult != DialogResult.Yes) return;

            try
            {
                string fontInfo =
                    "※絶対にこのファイルを自分で編集しないでください！\n" + 
                    "※フォント名などを編集してしまうとフォントが見つからず、Windows標準のフォントが割り当てられてしまいます。\n" + 
                    "※もし編集してしまった場合はこのファイルを削除することをお勧めします。\n" + 
                    $"FONTNAME={selectedFont.Name}\nFONTSIZE={selectedFont.Size}\nFONTSTYLE={selectedFont.Style}";

                File.WriteAllText("Font", fontInfo);

                FormUtils.ShowInformationMessageBox("フォントの保存に成功しました。Config.cfgのUSECUSTOMFONTをtrueにすることで起動時から保存されたフォントを使用できます。右クリック→Load Fontからでも読み込むことが可能です！");
            }
            catch
            {
                FormUtils.ShowErrorMessageBox("フォントの保存に失敗しました。もしFontファイルが作成されていたら削除することをお勧めします。");
            }
        }
        catch
        {
            FormUtils.ShowErrorMessageBox("フォントの変更に失敗しました。おそらく対応していないフォントです。");
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

            fontDictionaryLoad.TryGetValue("FONTNAME", out string? fontNameValue);
            fontDictionaryLoad.TryGetValue("FONTSIZE", out string? fontSizeValue);
            fontDictionaryLoad.TryGetValue("FONTSTYLE", out string? fontStyleValue);

            fontNameValue ??= string.Empty;
            fontSizeValue ??= string.Empty;
            fontStyleValue ??= string.Empty;

            if (string.IsNullOrEmpty(fontNameValue) || string.IsNullOrEmpty(fontSizeValue) || string.IsNullOrEmpty(fontStyleValue))
            {
                FormUtils.ShowErrorMessageBox("Fontファイルのフォント情報が不正であったため、読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。");
                return;
            }

            try
            {
                LogUtils.DebugLogger($"Font loaded: {fontNameValue} {fontSizeValue} {fontStyleValue}");
                inGameValue.Font = new Font(fontNameValue, float.Parse(fontSizeValue),
                    (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleValue));
                MessageBox.Show(
                    $"フォントの読み込みに成功しました。\n\nフォント名: {fontNameValue}\nサイズ: {fontSizeValue}\nスタイル: {fontStyleValue}",
                    "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch
            {
                FormUtils.ShowErrorMessageBox("Fontファイルのフォント情報が不正、もしくは非対応であったため読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。");
            }
        }
        else
        {
            FormUtils.ShowErrorMessageBox("Fontファイルが存在しません。一度Change Fontでフォントを保存してください。");
        }
    }

    private void ResetFontToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (InGameOverlayFont == null)
        {
            FormUtils.ShowErrorMessageBox("InGameOverlayFontが見つからなかったため、フォントのリセットに失敗しました。");
            return;
        }

        var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string? fontsizeValue);
        if (!fontsizeResult)
        {
            FormUtils.ShowErrorMessageBox("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。");
            inGameValue.Font = new Font(InGameOverlayFont, 19F);
        }
        else
        {
            var result = float.TryParse(fontsizeValue, out float fontsize);
            if (!result)
            {
                FormUtils.ShowErrorMessageBox("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。");
                inGameValue.Font = new Font(InGameOverlayFont, 19F);
            }
            else
            {
                inGameValue.Font = new Font(InGameOverlayFont, fontsize);
            }

            FormUtils.ShowInformationMessageBox("フォントのリセットが完了しました！");
        }

        LogUtils.DebugLogger("Font reset.");
    }
    #endregion

    #region Move Window
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
    #endregion

    #region Close
    private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        => Close();

    private void RealtimePPUR_Closed(object sender, EventArgs e)
        => Application.Exit();
    #endregion

    #region IngameOverlay
    private void OsuModeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var lefttest = configDictionary.TryGetValue("LEFT", out string? leftvalue);
        var toptest = configDictionary.TryGetValue("TOP", out string? topvalue);

        if (!lefttest || !toptest || string.IsNullOrEmpty(leftvalue) || string.IsNullOrEmpty(topvalue))
        {
            FormUtils.ShowErrorMessageBox("Config.cfgにLEFTまたはTOPの値が存在しなかったため、InGameOverlayの起動に失敗しました。");
            return;
        }

        var leftResult = int.TryParse(leftvalue, out int left);
        var topResult = int.TryParse(topvalue, out int top);

        if ((!leftResult || !topResult) && !IsOsuMode)
        {
            FormUtils.ShowErrorMessageBox("Config.cfgのLEFT、またはTOPの値が不正であったため、InGameOverlayの起動に失敗しました。LEFT、TOPには数値以外入力しないでください。");
            return;
        }

        osuModeValue.left = left;
        osuModeValue.top = top;
        osuModeToolStripMenuItem.Checked = !IsOsuMode;
    }
    
    private bool IsOsuMode
        => osuModeToolStripMenuItem.Checked;

    private void ChangePriorityToolStripMenuItem_Click(object sender, EventArgs e)
    {
        ChangePriorityForm priorityForm = new();
        priorityForm.Show();

        priorityForm.PriorityChanged += (s, e) =>
        {
            if (s is not string priorityValue) return;
            ingameoverlayPriority = priorityValue;
        };
    }

    private void RenderIngameOverlay(HitsResult hits, BeatmapData? calculatedData, int currentGamemodeValue)
    {
        CheckOsuMode();
        if (!overlayEnabled) return;

        var inGameValueText = SetIngameValue(calculatedData, hits, currentGamemodeValue);

        using (Bitmap tempBitmap = new(1, 1))
        using (Graphics g = Graphics.FromImage(tempBitmap))
        {
            var size = g.MeasureString(inGameValueText, inGameValue.Font);
            inGameValue.Size = new Size((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height));
        }

        Bitmap canvas = new(inGameValue.Width, inGameValue.Height);

        using (Graphics g = Graphics.FromImage(canvas))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.DrawString(inGameValueText, inGameValue.Font, Brushes.White, 0, 0);
        }

        inGameValue.Image = canvas;
    }

    private void CheckOsuMode()
    {
        void EnableOverlay(Rect rect)
        {
            if (!overlayEnabled)
            {
                windowLocation = Location;
                overlayEnabled = true;
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
            Location = new Point(rect.Left + osuModeValue.left + 2, rect.Top + osuModeValue.top);
        }

        void DisableOverlay()
        {
            switch (softwareMode)
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

            Location = windowLocation;
            overlayEnabled = false;

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

        if (IsOsuMode)
        {
            if (_osuProcess != null)
            {
                IntPtr osuMainWindowHandle = _osuProcess.MainWindowHandle;

                bool windowRect = GetWindowRect(osuMainWindowHandle, out Rect rect);
                bool isPlaying = baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing;

                if (windowRect && isPlaying && GetForegroundWindow() == osuMainWindowHandle && osuMainWindowHandle != IntPtr.Zero)
                {
                    EnableOverlay(rect);
                }
                else if (overlayEnabled)
                {
                    DisableOverlay();
                }
            }
            else if (overlayEnabled)
            {
                DisableOverlay();   
            }
        }
        else if (overlayEnabled)
        {
            DisableOverlay();
        }
    }

    private string SetIngameValue(BeatmapData? calculatedData, HitsResult hits, int currentGamemodeValue)
    {
        if (calculatedData == null || hits == null) return string.Empty;

        double starRatingValue = MathUtils.IsNaNWithNum(Math.Round(calculatedData?.CurrentDifficultyAttributes?.StarRating ?? 0, 2));
        double fullSrValue = MathUtils.IsNaNWithNum(Math.Round(calculatedData?.DifficultyAttributes?.StarRating ?? 0, 2));
        double ssppValue = MathUtils.IsNaNWithNum(calculatedData?.PerformanceAttributes?.Total);
        double currentPpValue = MathUtils.IsNaNWithNum(calculatedData?.CurrentPerformanceAttributes?.Total);
        double ifFcPpValue = MathUtils.IsNaNWithNum(calculatedData?.PerformanceAttributesIffc?.Total);
        double lossModePpValue = MathUtils.IsNaNWithNum(calculatedData?.PerformanceAttributesLossMode?.Total);

        var leaderBoardData = OsuUtils.GetLeaderBoard(baseAddresses.LeaderBoard, baseAddresses.Player.Score);
        double healthPercentage = MathUtils.IsNaNWithNum(Math.Round(baseAddresses.Player.HP / 2, 1));
        int userScore = hits.Score;

        int currentPosition = leaderBoardData["currentPosition"];
        int higherScore = leaderBoardData["higherScore"];
        int highestScore = leaderBoardData["highestScore"];

        var ingameoverlayPriorityArray = ingameoverlayPriority.Replace(" ", "").Split('/');

        string displayFormat = "";
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
                            if (calculatedData != null)
                                displayFormat += "ACC: " + Math.Round(baseAddresses.Player.Accuracy, 2) + " / " + Math.Round(CalculatorUtils.GetAccuracy(calculatedData.HitResultLossMode, currentGamemode) * 100, 2) + "%\n";
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
                        displayFormat += $"Hits: {OsuUtils.ConvertHits(currentGamemode, hits)}\n";
                    }

                    break;

                case 6:
                    if (ifFCHitsToolStripMenuItem.Checked && calculatedData != null)
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
                    if (expectedManiaScoreToolStripMenuItem.Checked && currentGamemodeValue == 3 && calculatedData != null)
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
                        var progress = baseAddresses.GeneralData.TotalAudioTime > 0
                            ? Math.Round(baseAddresses.GeneralData.AudioTime / baseAddresses.GeneralData.TotalAudioTime * 100, 1)
                            : 0;
                        displayFormat += "Progress: " + progress.ToString() + "%\n";
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
                    if (highestScoreToolStripMenuItem.Checked)
                    {
                        if (highestScore == 0)
                        {
                            displayFormat += "HighestDiff: No score" + "\n";
                        }
                        else if (currentPosition == 1)
                        {
                            displayFormat += "HighestDiff: Top!" + "\n";
                        }
                        else
                        {
                            displayFormat += "HighestDiff: " + (highestScore - userScore) + "\n";
                        }
                    }

                    break;

                case 16:
                    if (userScoreToolStripMenuItem.Checked)
                    {
                        displayFormat += "Score: " + userScore + "\n";
                    }

                    break;

                case 17:
                    if (currentBPMToolStripMenuItem.Checked && calculatedData != null)
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
                        var mods = OsuUtils.ParseMods(baseAddresses.Player.Mods.Value).Display;
                        var currentRank = CalculatorUtils.GetCurrentRank(calculatedData?.HitResults ?? [], currentGamemodeValue, mods);
                        var currentRankLossMode = CalculatorUtils.GetCurrentRank(calculatedData?.HitResultLossMode ?? [], currentGamemodeValue, mods);
                        displayFormat += "Rank: " + currentRank + " / " + currentRankLossMode + "\n";
                    }

                    break;

                case 19:
                    if (remainingNotesToolStripMenuItem.Checked && calculatedData != null)
                    {
                        var totalNotes = calculatedData.TotalHitObjectCount;

                        int currentNotes = currentGamemodeValue switch
                        {
                            0 => hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitMiss,
                            1 => hits.Hit300 + hits.Hit100 + hits.HitMiss,
                            2 => hits.Hit300 + hits.Hit100 + hits.HitMiss,
                            3 => hits.HitGeki + hits.Hit300 + hits.HitKatu + hits.Hit100 + hits.Hit50 + hits.HitMiss,
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
    #endregion

    #region Config
    private bool CheckConfigDictionary(string key)
        => configDictionary.TryGetValue(key, out string? test) && test == "true";

    private string CheckConfigDictionary(string key, string value)
        => configDictionary.TryGetValue(key, out string? test) ? test : value;

    private int CheckConfigDictionary(string key, int value)
    {
        if (configDictionary.TryGetValue(key, out string? test))
        {
            var result = int.TryParse(test, out int testInt);
            return result ? testInt : value;
        }

        return value;
    }

    // Save Config
    private void SaveConfigToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            const string filePath = "Config.cfg";
            if (!File.Exists(filePath))
            {
                FormUtils.ShowErrorMessageBox("Config.cfgが見つかりませんでした。RealtimePPURをダウンロードし直してください。");
                return;
            }

            var parameters = new Dictionary<string, string>
            {
                { "SR", ConfigUtils.ConfigValueToString(sRToolStripMenuItem.Checked) },
                { "SSPP", ConfigUtils.ConfigValueToString(sSPPToolStripMenuItem.Checked) },
                { "CURRENTPP", ConfigUtils.ConfigValueToString(currentPPToolStripMenuItem.Checked ) },
                { "CURRENTACC", ConfigUtils.ConfigValueToString(currentACCToolStripMenuItem.Checked) },
                { "HITS", ConfigUtils.ConfigValueToString(hitsToolStripMenuItem.Checked) },
                { "IFFCHITS", ConfigUtils.ConfigValueToString(ifFCHitsToolStripMenuItem.Checked) },
                { "UR", ConfigUtils.ConfigValueToString(uRToolStripMenuItem.Checked) },
                { "OFFSETHELP", ConfigUtils.ConfigValueToString(offsetHelpToolStripMenuItem.Checked) },
                { "EXPECTEDMANIASCORE", ConfigUtils.ConfigValueToString(expectedManiaScoreToolStripMenuItem.Checked) },
                { "AVGOFFSET", ConfigUtils.ConfigValueToString(avgOffsetToolStripMenuItem.Checked) },
                { "PROGRESS", ConfigUtils.ConfigValueToString(progressToolStripMenuItem.Checked) },
                { "IFFCPP", ConfigUtils.ConfigValueToString(ifFCPPToolStripMenuItem.Checked) },
                { "HEALTHPERCENTAGE", ConfigUtils.ConfigValueToString(healthPercentageToolStripMenuItem.Checked) },
                { "CURRENTPOSITION", ConfigUtils.ConfigValueToString(currentPositionToolStripMenuItem.Checked) },
                { "HIGHERSCOREDIFF", ConfigUtils.ConfigValueToString(higherScoreToolStripMenuItem.Checked) },
                { "USERSCORE", ConfigUtils.ConfigValueToString(userScoreToolStripMenuItem.Checked) },
                { "CURRENTBPM", ConfigUtils.ConfigValueToString(currentBPMToolStripMenuItem.Checked) },
                { "CURRENTRANK", ConfigUtils.ConfigValueToString(currentRankToolStripMenuItem.Checked) },
                { "REMAININGNOTES", ConfigUtils.ConfigValueToString(remainingNotesToolStripMenuItem.Checked) },
                { "PPLOSSMODE", ConfigUtils.ConfigValueToString(pPLossModeToolStripMenuItem.Checked) },
                { "CALCULATEFIRST", ConfigUtils.ConfigValueToString(calculateFirstToolStripMenuItem.Checked) },
                { "DISCORDRICHPRESENCE", ConfigUtils.ConfigValueToString(discordRichPresenceToolStripMenuItem.Checked) }
            };

            ConfigUtils.WriteConfigFile(filePath, parameters);
            FormUtils.ShowInformationMessageBox("Config.cfgの保存が完了しました！");
        }
        catch (Exception error)
        {
            ErrorLogger(error);
            FormUtils.ShowErrorMessageBox("Config.cfgの保存に失敗しました。");
        }
    }
    #endregion

    #region Error Logger
    private void ErrorLogger(Exception error)
    {
        try
        {
            if (error.Message == prevErrorMessage) return;

            LogUtils.DebugLogger("Error: " + error.Message, true, true);
            prevErrorMessage = error.Message;
        }
        catch
        {
            LogUtils.DebugLogger("Error Logger Failed");
        }
    }
    #endregion

    #region Form Methods
    private void RoundCorners()
        => Region = FormUtils.RoundCorners(Width, Height);
    #endregion

    #region Event Handler
    private void URGraphToolStripMenuItem_Click(object sender, EventArgs e)
    {
        FormUtils.ShowErrorMessageBox("現在、この機能は無効化されています。アップデートで機能の修正が終わり次第、有効化されます。");
        //if (unstableRateGraph == null || unstableRateGraph.IsDisposed) unstableRateGraph = new UnstableRateGraph(this);
        //unstableRateGraph.Show();
    }

    private void StrainGraphToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (strainGraph == null || strainGraph.IsDisposed) strainGraph = new StrainGraph();
        strainGraph.Show();
    }
    #endregion
}
