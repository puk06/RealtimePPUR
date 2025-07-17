using MathNet.Numerics.Interpolation;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace RealtimePPUR.Forms
{
    public partial class UnstableRateGraph : Form
    {
        private readonly RealtimePpur mainForm;

        public UnstableRateGraph(RealtimePpur mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();

            UpdateLoop();
        }

        private async void UpdateLoop()
        {
            while (true)
            {
                await Task.Delay(50);
                try
                {
                    var data = mainForm.UnstableRateArray;
                    if (data == null) continue;

                    var dataCount = data.Count;
                    if (dataCount == 0) continue;

                    var dict = new Dictionary<int, int>();
                    foreach (var i in data)
                    {
                        if (dict.ContainsKey(i))
                            dict[i]++;
                        else
                            dict[i] = 1;
                    }

                    const int minX = -100;
                    const int maxX = 100;

                    for (int i = minX; i <= maxX; i++)
                    {
                        dict.TryAdd(i, 0);
                    }

                    var sortedDict = dict.OrderBy(x => x.Key).ToList();

                    var compressedDict = new Dictionary<int, int>();
                    compressedDict.Clear();
                    for (int i = 0; i < sortedDict.Count; i += 5)
                    {
                        int xValue = sortedDict[i].Key;
                        var sum = sortedDict.Skip(i).Take(5).Sum(x => x.Value);
                        compressedDict[xValue] = sum;
                    }

                    var xValues = compressedDict.Select(x => (double)x.Key).ToArray();
                    var yValues = compressedDict.Select(x => (double)x.Value / dataCount).ToArray();
                    var spline = CubicSpline.InterpolateNatural(xValues, yValues);

                    var lineSeries = new LineSeries
                    {
                        Color = OxyColors.Blue,
                        StrokeThickness = 2
                    };
                    var areaSeries = new AreaSeries
                    {
                        Fill = OxyColors.LightBlue,
                        StrokeThickness = 2
                    };

                    const double step = 0.05;
                    for (double x = xValues.First(); x <= xValues.Last(); x += step)
                    {
                        double y = spline.Interpolate(x);
                        if (y < 0) y = 0;
                        lineSeries.Points.Add(new DataPoint(x, y));
                        areaSeries.Points.Add(new DataPoint(x, y));
                        areaSeries.Points2.Add(new DataPoint(x, 0));
                    }

                    double averageX = data.Average();
                    var averageLine = new LineAnnotation
                    {
                        Type = LineAnnotationType.Vertical,
                        X = averageX,
                        Color = OxyColors.Red,
                        StrokeThickness = 2,
                        LineStyle = LineStyle.Solid
                    };

                    var model = new PlotModel { PlotType = PlotType.XY };
                    model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, Minimum = minX, Maximum = maxX });
                    model.Annotations.Add(averageLine);
                    model.Series.Add(lineSeries);
                    model.Series.Add(areaSeries);

                    unstableLateGraph.Model = model;
                    unstableLateGraph.InvalidatePlot(true);

                    ValueLabel.Text = $"Average: {averageX:F2} Total: {dataCount}";
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
