using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.DataModel;
using Master40.DB.ReportingModel;
using Master40.Tools.DistributionProvider;
using Microsoft.EntityFrameworkCore;
using Zeiss.IMT.PiWeb.Api.Common.Data;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;
using Attribute = Zeiss.IMT.PiWeb.Api.DataService.Rest.Attribute;

namespace Mater40.PiWebApi
{
    public class ZeissConnector
    {
        private ZeissApiClient ApiClient;
        private static int _counter;
        //private static string _partPath;
        private static Dictionary<string, InspectionPlanPart> piWebPartsDictionary = new Dictionary<string, InspectionPlanPart>();
        private static Dictionary<string, InspectionPlanCharacteristic> piWebCharacteristicsDictionary = new Dictionary<string, InspectionPlanCharacteristic>();
        private static List<string> _transferredParts = new List<string>();
        public ZeissConnector()
        {
            ApiClient = new ZeissApiClient();
            _counter = 0;
            //_partPath = "";
        }

        public static string CheckForPart(M_Article part, string path)
        {
            var partInformation = CreatePart(part, path);
            if (!part.Operations.Any()) return partInformation.Item2;
            foreach (var operation in part.Operations)
            {
                foreach (var characteristic in operation.Characteristics)
                {
                    AddCharacteristicsWithAttributes(partInformation.Item1, characteristic, partInformation.Item2);
                }
            }

            return partInformation.Item2;
        }

        private static Tuple<InspectionPlanPart, string> CreatePart(M_Article part, string path)
        {
            var client = new ZeissApiClient().ApiClient;

            var partPath = "P" + path;

            partPath = partPath + part.Name + "/";

            var inspectionPlanPart = new InspectionPlanPart
            {
                Path = PathHelper.RoundtripString2PathInformation(partPath),
                Uuid = Guid.NewGuid()
            };

            var checkedPath = PathHelper.String2PartPathInformation("/" + part.Name + "/");
            var foundParts =  client.GetParts(checkedPath, depth: 0).Result;
            if (foundParts.Any()) return Tuple.Create(foundParts.First(), partPath);
            if (_transferredParts.Contains(partPath)) return Tuple.Create(inspectionPlanPart, partPath);
            client.CreateParts(new[] {inspectionPlanPart}).Wait(TimeSpan.FromSeconds(1));
            _transferredParts.Add(partPath);
            return Tuple.Create(inspectionPlanPart, partPath);
        }

