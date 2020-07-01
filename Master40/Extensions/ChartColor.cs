using System.Collections.Generic;
using ChartJSCore.Helpers;

namespace Master40.Extensions
{

    public sealed class ChartColors
    {
        /// <summary>
        /// Pastell Color Combo from 
        /// http://www.colorcombos.com/color-schemes/12103/ColorCombo12103.html
        /// </summary>
        public List<ChartColor> Color { get; } = new List<ChartColor>
        {
            ChartColor.FromRgba(246, 167, 30, 0.6), // orange
            ChartColor.FromRgba(227, 76, 87, 0.6), // light red
            ChartColor.FromRgba(205, 10, 10, 0.6), // dard red
            ChartColor.FromRgba(184, 224, 112, 0.6), // yellow green
            ChartColor.FromRgba(125, 180, 86, 0.6), // green
            ChartColor.FromRgba(107, 166, 81, 0.6), // dark green
            ChartColor.FromRgba(98, 186, 184, 0.6), // aqua
            ChartColor.FromRgba(83, 172, 170, 0.6), // light turquise
            ChartColor.FromRgba(30, 123, 119, 0.6), // light blue 
            ChartColor.FromRgba(32, 84, 96, 0.6), // blue dianne
            ChartColor.FromRgba(4, 45, 64, 0.6), // blue whale
            ChartColor.FromRgba(7, 52, 74, 0.6) // midnight blue
        };
        public static ChartColor Transparent = ChartColor.FromRgba(0, 0, 0, 0.0);

        public ChartColor Get(int index, double alpha = 0.6)
        {
            var color = Color[index];
            color.Alpha = alpha;
            return color;
        }
    };


    /// <summary>
    /// All Posible gantt colors.
    /// </summary>
    public enum GanttColors
    {
        CRIMSON,
        MEDIUMAQUAMARINE,
        CADETBLUE,
        ORANGE,
        DARKSLATEGREY,
        KHAKI,
        DARKGOLDENROD,
        STEELBLUE
    }
    
}
