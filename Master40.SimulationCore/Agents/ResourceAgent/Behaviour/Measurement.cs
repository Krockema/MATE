using Master40.DB.Enums;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using static FJobInformations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Measurement : SimulationCore.Types.Behaviour
    {
        private MeasurementValuesGenerator measurementValuesGenerator;

        public static Measurement Get(Seed seed)
        {
            return new Measurement(seed);
        }

        public Measurement(Seed seed, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
        {
            measurementValuesGenerator = new MeasurementValuesGenerator(seed.Value);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.BucketScope.CreateMeasurements msg: CreateMeasurement(fJobInformation: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Register the Resource in the System on Startup and Save the Hub agent.
        /// </summary>
        private void CreateMeasurement(FJobInformation fJobInformation)
        {
            // !Collector.Config.GetOption<CreateQualityData>().Value
            var job = ((FOperations.FOperation)fJobInformation.Job);
            var measurements = new Measurements();

            foreach (var characteristic in job.Operation.Characteristics)
            {
                foreach (var attribute in characteristic.Attributes)
                {
                    var attr = MessageFactory.CreateMeasurement(job, characteristic, attribute);
                    attr.TimeStamp = Agent.CurrentTime;
                    attr.ResourceTool += "(" + fJobInformation.Setup.Id + ")";
                    attr.MeasurementValue = measurementValuesGenerator.GetRandomMeasurementValues(attribute.Value
                        , attribute.Tolerance_Min
                        , attribute.Tolerance_Max
                        , /* TODO: kommt noch */ fJobInformation.Setup.ZForPrecision);
                    measurements.Add(attr);
                }
            }

            Agent.Context.System.EventStream.Publish(@event: measurements);
        }
    }
}
