using ApsRT.Core.Planning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalPack.Common.Drawing;
using ZedGraph;

namespace ApsRTCustomComponents
{
    internal static class GraphsOutput
    {

        internal static void GrowingOutput(LineItem curve, IDictionary<int, DateTime> points)
        {
            curve.Clear();
            var dts = points.Select(x => x.Value).ToArray();
            Array.Sort(dts); // Насколько это быстрее OrderBy - вопрос
            var num = 1;
            foreach (var point in dts)
            {
                curve.AddPoint(new XDate(point), num);
                num++;
            }
        }

        internal static IEnumerable<(T element, LineItem curve)> UsageOutput<T>(GraphPane pane, IDictionary<int, DateTime> points, IDictionary<int, CachedInterval> durations, Dictionary<int, (T[] elements, double count)> elements, int maxCount = 0)
        {
            var allPoints = points.SelectMany(point =>
            elements[point.Key].elements.Select(el =>
            (ModID: point.Key, Start: point.Value, Count: elements[point.Key].count, Value: el)
            )
            );

            return UsageOutput<T>(allPoints, pane, points, durations, maxCount);

        }

        static IEnumerable<(T element, LineItem curve)> UsageOutput<T>(IEnumerable<(int ModID, DateTime Start, double Count, T Value)> allPoints, GraphPane pane, IDictionary<int, DateTime> points, IDictionary<int, CachedInterval> durations, int maxCount = 0)
        {
            pane.CurveList.Clear();
            var groups = allPoints.GroupBy(x => x.Value).ToArray();

            for (int i = 0; i < groups.Length; i++)
            {
                if (maxCount != 0 && i > maxCount)
                {
                    yield break;
                }

                var currentGroup = groups[i];
                var currentKey = currentGroup.Key;

                var listLoc = new PointPairList();
                var curve = pane.AddCurve(currentKey == null || string.IsNullOrEmpty(currentKey.ToString()) ? "пусто" : currentKey.ToString(), listLoc, Colors.StaticColors[i % Colors.StaticColors.Length], SymbolType.None);

                var sortedPoints = currentGroup.SelectMany(point => new[]
                {
                    (Start: point.Start, Count: point.Count),
                    (Start: point.Start + durations[point.ModID].Duration, Count: -point.Count),
                }).OrderBy(x => x.Start).ToArray();

                double num = 0;

                foreach (var point in sortedPoints)
                {
                    curve.AddPoint(new XDate(point.Start), num);
                    curve.AddPoint(new XDate(point.Start), num + point.Count);
                    num += point.Count;
                    //var pt = new XDate(point.Key.AddDays(1));
                    //curveLoc.AddPoint(pt, num);
                    //curveLoc.AddPoint(pt, 0);
                }

                yield return (currentKey, curve);

            }
        }

        internal static IEnumerable<(T element, LineItem curve)> UsageOutput<T>(GraphPane pane, IDictionary<int, DateTime> points, IDictionary<int, CachedInterval> durations, Dictionary<int, (T element, double count)> elements, int maxCount = 0)
        {
            var allPoints = points.Select(point => (ModID: point.Key, Start: point.Value, Count: elements[point.Key].count, Value: elements[point.Key].element));

            return UsageOutput<T>(allPoints, pane, points, durations, maxCount);
        }

    }
}
