using RealtimePPUR.Utils;
using System.Drawing.Drawing2D;

namespace RealtimePPUR.Forms;

public partial class UnstableRateBar : Form
{
    private readonly Main _mainForm;
    private Bitmap? _backgroundCache;

    private static readonly Brush _centerLineBrush = new SolidBrush(Color.FromArgb(50, 188, 231));

    private readonly Font statsFont;

    public UnstableRateBar(Main mainForm)
    {
        _mainForm = mainForm;
        InitializeComponent();

        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.ResizeRedraw, true);
        UpdateStyles();

        GenerateBackground();

        Region = FormUtils.RoundCorners(Width, Height);
        BackgroundImage = null;

        statsFont = new Font(_mainForm.InGameOverlayFont, 13F);

        // 更新ループ
        _ = UpdateLoop();
    }

    private async Task UpdateLoop()
    {
        while (!IsDisposed)
        {
            TopMost = true;
            await Task.Delay(30);
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        GenerateBackground();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (_backgroundCache != null)
            e.Graphics.DrawImageUnscaled(_backgroundCache, 0, 0);

        GenerateBars(e.Graphics);
        GenerateCenterLine(e.Graphics);
        GenerateStats(e.Graphics);
    }

    private void GenerateBackground()
    {
        _backgroundCache?.Dispose();
        _backgroundCache = new Bitmap(Width, Height);

        using var g = Graphics.FromImage(_backgroundCache);
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // 背景グラデーション
        g.Clear(Color.Transparent);

        // 中央ライン
        int centerY = Height / 2;
        var centerRect = new Rectangle(0, centerY - 4, Width, 8);
        g.FillRectangle(_centerLineBrush, centerRect);
    }

    private void GenerateBars(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (_mainForm.UnstableRateArray == null || _mainForm.UnstableRateArray.Count == 0) return;

        var values = _mainForm.UnstableRateArray.TakeLast(30).ToList();
        values.Reverse();

        for (int i = 0; i < values.Count; i++)
        {
            var value = values[i];
            if (Math.Abs(value) > 50) continue;

            int valueX = (int)(Width * ((value + 50) / 100.0));
            var alpha = 170 - (i * 2);
            if (alpha <= 0) continue;

            using var barBrush = new SolidBrush(Color.FromArgb(alpha, 50, 188, 231));

            var rect = new Rectangle(valueX - 2, 7, 4, Height - 14);

            using var path = RoundedRect(rect, 2);
            g.FillPath(barBrush, path);
        }

        if (values.Count > 0)
        {
            var firstValue = values[0];
            if (Math.Abs(firstValue) > 50) return;

            int valueX = (int)(Width * ((firstValue + 50) / 100.0));

            using var barBrush = new SolidBrush(Color.Red);

            var rect = new Rectangle(valueX - 2, 2, 4, Height - 4);

            using var path = RoundedRect(rect, 2);
            g.FillPath(barBrush, path);
        }
    }

    private void GenerateStats(Graphics g)
    {
        var ur = OsuUtils.CalculateUnstableRate(_mainForm.UnstableRateArray);
        g.DrawString("UR: " + ur.ToString("F2"), statsFont, Brushes.White, 0, 0);
    }

    private void GenerateCenterLine(Graphics g)
    {
        int centerX = Width / 2;
        var verticalLineRect = new Rectangle(centerX - 2, 0, 4, Height);
        using var whiteBrush = new SolidBrush(Color.White);
        g.FillRectangle(whiteBrush, verticalLineRect);
    }

    // 角丸矩形を作る
    private static GraphicsPath RoundedRect(Rectangle rect, int radius)
    {
        int diameter = radius * 2;
        var path = new GraphicsPath();

        path.AddArc(rect.Left, rect.Top, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Top, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.Left, rect.Bottom - diameter, diameter, diameter, 90, 90);
        path.CloseFigure();

        return path;
    }

    private Point mousePoint = Point.Empty;

    private void URBar_MouseDown(object sender, MouseEventArgs e)
    {
        if ((e.Button & MouseButtons.Left) == MouseButtons.Left) mousePoint = new Point(e.X, e.Y);
    }

    private void URBar_MouseMove(object sender, MouseEventArgs e)
    {
        if ((e.Button & MouseButtons.Left) != MouseButtons.Left) return;
        Left += e.X - mousePoint.X;
        Top += e.Y - mousePoint.Y;
    }
}
