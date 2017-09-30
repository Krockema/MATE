using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Master40.Extensions
{

    public sealed class ChartColor
    {
        /// <summary>
        /// Pastell Color Combo from 
        /// http://www.colorcombos.com/color-schemes/12103/ColorCombo12103.html
        /// </summary>
        public List<string> Color { get; } = new List<string>
        {
            "rgba(246, 167, 30, 0.6)", // orange
            "rgba(227, 76, 87, 0.6)", // light red
            "rgba(205, 10, 10, 0.6)", // dard red
            "rgba(184, 224, 112, 0.6)", // yellow green
            "rgba(125, 180, 86, 0.6)", // green
            "rgba(107, 166, 81, 0.6)", // dark green
            "rgba(98, 186, 184, 0.6)", // aqua
            "rgba(83, 172, 170, 0.6)", // light turquise
            "rgba(30, 123, 119, 0.6)", // light blue 
            "rgba(32, 84, 96, 0.6)", // blue dianne
            "rgba(4, 45, 64, 0.6)", // blue whale
            "rgba(7, 52, 74, 0.6)" // midnight blue
        };
        public const string Transparent = "rgba(0, 0, 0, 0.0)";
        
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
