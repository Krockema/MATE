using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;
using Attribute = Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute;

namespace Mater40.PiWebApi
{
    public class MeasuredPart
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
                , new Attribute(WellKnownKeys.Characteristic.LowerSpecificationLimit , "0.9")
                , new Attribute(WellKnownKeys.Characteristic.UpperSpecificationLimit , "1.1")
                , new Attribute(WellKnownKeys.Characteristic.UpperWarningLimit, "1.025")
                , new Attribute(WellKnownKeys.Characteristic.LowerWarningLimit, "0.975")
                , new Attribute(WellKnownKeys.Characteristic.DesiredValue, "1")
            }
        };

        public static DataMeasurement CreateMeasurementData(double value)
        {
            return new DataMeasurement
            {
                Uuid = Guid.NewGuid()
                ,PartUuid = subPart.Uuid
                // Attribute besteht aus dem Operator Key und der Eintragsnummer (PiWeb Kataloge -> Operator )
                ,Attributes = new []{ new Attribute( WellKnownKeys.Measurement.InspectorName, 1) }
                , Characteristics = new []{ new DataCharacteristic
                {
                    Path = InspectionPlanCharacteristic.Path,
                    Uuid = InspectionPlanCharacteristic.Uuid,
                    Value = new DataValue(value),
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
