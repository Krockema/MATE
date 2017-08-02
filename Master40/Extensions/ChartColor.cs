using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Master40.Extensions
{

    public sealed class ChartColor
    {
        public List<string> Color { get; } = new List<string>
        {
            "rgba(120,67,109, 0.4)",
            "rgba(119, 132, 97, 0.4)",
            "rgba(62, 146, 173, 0.4)",
            "rgba(200, 75, 243, 0.4)"
        };
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
