using System;
using Master40.DB.Enums;
using Master40.SimulationCore.Agents.CollectorAgent;
using Master40.SimulationCore.Agents.ResourceAgent.Types;
using Master40.SimulationCore.Environment.Options;
using Master40.SimulationCore.Helper;
using Master40.SimulationCore.Helper.DistributionProvider;
using Master40.Tools.DistributionProvider;
using static FJobInformations;

namespace Master40.SimulationCore.Agents.ResourceAgent.Behaviour
{
    public class Measurement : SimulationCore.Types.Behaviour
    {
        private readonly MeasurementValuesGenerator _measurementValuesGenerator;
        private readonly DeflectionGenerator _deflectionGenerator;

        public static Measurement Get(Seed seed)
        {
            return new Measurement(seed);
        }

        public Measurement(Seed seed, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
        {
            _measurementValuesGenerator = new MeasurementValuesGenerator(seed.Value);
            _deflectionGenerator = new DeflectionGenerator(seed.Value);
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
            var resourceUsage = _deflectionGenerator.AddUsage(setupId: fJobInformation.Setup.Id);
            
            foreach (var characteristic in job.Operation.Characteristics)
            {
                foreach (var attribute in characteristic.Attributes)
                {
                    var deflectionValue = attribute.Value -
                                          _deflectionGenerator.GetOneDirectionalDeflection(resourceUsage);
                    var quantil = Quantiles.GetFor(fJobInformation.Setup.Quantil);
                    var attr = MessageFactory.CreateMeasurement(job, characteristic, attribute);
                    attr.TimeStamp = Agent.CurrentTime;
                    attr.ResourceTool += "(" + fJobInformation.Setup.Id + ")";
                    attr.MeasurementValue = _measurementValuesGenerator.GetRandomMeasurementValues(deflectionValue
                        , attribute.Tolerance_Min
                        , attribute.Tolerance_Max
                        , quantil.Z);
                    attr.ArticleName = job.Operation.Article.Name;
                    attr.Resource = fJobInformation.Setup.Resource.Name + "(" + fJobInformation.Setup.ResourceId + ")";
                    attr.ArticleKey = job.ArticleKey;
                    measurements.Add(attr);
                }
            }
            
            Agent.Context.System.EventStream.Publish(@event: measurements);
        }
    }
}
