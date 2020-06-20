using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Mathematics;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using Perfolizer.Horology;

namespace DotNetCross.Sorting.Benchmarks
{
    // https://github.com/damageboy/VxSort/blob/research/Bench/Utils/SpeedupRatioColumn.cs
    public class SpeedupRatioColumn : BaselineCustomColumn
    {
        public enum RatioMetric
        {
            Min,
            Mean,
            Median,
        }

        public static readonly IColumn SpeedupOfMin = new SpeedupRatioColumn(RatioMetric.Min);
        public static readonly IColumn SpeedupOfMean = new SpeedupRatioColumn(RatioMetric.Mean);
        public static readonly IColumn SpeedupOfMedian = new SpeedupRatioColumn(RatioMetric.Median);

        public RatioMetric Metric { get; }

        private SpeedupRatioColumn(RatioMetric metric)
        {
            Metric = metric;
        }

        public override string Id => nameof(SpeedupRatioColumn) + "." + Metric;

        public override string ColumnName
        {
            get
            {
                return Metric switch
                {
                    RatioMetric.Mean => "SpeedupMean",
                    RatioMetric.Min => "SpeedupMin",
                    RatioMetric.Median => "SpeedupMedian",
                    _ => throw new NotSupportedException()
                };
            }
        }

        public override string GetValue(Summary summary, BenchmarkCase benchmarkCase, Statistics baseline,
            IReadOnlyDictionary<string, Metric> baselineMetric, Statistics current,
            IReadOnlyDictionary<string, Metric> currentMetric, bool isBaseline)
        {
            var ratio = GetRatioStatistics(current, baseline);
            if (ratio == null)
                return "NA";

            var cultureInfo = summary.GetCultureInfo();
            return Metric switch
            {
                RatioMetric.Mean => IsNonBaselinesPrecise(summary, baseline, benchmarkCase)
                    ? ratio.Mean.ToString("N3", cultureInfo)
                    : ratio.Mean.ToString("N2", cultureInfo),
                RatioMetric.Min => IsNonBaselinesPrecise(summary, baseline, benchmarkCase)
                    ? ratio.Min.ToString("N3", cultureInfo)
                    : ratio.Min.ToString("N2", cultureInfo),
                RatioMetric.Median => IsNonBaselinesPrecise(summary, baseline, benchmarkCase)
                    ? ratio.Median.ToString("N3", cultureInfo)
                    : ratio.Median.ToString("N2", cultureInfo),
                _ => throw new NotSupportedException()
            };
        }

        private static bool IsNonBaselinesPrecise(Summary summary, Statistics baselineStat, BenchmarkCase benchmarkCase)
        {
            string logicalGroupKey = summary.GetLogicalGroupKey(benchmarkCase);
            var nonBaselines = summary.GetNonBaselines(logicalGroupKey);
            return nonBaselines.Any(x => GetRatioStatistics(summary[x].ResultStatistics, baselineStat)?.Mean < 0.01);
        }

        //[CanBeNull]
        //private static Statistics GetRatioStatistics([CanBeNull] Statistics current, [CanBeNull] Statistics baseline)
        private static Statistics GetRatioStatistics(Statistics current, Statistics baseline)
        {
            if (current == null || current.N < 1)
                return null;
            if (baseline == null || baseline.N < 1)
                return null;
            try
            {
                return Statistics.Divide(baseline, current);
            }
            catch (DivideByZeroException)
            {
                return null;
            }
        }

        public override int PriorityInCategory => (int)Metric;
        public override bool IsNumeric => true;
        public override UnitType UnitType => UnitType.Dimensionless;

        public override string Legend
        {
            get
            {
                return Metric switch
                {
                    RatioMetric.Min => "Speedup of the minimum execution times ([Current]/[Baseline])",
                    RatioMetric.Mean => "Speedup of the mean execution times ([Current]/[Baseline])",
                    RatioMetric.Median => "Speedup of the median execution times ([Current]/[Baseline])",
                    _ => throw new ArgumentOutOfRangeException(nameof(Metric))
                };
            }
        }
    }

    public static class CommonExtensions
    {
        public static string ToTimeStr(this double value, TimeUnit unit = null, int unitNameWidth = 1,
            bool showUnit = true, string format = "N4",
            Encoding encoding = null)
        {
            unit = unit ?? TimeUnit.GetBestTimeUnit(value);
            double unitValue = TimeUnit.Convert(value, TimeUnit.Nanosecond, unit);
            if (showUnit)
            {
                string unitName = unit.Name.ToString(NumberFormatInfo.InvariantInfo).PadLeft(unitNameWidth);
                return $"{unitValue.ToStr(format)} {unitName}";
            }

            return $"{unitValue.ToStr(format)}";
        }

        public static string ToStr(this double value, string format = "0.##")
        {
            // Here we should manually create an object[] for string.Format
            // If we write something like
            //     string.Format(HostEnvironmentInfo.MainCultureInfo, $"{{0:{format}}}", value)
            // it will be resolved to:
            //     string.Format(System.IFormatProvider, string, params object[]) // .NET 4.5
            //     string.Format(System.IFormatProvider, string, object)          // .NET 4.6
            // Unfortunately, Mono doesn't have the second overload (with object instead of params object[]).
            var args = new object[] { value };
            return string.Format($"{{0:{format}}}", args);
        }
    }

    public class TimePerNColumn : IColumn
    {
        public string Id => nameof(TimePerNColumn);
        public string ColumnName => "Time / N";

        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase) => false;

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase) => "";

        public bool IsAvailable(Summary summary) => true;
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Statistics;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Time;
        public string Legend => $"Time taken to process a single element";

        public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
        {
            //var valueOfN = benchmarkCase?.Parameters?.Items.SingleOrDefault(p => p.Name == "N")?.Value;
            var valueOfN = benchmarkCase?.Parameters?.Items.SingleOrDefault(p => p.Name == "Length")?.Value;
            var mean = summary[benchmarkCase]?.ResultStatistics.Mean;
            if (valueOfN == null || mean == null)
                return "N/A";
            var timePerN = mean.Value / (int)valueOfN;
            return timePerN.ToTimeStr(TimeUnit.GetBestTimeUnit(timePerN));
        }

        public override string ToString() => ColumnName;
    }
}
