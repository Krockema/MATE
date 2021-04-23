using System.Collections.Generic;
using Zeiss.PiWeb.Api.Definitions;

namespace Master40.PiWebApi.PiWebMapping
{
    public static class PiWebMappings
    {
        public static PiWebMapping GetFor(string attributeNameSsop)
            => GetAll.Find(x => x.AttributeNameSsop.Contains(attributeNameSsop));//GetAll.OrderBy(x => attributeNameSsop).First();
        public static List<PiWebMapping> GetAll { get; } = new List<PiWebMapping>()
        {
            new PiWebMapping("X", WellKnownKeys.Characteristic.X),
            new PiWebMapping("Y", WellKnownKeys.Characteristic.Y),
            new PiWebMapping("Z", WellKnownKeys.Characteristic.Z),
            new PiWebMapping("Width", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Height", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Length", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Diameter", WellKnownKeys.Characteristic.Diameter),
            new PiWebMapping("Depth", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance_Bottom", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance_Back", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance_Left", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance_Right", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance_Front", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Distance", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Delta", WellKnownKeys.Characteristic.Length),
            new PiWebMapping("Angle", WellKnownKeys.Characteristic.Angle),
            new PiWebMapping("FromBorder", WellKnownKeys.Characteristic.Length),
        };
    }
}