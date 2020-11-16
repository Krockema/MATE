using System;
using System.Threading.Tasks;
using Zeiss.PiWeb.Api.Definitions;
using Zeiss.PiWeb.Api.Rest.Dtos;
using Zeiss.PiWeb.Api.Rest.Dtos.Data;
using Zeiss.PiWeb.Api.Rest.HttpClient.Data;

namespace Master40.PiWebApi
{
    public class MeasuredPart
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
                , new AttributeDto(WellKnownKeys.Characteristic.LowerSpecificationLimit , "0.9")
                , new AttributeDto(WellKnownKeys.Characteristic.UpperSpecificationLimit , "1.1")
                , new AttributeDto(WellKnownKeys.Characteristic.UpperWarningLimit, "1.025")
                , new AttributeDto(WellKnownKeys.Characteristic.LowerWarningLimit, "0.975")
                , new AttributeDto(WellKnownKeys.Characteristic.DesiredValue, "1")
            }
        };

        public static DataMeasurementDto CreateMeasurementData(double value)
        {
            return new DataMeasurementDto
            {
                Uuid = Guid.NewGuid()
                ,PartUuid = subPart.Uuid
                // Attribute besteht aus dem Operator Key und der Eintragsnummer (PiWeb Kataloge -> Operator )
                ,Attributes = new []{ new AttributeDto( WellKnownKeys.Measurement.InspectorName, 1) }
                , Characteristics = new []{ new DataCharacteristicDto
                {
                    Path = InspectionPlanCharacteristic.Path,
                    Uuid = InspectionPlanCharacteristic.Uuid,
                    Value = new DataValueDto(value),
                    Timestamp = DateTime.Now + TimeSpan.FromSeconds(1)
                } }
                
            };
        }

        public static async Task Undo(DataServiceRestClient client)
        {
            await client.DeleteParts(InspectionPlan.newPart.Path);
        }
    }
}
