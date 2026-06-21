using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;
using RealtimePPUR.Services;

namespace RealtimePPUR;

public partial class InGameOverlay : Window
{
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowRect
    {
        public int Left, Top, Right, Bottom;
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hWnd, out WindowRect rect);

    private readonly DispatcherTimer _windowRefleshTimer;

    public InGameOverlay()
    {
        InitializeComponent();

        _windowRefleshTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1000.0 / 30)
        };
        _windowRefleshTimer.Tick += OnWindowReflesh;
        _windowRefleshTimer.Start();

        RealtimePPCalculator.Instance.OnCalculate += OnCalculate;
    }

    private async void OnWindowReflesh(object? sender, EventArgs e)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var currentMemoryData = RealtimePPCalculator.Instance.CurrentMemoryData;

            var process = currentMemoryData.OsuProcess;
            if (process == null) return;

            var handle = process.MainWindowHandle;
            if (!GetWindowRect(handle, out WindowRect rect)) return;

            var shouldEnableOverlay = currentMemoryData.IsPlaying && handle == GetForegroundWindow();
            ToggleOverlay(shouldEnableOverlay, handle);

            Position = new Avalonia.PixelPoint(rect.Left + 2, rect.Top + 25 + 50);
            if (Topmost != shouldEnableOverlay) Topmost = shouldEnableOverlay;
        });
    }

    private async void OnCalculate()
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!IsVisible) return;
            IngameOverlayValue.Text = GenerateInGameValue();
        });
    }

    private static int[] testValues = { 1, 2, 3, 4, 5 }; // TODO: 消す

    private string GenerateInGameValue()
    {
        var memory = RealtimePPCalculator.Instance.CurrentMemoryData;
        var calculator = RealtimePPCalculator.Instance;

        return InGameValueBuilder.Build(memory, calculator, testValues);
    }

    private void ToggleOverlay(bool value, IntPtr targetWindow)
    {
        var hasChanged = IsVisible != value;
        if (hasChanged)
        {
            IsVisible = value;
            if (value) SetForegroundWindow(targetWindow); // アクティブ時にウィンドウのフォーカスが外れるため
        }
    }
}