        private static void AddCharacteristicsWithAttributes(InspectionPlanPart inspectionPlanPart, M_Characteristic characteristic, string path)
        {
            var client = new ZeissApiClient().ApiClient;
            //Search for Characteristics which are associated with the part --> build path out of given part and characteristic and map attributes to PiWeb
            var characteristicPath = path.Replace(":/", "C:/");
            characteristicPath = characteristicPath + characteristic.Name + "/";

            var inspectionPlanCharacteristics = new List<InspectionPlanCharacteristic>();
            var inspectionPlanCharacteristic = new InspectionPlanCharacteristic
            {
                Path = PathHelper.RoundtripString2PathInformation(characteristicPath),
                Uuid = Guid.NewGuid()
            };

            //inspectionPlanCharacteristics.Add(inspectionPlanCharacteristic);
            var characteristicExists = client.GetCharacteristics(inspectionPlanCharacteristic.Path).Result;
            if(!characteristicExists.Any()) client.CreateCharacteristics(new[] {inspectionPlanCharacteristic}).Wait(TimeSpan.FromSeconds(1));

            var characteristicSupportPath = characteristicPath.Replace(":/", "C:/");

            foreach (var attribute in characteristic.Attributes)
            {
                var subCharacteristicPath = characteristicSupportPath + attribute.Name + "/";
                var attributeIdentifier = attribute.Name.Split(" ").Last();

                var inspectionPlanSubCharacteristic = new InspectionPlanCharacteristic
                {
                    Path = PathHelper.RoundtripString2PathInformation(subCharacteristicPath),
                    Uuid = Guid.NewGuid(),
                    Attributes = new[] {
                        new Attribute(SsopToPiWebMappings.GetFor(attributeIdentifier).AttributeNamePiWeb, attribute.Value)
                        , new Attribute(WellKnownKeys.Characteristic.DesiredValue, attribute.Value)
                        , new Attribute(WellKnownKeys.Characteristic.NominalValue, attribute.Value)
                        , new Attribute(WellKnownKeys.Characteristic.LowerSpecificationLimit, attribute.Value + attribute.Tolerance_Min)
                        , new Attribute(WellKnownKeys.Characteristic.UpperSpecificationLimit, attribute.Value + attribute.Tolerance_Max)
                        , new Attribute(WellKnownKeys.Characteristic.LowerTolerance, attribute.Value + attribute.Tolerance_Min)
                        , new Attribute(WellKnownKeys.Characteristic.UpperTolerance, attribute.Value + attribute.Tolerance_Max)
                        , new Attribute(12012, characteristic.Operation.ResourceToolId)
                        //possible ValueType?
                    }
                };
                inspectionPlanCharacteristics.Add(inspectionPlanSubCharacteristic);
                var inspectionPlanSubCharacteristicExists =
                    client.GetCharacteristics(inspectionPlanSubCharacteristic.Path).Result;
                if(!inspectionPlanSubCharacteristicExists.Any()) client.CreateCharacteristics(new[] {inspectionPlanSubCharacteristic}).Wait(TimeSpan.FromSeconds(1));
            }

            //if (client.GetCharacteristics(inspectionPlanPart.Path).Result
            //    .FirstOrDefault(c => c.Path.Name.Contains(characteristic.Name)) == null) client.CreateCharacteristics(new[] {inspectionPlanCharacteristic});
        }

        //public static void CreateMeasurements(M_Attribute attribute ,InspectionPlanPart inspectionPlanPart, InspectionPlanCharacteristic inspectionPlanCharacteristic)
        //{
        //    var client = new ZeissApiClient().ApiClient;
        //    var generator = new MeasurementValuesGenerator(1000);
        //    //var deflectionGenerator = new DeflectionGenerator(1000);
        //
        //    var measuredPart = new List<DataMeasurement>();
        //    for (int i = 0; i < 100; i++)
        //    {
        //        measuredPart.Add(CreateMeasurementData(generator.GetRandomMeasurementValues(
        //            estimatedValue: attribute.Value, toleranceMax: attribute.Tolerance_Max, toleranceMin: attribute.Tolerance_Min, zForPrecision: 0.97)  //zForPrecision aus Operation
        //            , inspectionPlanPart, inspectionPlanCharacteristic));
        //    }
        //    
        //    client.CreateMeasurementValues(measuredPart.ToArray());
        //}

        public static void TransferMeasurementsToPiWeb(SimulationMeasurement measure)   //SimulationMeasurement
        {
            if (_counter == 0) GetProductStructure();

            var inspectionPlanPart = piWebPartsDictionary[measure.ArticleName];
            var inspectionPlanCharacteristic = piWebCharacteristicsDictionary[measure.AttributeName];

            CreateMeasurements(measure, inspectionPlanPart, inspectionPlanCharacteristic);
        }

