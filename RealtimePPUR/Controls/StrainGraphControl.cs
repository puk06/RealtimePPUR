using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using RealtimePPUR.ViewModels;

namespace RealtimePPUR.Controls;

public class StrainGraphControl : Control
{
    public static readonly StyledProperty<StrainGraphViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<StrainGraphControl, StrainGraphViewModel?>(nameof(ViewModel));

    public StrainGraphViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    private readonly IBrush _backgroundBrush = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
    private readonly Pen _progressPen = new(new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)), 2);
    private readonly Pen _gridPen = new(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1);

    public StrainGraphControl()
    {
        ClipToBounds = true;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        if (DataContext is StrainGraphViewModel vm)
        {
            ViewModel = vm;
            vm.DataChanged += InvalidateGraph;
            vm.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        if (ViewModel != null)
        {
            ViewModel.DataChanged -= InvalidateGraph;
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(StrainGraphViewModel.ProgressX))
        {
            InvalidateVisual();
        }
    }

    private void InvalidateGraph()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = new Rect(0, 0, Bounds.Width, Bounds.Height);
        context.FillRectangle(_backgroundBrush, bounds);

        if (ViewModel == null || !ViewModel.HasData || ViewModel.TotalCount == 0)
            return;

        var strains = ViewModel.Strains;
        var totalCount = ViewModel.TotalCount;
        var maxValue = GetMaxValue(strains, totalCount);
        if (maxValue <= 0) maxValue = 1;

        DrawGrid(context, bounds);
        DrawStrains(context, bounds, strains, totalCount, maxValue);
        DrawProgressLine(context, bounds, ViewModel.ProgressX);
    }

    private void DrawGrid(DrawingContext context, Rect bounds)
    {
        for (int i = 1; i < 4; i++)
        {
            var y = bounds.Height * i / 4;
            context.DrawLine(_gridPen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    private void DrawStrains(DrawingContext context, Rect bounds, IReadOnlyList<float[]> strains, int totalCount, float maxValue)
    {
        for (int s = strains.Count - 1; s >= 0; s--)
        {
            if (s >= ViewModel!.SkillItems.Count || !ViewModel.SkillItems[s].IsVisible)
                continue;

            var skillData = strains[s];
            var color = ViewModel.SkillItems[s].LineColor;
            var lineColor = color;
            var fillColor = Color.FromArgb(15, color.R, color.G, color.B);

            var lineGeometry = new StreamGeometry();
            var fillGeometry = new StreamGeometry();

            using (var lineCtx = lineGeometry.Open())
            using (var fillCtx = fillGeometry.Open())
            {
                if (totalCount <= 0) continue;

                var firstX = bounds.Width * 0 / (totalCount - 1);
                var firstValue = 0 < skillData.Length ? skillData[0] : 0;
                var firstY = bounds.Height - (bounds.Height * firstValue / maxValue);

                fillCtx.BeginFigure(new Point(0, bounds.Height), true);
                fillCtx.LineTo(new Point(firstX, firstY));
                lineCtx.BeginFigure(new Point(firstX, firstY), false);

                for (int i = 1; i < totalCount; i++)
                {
                    var x = bounds.Width * i / (totalCount - 1);
                    var value = i < skillData.Length ? skillData[i] : 0;
                    var y = bounds.Height - (bounds.Height * value / maxValue);

                    lineCtx.LineTo(new Point(x, y));
                    fillCtx.LineTo(new Point(x, y));
                }

                fillCtx.LineTo(new Point(bounds.Width, bounds.Height));
                fillCtx.EndFigure(true);
            }

            context.DrawGeometry(new SolidColorBrush(fillColor), null, fillGeometry);
            context.DrawGeometry(null, new Pen(new SolidColorBrush(lineColor), 1.5), lineGeometry);
        }
    }

    private void DrawProgressLine(DrawingContext context, Rect bounds, double progress)
    {
        var x = bounds.Width * progress;
        context.DrawLine(_progressPen, new Point(x, 0), new Point(x, bounds.Height));
    }

    private float GetMaxValue(IReadOnlyList<float[]> strains, int totalCount)
    {
        float max = 0;
        for (int s = 0; s < strains.Count; s++)
        {
            if (ViewModel!.SkillItems.Count <= s || !ViewModel.SkillItems[s].IsVisible)
                continue;

            var skillData = strains[s];
            for (int i = 0; i < skillData.Length && i < totalCount; i++)
            {
                if (skillData[i] > max) max = skillData[i];
            }
        }
        return max;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (ViewModel == null || !ViewModel.HasData || Bounds.Width <= 0)
            return;

        var pos = e.GetPosition(this);
        var normalizedX = pos.X / Bounds.Width;
        ViewModel.UpdateMousePosition(normalizedX);
    }
}
