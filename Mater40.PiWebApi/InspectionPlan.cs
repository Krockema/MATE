using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;
using Attribute = Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute;

namespace Mater40.PiWebApi
{
    public class InspectionPlan
    {
        public static readonly InspectionPlanPart newPart = new InspectionPlanPart
        {
            // zeiss-piweb.github.io/PiWeb-Api/sdk/#-example--using-the-pathhelper-class
            Path = PathHelper.RoundtripString2PathInformation("P:/NewPart/")
            ,Uuid = Guid.NewGuid()
        };

        public static readonly InspectionPlanPart subPart = new InspectionPlanPart
        {
            Path = PathHelper.RoundtripString2PathInformation("PP:/NewPart/SubPart/")
            ,Uuid = Guid.NewGuid()
        };

        public static readonly InspectionPlanCharacteristic InspectionPlanCharacteristic = new InspectionPlanCharacteristic
        {
            Path = PathHelper.RoundtripString2PathInformation("PPC:/NewPart/SubPart/Characteristic1/")
            , Uuid = Guid.NewGuid()
            , Attributes = new []{ new Attribute(WellKnownKeys.Characteristic.Number, "128")
                                 , new Attribute(2003, "MeinCoolesMerkmal_1")
                                 , new Attribute(WellKnownKeys.Characteristic.LowerTolerance, "0.4")
                                 , new Attribute(WellKnownKeys.Characteristic.UpperTolerance , "0.6")
            }
        };

        public static readonly InspectionPlanCharacteristic InspectionPlanCharacteristicV2 = new InspectionPlanCharacteristic
        {
            Path = PathHelper.RoundtripString2PathInformation("PPC:/NewPart/SubPart/Characteristic2/")
            , Uuid = Guid.NewGuid()
            , Attributes = new []{ new Attribute(WellKnownKeys.Characteristic.Number, "128")
                                 , new Attribute(2003, "MeinCoolesMerkmal_2") }
        };

        public static async Task CreateInspectionPlan(DataServiceRestClient client)
        {
            await client.CreateParts(new[] {newPart, subPart});
            await client.CreateCharacteristics(new[] {InspectionPlanCharacteristic, InspectionPlanCharacteristicV2});
        }

        public static async Task Undo(DataServiceRestClient client)
        {
            var part = client.GetParts(newPart.Path, depth: 0); // uint.MaxValue to all 
            await client.DeleteParts(new[] { part.Result.First().Uuid });
        }

        public static async Task GetCharacteristics(DataServiceRestClient client)
        {
            var characteristics = await client.GetCharacteristics(subPart.Path
                , requestedCharacteristicAttributes: new AttributeSelector(new[] {WellKnownKeys.Characteristic.Number}));

        }

        public static Guid SubPartUuid(DataServiceRestClient client)
        {
            var part = client.GetParts(InspectionPlan.subPart.Path).Result.First();

            return part.Uuid;
        }
    }
}
