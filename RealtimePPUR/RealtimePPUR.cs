using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;
using Octokit;
using osu.Game.IO;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using OsuMemoryDataProvider;
using OsuMemoryDataProvider.OsuMemoryModels;
using OsuMemoryDataProvider.OsuMemoryModels.Abstract;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RealtimePPUR
{
    public sealed partial class RealtimePpur : Form
    {
        private System.Windows.Forms.Label _currentPp, _sr, _sspp, _good, _ok, _miss, _avgoffset, _ur, _avgoffsethelp;

        private readonly PrivateFontCollection _fontCollection;
        private readonly int _calculationSpeedDetectedValue;
        private readonly bool _speedReduction;
        private readonly string _ingameoverlayPriority;

        private Point _mousePoint;
        private string _displayFormat;
        private int _mode, _x, _y;
        private long _prevCalculationSpeed;
        private bool _isosumode;
        private bool _nowPlaying;
        private int _currentBackgroundImage = 1;
        private bool _isDbLoaded;
        private string _osuDirectory;
        private string _songsPath;
        private string _preMd5;
        private PPCalculator _calculator;
        private bool _isplaying;
        private bool _isResultScreen;
        private double _avgOffset;
        private int _urValue;
        private const bool IsNoClassicMod = true;
        private int _currentBeatmapGamemode;
        private int _currentOsuGamemode;
        private int _currentGamemode;
        private int _preOsuGamemode;

        private readonly Dictionary<string, string> _configDictionary = new();
        private readonly StructuredOsuMemoryReader _sreader = new();
        private readonly OsuBaseAddresses _baseAddresses = new();
        public static readonly Dictionary<int, string> OsuMods = new()
        {
            { 1, "nf" },
            { 2, "ez" },
            { 8, "hd" },
            { 16, "hr" },
            { 32, "sd" },
            { 64, "dt" },
            { 128, "rx" },
            { 256, "ht" },
            { 512, "nc" },
            { 1024, "fl" },
            { 2048, "at" },
            { 16384, "pf" }
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
            _fontCollection = new PrivateFontCollection();
            _fontCollection.AddFontFile("./src/Fonts/MPLUSRounded1c-ExtraBold.ttf");
            _fontCollection.AddFontFile("./src/Fonts/Nexa Light.otf");
            InitializeComponent();

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
                _speedReduction = false;
                _calculationSpeedDetectedValue = 100;
                _ingameoverlayPriority = "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16";
                inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
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
                    _configDictionary[name] = value;
                }

                if (_configDictionary.TryGetValue("UPDATECHECK", out string test11) && test11 == "true")
                {
                    GithubUpdateChecker();
                }

                var defaultmodeTest = _configDictionary.TryGetValue("DEFAULTMODE", out string defaultmodestring);
                if (defaultmodeTest)
                {
                    var defaultModeResult = int.TryParse(defaultmodestring, out int defaultmode);
                    if (!defaultModeResult || !(defaultmode == 0 || defaultmode == 1 || defaultmode == 2))
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
                                _currentBackgroundImage = 2;
                                RoundCorners();
                                _mode = 1;
                                break;

                            case 2:
                                ClientSize = new Size(316, 65);
                                BackgroundImage = Properties.Resources.UR;
                                _currentBackgroundImage = 3;
                                RoundCorners();
                                foreach (Control control in Controls)
                                {
                                    if (control.Name == "inGameValue") continue;
                                    control.Location = control.Location with { Y = control.Location.Y - 65 };
                                }
                                _mode = 2;
                                break;
                        }
                    }
                }

                sRToolStripMenuItem.Checked = _configDictionary.TryGetValue("SR", out string test) && test == "true";
                sSPPToolStripMenuItem.Checked = _configDictionary.TryGetValue("SSPP", out string test2) && test2 == "true";
                currentPPToolStripMenuItem.Checked = _configDictionary.TryGetValue("CURRENTPP", out string test3) && test3 == "true";
                currentACCToolStripMenuItem.Checked = _configDictionary.TryGetValue("CURRENTACC", out string test4) && test4 == "true";
                hitsToolStripMenuItem.Checked = _configDictionary.TryGetValue("HITS", out string test5) && test5 == "true";
                uRToolStripMenuItem.Checked = _configDictionary.TryGetValue("UR", out string test6) && test6 == "true";
                offsetHelpToolStripMenuItem.Checked = _configDictionary.TryGetValue("OFFSETHELP", out string test7) && test7 == "true";
                avgOffsetToolStripMenuItem.Checked = _configDictionary.TryGetValue("AVGOFFSET", out string test8) && test8 == "true";
                progressToolStripMenuItem.Checked = _configDictionary.TryGetValue("PROGRESS", out string test9) && test9 == "true";
                ifFCPPToolStripMenuItem.Checked = _configDictionary.TryGetValue("IFFCPP", out string test13) && test13 == "true";
                ifFCHitsToolStripMenuItem.Checked = _configDictionary.TryGetValue("IFFCHITS", out string test14) && test14 == "true";
                expectedManiaScoreToolStripMenuItem.Checked = _configDictionary.TryGetValue("EXPECTEDMANIASCORE", out string test15) && test15 == "true";
                healthPercentageToolStripMenuItem.Checked = _configDictionary.TryGetValue("HEALTHPERCENTAGE", out string test17) && test17 == "true";
                currentPositionToolStripMenuItem.Checked = _configDictionary.TryGetValue("CURRENTPOSITION", out string test18) && test18 == "true";
                higherScoreToolStripMenuItem.Checked = _configDictionary.TryGetValue("HIGHERSCOREDIFF", out string test19) && test19 == "true";
                highestScoreToolStripMenuItem.Checked = _configDictionary.TryGetValue("HIGHESTSCOREDIFF", out string test20) && test20 == "true";
                userScoreToolStripMenuItem.Checked = _configDictionary.TryGetValue("USERSCORE", out string test21) && test21 == "true";
                _speedReduction = _configDictionary.TryGetValue("SPEEDREDUCTION", out string test10) && test10 == "true";
                _ingameoverlayPriority = _configDictionary.TryGetValue("INGAMEOVERLAYPRIORITY", out string test16) ? test16 : "1/2/3/4/5/6/7/8/9/10/11/12/13/14/15/16";

                if (_configDictionary.TryGetValue("USECUSTOMFONT", out string test12) && test12 == "true")
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
                                var fontsizeResult = _configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                                if (!fontsizeResult)
                                {
                                    MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                                }
                                else
                                {
                                    var result = float.TryParse(fontsizeValue, out float fontsize);
                                    if (!result)
                                    {
                                        MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                                    }
                                    else
                                    {
                                        inGameValue.Font = new Font(_fontCollection.Families[0], fontsize);
                                    }
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Fontファイルのフォント情報が不正であったため、デフォルトのフォントが適用されます。一度Fontファイルを削除してみることをお勧めします。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            var fontsizeResult = _configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                            if (!fontsizeResult)
                            {
                                MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                            }
                            else
                            {
                                var result = float.TryParse(fontsizeValue, out float fontsize);
                                if (!result)
                                {
                                    MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                                }
                                else
                                {
                                    inGameValue.Font = new Font(_fontCollection.Families[0], fontsize);
                                }
                            }
                        }
                    }
                    else
                    {
                        var fontsizeResult = _configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                        if (!fontsizeResult)
                        {
                            MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                        }
                        else
                        {
                            var result = float.TryParse(fontsizeValue, out float fontsize);
                            if (!result)
                            {
                                MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                            }
                            else
                            {
                                inGameValue.Font = new Font(_fontCollection.Families[0], fontsize);
                            }
                        }
                    }
                }
                else
                {
                    var fontsizeResult = _configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
                    if (!fontsizeResult)
                    {
                        MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                    }
                    else
                    {
                        var result = float.TryParse(fontsizeValue, out float fontsize);
                        if (!result)
                        {
                            MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                        }
                        else
                        {
                            inGameValue.Font = new Font(_fontCollection.Families[0], fontsize);
                        }
                    }
                }

                var speedReductionValueResult = _configDictionary.TryGetValue("SPEEDREDUCTIONVALUE", out string speedReductionValue);
                if (!speedReductionValueResult && _speedReduction)
                {
                    MessageBox.Show("Config.cfgにSPEEDREDUCTIONVALUEの値が存在しないため、初期値の100が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _calculationSpeedDetectedValue = 100;
                }
                else if (_speedReduction)
                {
                    var tryResult = int.TryParse(speedReductionValue, out _calculationSpeedDetectedValue);
                    if (!tryResult)
                    {
                        MessageBox.Show("Config.cfgのSPEEDREDUCTIONVALUEの値が不正であったため、初期値の100が設定されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _calculationSpeedDetectedValue = 100;
                    }
                }
            }
            Thread updateMemoryThread = new(UpdateMemoryData)
            {
                IsBackground = true
            };
            updateMemoryThread.Start();
            UpdateLoop();
        }

        private async void UpdateLoop()
        {
            while (true)
            {
                await Task.Delay(1);
                await Loop();
            }
        }

        private async Task Loop()
        {
            try
            {
                TopMost = true;
                var currentStatus = _baseAddresses.GeneralData.OsuStatus;
                if (currentStatus == OsuMemoryStatus.Playing) _isplaying = true;
                else if (currentStatus != OsuMemoryStatus.ResultsScreen) _isplaying = false;

                _isResultScreen = currentStatus == OsuMemoryStatus.ResultsScreen;

                string osuBeatmapPath = Path.Combine(_songsPath ?? "", _baseAddresses.Beatmap.FolderName ?? "",
                    _baseAddresses.Beatmap.OsuFileName ?? "");
                if (!File.Exists(osuBeatmapPath)) throw new Exception("Can't find Beatmap;-;");

                _currentOsuGamemode = currentStatus switch
                {
                    OsuMemoryStatus.Playing => _baseAddresses.Player.Mode,
                    OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.Mode,
                    _ => _baseAddresses.GeneralData.GameMode
                };

                if (_preMd5 != _baseAddresses.Beatmap.Md5)
                {
                    int currentBeatmapGamemodeTemp = await GetMapMode(osuBeatmapPath);
                    if (currentBeatmapGamemodeTemp is -1 or not (0 or 1 or 2 or 3)) throw new Exception("Can't get Beatmap Mode;-;");
                    _currentBeatmapGamemode = currentBeatmapGamemodeTemp;
                    _currentGamemode = _currentBeatmapGamemode;

                    if (_currentBeatmapGamemode == 0) _currentGamemode = _currentOsuGamemode;
                    

                    if (_calculator == null)
                    {
                        _calculator = new PPCalculator(osuBeatmapPath, _currentGamemode);
                    }
                    else
                    {
                        _calculator.SetMode(_currentGamemode);
                        _calculator.SetMap(osuBeatmapPath, _currentGamemode);
                    }

                    _preMd5 = _baseAddresses.Beatmap.Md5;
                }

                if (_currentOsuGamemode != _preOsuGamemode)
                {
                    if (_currentBeatmapGamemode == 0 && _currentOsuGamemode is 0 or 1 or 2 or 3)
                    {
                        _calculator.SetMode(_currentOsuGamemode);
                        _currentGamemode = _currentOsuGamemode;
                    }
                    _preOsuGamemode = _currentOsuGamemode;
                }

                string[] mods = currentStatus switch
                {
                    OsuMemoryStatus.Playing => ParseMods(_baseAddresses.Player.Mods.Value),
                    OsuMemoryStatus.ResultsScreen => ParseMods(_baseAddresses.ResultsScreen.Mods.Value),
                    OsuMemoryStatus.MainMenu => ParseMods(_baseAddresses.GeneralData.Mods),
                    _ => ParseMods(_baseAddresses.GeneralData.Mods)
                };
                if (_isplaying) mods = ParseMods(_baseAddresses.Player.Mods.Value);

                HitsResult hits = new()
                {
                    HitGeki = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.HitGeki,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.HitGeki,
                        _ => 0
                    },
                    Hit300 = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.Hit300,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.Hit300,
                        _ => 0
                    },
                    HitKatu = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.HitKatu,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.HitKatu,
                        _ => 0
                    },
                    Hit100 = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.Hit100,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.Hit100,
                        _ => 0
                    },
                    Hit50 = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.Hit50,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.Hit50,
                        _ => 0
                    },
                    HitMiss = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.HitMiss,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.HitMiss,
                        _ => 0
                    },
                    Combo = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.MaxCombo,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.MaxCombo,
                        _ => 0
                    },
                    Score = currentStatus switch
                    {
                        OsuMemoryStatus.Playing => _baseAddresses.Player.Score,
                        OsuMemoryStatus.ResultsScreen => _baseAddresses.ResultsScreen.Score,
                        _ => 0
                    }
                };

                double acc = Math.Round(CalculateAcc(hits, _currentGamemode), 2);

                Stopwatch stopwatch = new();
                stopwatch.Start();
                var calcArgs = new CalculateArgs
                {
                    Accuracy = acc,
                    Greats = hits.Hit300,
                    Goods = hits.Hit100,
                    Mehs = hits.Hit50,
                    Oks = hits.HitKatu,
                    Misses = hits.HitMiss,
                    Combo = hits.Combo,
                    Score = hits.Score,
                    NoClassicMod = IsNoClassicMod,
                    Mods = mods,
                    Time = _baseAddresses.GeneralData.AudioTime
                };

                if (_isplaying)
                {
                    calcArgs.Greats = _baseAddresses.Player.Hit300;
                    calcArgs.Goods = _baseAddresses.Player.Hit100;
                    calcArgs.Mehs = _baseAddresses.Player.Hit50;
                    calcArgs.Oks = _baseAddresses.Player.HitKatu;
                    calcArgs.Misses = _baseAddresses.Player.HitMiss;
                    calcArgs.Combo = _baseAddresses.Player.MaxCombo;
                    calcArgs.Score = _baseAddresses.Player.Score;
                }

                var result = _calculator.Calculate(calcArgs, currentStatus == OsuMemoryStatus.Playing || _isplaying, _isResultScreen && !_isplaying);
                stopwatch.Stop();
                if (result.DifficultyAttributes == null || result.PerformanceAttributes == null || result.CurrentDifficultyAttributes == null || result.CurrentPerformanceAttributes == null) throw new Exception("Can't calculate PP;-;");

                var leaderBoardData = GetLeaderBoard(_baseAddresses.LeaderBoard, _baseAddresses.Player.Score);
                double sr = IsNaNWithNum(Math.Round(result.CurrentDifficultyAttributes.StarRating, 2));
                double fullSr = IsNaNWithNum(Math.Round(result.DifficultyAttributes.StarRating, 2));
                double sspp = IsNaNWithNum(result.PerformanceAttributes.Total);
                double currentPp = IsNaNWithNum(result.CurrentPerformanceAttributes.Total);
                double ifFcpp = 100;

                int geki = hits.HitGeki;
                int good = hits.Hit300;
                int katu = hits.HitKatu;
                int ok = hits.Hit100;
                int bad = hits.Hit50;
                int miss = hits.HitMiss;

                if (_isplaying)
                {
                    geki = _baseAddresses.Player.HitGeki;
                    good = _baseAddresses.Player.Hit300;
                    katu = _baseAddresses.Player.HitKatu;
                    ok = _baseAddresses.Player.Hit100;
                    bad = _baseAddresses.Player.Hit50;
                    miss = _baseAddresses.Player.HitMiss;
                }

                int currentCalculationSpeed = 0;
                int ifFcGood = 100;
                int ifFcOk = 100;
                int ifFcBad = 100;
                int ifFcMiss = 0;
                double healthPercentage = IsNaNWithNum(Math.Round(_baseAddresses.Player.HP / 2, 1));
                int userScore = hits.Score;

                int currentPosition = leaderBoardData["currentPosition"];
                int higherScore = leaderBoardData["higherScore"];
                int highestScore = leaderBoardData["highestScore"];

                if (currentStatus == OsuMemoryStatus.Playing)
                {
                    _urValue = (int)Math.Round(CalculateUnstableRate(_baseAddresses.Player.HitErrors));
                    _avgOffset = IsNaNWithNum(_baseAddresses.Player.HitErrors == null || _baseAddresses.Player.HitErrors.Count == 0 ? 0 : -Math.Round(CalculateAverage(_baseAddresses.Player.HitErrors), 2));
                }
                double avgOffsethelp = _baseAddresses.Player.HitErrors == null || _baseAddresses.Player.HitErrors.Count == 0 ? 0 : -_avgOffset;

                if (_prevCalculationSpeed == 0)
                {
                    _prevCalculationSpeed = stopwatch.ElapsedMilliseconds;
                }
                else if (currentCalculationSpeed - _prevCalculationSpeed > _calculationSpeedDetectedValue &&
                         _speedReduction)
                {
                    new ToastContentBuilder()
                        .AddText("Calculation Speed Reduction Detected!")
                        .AddText("Calculation speed is slower than usual! \nCurrent Calculation speed: " +
                                 currentCalculationSpeed + "ms")
                        .Show();
                }
                _prevCalculationSpeed = stopwatch.ElapsedMilliseconds;

                _avgoffset.Text = _avgOffset.ToString(CultureInfo.CurrentCulture = new CultureInfo("en-us")) + "ms";
                _avgoffset.Width = TextRenderer.MeasureText(_avgoffset.Text, _avgoffset.Font).Width;
                _ur.Text = _urValue.ToString("F0");
                _ur.Width = TextRenderer.MeasureText(_ur.Text, _ur.Font).Width;
                _avgoffsethelp.Text = avgOffsethelp.ToString("F0");
                _avgoffsethelp.Width = TextRenderer.MeasureText(_avgoffsethelp.Text, _avgoffsethelp.Font).Width;
                _sr.Text = sr.ToString(CultureInfo.CurrentCulture = new CultureInfo("en-us"));
                _sr.Width = TextRenderer.MeasureText(_sr.Text, _sr.Font).Width;
                _sspp.Text = sspp.ToString("F0");
                _sspp.Width = TextRenderer.MeasureText(_sspp.Text, _sspp.Font).Width;
                _currentPp.Text = currentPp.ToString("F0");
                _currentPp.Width = TextRenderer.MeasureText(_currentPp.Text, _currentPp.Font).Width;
                _currentPp.Left = ClientSize.Width - _currentPp.Width - 35;

                switch (_currentGamemode)
                {
                    case 0:
                        _good.Text = good.ToString();
                        _good.Width = TextRenderer.MeasureText(_good.Text, _good.Font).Width;
                        _good.Left = (ClientSize.Width - _good.Width) / 2 - 120;

                        _ok.Text = (ok + bad).ToString();
                        _ok.Width = TextRenderer.MeasureText(_ok.Text, _ok.Font).Width;
                        _ok.Left = (ClientSize.Width - _ok.Width) / 2 - 61;

                        _miss.Text = miss.ToString();
                        _miss.Width = TextRenderer.MeasureText(_miss.Text, _miss.Font).Width;
                        _miss.Left = (ClientSize.Width - _miss.Width) / 2 - 3;
                        break;

                    case 1:
                        _good.Text = good.ToString();
                        _good.Width = TextRenderer.MeasureText(_good.Text, _good.Font).Width;
                        _good.Left = (ClientSize.Width - _good.Width) / 2 - 120;

                        _ok.Text = ok.ToString();
                        _ok.Width = TextRenderer.MeasureText(_ok.Text, _ok.Font).Width;
                        _ok.Left = (ClientSize.Width - _ok.Width) / 2 - 61;

                        _miss.Text = miss.ToString();
                        _miss.Width = TextRenderer.MeasureText(_miss.Text, _miss.Font).Width;
                        _miss.Left = (ClientSize.Width - _miss.Width) / 2 - 3;
                        break;

                    case 2:
                        _good.Text = good.ToString();
                        _good.Width = TextRenderer.MeasureText(_good.Text, _good.Font).Width;
                        _good.Left = (ClientSize.Width - _good.Width) / 2 - 120;

                        _ok.Text = (ok + bad).ToString();
                        _ok.Width = TextRenderer.MeasureText(_ok.Text, _ok.Font).Width;
                        _ok.Left = (ClientSize.Width - _ok.Width) / 2 - 61;

                        _miss.Text = miss.ToString();
                        _miss.Width = TextRenderer.MeasureText(_miss.Text, _miss.Font).Width;
                        _miss.Left = (ClientSize.Width - _miss.Width) / 2 - 3;
                        break;

                    case 3:
                        _good.Text = (good + geki).ToString();
                        _good.Width = TextRenderer.MeasureText(_good.Text, _good.Font).Width;
                        _good.Left = (ClientSize.Width - _good.Width) / 2 - 120;

                        _ok.Text = (katu + ok + bad).ToString();
                        _ok.Width = TextRenderer.MeasureText(_ok.Text, _ok.Font).Width;
                        _ok.Left = (ClientSize.Width - _ok.Width) / 2 - 61;

                        _miss.Text = miss.ToString();
                        _miss.Width = TextRenderer.MeasureText(_miss.Text, _miss.Font).Width;
                        _miss.Left = (ClientSize.Width - _miss.Width) / 2 - 3;
                        break;
                }

                _displayFormat = "";
                var ingameoverlayPriorityArray = _ingameoverlayPriority.Replace(" ", "").Split('/');
                foreach (var priorityValue in ingameoverlayPriorityArray)
                {
                    var priorityValueResult = int.TryParse(priorityValue, out int priorityValueInt);
                    if (!priorityValueResult) continue;
                    switch (priorityValueInt)
                    {
                        case 1:
                            if (sRToolStripMenuItem.Checked)
                            {
                                _displayFormat += "SR: " +
                                                 sr.ToString(CultureInfo.CurrentCulture = new CultureInfo("en-us")) +
                                                 " / " + fullSr.ToString(CultureInfo.CurrentCulture = new CultureInfo("en-us")) + "\n";
                            }

                            break;

                        case 2:
                            if (sSPPToolStripMenuItem.Checked)
                            {
                                _displayFormat += "SSPP: " + sspp.ToString("F0") + "pp\n";
                            }

                            break;

                        case 3:
                            if (currentPPToolStripMenuItem.Checked)
                            {
                                if (ifFCPPToolStripMenuItem.Checked && _currentGamemode != 3)
                                {
                                    _displayFormat += "PP: " + currentPp.ToString("F0") + " / " + ifFcpp.ToString("F0") +
                                                     "pp\n";
                                }
                                else
                                {
                                    _displayFormat += "PP: " + currentPp.ToString("F0") + "pp\n";
                                }
                            }

                            break;

                        case 4:
                            if (currentACCToolStripMenuItem.Checked)
                            {
                                _displayFormat += "ACC: " + Math.Round(_baseAddresses.Player.Accuracy, 2) + "%\n";
                            }

                            break;

                        case 5:
                            if (hitsToolStripMenuItem.Checked)
                            {
                                switch (_currentGamemode)
                                {
                                    case 0:
                                        _displayFormat += $"Hits: {good}/{ok}/{bad}/{miss}\n";
                                        break;

                                    case 1:
                                        _displayFormat += $"Hits: {good}/{ok}/{miss}\n";
                                        break;

                                    case 2:
                                        _displayFormat += $"Hits: {good}/{ok}/{bad}/{miss}\n";
                                        break;

                                    case 3:
                                        _displayFormat += $"Hits: {geki}/{good}/{katu}/{ok}/{bad}/{miss}\n";
                                        break;
                                }
                            }

                            break;

                        case 6:
                            if (ifFCHitsToolStripMenuItem.Checked)
                            {
                                switch (_currentGamemode)
                                {
                                    case 0:
                                        _displayFormat += $"ifFCHits: {ifFcGood}/{ifFcOk}/{ifFcBad}/{ifFcMiss}\n";
                                        break;

                                    case 1:
                                        _displayFormat += $"ifFCHits: {ifFcGood}/{ifFcOk}/{ifFcMiss}\n";
                                        break;

                                    case 2:
                                        _displayFormat += $"ifFCHits: {ifFcGood}/{ifFcOk}/{ifFcBad}/{ifFcMiss}\n";
                                        break;
                                }
                            }

                            break;

                        case 7:
                            if (uRToolStripMenuItem.Checked)
                            {
                                _displayFormat += "UR: " + _urValue.ToString("F0") + "\n";
                            }

                            break;

                        case 8:
                            if (offsetHelpToolStripMenuItem.Checked)
                            {
                                _displayFormat += "OffsetHelp: " + avgOffsethelp.ToString("F0") + "\n";
                            }

                            break;

                        case 9:
                            if (expectedManiaScoreToolStripMenuItem.Checked && _currentGamemode == 3)
                            {
                                _displayFormat += "ManiaScore: " + 0 + "\n";
                            }

                            break;

                        case 10:
                            if (avgOffsetToolStripMenuItem.Checked)
                            {
                                _displayFormat += "AvgOffset: " +
                                                 _avgOffset.ToString(CultureInfo.CurrentCulture = new CultureInfo("en-us")) + "\n";
                            }

                            break;

                        case 11:
                            if (progressToolStripMenuItem.Checked)
                            {
                                _displayFormat += "Progress: " + (int)Math.Round(_baseAddresses.GeneralData.AudioTime / _baseAddresses.GeneralData.TotalAudioTime * 100) + "%\n";
                            }

                            break;

                        case 12:
                            if (healthPercentageToolStripMenuItem.Checked)
                            {
                                _displayFormat += "HP: " + healthPercentage + "%\n";
                            }

                            break;

                        case 13:
                            if (currentPositionToolStripMenuItem.Checked && currentPosition != 0)
                            {
                                if (currentPosition > 50)
                                {
                                    _displayFormat += "Position: >#50" + "\n";
                                }
                                else
                                {
                                    _displayFormat += "Position: #" + currentPosition + "\n";
                                }
                            }

                            break;

                        case 14:
                            if (higherScoreToolStripMenuItem.Checked && higherScore != 0)
                            {
                                _displayFormat += "HigherDiff: " + (higherScore - userScore) + "\n";
                            }

                            break;

                        case 15:
                            switch (highestScoreToolStripMenuItem.Checked)
                            {
                                case true when highestScore != 0 && currentPosition == 1:
                                    _displayFormat += "HighestDiff: You're Top!!" + "\n";
                                    break;
                                case true when highestScore != 0:
                                    _displayFormat += "HighestDiff: " + (highestScore - userScore) + "\n";
                                    break;
                            }

                            break;

                        case 16:
                            if (userScoreToolStripMenuItem.Checked)
                            {
                                _displayFormat += "Score: " + userScore + "\n";
                            }

                            break;
                    }
                }

                inGameValue.Text = _displayFormat;

                if (_isosumode)
                {
                    var processes = Process.GetProcessesByName("osu!");
                    if (processes.Length > 0)
                    {
                        Process osuProcess = processes[0];
                        IntPtr osuMainWindowHandle = osuProcess.MainWindowHandle;
                        if (GetWindowRect(osuMainWindowHandle, out Rect rect) && _baseAddresses.GeneralData.OsuStatus == OsuMemoryStatus.Playing &&
                            GetForegroundWindow() == osuMainWindowHandle && osuMainWindowHandle != IntPtr.Zero)
                        {
                            if (!_nowPlaying)
                            {
                                _x = Location.X;
                                _y = Location.Y;
                                _nowPlaying = true;
                            }

                            BackgroundImage = null;
                            _currentBackgroundImage = 0;
                            inGameValue.Visible = true;
                            _avgoffsethelp.Visible = false;
                            _sr.Visible = false;
                            _sspp.Visible = false;
                            _currentPp.Visible = false;
                            _good.Visible = false;
                            _ok.Visible = false;
                            _miss.Visible = false;
                            _avgoffset.Visible = false;
                            _ur.Visible = false;
                            Region = null;
                            Size = new Size(inGameValue.Width, inGameValue.Height);
                            Location = new Point(rect.Left + _osuModeValue["left"] + 2, rect.Top + _osuModeValue["top"]);
                        }
                        else if (_nowPlaying)
                        {
                            switch (_mode)
                            {
                                case 0:
                                    if (_currentBackgroundImage != 1)
                                    {
                                        ClientSize = new Size(316, 130);
                                        RoundCorners();
                                        BackgroundImage = Properties.Resources.PPUR;
                                        _currentBackgroundImage = 1;
                                    }

                                    break;

                                case 1:
                                    if (_currentBackgroundImage != 2)
                                    {
                                        ClientSize = new Size(316, 65);
                                        RoundCorners();
                                        BackgroundImage = Properties.Resources.PP;
                                        _currentBackgroundImage = 2;
                                    }

                                    break;

                                case 2:
                                    if (_currentBackgroundImage != 3)
                                    {
                                        ClientSize = new Size(316, 65);
                                        RoundCorners();
                                        BackgroundImage = Properties.Resources.UR;
                                        _currentBackgroundImage = 3;
                                    }

                                    break;
                            }

                            if (_nowPlaying)
                            {
                                Location = new Point(_x, _y);
                                _nowPlaying = false;
                            }

                            inGameValue.Visible = false;
                            _sr.Visible = true;
                            _sspp.Visible = true;
                            _currentPp.Visible = true;
                            _good.Visible = true;
                            _ok.Visible = true;
                            _miss.Visible = true;
                            _avgoffset.Visible = true;
                            _ur.Visible = true;
                            _avgoffsethelp.Visible = true;
                        }
                    }
                    else if (_nowPlaying)
                    {
                        switch (_mode)
                        {
                            case 0:
                                if (_currentBackgroundImage != 1)
                                {
                                    ClientSize = new Size(316, 130);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.PPUR;
                                    _currentBackgroundImage = 1;
                                }

                                break;

                            case 1:
                                if (_currentBackgroundImage != 2)
                                {
                                    ClientSize = new Size(316, 65);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.PP;
                                    _currentBackgroundImage = 2;
                                }

                                break;

                            case 2:
                                if (_currentBackgroundImage != 3)
                                {
                                    ClientSize = new Size(316, 65);
                                    RoundCorners();
                                    BackgroundImage = Properties.Resources.UR;
                                    _currentBackgroundImage = 3;
                                }

                                break;
                        }

                        if (_nowPlaying)
                        {
                            Location = new Point(_x, _y);
                            _nowPlaying = false;
                        }

                        inGameValue.Visible = false;
                        _sr.Visible = true;
                        _sspp.Visible = true;
                        _currentPp.Visible = true;
                        _good.Visible = true;
                        _ok.Visible = true;
                        _miss.Visible = true;
                        _avgoffset.Visible = true;
                        _ur.Visible = true;
                        _avgoffsethelp.Visible = true;
                    }
                }
                else if (_nowPlaying)
                {
                    switch (_mode)
                    {
                        case 0:
                            if (_currentBackgroundImage != 1)
                            {
                                ClientSize = new Size(316, 130);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.PPUR;
                                _currentBackgroundImage = 1;
                            }

                            break;

                        case 1:
                            if (_currentBackgroundImage != 2)
                            {
                                ClientSize = new Size(316, 65);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.PP;
                                _currentBackgroundImage = 2;
                            }

                            break;

                        case 2:
                            if (_currentBackgroundImage != 3)
                            {
                                ClientSize = new Size(316, 65);
                                RoundCorners();
                                BackgroundImage = Properties.Resources.UR;
                                _currentBackgroundImage = 3;
                            }

                            break;
                    }

                    if (_nowPlaying)
                    {
                        Location = new Point(_x, _y);
                        _nowPlaying = false;
                    }

                    inGameValue.Visible = false;
                    _sr.Visible = true;
                    _sspp.Visible = true;
                    _currentPp.Visible = true;
                    _good.Visible = true;
                    _ok.Visible = true;
                    _miss.Visible = true;
                    _avgoffset.Visible = true;
                    _ur.Visible = true;
                    _avgoffsethelp.Visible = true;
                }
            }
            catch
            {
                if (!_nowPlaying) inGameValue.Text = "";
                _sr.Text = "0";
                _sspp.Text = "0";
                _currentPp.Text = "0";
                _good.Text = "0";
                _ok.Text = "0";
                _miss.Text = "0";
                _avgoffset.Text = "0ms";
                _ur.Text = "0";
                _avgoffsethelp.Text = "0";
            }
        }

        private static string[] ParseMods(int mods)
        {
            List<string> activeMods = new();
            for (int i = 0; i < 14; i++)
            {
                int bit = 1 << i;
                if (bit is 4 or 4096 or 8192) continue;
                if ((mods & bit) == bit) activeMods.Add(OsuMods[bit]);
            }
            if (activeMods.Contains("nc") && activeMods.Contains("dt")) activeMods.Remove("nc");
            return activeMods.ToArray();
        }

        private static double IsNaNWithNum(double number) => double.IsNaN(number) ? 0 : number;

        public static Task<int> GetMapMode(string file)
        {
            using var stream = File.OpenRead(file);
            using var reader = new LineBufferedReader(stream);
            int count = 0;
            while (reader.ReadLine() is { } line)
            {
                if (count > 20) return Task.FromResult(-1);
                if (line.StartsWith("Mode")) return Task.FromResult(int.Parse(line.Split(':')[1].Trim()));
                count++;
            }

            return Task.FromResult(-1);
        }

        private static double CalculateAverage(IReadOnlyCollection<int> array)
        {
            if (array == null || array.Count == 0) return 0;
            return (double)array.Sum() / array.Count;
        }

        private void UpdateMemoryData()
        {
            while (true)
            {
                try
                {
                    if (!_isDbLoaded)
                    {
                        if (Process.GetProcessesByName("osu!").Length > 0)
                        {
                            Process osuProcess = Process.GetProcessesByName("osu!")[0];
                            _osuDirectory = Path.GetDirectoryName(osuProcess.MainModule.FileName);
                        }
                        else
                        {
                            using RegistryKey registryKey = Registry.ClassesRoot.OpenSubKey("osu\\DefaultIcon");
                            if (registryKey != null)
                            {
                                string str = registryKey.GetValue(null).ToString().Remove(0, 1);
                                _osuDirectory = str.Remove(str.Length - 11);
                            }
                        }

                        if (string.IsNullOrEmpty(_osuDirectory) || !Directory.Exists(_osuDirectory))
                            continue;

                        _songsPath = GetSongsFolderLocation(_osuDirectory);
                        _isDbLoaded = true;
                    }

                    if (!_sreader.CanRead) continue;

                    _sreader.TryRead(_baseAddresses.Beatmap);
                    _sreader.TryRead(_baseAddresses.Player);
                    _sreader.TryRead(_baseAddresses.GeneralData);
                    _sreader.TryRead(_baseAddresses.LeaderBoard);
                    _sreader.TryRead(_baseAddresses.ResultsScreen);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static Dictionary<string, int> GetLeaderBoard(OsuMemoryDataProvider.OsuMemoryModels.Direct.LeaderBoard leaderBoard, int score)
        {
            var currentPositionArray = leaderBoard.Players.ToArray();
            var currentPosition = currentPositionArray.Length + 1;
            if (currentPosition == 1 || !leaderBoard.HasLeaderBoard) return new Dictionary<string, int>
            {
                { "currentPosition", 0 },
                { "higherScore", 0 },
                { "highestScore", 0 }
            };

            foreach (var _ in leaderBoard.Players.Where(player => player.Score <= score)) currentPosition--;
            int higherScore = currentPosition - 2 <= 0 ? leaderBoard.Players[0].Score : leaderBoard.Players[currentPosition - 2].Score;
            int highestScore = leaderBoard.Players[0].Score;
            return new Dictionary<string, int>
            {
                { "currentPosition", currentPosition },
                { "higherScore", higherScore },
                { "highestScore", highestScore }
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
                _ => 100
            };
        }

        private static string GetSongsFolderLocation(string osuDirectory)
        {
            foreach (string file in Directory.GetFiles(osuDirectory))
            {
                if (!Regex.IsMatch(file, @"^osu!\.+\.cfg$")) continue;
                foreach (string readLine in File.ReadLines(file))
                {
                    if (!readLine.StartsWith("BeatmapDirectory")) continue;
                    return Path.Combine(osuDirectory, readLine.Split('=')[1].Trim(' '));
                }
            }
            return Path.Combine(osuDirectory, "Songs");
        }

        private static double CalculateUnstableRate(IReadOnlyCollection<int> hitErrors)
        {
            if (hitErrors == null || hitErrors.Count == 0) return 0;
            double sum = hitErrors.Sum(Math.Abs);
            double mean = sum / hitErrors.Count;
            double sumOfSquares = hitErrors.Sum(hitError => Math.Pow(Math.Abs(hitError) - mean, 2));
            double variance = sumOfSquares / hitErrors.Count;
            return Math.Sqrt(variance) * 10;
        }

        private static async void GithubUpdateChecker()
        {
            try
            {
                const string softwareReleasesLatest = "https://github.com/puk06/RealtimePPUR/releases/latest";
                if (!File.Exists("./src/version"))
                {
                    MessageBox.Show("versionファイルが存在しないのでアップデートチェックは無視されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                StreamReader currentVersion = new StreamReader("./src/version");
                string currentVersionString = await currentVersion.ReadToEndAsync();
                currentVersion.Close();
                var githubClient = new GitHubClient(new ProductHeaderValue("RealtimePPUR"));
                var latestRelease = await githubClient.Repository.Release.GetLatest("puk06", "RealtimePPUR");
                if (latestRelease.Name == currentVersionString) return;
                DialogResult result = MessageBox.Show($"最新バージョンがあります！\n\n現在: {currentVersionString} \n更新後: {latestRelease.Name}\n\nダウンロードページを開きますか？", "アップデートのお知らせ", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (result == DialogResult.Yes) Process.Start(softwareReleasesLatest);
            }
            catch
            {
                MessageBox.Show("アップデートチェック中にエラーが発生しました", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (_mode == 0) return;
            ClientSize = new Size(316, 130);
            BackgroundImage = Properties.Resources.PPUR;
            _currentBackgroundImage = 1;
            RoundCorners();
            if (_mode == 2)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y + 65 };
                }
            }
            _mode = 0;
        }

        private void realtimePPToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_mode == 1) return;
            ClientSize = new Size(316, 65);
            BackgroundImage = Properties.Resources.PP;
            _currentBackgroundImage = 2;
            RoundCorners();
            if (_mode == 2)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y + 65 };
                }
            }
            _mode = 1;
        }

        private void offsetHelperToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_mode == 2) return;
            ClientSize = new Size(316, 65);
            BackgroundImage = Properties.Resources.UR;
            _currentBackgroundImage = 3;
            RoundCorners();
            if (_mode == 0 || _mode == 1)
            {
                foreach (Control control in Controls)
                {
                    if (control.Name == "inGameValue") continue;
                    control.Location = control.Location with { Y = control.Location.Y - 65 };
                }
            }
            _mode = 2;
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
            var fontsizeResult = _configDictionary.TryGetValue("FONTSIZE", out string fontsizeValue);
            if (!fontsizeResult)
            {
                MessageBox.Show("Config.cfgにFONTSIZEの値がなかったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                var result = float.TryParse(fontsizeValue, out float fontsize);
                if (!result)
                {
                    MessageBox.Show("Config.cfgのFONTSIZEの値が不正であったため、初期値の19が適用されます。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    inGameValue.Font = new Font(_fontCollection.Families[0], 19F);
                    MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    inGameValue.Font = new Font(_fontCollection.Families[0], fontsize);
                    MessageBox.Show("フォントのリセットが完了しました！", "情報", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void RealtimePPUR_MouseDown(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left) _mousePoint = new Point(e.X, e.Y);
        }

        private void RealtimePPUR_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;
            Left += e.X - _mousePoint.X;
            Top += e.Y - _mousePoint.Y;
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e) => Close();

        private void osuModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var lefttest = _configDictionary.TryGetValue("LEFT", out string leftvalue);
            var toptest = _configDictionary.TryGetValue("TOP", out string topvalue);
            if (!lefttest || !toptest)
            {
                MessageBox.Show("Config.cfgにLEFTまたはTOPの値が存在しなかったため、osu! Modeの起動に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var leftResult = int.TryParse(leftvalue, out int left);
            var topResult = int.TryParse(topvalue, out int top);
            if ((!leftResult || !topResult) && !_isosumode)
            {
                MessageBox.Show("Config.cfgのLEFT、またはTOPの値が不正であったため、osu! Modeの起動に失敗しました。LEFT、TOPには数値以外入力しないでください。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _osuModeValue["left"] = left;
            _osuModeValue["top"] = top;
            _isosumode = !_isosumode;
            osuModeToolStripMenuItem.Checked = _isosumode;
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
    }
}
