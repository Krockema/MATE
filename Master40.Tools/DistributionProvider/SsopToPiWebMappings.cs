using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;
using Attribute = Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute;

namespace Master40.Tools.DistributionProvider
{
    public static class SsopToPiWebMappings
    {
        public static SsopToPiWebMapping GetFor(string attributeNameSsop)
            => GetAll.Find(x => x.AttributeNameSsop.Contains(attributeNameSsop));//GetAll.OrderBy(x => attributeNameSsop).First();
        public static List<SsopToPiWebMapping> GetAll { get; } = new List<SsopToPiWebMapping>()
        {
            new SsopToPiWebMapping("X", WellKnownKeys.Characteristic.X),
            new SsopToPiWebMapping("Y", WellKnownKeys.Characteristic.Y),
            new SsopToPiWebMapping("Z", WellKnownKeys.Characteristic.Z),
            new SsopToPiWebMapping("Width", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Height", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Length", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Diameter", WellKnownKeys.Characteristic.Diameter),
            new SsopToPiWebMapping("Depth", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance_Bottom", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance_Back", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance_Left", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance_Right", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance_Front", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Distance", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Delta", WellKnownKeys.Characteristic.Length),
            new SsopToPiWebMapping("Angle", WellKnownKeys.Characteristic.Angle),
            new SsopToPiWebMapping("FromBorder", WellKnownKeys.Characteristic.Length),
        };
    }
}