using System.Collections.Generic;
using System.Threading.Tasks;
using Master40.SimulationCore.Helper.DistributionProvider;
using Mater40.PiWebApi;
using Microsoft.Extensions.Configuration.UserSecrets;
using Xunit;
using Zeiss.IMT.PiWeb.Api.DataService.Rest;

namespace Master40.XUnitTest.ZeissApi
{
    public class PiWebApiTest
    {
        private ZeissApiClient ApiClient;

        public PiWebApiTest()
        {
            ApiClient = new ZeissApiClient();
        }

        [Fact]
        public async System.Threading.Tasks.Task TestConnectionAsync()
        {
            await ApiClient.PingServer();
        }


        [Fact]
        public async Task CreateInspectionPlan()
        {
            await InspectionPlan.CreateInspectionPlan(ApiClient.ApiClient);
        }

        [Fact]
        public async Task UndoInspectionPlan()
        {
            await InspectionPlan.Undo(ApiClient.ApiClient);
        }

        [Fact]
        public async Task CreateMeasurements()
        {
            var client = ApiClient.ApiClient;
            var generator = new MeasurementValuesGenerator(1000);
            var deflectionGenerator = new DeflectionGenerator(1000);
            var numberOfUses = deflectionGenerator.AddUsage(1);
            var angle = 10;

            await client.CreateParts(new[] {MeasuredPart.newPart, MeasuredPart.subPart});
            await client.CreateCharacteristics(new[] { MeasuredPart.InspectionPlanCharacteristic });

            var measuredPart = new List<DataMeasurement>();
            for (int i = 0; i < 100; i++)
            {
                measuredPart.Add(MeasuredPart.CreateMeasurementData(generator.GetRandomMeasurementValues(
                    estimatedValue: 1, toleranceMax: 1.1, toleranceMin: 0.9, zForPrecision: 0.97)
                ));
            }

            
            await client.CreateMeasurementValues(measuredPart.ToArray());

        }

        [Fact]
        public async Task UndoMeasurements()
        {
            await MeasuredPart.Undo(ApiClient.ApiClient);
        }
    }
}
