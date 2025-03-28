using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RealtimePPUR.Forms
{
    public partial class StrainGraph : Form
    {
        private readonly OxyColor Blue = OxyColor.Parse("#66ccff");
        private readonly OxyColor Green = OxyColor.Parse("#88b300");
        private readonly OxyColor Red = OxyColor.Parse("#ed1121");
        private readonly OxyColor Yellow = OxyColor.Parse("#ffcc22");
        private readonly OxyColor Pink = OxyColor.Parse("#f000ec");

        public StrainGraph()
        {
            InitializeComponent();

            RhythmCheckBox.Text = GetLabelString(0);
            ReadingCheckBox.Text = GetLabelString(1);
            ColourCheckBox.Text = GetLabelString(2);
            Stamina1CheckBox.Text = GetLabelString(3);
            Stamina2CheckBox.Text = GetLabelString(4);

            RhythmCheckBox.ForeColor = Blue.ToColor();
            ReadingCheckBox.ForeColor = Green.ToColor();
            ColourCheckBox.ForeColor = Red.ToColor();
            Stamina1CheckBox.ForeColor = Yellow.ToColor();
            Stamina2CheckBox.ForeColor = Pink.ToColor();
        }

        private List<float[]> globalValues = new();
        private string[] labelStrings = Array.Empty<string>();
        private int firstObjectTime = 0;
        private int lastStrainTime = 0;
        private int totalCount = 0;
        private double graphRight = 0;
        private double graphLeft = 0;
        private bool isGraphBoundsSet = false;

        public void SetValues(List<float[]> values, string[] labels, int firstTime)
        {
            globalValues.Clear();

            globalValues = values;
            labelStrings = labels;

            RhythmCheckBox.Text = GetLabelString(0);
            ReadingCheckBox.Text = GetLabelString(1);
            ColourCheckBox.Text = GetLabelString(2);
            Stamina1CheckBox.Text = GetLabelString(3);
            Stamina2CheckBox.Text = GetLabelString(4);

            firstObjectTime = firstTime;
            lastStrainTime = globalValues.Max(l => l.Length) * 400;
            totalCount = globalValues.Count > 0 ? globalValues.Max(l => l.Length) : 0;
            isGraphBoundsSet = false;

            RenderGraph();
        }

        private void RenderGraph()
        {
            try
            {
                StrainGraphPlot.Model = null;

                var rhythmLineSeries = new LineSeries
                {
                    Color = Blue,
                    StrokeThickness = 1,
                    Title = GetLabelString(0)
                };

                var rhythmAreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(10, Blue),
                    Title = GetLabelString(0)
                };

                var readingLineSeries = new LineSeries
                {
                    Color = Green,
                    StrokeThickness = 1,
                    Title = GetLabelString(1)
                };

                var readingAreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(10, Green),
                    Title = GetLabelString(1)
                };

                var colourLineSeries = new LineSeries
                {
                    Color = Red,
                    StrokeThickness = 1,
                    Title = GetLabelString(2)
                };

                var colourAreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(10, Red),
                    Title = GetLabelString(2)
                };

                var stamina1LineSeries = new LineSeries
                {
                    Color = Yellow,
                    StrokeThickness = 1,
                    Title = GetLabelString(3)
                };

                var stamina1AreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(10, Yellow),
                    Title = GetLabelString(3)
                };

                var stamina2LineSeries = new LineSeries
                {
                    Color = Pink,
                    StrokeThickness = 1,
                    Title = GetLabelString(4)
                };

                var stamina2AreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(10, Pink),
                    Title = GetLabelString(4)
                };

                var progressAreaSeries = new AreaSeries
                {
                    Color = OxyColor.FromAColor(150, OxyColors.White),
                    Title = "Progress"
                };


                var maxValue = 0f;
                for (int i = 0; i < totalCount; i++)
                {
                    rhythmLineSeries.Points.Add(new DataPoint(i, GetValue(i, 0)));
                    rhythmAreaSeries.Points.Add(new DataPoint(i, GetValue(i, 0)));
                    readingLineSeries.Points.Add(new DataPoint(i, GetValue(i, 1)));
                    readingAreaSeries.Points.Add(new DataPoint(i, GetValue(i, 1)));
                    colourLineSeries.Points.Add(new DataPoint(i, GetValue(i, 2)));
                    colourAreaSeries.Points.Add(new DataPoint(i, GetValue(i, 2)));
                    stamina1LineSeries.Points.Add(new DataPoint(i, GetValue(i, 3)));
                    stamina1AreaSeries.Points.Add(new DataPoint(i, GetValue(i, 3)));
                    stamina2LineSeries.Points.Add(new DataPoint(i, GetValue(i, 4)));
                    stamina2AreaSeries.Points.Add(new DataPoint(i, GetValue(i, 4)));

                    if (GetValue(i, 0) > maxValue) maxValue = GetValue(i, 0);
                    if (GetValue(i, 1) > maxValue) maxValue = GetValue(i, 1);
                    if (GetValue(i, 2) > maxValue) maxValue = GetValue(i, 2);
                    if (GetValue(i, 3) > maxValue) maxValue = GetValue(i, 3);
                    if (GetValue(i, 4) > maxValue) maxValue = GetValue(i, 4);
                }

                var plotModel = new PlotModel();

                if (Stamina2CheckBox.Checked)
                {
                    plotModel.Series.Add(stamina2LineSeries);
                    plotModel.Series.Add(stamina2AreaSeries);
                }

                if (Stamina1CheckBox.Checked)
                {
                    plotModel.Series.Add(stamina1LineSeries);
                    plotModel.Series.Add(stamina1AreaSeries);
                }

                if (ColourCheckBox.Checked)
                {
                    plotModel.Series.Add(colourLineSeries);
                    plotModel.Series.Add(colourAreaSeries);
                }

                if (ReadingCheckBox.Checked)
                {
                    plotModel.Series.Add(readingLineSeries);
                    plotModel.Series.Add(readingAreaSeries);
                }

                if (RhythmCheckBox.Checked)
                {
                    plotModel.Series.Add(rhythmLineSeries);
                    plotModel.Series.Add(rhythmAreaSeries);
                }

                plotModel.Axes.Add(new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Maximum = maxValue,
                });

                plotModel.PlotAreaBackground = OxyColor.FromAColor(170, OxyColors.Black);

                StrainGraphPlot.Model = plotModel;
                plotModel.InvalidatePlot(true);
            }
            catch
            {
                // ignored
            }
        }

        private void CheckChanged(object sender, EventArgs e) => RenderGraph();

        private void plotView1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (globalValues.Count == 0 || StrainGraphPlot.Model?.DefaultXAxis == null) return;
                var pos = StrainGraphPlot.Model.DefaultXAxis.InverseTransform(e.X, e.Y, StrainGraphPlot.Model.DefaultYAxis);

                if (pos.X < 0 || pos.X >= totalCount) return;

                label1.Text = $"Time: {GetTimeFromX(pos.X)}";
            }
            catch
            {
                // ignored
            }
        }

        private string GetTimeFromX(double x)
        {
            if (globalValues.Count == 0) return "0:00.00";

            var strainTime = TimeSpan.FromMilliseconds(firstObjectTime + (lastStrainTime * x / totalCount));
            string timeText = $"~{strainTime:mm\\:ss\\.ff}";

            return timeText;
        }

        public void UpdateSongProgress(int time)
        {
            if (globalValues.Count == 0) return;
            if (!isGraphBoundsSet) isGraphBoundsSet = UpdateGraphBounds();

            var progress = (time - firstObjectTime) / (double)(lastStrainTime - firstObjectTime);
            var progressValue = Math.Max(0, Math.Min(1, progress));

            progressLine.Location = new Point((int)(graphLeft + ((graphRight - graphLeft) * progressValue) - (progressLine.Width / 2)), progressLine.Location.Y);
        }

        private string GetLabelString(int index)
        {
            return labelStrings.Length > index ? labelStrings[index] : "-";
        }

        private float GetValue(int index, int time)
        {
            if (globalValues.Count <= time) return 0;
            return globalValues[time].Length > index ? globalValues[time][index] : 0;
        }

        private bool UpdateGraphBounds()
        {
            if (globalValues.Count == 0) return false;
            if (StrainGraphPlot.Model?.DefaultXAxis == null) return false;
            if (StrainGraphPlot.Model?.DefaultYAxis == null) return false;
            graphLeft = StrainGraphPlot.Model.DefaultXAxis.Transform(0, 0, StrainGraphPlot.Model.DefaultYAxis).X;
            graphRight = StrainGraphPlot.Model.DefaultXAxis.Transform(globalValues[0].Length, 0, StrainGraphPlot.Model.DefaultYAxis).X;

            return true;
        }
    }
}
