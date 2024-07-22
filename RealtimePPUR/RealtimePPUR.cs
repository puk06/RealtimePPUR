using DiscordRPC;
using Octokit;
using osu.Game.IO;
using osu.Game.Rulesets.Scoring;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimePPUR
{
    public sealed partial class RealtimePpur : Form
    {
        private const string CURRENT_VERSION = "v1.0.8-Release";

        private System.Windows.Forms.Label currentPp, sr, iffc, good, ok, miss, avgoffset, ur, avgoffsethelp;

        private readonly PrivateFontCollection fontCollection;
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
        private string preTitle;
        private PpCalculator calculator;
        private bool isplaying;
        private bool isResultScreen;
        private double avgOffset;
        private double avgOffsethelp;
        private int urValue;
        private const bool IS_NO_CLASSIC_MOD = true;
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

        private readonly Dictionary<string, string> configDictionary = new();
        private readonly StructuredOsuMemoryReader sreader = new();
        private readonly OsuBaseAddresses baseAddresses = new();
        private readonly string customSongsFolder;
        public static readonly Dictionary<int, string> OSU_MODS = new()
        {
            { 0, "NM" },
            { 1, "NF" },
            { 2, "EZ" },
            { 4, "TD" },
            { 8, "HD" },
            { 16, "HR" },
            { 32, "SD" },
            { 64, "DT" },
            { 128, "RX" },
            { 256, "HT" },
            { 512, "NC" },
            { 1024, "FL" },
            { 2048, "AT" },
            { 4096, "SO" },
            { 8192, "RX2" },
            { 16384, "PF" },
            { 32768, "4K" },
            { 65536, "5K" },
            { 131072, "6K" },
            { 262144, "7K" },
            { 524288, "8K" },
            { 1048576, "FI" },
            { 2097152, "RD" },
            { 4194304, "CM" },
            { 8388608, "TP" },
            { 16777216, "9K" },
            { 33554432, "CP" },
            { 67108864, "1K" },
            { 134217728, "3K" },
            { 268435456, "2K" },
            { 536870912, "SV2" },
            { 1073741824, "MR" }
        };
        private readonly Dictionary<string, int> _osuModeValue = new()
        {
            { "left", 0 },
            { "top", 0 }
        };

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out Rect rect);

        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int Left, Top, Right, Bottom;
        }

        public RealtimePpur()
        {
            fontCollection = new PrivateFontCollection();
            fontCollection.AddFontFile("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf");
            fontCollection.AddFontFile("./src/Fonts/Nexa Light.otf");
            InitializeComponent();

            if (File.Exists("Error.log")) File.Delete("Error.log");

            if (!File.Exists("Config.cfg"))
            {
                MessageBox.Show("Config.cfgがフォルダ内に存在しないため、すべての項目がOffとして設定されます。アップデートチェックのみ行われます。", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                GithubUpdateChecker();
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
                discordRichPresenceToolStripMenuItem.Checked = false;
                pPLossModeToolStripMenuItem.Checked = false;
                ingameoverlayPriority = "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16";
                inGameValue.Font = new Font(fontCollection.Families[0], 19F);
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

                if (configDictionary.TryGetValue("UPDATECHECK", out string test11) && test11 == "true")
                {
                    GithubUpdateChecker();
                }

                var defaultmodeTest = configDictionary.TryGetValue("DEFAULTMODE", out string defaultmodestring);
                if (defaultmodeTest)
                {
                    var defaultModeResult = int.TryParse(defaultmodestring, out int defaultmode);
                    if (!defaultModeResult || defaultmode is not (0 or 1 or 2))
                    {
                        MessageBox.Show("Config.cfgのDEFAULTMODEの値が不正であったため、初期値の0が適用されます。0、1、2のどれかを入力してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                    }
                }

                sRToolStripMenuItem.Checked = configDictionary.TryGetValue("SR", out string test) && test.ToLower() == "true";
                sSPPToolStripMenuItem.Checked = configDictionary.TryGetValue("SSPP", out string test2) && test2.ToLower() == "true";
                currentPPToolStripMenuItem.Checked = configDictionary.TryGetValue("CURRENTPP", out string test3) && test3.ToLower() == "true";
                currentACCToolStripMenuItem.Checked = configDictionary.TryGetValue("CURRENTACC", out string test4) && test4.ToLower() == "true";
                hitsToolStripMenuItem.Checked = configDictionary.TryGetValue("HITS", out string test5) && test5.ToLower() == "true";
                uRToolStripMenuItem.Checked = configDictionary.TryGetValue("UR", out string test6) && test6.ToLower() == "true";
                offsetHelpToolStripMenuItem.Checked = configDictionary.TryGetValue("OFFSETHELP", out string test7) && test7.ToLower() == "true";
                avgOffsetToolStripMenuItem.Checked = configDictionary.TryGetValue("AVGOFFSET", out string test8) && test8.ToLower() == "true";
                progressToolStripMenuItem.Checked = configDictionary.TryGetValue("PROGRESS", out string test9) && test9.ToLower() == "true";
                ifFCPPToolStripMenuItem.Checked = configDictionary.TryGetValue("IFFCPP", out string test13) && test13.ToLower() == "true";
                ifFCHitsToolStripMenuItem.Checked = configDictionary.TryGetValue("IFFCHITS", out string test14) && test14.ToLower() == "true";
                expectedManiaScoreToolStripMenuItem.Checked = configDictionary.TryGetValue("EXPECTEDMANIASCORE", out string test15) && test15.ToLower() == "true";
                healthPercentageToolStripMenuItem.Checked = configDictionary.TryGetValue("HEALTHPERCENTAGE", out string test17) && test17.ToLower() == "true";
                currentPositionToolStripMenuItem.Checked = configDictionary.TryGetValue("CURRENTPOSITION", out string test18) && test18.ToLower() == "true";
                higherScoreToolStripMenuItem.Checked = configDictionary.TryGetValue("HIGHERSCOREDIFF", out string test19) && test19.ToLower() == "true";
                highestScoreToolStripMenuItem.Checked = configDictionary.TryGetValue("HIGHESTSCOREDIFF", out string test20) && test20.ToLower() == "true";
                userScoreToolStripMenuItem.Checked = configDictionary.TryGetValue("USERSCORE", out string test21) && test21.ToLower() == "true";
                pPLossModeToolStripMenuItem.Checked = configDictionary.TryGetValue("PPLOSSMODE", out string test22) && test22.ToLower() == "true";
                discordRichPresenceToolStripMenuItem.Checked = configDictionary.TryGetValue("DISCORDRICHPRESENCE", out string test23) && test23.ToLower() == "true";
                ingameoverlayPriority = configDictionary.TryGetValue("INGAMEOVERLAYPRIORITY", out string test16) ? test16 : "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16";
                if (configDictionary.TryGetValue("CUSTOMSONGSFOLDER", out string test24) && test24.ToLower() != "songs")
                {
                    customSongsFolder = test24;
                }
                else
                {
                    customSongsFolder = "";
                }


                if (configDictionary.TryGetValue("USECUSTOMFONT", out string test12) && test12 == "true")
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

                        if (fontDictionary.Count == 3 && fontName && fontNameValue != "" && fontSize && fontSizeValue != "" && fontStyle && fontStyleValue != "")
                        {
                            try
                            {
                                inGameValue.Font = new Font(fontNameValue, float.Parse(fontSizeValue),
                                    (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleValue));
                            }
                            catch
                            {
                                MessageBox.Show("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                                if (!fontsizeResult)
                                {
                                    MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                                }
                                else
                                {
                                    var result = float.TryParse(fontsizeValue, out float fontsize);
                                    if (!result)
                                    {
                                        MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                                    }
                                    else
                                    {
                                        inGameValue.Font = new Font(fontCollection.Families[0], fontsize);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                            if (!fontsizeResult)
                            {
                                MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                            }
                            else
                            {
                                var result = float.TryParse(fontsizeValue, out float fontsize);
                                if (!result)
                                {
                                    MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                                }
                                else
                                {
                                    inGameValue.Font = new Font(fontCollection.Families[0], fontsize);
                                }
                            }
                        }
                    }
                    else
                    {
                        var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                        if (!fontsizeResult)
                        {
                            MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                        }
                        else
                        {
                            var result = float.TryParse(fontsizeValue, out float fontsize);
                            if (!result)
                            {
                                MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                            }
                            else
                            {
                                inGameValue.Font = new Font(fontCollection.Families[0], fontsize);
                            }
                        }
                    }
                }
                else
                {
                    var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                    if (!fontsizeResult)
                    {
                        MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                    }
                    else
                    {
                        var result = float.TryParse(fontsizeValue, out float fontsize);
                        if (!result)
                        {
                            MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                        }
                        else
                        {
                            inGameValue.Font = new Font(fontCollection.Families[0], fontsize);
                        }
                    }
                }
            }
        }

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

        private async void UpdateLoop()
        {
            while (true)
            {
                await Task.Delay(15);
                try
                {
                    if (Process.GetProcessesByName("osu!").Length == 0) throw new Exception("osu! is not running.");
                    bool isplaying = this.isplaying;
                    bool isResultScreen = this.isResultScreen;
                    int currentGamemode = this.currentGamemode;
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

                    if (isplaying)
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

                    var leaderBoardData = GetLeaderBoard(baseAddresses.LeaderBoard, baseAddresses.Player.Score);
                    double sr = IsNaNWithNum(Math.Round(calculatedObject.CurrentDifficultyAttributes.StarRating, 2));
                    double fullSr = IsNaNWithNum(Math.Round(calculatedObject.DifficultyAttributes.StarRating, 2));
                    double sspp = IsNaNWithNum(calculatedObject.PerformanceAttributes.Total);
                    double currentPp = IsNaNWithNum(calculatedObject.CurrentPerformanceAttributes.Total);
                    double ifFcpp = IsNaNWithNum(calculatedObject.PerformanceAttributesIffc.Total);

                    int geki = hits.HitGeki;
                    int good = hits.Hit300;
                    int katu = hits.HitKatu;
                    int ok = hits.Hit100;
                    int bad = hits.Hit50;
                    int miss = hits.HitMiss;

                    double healthPercentage = IsNaNWithNum(Math.Round(baseAddresses.Player.HP / 2, 1));
                    int userScore = hits.Score;

                    int currentPosition = leaderBoardData["currentPosition"];
                    int higherScore = leaderBoardData["higherScore"];
                    int highestScore = leaderBoardData["highestScore"];

                    avgoffset.Text = Math.Round(avgOffset, 2) + "ms";
                    avgoffset.Width = TextRenderer.MeasureText(avgoffset.Text, avgoffset.Font).Width;

                    ur.Text = urValue.ToString();
                    ur.Width = TextRenderer.MeasureText(ur.Text, ur.Font).Width;

                    avgoffsethelp.Text = avgOffsethelp.ToString();
                    avgoffsethelp.Width = TextRenderer.MeasureText(avgoffsethelp.Text, avgoffsethelp.Font).Width;

                    this.sr.Text = sr.ToString();
                    this.sr.Width = TextRenderer.MeasureText(this.sr.Text, this.sr.Font).Width;

                    iffc.Text = (isplaying || isResultScreen) && currentGamemode != 3 ? Math.Round(ifFcpp) + " / " + Math.Round(sspp) : Math.Round(sspp).ToString();
                    iffc.Width = TextRenderer.MeasureText(iffc.Text, iffc.Font).Width;

                    this.currentPp.Text = Math.Round(currentPp).ToString();
                    this.currentPp.Width = TextRenderer.MeasureText(this.currentPp.Text, this.currentPp.Font).Width;
                    this.currentPp.Left = ClientSize.Width - this.currentPp.Width - 35;

                    switch (this.currentGamemode)
                    {
                        case 0:
                            this.good.Text = good.ToString();
                            this.good.Width = TextRenderer.MeasureText(this.good.Text, this.good.Font).Width;
                            this.good.Left = (ClientSize.Width - this.good.Width) / 2 - 120;

                            this.ok.Text = (ok + bad).ToString();
                            this.ok.Width = TextRenderer.MeasureText(this.ok.Text, this.ok.Font).Width;
                            this.ok.Left = (ClientSize.Width - this.ok.Width) / 2 - 61;

                            this.miss.Text = miss.ToString();
                            this.miss.Width = TextRenderer.MeasureText(this.miss.Text, this.miss.Font).Width;
                            this.miss.Left = (ClientSize.Width - this.miss.Width) / 2 - 3;
                            break;

                        case 1:
                            this.good.Text = good.ToString();
                            this.good.Width = TextRenderer.MeasureText(this.good.Text, this.good.Font).Width;
                            this.good.Left = (ClientSize.Width - this.good.Width) / 2 - 120;

                            this.ok.Text = ok.ToString();
                            this.ok.Width = TextRenderer.MeasureText(this.ok.Text, this.ok.Font).Width;
                            this.ok.Left = (ClientSize.Width - this.ok.Width) / 2 - 61;

                            this.miss.Text = miss.ToString();
                            this.miss.Width = TextRenderer.MeasureText(this.miss.Text, this.miss.Font).Width;
                            this.miss.Left = (ClientSize.Width - this.miss.Width) / 2 - 3;
                            break;

                        case 2:
                            this.good.Text = good.ToString();
                            this.good.Width = TextRenderer.MeasureText(this.good.Text, this.good.Font).Width;
                            this.good.Left = (ClientSize.Width - this.good.Width) / 2 - 120;

                            this.ok.Text = (ok + bad).ToString();
                            this.ok.Width = TextRenderer.MeasureText(this.ok.Text, this.ok.Font).Width;
                            this.ok.Left = (ClientSize.Width - this.ok.Width) / 2 - 61;

                            this.miss.Text = miss.ToString();
                            this.miss.Width = TextRenderer.MeasureText(this.miss.Text, this.miss.Font).Width;
                            this.miss.Left = (ClientSize.Width - this.miss.Width) / 2 - 3;
                            break;

                        case 3:
                            this.good.Text = (good + geki).ToString();
                            this.good.Width = TextRenderer.MeasureText(this.good.Text, this.good.Font).Width;
                            this.good.Left = (ClientSize.Width - this.good.Width) / 2 - 120;

                            this.ok.Text = (katu + ok + bad).ToString();
                            this.ok.Width = TextRenderer.MeasureText(this.ok.Text, this.ok.Font).Width;
                            this.ok.Left = (ClientSize.Width - this.ok.Width) / 2 - 61;

                            this.miss.Text = miss.ToString();
                            this.miss.Width = TextRenderer.MeasureText(this.miss.Text, this.miss.Font).Width;
                            this.miss.Left = (ClientSize.Width - this.miss.Width) / 2 - 3;
                            break;
                    }

                    displayFormat = "";
                    var ingameoverlayPriorityArray = ingameoverlayPriority.Replace(" ", "").Split('/');
                    foreach (var priorityValue in ingameoverlayPriorityArray)
                    {
                        var priorityValueResult = int.TryParse(priorityValue, out int priorityValueInt);
                        if (!priorityValueResult) continue;
                        switch (priorityValueInt)
                        {
                            case 1:
                                if (sRToolStripMenuItem.Checked)
                                {
                                    if (pPLossModeToolStripMenuItem.Checked && this.currentGamemode is 1 or 3)
                                    {
                                        displayFormat += "SR: " + sr + "\n";
                                    }
                                    else
                                    {
                                        displayFormat += "SR: " + sr + " / " + fullSr + "\n";
                                    }
                                }

                                break;

                            case 2:
                                if (sSPPToolStripMenuItem.Checked)
                                {
                                    displayFormat += "SSPP: " + Math.Round(sspp) + "pp\n";
                                }

                                break;

                            case 3:
                                if (currentPPToolStripMenuItem.Checked)
                                {
                                    displayFormat += ifFCPPToolStripMenuItem.Checked switch
                                    {
                                        true when currentGamemode != 3 => "PP: " + Math.Round(currentPp) + " / " + Math.Round(ifFcpp) + "pp\n",
                                        true => "PP: " + Math.Round(currentPp) + " / " + Math.Round(sspp) + "pp\n",
                                        _ => "PP: " + Math.Round(currentPp) + "pp\n"
                                    };
                                }

                                break;

                            case 4:
                                if (currentACCToolStripMenuItem.Checked)
                                {
                                    displayFormat += "ACC: " + Math.Round(baseAddresses.Player.Accuracy, 2) + "%\n";
                                }

                                break;

                            case 5:
                                if (hitsToolStripMenuItem.Checked)
                                {
                                    switch (currentGamemode)
                                    {
                                        case 0:
                                            displayFormat += $"Hits: {good}/{ok}/{bad}/{miss}\n";
                                            break;

                                        case 1:
                                            displayFormat += $"Hits: {good}/{ok}/{miss}\n";
                                            break;

                                        case 2:
                                            displayFormat += $"Hits: {good}/{ok}/{bad}/{miss}\n";
                                            break;

                                        case 3:
                                            displayFormat += $"Hits: {geki}/{good}/{katu}/{ok}/{bad}/{miss}\n";
                                            break;
                                    }
                                }

                                break;

                            case 6:
                                if (ifFCHitsToolStripMenuItem.Checked)
                                {
                                    int ifFcGood = calculatedObject.IfFcHitResult[HitResult.Great];
                                    int ifFcOk = currentGamemode == 2
                                        ? calculatedObject.IfFcHitResult[HitResult.LargeTickHit]
                                        : calculatedObject.IfFcHitResult[HitResult.Ok];
                                    int ifFcBad = currentGamemode switch
                                    {
                                        0 => calculatedObject.IfFcHitResult[HitResult.Meh],
                                        1 => 0,
                                        2 => calculatedObject.IfFcHitResult[HitResult.SmallTickHit],
                                        _ => 0
                                    };
                                    const int ifFcMiss = 0;

                                    switch (currentGamemode)
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
                                    displayFormat += "OffsetHelp: " + Math.Round(avgOffsethelp) + "\n";
                                }

                                break;

                            case 9:
                                if (expectedManiaScoreToolStripMenuItem.Checked && currentGamemode == 3)
                                {
                                    displayFormat += "ManiaScore: " + calculatedObject.ExpectedManiaScore + "\n";
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
                                        displayFormat += "HighestDiff: You're Top!!" + "\n";
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
                        }
                    }

                    inGameValue.Text = displayFormat;

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
                                this.sr.Visible = false;
                                iffc.Visible = false;
                                this.currentPp.Visible = false;
                                this.good.Visible = false;
                                this.ok.Visible = false;
                                this.miss.Visible = false;
                                avgoffset.Visible = false;
                                ur.Visible = false;
                                Region = null;
                                Size = new Size(inGameValue.Width, inGameValue.Height);
                                Location = new Point(rect.Left + _osuModeValue["left"] + 2,
                                    rect.Top + _osuModeValue["top"]);
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
                                this.sr.Visible = true;
                                iffc.Visible = true;
                                this.currentPp.Visible = true;
                                this.good.Visible = true;
                                this.ok.Visible = true;
                                this.miss.Visible = true;
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
                            this.sr.Visible = true;
                            iffc.Visible = true;
                            this.currentPp.Visible = true;
                            this.good.Visible = true;
                            this.ok.Visible = true;
                            this.miss.Visible = true;
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
                        this.sr.Visible = true;
                        iffc.Visible = true;
                        this.currentPp.Visible = true;
                        this.good.Visible = true;
                        this.ok.Visible = true;
                        this.miss.Visible = true;
                        avgoffset.Visible = true;
                        ur.Visible = true;
                        avgoffsethelp.Visible = true;
                    }
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

                        if (string.IsNullOrEmpty(tempOsuDirectory) || !Directory.Exists(tempOsuDirectory)) throw new Exception("osu! directory not found.");

                        osuDirectory = tempOsuDirectory;
                        songsPath = GetSongsFolderLocation(osuDirectory);
                        isDirectoryLoaded = true;
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
                    bool isplaying = this.isplaying;
                    bool isResultScreen = this.isResultScreen;
                    string currentMapString = baseAddresses.Beatmap.MapString;
                    string currentOsuFileName = baseAddresses.Beatmap.OsuFileName;
                    OsuMemoryStatus status = currentStatus;

                    if (status == OsuMemoryStatus.Playing)
                    {
                        double currentUr = baseAddresses.Player.HitErrors == null || baseAddresses.Player.HitErrors.Count == 0 ? 0 : CalculateUnstableRate(baseAddresses.Player.HitErrors);
                        double currentAvgOffset = CalculateAverage(baseAddresses.Player.HitErrors);
                        if (!double.IsNaN(currentUr)) urValue = (int)Math.Round(currentUr);
                        if (!double.IsNaN(currentAvgOffset)) avgOffset = baseAddresses.Player.HitErrors == null || baseAddresses.Player.HitErrors.Count == 0 ? 0 : -Math.Round(currentAvgOffset, 2);
                        avgOffsethelp = (int)Math.Round(-avgOffset);
                    }

                    if (preTitle != currentMapString)
                    {
                        string osuBeatmapPath = Path.Combine(songsPath ?? "", baseAddresses.Beatmap.FolderName ?? "",
                            currentOsuFileName ?? "");
                        if (!File.Exists(osuBeatmapPath)) throw new Exception("Beatmap file not found.");

                        int currentBeatmapGamemodeTemp = await GetMapMode(osuBeatmapPath);
                        if (currentBeatmapGamemodeTemp is -1 or not (0 or 1 or 2 or 3)) throw new Exception("Invalid gamemode.");

                        currentBeatmapGamemode = currentBeatmapGamemodeTemp;
                        currentGamemode = currentBeatmapGamemode == 0 ? currentOsuGamemode : currentBeatmapGamemode;

                        if (calculator == null)
                        {
                            calculator = new PpCalculator(osuBeatmapPath, currentGamemode);
                        }
                        else
                        {
                            calculator.SetMap(osuBeatmapPath, currentGamemode);
                        }

                        preTitle = currentMapString;
                    }

                    if (currentOsuGamemode != preOsuGamemode)
                    {
                        if (calculator == null) continue;
                        if (currentBeatmapGamemode == 0 && currentOsuGamemode is 0 or 1 or 2 or 3)
                        {
                            calculator.SetMode(currentOsuGamemode);
                            currentGamemode = currentOsuGamemode;
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

                    if (isplaying)
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

                    if (isplaying) mods = ParseMods(baseAddresses.Player.Mods.Value).Calculate;

                    double acc = CalculateAcc(hits, currentGamemode);

                    var calcArgs = new CalculateArgs
                    {
                        Accuracy = acc,
                        Combo = hits.Combo,
                        Score = hits.Score,
                        NoClassicMod = IS_NO_CLASSIC_MOD,
                        Mods = mods,
                        Time = baseAddresses.GeneralData.AudioTime,
                        PplossMode = pPLossModeToolStripMenuItem.Checked
                    };
                    var result = calculator?.Calculate(calcArgs, isplaying,
                        isResultScreen && !isplaying, hits);
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
            bool configDialog = false;

            while (!isConnectedToDiscord)
            {
                try
                {
                    _client = new DiscordRpcClient("1237279508239749211");
                    _client.Initialize();
                    isConnectedToDiscord = true;
                }
                catch (Exception e)
                {
                    Thread.Sleep(5000);
                    ErrorLogger(e);
                }
            }

            while (true)
            {
                try
                {
                    Thread.Sleep(2000);

                    if (Process.GetProcessesByName("osu!").Length == 0)
                    {
                        _client.ClearPresence();
                        continue;
                    }

                    if (!string.IsNullOrEmpty(osuDirectory) && !configDialog && discordRichPresenceToolStripMenuItem.Checked)
                    {
                        try
                        {
                            bool configChecked = CheckConfigValue(osuDirectory, "DiscordRichPresence", "1");
                            if (configChecked)
                            {
                                MessageBox.Show(
                                    "osu!の設定で、DiscordRichPresenceがオンになっています。\nこれにより、RealtimePPURのRichPresenceが上書きされる可能性があります。osu!の設定で無効化することができます。",
                                    "RealtimePPUR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }

                            configDialog = true;
                        }
                        catch (Exception e)
                        {
                            ErrorLogger(e);
                        }
                    }

                    if (!discordRichPresenceToolStripMenuItem.Checked)
                    {
                        _client.ClearPresence();
                        continue;
                    }

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

                    switch (baseAddresses.GeneralData.OsuStatus)
                    {
                        case OsuMemoryStatus.Playing when !baseAddresses.Player.IsReplay:
                            _client.SetPresence(new RichPresence
                            {
                                Details = RichPresenceStringChecker(baseAddresses.BanchoUser.Username + ConvertStatus(baseAddresses.GeneralData.OsuStatus)),
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
                                Details = RichPresenceStringChecker($"{baseAddresses.BanchoUser.Username} is Watching {baseAddresses.Player.Username}'s play"),
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
                                Details = RichPresenceStringChecker(baseAddresses.BanchoUser.Username + ConvertStatus(baseAddresses.GeneralData.OsuStatus)),
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

        private static string RichPresenceStringChecker(string value)
        {
            if (value == null) return "Unknown";
            if (value.Length > 128) value = value[..128];
            return value;
        }

        private static string ConvertStatus(OsuMemoryStatus status)
        {
            return status switch
            {
                OsuMemoryStatus.EditingMap => " is Editing Map",
                OsuMemoryStatus.GameShutdownAnimation => " is Shutting Down osu!",
                OsuMemoryStatus.GameStartupAnimation => " is Starting Up osu!",
                OsuMemoryStatus.MainMenu => " is in Main Menu",
                OsuMemoryStatus.MultiplayerRoom => " is in Multiplayer Room",
                OsuMemoryStatus.MultiplayerResultsscreen => " is in Multiplayer Results",
                OsuMemoryStatus.MultiplayerSongSelect => " is in Multiplayer Song Select",
                OsuMemoryStatus.NotRunning => " is Not Running osu!",
                OsuMemoryStatus.OsuDirect => " is Searching Maps",
                OsuMemoryStatus.Playing => " is Playing Map",
                OsuMemoryStatus.ResultsScreen => " in Results",
                OsuMemoryStatus.SongSelect => " is Selecting Songs",
                OsuMemoryStatus.Unknown => " is Unknown",
                _ => " is Unknown"
            };
        }

        private static string ConvertHits(int mode, HitsResult hits)
        {
            return mode switch
            {
                0 => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                1 => $"[{hits.Hit300}/{hits.Hit100}/{hits.HitMiss}]",
                2 => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                3 => $"[{hits.HitGeki}/{hits.Hit300}/{hits.HitKatu}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]",
                _ => $"[{hits.Hit300}/{hits.Hit100}/{hits.Hit50}/{hits.HitMiss}]"
            };
        }

        private static Mods ParseMods(int mods)
        {
            List<string> activeModsCalc = new();
            List<string> activeModsShow = new();

            for (int i = 0; i < 32; i++)
            {
                int bit = 1 << i;
                if ((mods & bit) != bit) continue;
                activeModsCalc.Add(OSU_MODS[bit].ToLower());
                activeModsShow.Add(OSU_MODS[bit]);
            }

            if (activeModsCalc.Contains("nc") && activeModsCalc.Contains("dt")) activeModsCalc.Remove("nc");
            if (activeModsShow.Contains("NC") && activeModsShow.Contains("DT")) activeModsShow.Remove("DT");
            if (activeModsShow.Count == 0) activeModsShow.Add("NM");

            return new Mods()
            {
                Calculate = activeModsCalc.ToArray(),
                Show = activeModsShow.ToArray()
            };
        }

        private static double CalculateAcc(HitsResult hits, int mode)
        {
            return mode switch
            {
                0 => (double)(100 * (6 * hits.Hit300 + 2 * hits.Hit100 + hits.Hit50)) /
                     (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss)),
                1 => (double)(100 * (2 * hits.Hit300 + hits.Hit100)) / (2 * (hits.Hit300 + hits.Hit100 + hits.HitMiss)),
                2 => (double)(100 * (hits.Hit300 + hits.Hit100 + hits.Hit50)) /
                     (hits.Hit300 + hits.Hit100 + hits.Hit50 + hits.HitKatu + hits.HitMiss),
                3 => (double)(100 * (6 * hits.HitGeki + 6 * hits.Hit300 + 4 * hits.HitKatu + 2 * hits.Hit100 + hits.Hit50)) /
                     (6 * (hits.Hit50 + hits.Hit100 + hits.Hit300 + hits.HitMiss + hits.HitGeki + hits.HitKatu)),
                _ => throw new ArgumentException("Invalid mode provided.")
            };
        }

        private static double IsNaNWithNum(double number) => double.IsNaN(number) ? 0 : number;

        public static Task<int> GetMapMode(string file)
        {
            using var stream = File.OpenRead(file);
            using var reader = new LineBufferedReader(stream);
            int count = 0;
            while (reader.ReadLine() is { } line)
            {
                if (count > 20) return Task.FromResult(0);
                if (line.StartsWith("Mode")) return Task.FromResult(int.Parse(line.Split(':')[1].Trim()));
                count++;
            }

            return Task.FromResult(-1);
        }

        private static double CalculateAverage(IReadOnlyCollection<int> array)
        {
            if (array == null || array.Count == 0) return 0;
            var sortedArray = array.OrderBy(x => x).ToArray();
            int count = sortedArray.Length;
            double q1 = sortedArray[(int)(count * 0.25)];
            double q3 = sortedArray[(int)(count * 0.75)];
            double iqr = q3 - q1;
            var filteredArray = sortedArray.Where(x => x >= q1 - 1.5 * iqr && x <= q3 + 1.5 * iqr);
            return filteredArray.Average();
        }

        private static double GetPercentile(IReadOnlyList<int> sortedData, double percentile)
        {
            int N = sortedData.Count;
            double n = (N - 1) * percentile + 1;
            if (n == 1d) return sortedData[0];
            if (n == N) return sortedData[N - 1];
            int k = (int)n;
            double d = n - k;
            return sortedData[k - 1] + d * (sortedData[k] - sortedData[k - 1]);
        }

        private static Dictionary<string, int> GetLeaderBoard(OsuMemoryDataProvider.OsuMemoryModels.Direct.LeaderBoard leaderBoard, int score)
        {
            var currentPositionArray = leaderBoard.Players.ToArray();
            var currentPosition = currentPositionArray.Length + 1;
            if (currentPosition == 1 || !leaderBoard.HasLeaderBoard)
            {
                return new Dictionary<string, int>
                {
                    { "currentPosition", 0 },
                    { "higherScore", 0 },
                    { "highestScore", 0 }
                };
            }

            foreach (var _ in leaderBoard.Players.Where(player => player.Score <= score)) currentPosition--;
            int higherScore = currentPosition - 2 <= 0
                ? leaderBoard.Players[0].Score
                : leaderBoard.Players[currentPosition - 2].Score;
            int highestScore = leaderBoard.Players[0].Score;
            return new Dictionary<string, int>
            {
                { "currentPosition", currentPosition },
                { "higherScore", higherScore },
                { "highestScore", highestScore }
            };
        }

        private string GetSongsFolderLocation(string osuDirectory)
        {
            string userName = Environment.UserName;
            string file = Path.Combine(osuDirectory, $"osu!.{userName}.cfg");
            if (!File.Exists(file))
            {
                MessageBox.Show("osu!.Username.cfgが見つからなかったため、Songsフォルダを自動検出できませんでした。\nConfigファイルのSongsFolderを参照します(もし設定されてなかったらデフォルトのSongsフォルダが参照されます。)。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return string.IsNullOrEmpty(customSongsFolder) ? Path.Combine(osuDirectory, "Songs") : customSongsFolder;
            }

            foreach (string readLine in File.ReadLines(file))
            {
                if (!readLine.StartsWith("BeatmapDirectory")) continue;
                string path = readLine.Split('=')[1].Trim(' ');
                return path == "Songs" ? Path.Combine(osuDirectory, "Songs") : path;
            }

            MessageBox.Show("BeatmapDirectoryが見つからなかったため、Songsフォルダを自動検出できませんでした。\nConfigファイルのSongsFolderを参照します(もし設定されてなかったらデフォルトのSongsフォルダが参照されます。)。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.IsNullOrEmpty(customSongsFolder) ? Path.Combine(osuDirectory, "Songs") : customSongsFolder;
        }

        private static bool CheckConfigValue(string osuDirectory, string parameter, string value)
        {
            string userName = Environment.UserName;
            string file = Path.Combine(osuDirectory, $"osu!.{userName}.cfg");
            if (!File.Exists(file)) throw new Exception("Configuration file not found.");
            foreach (string readLine in File.ReadLines(file))
            {
                if (!readLine.StartsWith(parameter)) continue;
                string configValue = readLine.Split('=')[1].Trim(' ');
                return configValue == value;
            }
            throw new Exception("Parameter not found.");
        }

        private static double CalculateUnstableRate(IReadOnlyCollection<int> hitErrors)
        {
            if (hitErrors == null || hitErrors.Count == 0) return 0;
            double totalAll = hitErrors.Sum(hit => (long)hit);
            double average = totalAll / hitErrors.Count;
            double variance = hitErrors.Sum(hit => Math.Pow(hit - average, 2)) / hitErrors.Count;
            double unstableRate = Math.Sqrt(variance) * 10;
            return unstableRate > 10000 ? double.NaN : unstableRate;
        }

        private static async void GithubUpdateChecker()
        {
            try
            {
                var latestRelease = await GetVersion(CURRENT_VERSION);
                if (latestRelease == CURRENT_VERSION) return;
                DialogResult result = MessageBox.Show($"最新バージョンがあります！\n\n現在: {CURRENT_VERSION} \n更新後: {latestRelease}\n\nダウンロードしますか？", "アップデートのお知らせ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result != DialogResult.Yes) return;

                if (!File.Exists("./Updater/RealtimePPUR.Updater.exe"))
                {
                    MessageBox.Show("アップデーターが見つかりませんでした。手動でダウンロードしてください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string updaterPath = Path.GetFullPath("./Updater/RealtimePPUR.Updater.exe");
                ProcessStartInfo args = new()
                {
                    FileName = $"\"{updaterPath}\"",
                    Arguments = CURRENT_VERSION,
                    UseShellExecute = true
                };

                Process.Start(args);
            }
            catch (Exception exception)
            {
                MessageBox.Show("アップデートチェック中にエラーが発生しました" + exception.Message, "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static async Task<string> GetVersion(string currentVersion)
        {
            try
            {
                var releaseType = currentVersion.Split('-')[1];
                var githubClient = new GitHubClient(new ProductHeaderValue("RealtimePPUR"));
                var tags = await githubClient.Repository.GetAllTags("puk06", "RealtimePPUR");
                string latestVersion = currentVersion;
                foreach (var tag in tags)
                {
                    if (releaseType == "Release")
                    {
                        if (tag.Name.Split("-")[1] != "Release") continue;
                        latestVersion = tag.Name;
                        break;
                    }

                    latestVersion = tag.Name;
                    break;
                }

                return latestVersion;
            }
            catch
            {
                throw new Exception("アップデートの取得に失敗しました");
            }
        }

        private void ErrorLogger(Exception error)
        {
            try
            {
                if (error.Message == prevErrorMessage) return;
                prevErrorMessage = error.Message;
                const string filePath = "Error.log";
                StreamWriter sw = File.Exists(filePath) ? File.AppendText(filePath) : File.CreateText(filePath);
                sw.WriteLine("[" + DateTime.Now + "]");
                sw.WriteLine(error);
                sw.WriteLine();
                sw.Close();
            }
            catch
            {
                Console.WriteLine("エラーログの書き込みに失敗しました");
            }
        }

        private void RoundCorners()
        {
            const int radius = 11;
            const int diameter = radius * 2;
            GraphicsPath gp = new GraphicsPath();
            gp.AddPie(0, 0, diameter, diameter, 180, 90);
            gp.AddPie(Width - diameter, 0, diameter, diameter, 270, 90);
            gp.AddPie(0, Height - diameter, diameter, diameter, 90, 90);
            gp.AddPie(Width - diameter, Height - diameter, diameter, diameter, 0, 90);
            gp.AddRectangle(new Rectangle(radius, 0, Width - diameter, Height));
            gp.AddRectangle(new Rectangle(0, radius, radius, Height - diameter));
            gp.AddRectangle(new Rectangle(Width - radius, radius, radius, Height - diameter));
            Region = new Region(gp);
        }

        private void realtimePPURToolStripMenuItem_Click(object sender, EventArgs e)
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
            mode = 0;
        }

        private void realtimePPToolStripMenuItem_Click(object sender, EventArgs e)
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
            mode = 1;
        }

        private void offsetHelperToolStripMenuItem_Click(object sender, EventArgs e)
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
            mode = 2;
        }

        private void changeFontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog font = new FontDialog();
            try
            {
                if (font.ShowDialog() == DialogResult.Cancel) return;

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
                        MessageBox.Show(
                            "フォントの保存に成功しました。Config.cfgのUSECUSTOMFONTをtrueにすることで起動時から保存されたフォントを使用できます。右クリック→Load Fontからでも読み込むことが可能です！",
                            "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        FileStream fs = File.Create("Font");
                        string fontInfo =
                            $"※絶対にこのファイルを自分で編集しないでください！\n※フォント名などを編集してしまうとフォントが見つからず、Windows標準のフォントが割り当てられてしまいます。\n※もし編集してしまった場合はこのファイルを削除することをお勧めします。\nFONTNAME={font.Font.Name}\nFONTSIZE={font.Font.Size}\nFONTSTYLE={font.Font.Style}";
                        byte[] fontInfoByte = System.Text.Encoding.UTF8.GetBytes(fontInfo);
                        fs.Write(fontInfoByte, 0, fontInfoByte.Length);
                        fs.Close();
                        MessageBox.Show(
                            "フォントの保存に成功しました。Config.cfgのUSECUSTOMFONTをtrueにすることで起動時から保存されたフォントを使用できます。右クリック→Load Fontからでも読み込むことが可能です！",
                            "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch
                {
                    MessageBox.Show("フォントの保存に失敗しました。もしFontファイルが作成されていたら削除することをお勧めします。", "エラー", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("フォントの変更に失敗しました。対応していないフォントです。", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void loadFontToolStripMenuItem_Click(object sender, EventArgs e)
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

                if (fontDictionaryLoad.Count == 3 && fontName && fontNameValue != "" && fontSize && fontSizeValue != "" && fontStyle && fontStyleValue != "")
                {
                    try
                    {
                        inGameValue.Font = new Font(fontNameValue, float.Parse(fontSizeValue), (FontStyle)Enum.Parse(typeof(FontStyle), fontStyleValue));
                        MessageBox.Show($"フォントの読み込みに成功しました。\n\nフォント名: {fontNameValue}\nサイズ: {fontSizeValue}\nスタイル: {fontStyleValue}", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch
                    {
                        MessageBox.Show("Fontファイルのフォント情報が不正、もしくは非対応であったため読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Fontファイルのフォント情報が不正であったため、読み込まれませんでした。一度Fontファイルを削除してみることをお勧めします。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Fontファイルが存在しません。一度Change Fontでフォントを保存してください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetFontToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            var fontsizeResult = configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
            if (!fontsizeResult)
            {
                MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var result = float.TryParse(fontsizeValue, out float fontsize);
                if (!result)
                {
                    MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    inGameValue.Font = new Font(fontCollection.Families[0], 19F);
                    MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    inGameValue.Font = new Font(fontCollection.Families[0], fontsize);
                    MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

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

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void osuModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lefttest = configDictionary.TryGetValue("LEFT", out string leftvalue);
            var toptest = configDictionary.TryGetValue("TOP", out string topvalue);
            if (!lefttest || !toptest)
            {
                MessageBox.Show("Config.cfgにLEFTまたはTOPの値が存在しなかったため、osu! Modeの起動に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var leftResult = int.TryParse(leftvalue, out int left);
            var topResult = int.TryParse(topvalue, out int top);
            if ((!leftResult || !topResult) && !isosumode)
            {
                MessageBox.Show("Config.cfgのLEFT、またはTOPの値が不正であったため、osu! Modeの起動に失敗しました。LEFT、TOPには数値以外入力しないでください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _osuModeValue["left"] = left;
            _osuModeValue["top"] = top;
            isosumode = !isosumode;
            osuModeToolStripMenuItem.Checked = isosumode;
        }

        private void RealtimePPUR_Closed(object sender, EventArgs e) => System.Windows.Forms.Application.Exit();

        private void changePriorityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form priorityForm = new ChangePriorityForm();
            priorityForm.Show();
        }

        private void sRToolStripMenuItem_Click(object sender, EventArgs e) => sRToolStripMenuItem.Checked = !sRToolStripMenuItem.Checked;

        private void ifFCPPToolStripMenuItem_Click(object sender, EventArgs e) => ifFCPPToolStripMenuItem.Checked = !ifFCPPToolStripMenuItem.Checked;

        private void ifFCHitsToolStripMenuItem_Click(object sender, EventArgs e) => ifFCHitsToolStripMenuItem.Checked = !ifFCHitsToolStripMenuItem.Checked;

        private void expectedManiaScoreToolStripMenuItem_Click(object sender, EventArgs e) => expectedManiaScoreToolStripMenuItem.Checked = !expectedManiaScoreToolStripMenuItem.Checked;

        private void currentPPToolStripMenuItem_Click(object sender, EventArgs e) => currentPPToolStripMenuItem.Checked = !currentPPToolStripMenuItem.Checked;

        private void currentPositionToolStripMenuItem_Click(object sender, EventArgs e) => currentPositionToolStripMenuItem.Checked = !currentPositionToolStripMenuItem.Checked;

        private void higherScoreToolStripMenuItem_Click(object sender, EventArgs e) => higherScoreToolStripMenuItem.Checked = !higherScoreToolStripMenuItem.Checked;

        private void highestScoreToolStripMenuItem_Click(object sender, EventArgs e) => highestScoreToolStripMenuItem.Checked = !highestScoreToolStripMenuItem.Checked;

        private void userScoreToolStripMenuItem_Click(object sender, EventArgs e) => userScoreToolStripMenuItem.Checked = !userScoreToolStripMenuItem.Checked;

        private void sSPPToolStripMenuItem_Click(object sender, EventArgs e) => sSPPToolStripMenuItem.Checked = !sSPPToolStripMenuItem.Checked;

        private void hitsToolStripMenuItem_Click(object sender, EventArgs e) => hitsToolStripMenuItem.Checked = !hitsToolStripMenuItem.Checked;

        private void uRToolStripMenuItem_Click(object sender, EventArgs e) => uRToolStripMenuItem.Checked = !uRToolStripMenuItem.Checked;

        private void offsetHelpToolStripMenuItem_Click(object sender, EventArgs e) => offsetHelpToolStripMenuItem.Checked = !offsetHelpToolStripMenuItem.Checked;

        private void currentACCToolStripMenuItem_Click(object sender, EventArgs e) => currentACCToolStripMenuItem.Checked = !currentACCToolStripMenuItem.Checked;

        private void progressToolStripMenuItem_Click(object sender, EventArgs e) => progressToolStripMenuItem.Checked = !progressToolStripMenuItem.Checked;

        private void avgOffsetToolStripMenuItem_Click(object sender, EventArgs e) => avgOffsetToolStripMenuItem.Checked = !avgOffsetToolStripMenuItem.Checked;

        private void healthPercentageToolStripMenuItem_Click(object sender, EventArgs e) => healthPercentageToolStripMenuItem.Checked = !healthPercentageToolStripMenuItem.Checked;

        private void discordRichPresenceToolStripMenuItem_Click(object sender, EventArgs e) => discordRichPresenceToolStripMenuItem.Checked = !discordRichPresenceToolStripMenuItem.Checked;

        private void pPLossModeToolStripMenuItem_Click(object sender, EventArgs e) => pPLossModeToolStripMenuItem.Checked = !pPLossModeToolStripMenuItem.Checked;

        private void saveConfigToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                const string filePath = "Config.cfg";
                if (!File.Exists(filePath))
                {
                    MessageBox.Show("Config.cfgが見つかりませんでした。RealtimePPURをダウンロードし直してください。", "エラー", MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                StreamReader sr = new(filePath);
                string[] lines = File.ReadAllLines(filePath);
                sr.Close();

                for (var i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("SR="))
                    {
                        lines[i] = $"SR={(sRToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("SSPP="))
                    {
                        lines[i] = $"SSPP={(sSPPToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("CURRENTPP="))
                    {
                        lines[i] = $"CURRENTPP={(currentPPToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("CURRENTACC="))
                    {
                        lines[i] = $"CURRENTACC={(currentACCToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("HITS="))
                    {
                        lines[i] = $"HITS={(hitsToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("IFFCHITS="))
                    {
                        lines[i] = $"IFFCHITS={(ifFCHitsToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("UR="))
                    {
                        lines[i] = $"UR={(uRToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("OFFSETHELP="))
                    {
                        lines[i] = $"OFFSETHELP={(offsetHelpToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("EXPECTEDMANIASCORE="))
                    {
                        lines[i] =
                            $"EXPECTEDMANIASCORE={(expectedManiaScoreToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("AVGOFFSET="))
                    {
                        lines[i] = $"AVGOFFSET={(avgOffsetToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("PROGRESS="))
                    {
                        lines[i] = $"PROGRESS={(progressToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("IFFCPP="))
                    {
                        lines[i] = $"IFFCPP={(ifFCPPToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("HEALTHPERCENTAGE="))
                    {
                        lines[i] = $"HEALTHPERCENTAGE={(healthPercentageToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("CURRENTPOSITION="))
                    {
                        lines[i] = $"CURRENTPOSITION={(currentPositionToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("HIGHERSCOREDIFF="))
                    {
                        lines[i] = $"HIGHERSCOREDIFF={(higherScoreToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                    else if (lines[i].StartsWith("USERSCORE="))
                    {
                        lines[i] = $"USERSCORE={(userScoreToolStripMenuItem.Checked ? "true" : "false")}";
                    }
                }

                StreamWriter sw = new(filePath, false);
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }

                sw.Close();
                MessageBox.Show("Config.cfgの保存が完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception error)
            {
                ErrorLogger(error);
                MessageBox.Show("Config.cfgの保存に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    public class Mods
    {
        public string[] Calculate { get; set; }
        public string[] Show { get; set; }
    }
}
