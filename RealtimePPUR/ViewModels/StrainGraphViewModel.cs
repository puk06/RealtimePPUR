using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Media;
using RealtimePPUR.Services;

namespace RealtimePPUR.ViewModels;

public class SkillVisibilityItem : INotifyPropertyChanged
{
    public string Label { get; }
    public int Index { get; }
    public IBrush Brush { get; }
    public Color LineColor { get; }
    public bool IsSupported { get; }

    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible != value)
            {
                _isVisible = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
                VisibilityChanged?.Invoke();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? VisibilityChanged;

    public SkillVisibilityItem(string label, int index, Color color, bool isSupported = true)
    {
        Label = label;
        Index = index;
        LineColor = color;
        Brush = new SolidColorBrush(color);
        IsSupported = isSupported;
        if (!isSupported) _isVisible = false;
    }
}

public class StrainGraphViewModel : INotifyPropertyChanged
{
    private static readonly Color Blue = Color.Parse("#66ccff");
    private static readonly Color Green = Color.Parse("#88b300");
    private static readonly Color Red = Color.Parse("#ed1121");
    private static readonly Color Yellow = Color.Parse("#ffcc22");
    private static readonly Color Pink = Color.Parse("#f000ec");
    private static readonly Color[] DefaultColors =
    [
        Blue, Green, Red, Yellow, Pink,
        Color.Parse("#ff8800"),
        Color.Parse("#8800ff"),
        Color.Parse("#00ff88")
    ];

    private string _currentTime = "0:00.00";
    public string CurrentTime
    {
        get => _currentTime;
        set { _currentTime = value; OnPropertyChanged(); }
    }

    private double _progressX = 0;
    public double ProgressX
    {
        get => _progressX;
        set { _progressX = value; OnPropertyChanged(); }
    }

    private bool _hasData = false;
    public bool HasData
    {
        get => _hasData;
        set { _hasData = value; OnPropertyChanged(); }
    }

    private List<float[]> _strains = [];
    private string[] _skillNames = [];
    private int _firstObjectTime;
    private int _lastStrainTime;
    private int _totalCount;

    public ObservableCollection<SkillVisibilityItem> SkillItems { get; } = [];

    public IReadOnlyList<float[]> Strains => _strains;
    public int TotalCount => _totalCount;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    public event Action? DataChanged;

    public void SetValues(StrainList strainList, int firstTime)
    {
        _strains = strainList.Strains;
        _skillNames = strainList.SkillNames;
        _firstObjectTime = firstTime;
        _totalCount = _strains.Count > 0 ? _strains.Max(l => l.Length) : 0;
        _lastStrainTime = _totalCount * 400;
        HasData = _totalCount > 0;

        foreach (var item in SkillItems)
            item.VisibilityChanged -= OnVisibilityChanged;

        SkillItems.Clear();
        for (int i = 0; i < _skillNames.Length; i++)
        {
            var color = GetSkillColor(i);
            var item = new SkillVisibilityItem(_skillNames[i], i, color);
            item.VisibilityChanged += OnVisibilityChanged;
            SkillItems.Add(item);
        }

        foreach (var unsupportedName in strainList.UnsupportedSkillNames)
        {
            var color = Color.FromArgb(128, 128, 128, 128);
            var item = new SkillVisibilityItem($"Not Supported ({unsupportedName})", _skillNames.Length, color, false);
            SkillItems.Add(item);
        }

        DataChanged?.Invoke();
    }

    private void OnVisibilityChanged()
    {
        DataChanged?.Invoke();
    }

    public void UpdateMousePosition(double normalizedX)
    {
        if (_strains.Count == 0 || _totalCount == 0 || normalizedX < 0 || normalizedX > 1) return;
        var strainTime = TimeSpan.FromMilliseconds(_firstObjectTime + (_lastStrainTime * normalizedX));
        CurrentTime = $"~{strainTime:mm\\:ss\\.ff}";
    }

    public void UpdateSongProgress(int time, bool isDoubleTime, bool isHalfTime)
    {
        if (_strains.Count == 0 || _totalCount == 0) return;
        var progress = (time - _firstObjectTime) / (double)_lastStrainTime;

        if (isDoubleTime) progress /= 1.5;
        else if (isHalfTime) progress /= 0.75;

        ProgressX = Math.Max(0, Math.Min(1, progress));
    }

    private Color GetSkillColor(int index)
    {
        if (index < DefaultColors.Length) return DefaultColors[index];
        var hue = ((index * 47.0) % 360) / 360.0;
        var (r, g, b) = HsvToRgb(hue, 0.8, 0.9);
        return new Color(255, r, g, b);
    }

    private static (byte r, byte g, byte b) HsvToRgb(double h, double s, double v)
    {
        int hi = Convert.ToInt32(Math.Floor(h * 6)) % 6;
        double f = (h * 6) - Math.Floor(h * 6);
        double p = v * (1 - s);
        double q = v * (1 - (f * s));
        double t = v * (1 - ((1 - f) * s));

        return hi switch
        {
            0 => ((byte)(v * 255), (byte)(t * 255), (byte)(p * 255)),
            1 => ((byte)(q * 255), (byte)(v * 255), (byte)(p * 255)),
            2 => ((byte)(p * 255), (byte)(v * 255), (byte)(t * 255)),
            3 => ((byte)(p * 255), (byte)(q * 255), (byte)(v * 255)),
            4 => ((byte)(t * 255), (byte)(p * 255), (byte)(v * 255)),
            _ => ((byte)(v * 255), (byte)(p * 255), (byte)(q * 255))
        };
    }
}
