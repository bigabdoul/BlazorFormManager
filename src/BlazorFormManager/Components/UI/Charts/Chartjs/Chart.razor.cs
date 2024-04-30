using System;
using System.Text.Json.Serialization;

namespace BlazorFormManager.Components.UI.Charts.Chartjs
{
    /// <summary>
    /// Represents a Chartjs configuration object.
    /// </summary>
    public class ChartConfig
    {
        /// <summary>
        /// Gets or sets the chart type.
        /// </summary>
        [JsonIgnore]
        public ChartType ChartType { get; set; } = ChartType.Bar;

        /// <summary>
        /// Gets or sets the chart type.
        /// </summary>
        public string Type
        {
            get => ChartType.ToString().ToLower();
            set
            {
                if (Enum.TryParse<ChartType>(value, ignoreCase: true, out var t))
                {
                    ChartType = t;
                }
                else
                {
                    Console.WriteLine($"Unsupported chart type: {value}. Falling back to 'bar'");
                    ChartType = ChartType.Bar;
                }
            }
        } 

        /// <summary>
        /// Gets or sets the chart's data.
        /// </summary>
        public ChartData? Data { get; set; }

        /// <summary>
        /// Gets or sets the chart's options.
        /// </summary>
        public ChartOptions? Options { get; set; }

    }

    /// <summary>
    /// Encapsulates Chartjs data.
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// Gets or sets the array of labels.
        /// </summary>
        public string[]? Labels { get; set; }

