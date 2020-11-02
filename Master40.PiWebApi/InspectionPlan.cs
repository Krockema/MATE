using System;
using System.Linq;
using System.Threading.Tasks;
using Zeiss.PiWeb.Api.Definitions;
using Zeiss.PiWeb.Api.Rest.Dtos;
using Zeiss.PiWeb.Api.Rest.Dtos.Data;
using Zeiss.PiWeb.Api.Rest.HttpClient.Data;

namespace Master40.PiWebApi
{
    public class InspectionPlan
    {
        public static readonly InspectionPlanPartDto newPart = new InspectionPlanPartDto
        {
            // zeiss-piweb.github.io/PiWeb-Api/sdk/#-example--using-the-pathhelper-class
            Path = PathHelper.RoundtripString2PathInformation("P:/NewPart/")
            ,Uuid = Guid.NewGuid()
        };

        public static readonly InspectionPlanPartDto subPart = new InspectionPlanPartDto
        {
            Path = PathHelper.RoundtripString2PathInformation("PP:/NewPart/SubPart/")
            ,Uuid = Guid.NewGuid()
        };

        public static readonly InspectionPlanCharacteristicDto InspectionPlanCharacteristic = new InspectionPlanCharacteristicDto
        {
            Path = PathHelper.RoundtripString2PathInformation("PPC:/NewPart/SubPart/Characteristic1/")
            , Uuid = Guid.NewGuid()
            , Attributes = new []{ new AttributeDto(WellKnownKeys.Characteristic.Number, "128")
                                 , new AttributeDto(2003, "MeinCoolesMerkmal_1")
                                 , new AttributeDto(WellKnownKeys.Characteristic.LowerTolerance, "0.4")
                                 , new AttributeDto(WellKnownKeys.Characteristic.UpperTolerance , "0.6")
            }
        };

        public static readonly InspectionPlanCharacteristicDto InspectionPlanCharacteristicV2 = new InspectionPlanCharacteristicDto
        {
            Path = PathHelper.RoundtripString2PathInformation("PPC:/NewPart/SubPart/Characteristic2/")
            , Uuid = Guid.NewGuid()
            , Attributes = new []{ new AttributeDto(WellKnownKeys.Characteristic.Number, "128")
                                 , new AttributeDto(2003, "MeinCoolesMerkmal_2") }
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