        private static void GetProductStructure()
        {
            _counter++;
            var client = new ZeissApiClient().ApiClient;
            var parts = client.GetParts(PathHelper.RoundtripString2PathInformation("P:/Dump-Truck/"), depth: 3).Result;
            foreach (var part in parts)
            {
                if(!piWebPartsDictionary.ContainsKey(part.Path.Name)) piWebPartsDictionary.Add(part.Path.Name, part);
                var characteristics = client.GetCharacteristics(part.Path, depth: 2).Result;
                foreach (var characteristic in characteristics)
                {
                    if(!piWebCharacteristicsDictionary.ContainsKey(characteristic.Path.Name)) piWebCharacteristicsDictionary.Add(characteristic.Path.Name, characteristic);
                }
            }
        }
        private static void CreateMeasurements(SimulationMeasurement measure, InspectionPlanPart inspectionPlanPart, InspectionPlanCharacteristic inspectionPlanCharacteristic)
        {
            var client = new ZeissApiClient().ApiClient;

            

            //var existingDataMeasurement = client.GetMeasurementValues(
            //    inspectionPlanPart.Path,
            //    new MeasurementValueFilterAttributes
            //    {
            //        SearchCondition = new GenericSearchAttributeCondition
            //        {
            //            Attribute = 14,//WellKnownKeys.Measurement.PartsId,
            //            Operation = Operation.Equal,
            //            Value = measure.ArticleKey.ToString()
            //        }
            //    }).Result;
            //
            //if (existingDataMeasurement.Any()) UpdateMeasurement(existingDataMeasurement.First(), measure, inspectionPlanPart, inspectionPlanCharacteristic);
            //
            //else
            //{
            //    var measuredPart = CreateMeasurementData(measure, inspectionPlanPart, inspectionPlanCharacteristic);
            //    client.CreateMeasurementValues(new[] { measuredPart }).Wait(TimeSpan.FromSeconds(1));
            //}
            var measuredPart = CreateMeasurementData(measure, inspectionPlanPart, inspectionPlanCharacteristic);
            client.CreateMeasurementValues(new[] { measuredPart }).Wait(TimeSpan.FromSeconds(1));

        }

        private static void UpdateMeasurement(DataMeasurement dataMeasurement, SimulationMeasurement measure, InspectionPlanPart inspectionPlanPart, InspectionPlanCharacteristic inspectionPlanCharacteristic)
        {
            var client = new ZeissApiClient().ApiClient;

            var dataCharacteristics = dataMeasurement.Characteristics;
            var dataCharacteristicsList = dataCharacteristics.ToList();
            dataCharacteristicsList.Add(new DataCharacteristic
            {
                Path = inspectionPlanCharacteristic.Path,
                Uuid = inspectionPlanCharacteristic.Uuid,
                Value = new DataValue
                {
                    Attributes = new[]
                    {
                        new Attribute(WellKnownKeys.Value.MeasuredValue, measure.MeasurementValue),
                        new Attribute(18, measure.Resource),
                        new Attribute(19, measure.ResourceTool),
                        //new Attribute(20, new DateTime(measure.TimeStamp, DateTimeKind.Utc))
                    }
                },
                Timestamp = new DateTime(measure.TimeStamp)
            });

            dataMeasurement.Characteristics = dataCharacteristicsList.ToArray();
            
            client.UpdateMeasurementValues(new []{dataMeasurement}).Wait(TimeSpan.FromSeconds(1));
        }
        private static DataMeasurement CreateMeasurementData(SimulationMeasurement measure, InspectionPlanPart inspectionPlanPart, InspectionPlanCharacteristic inspectionPlanCharacteristic)
        {
            var dataMeasurement = new DataMeasurement
            {
                Uuid = Guid.NewGuid(),
                PartUuid = inspectionPlanPart.Uuid,
                Attributes = new[]
                {
                    new Attribute(WellKnownKeys.Measurement.InspectorName, 1),
                    new Attribute(WellKnownKeys.Measurement.PartsId, measure.ArticleKey), 
                },
                Characteristics = new[]{ new DataCharacteristic
                {
                    Path = inspectionPlanCharacteristic.Path,
                    Uuid = inspectionPlanCharacteristic.Uuid,
                    Value = new DataValue
                    {
                        Attributes = new []
                        {
                            new Attribute(WellKnownKeys.Value.MeasuredValue, measure.MeasurementValue),
                            new Attribute(18, measure.Resource),
                            new Attribute(19, measure.ResourceTool),
                            //new Attribute(20, new DateTime(measure.TimeStamp, DateTimeKind.Utc))
                        }
                    },
                    Timestamp = new DateTime(measure.TimeStamp),
                } }
            };
            return dataMeasurement;
        }
    }
}