        /// <summary>
        /// Gets or sets the datasets.
        /// </summary>
        public ChartDataset[]? Datasets { get; set; }
    }

    /// <summary>
    /// Represents a Chartjs dataset.
    /// </summary>
    public class ChartDataset
    {
        /// <summary>
        /// Gets or sets the dataset's label.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public double[]? Data { get; set; }

        /// <summary>
        /// Gets or sets the chart's border width.
        /// </summary>
        public int? BorderWidth { get; set; }

        /// <summary>
        /// Gets or sets the dataset type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Determines whether to show a line.
        /// </summary>
        public bool? ShowLine { get; set; }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public string? BackgroundColor { get; set; } = "#4e73df";

        /// <summary>
        /// Gets or sets the hover background color.
        /// </summary>
        public string? HoverBackgroundColor { get; set; } = "#2e59d9";

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public string? BorderColor { get; set; } = "#4e73df";

        /// <summary>
        /// Determines whether to fill the dataset.
        /// </summary>
        public bool? Fill { get; set; }
    }

    /// <summary>
    /// Represents Chartjs options.
    /// </summary>
    public class ChartOptions
    {
        /// <summary>
        /// Determines whether to maintain the aspect ratio.
        /// </summary>
        public bool? MaintainAspectRatio { get; set; }

        //public ChartLayout? Layout { get; set; }

        /// <summary>
        /// Gets or sets the scales of a chart.
        /// </summary>
        public ChartScale? Scales { get; set; }

        /// <summary>
        /// Gets or sets the chart's legend object.
        /// </summary>
        public ChartLegend? Legend { get; set; }

        /// <summary>
        /// Gets or sets the chart tooltips.
        /// </summary>
        public ChartTooltip? Tooltips { get; set; }

        /// <summary>
        /// Gets or sets the chart tooltips on hover.
        /// </summary>
        public ChartTooltip? Hover { get; set; }
    }

    /// <summary>
    /// Represents a chart layout.
    /// </summary>
    public class ChartLayout
    {
        /// <summary>
        /// Gets or sets the padding.
        /// </summary>
        public ChartPadding? Padding { get; set; }
    }

    /// <summary>
    /// Represents a chart padding.
    /// </summary>
    public class ChartPadding
    {
        /// <summary>
        /// The left padding amount.
        /// </summary>
        public int? Left { get; set; }

        /// <summary>
        /// The right padding amount.
        /// </summary>
        public int? Right { get; set; }

        /// <summary>
        /// The top padding amount.
        /// </summary>
        public int? Top { get; set; }

        /// <summary>
        /// The bottom padding amount.
        /// </summary>
        public int? Bottom { get; set; }
    }

    /// <summary>
    /// Represents a chart legend.
    /// </summary>
    public class ChartLegend
    {
        /// <summary>
        /// Determines whether to display the legend.
        /// </summary>
        public bool? Display { get; set; }
    }

    /// <summary>
    /// Represents a chart tooltip object.
    /// </summary>
    public class ChartTooltip
    {
        /// <summary>
        /// Determines whether to intersect the tooltip.
        /// </summary>
        public bool? Intersect { get; set; }
    }

    /// <summary>
    /// Represents a chart filler object.
    /// </summary>
    public class ChartFiller
    {
        /// <summary>
        /// Determines whether to propagate the filler.
        /// </summary>
        public bool? Propagate { get; set; }
    }

    /// <summary>
    /// Represents a chart plugin.
    /// </summary>
    public class ChartPlugin
    {
        /// <summary>
        /// Gets or sets a chart filler.
        /// </summary>
        public ChartFiller? Filler { get; set; }
    }

    /// <summary>
    /// Represents the x and y scales of a chart
    /// </summary>
    public class ChartScale
    {
        /// <summary>
        /// Gets the X scale.
        /// </summary>
        public AxisScale? X { get; set; }

        /// <summary>
        /// Gets the Y scale.
        /// </summary>
        public AxisScale? Y { get; set; }
    }

    /// <summary>
    /// Represents the X scale data.
    /// </summary>
    public class AxisScale
    {
        /// <summary>
        /// Determines whether to begin the X scale at zero.
        /// </summary>
        public bool? BeginAtZero { get; set; }

        /// <summary>
        /// Gets or sets the axis time.
        /// </summary>
        public AxisTime? Time { get; set; }

        /// <summary>
        /// Gets or sets the axis grid lines.
        /// </summary>
        public AxisGridLines? GridLines { get; set; }

        /// <summary>
        /// Gets or sets the axis ticks.
        /// </summary>
        public AxisTicks? Ticks { get; set; }

        /// <summary>
        /// Determines whether to reverse the axis.
        /// </summary>
        public bool? Reverse { get; set; }

        /// <summary>
        /// Determines whether to display the axis.
        /// </summary>
        public bool? Display { get; set; }

        /// <summary>
        /// Gets or sets a collection of border dash values.
        /// </summary>
        public double[]? BorderDash { get; set; }
    }

    /// <summary>
    /// Represents a time scale on an axis.
    /// </summary>
    public class AxisTime
    {
        /// <summary>
        /// Gets or sets the time unit.
        /// </summary>
        public string? Unit { get; set; }
    }

    /// <summary>
    /// Represents an axis grid lines.
    /// </summary>
    public class AxisGridLines
    {
        /// <summary>
        /// Whether to display the grid lines.
        /// </summary>
        public bool? Display { get; set; }

        /// <summary>
        /// Whether to draw the grid lines' border.
        /// </summary>
        public bool? DrawBorder { get; set; }

        /// <summary>
        /// The color of the grid lines.
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// The border dash thickness.
        /// </summary>
        public int[]? BorderDash { get; set; }

        /// <summary>
        /// The zero-line color.
        /// </summary>
        public string? ZeroLineColor { get; set; }

        /// <summary>
        /// The zero-line border dask thickness.
        /// </summary>
        public int[]? ZeroLineBorderDash { get; set; }
    }

    /// <summary>
    /// Represents an object that encapsulates axis-related tick values.
    /// </summary>
    public class AxisTicks
    {
        /// <summary>
        /// The minimum value.
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// The maximum value.
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// The maximum ticks limit.
        /// </summary>
        public double MaxTicksLimit { get; set; }

        /// <summary>
        /// The padding amount.
        /// </summary>
        public int? Padding { get; set; }

        /// <summary>
        /// The step size.
        /// </summary>
        public double StepSize { get; set; }
    }

    /// <summary>
    /// Provides enumerated values for supported chart types.
    /// </summary>
    public enum ChartType
    {
        /// <summary>
        /// Area chart type.
        /// </summary>
        Area,

        /// <summary>
        /// Bar chart type.
        /// </summary>
        Bar,

        /// <summary>
        /// Bubble chart type.
        /// </summary>
        Bubble,

        /// <summary>
        /// Doughnut chart type.
        /// </summary>
        Doughnut,

        /// <summary>
        /// Line chart type.
        /// </summary>
        Line,

        /// <summary>
        /// Mixed chart type.
        /// </summary>
        Mixed,

        /// <summary>
        /// Polar chart type.
        /// </summary>
        Polar,

        /// <summary>
        /// Radar chart type.
        /// </summary>
        Radar,

        /// <summary>
        /// Scatter chart type.
        /// </summary>
        Scatter,
    }
}
