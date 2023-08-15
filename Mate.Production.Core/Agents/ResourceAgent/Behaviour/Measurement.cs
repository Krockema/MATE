using Mate.DataCore.Nominal;
using Mate.PiWebApi.DistributionProvider;
using Mate.Production.Core.Agents.ResourceAgent.Types;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Environment.Records;
using Mate.Production.Core.Environment.Records.Reporting;
using Mate.Production.Core.Helper;
using Mate.Production.Core.Helper.DistributionProvider;

namespace Mate.Production.Core.Agents.ResourceAgent.Behaviour
{
    public class Measurement : Core.Types.Behaviour
    {
        private readonly MeasurementValuesGenerator _measurementValuesGenerator;
        private readonly DeflectionGenerator _deflectionGenerator;

        public static Measurement Get(Environment.Options.Seed seed)
        {
            return new Measurement(seed);
        }

        public Measurement(Environment.Options.Seed seed, SimulationType simulationType = SimulationType.None) : base(childMaker: null, simulationType: simulationType)
        {
            _measurementValuesGenerator = new MeasurementValuesGenerator(seed.Value);
            _deflectionGenerator = new DeflectionGenerator(seed.Value);
        }

        public override bool Action(object message)
        {
            switch (message)
            {
                case Resource.Instruction.Default.CreateMeasurements msg: CreateMeasurement(fMeasurementInformation: msg.GetObjectFromMessage); break;
                // case BasicInstruction.ResourceBrakeDown msg: BreakDown((Resource)agent, msg.GetObjectFromMessage); break;
                default: return false;
            }
            return true;
        }

        /// <summary>
        /// Register the Resource in the System on Startup and Save the Hub agent.
        /// </summary>
        private void CreateMeasurement(MeasurementInformationRecord fMeasurementInformation)
        {
            // !Collector.Config.GetOption<CreateQualityData>().Value
            var job = (OperationRecord)fMeasurementInformation.Job;
            var measurements = new Measurements();
            var resourceUsage = _deflectionGenerator.AddUsage(setupId: fMeasurementInformation.CapabilityProviderId);
            
            foreach (var characteristic in job.Operation.Characteristics)
            {
                foreach (var attribute in characteristic.Attributes)
                {
                    var deflectionValue = attribute.Value -
                                          _deflectionGenerator.GetOneDirectionalDeflection(resourceUsage);
                    var quantil = Quantiles.GetFor(fMeasurementInformation.Quantile);
                    var attr = MessageFactory.CreateMeasurement(job, characteristic, attribute, fMeasurementInformation);
                    attr.TimeStamp = Agent.CurrentTime.ToFileTimeUtc();
                    attr.ResourceTool += "(" + fMeasurementInformation.CapabilityProviderId + ")";
                    attr.MeasurementValue = _measurementValuesGenerator.GetRandomMeasurementValues(deflectionValue
                        , attribute.Tolerance_Min
                        , attribute.Tolerance_Max
                        , quantil.Z);
                    attr.ArticleName = job.Operation.Article.Name;
                    attr.Resource = fMeasurementInformation.Resource + "(" + fMeasurementInformation.CapabilityProviderId + ")";
                    attr.ArticleKey = job.ArticleKey;
                    measurements.Add(attr);
                }
            }
            
            Agent.Context.System.EventStream.Publish(@event: measurements);
        }
    }
}
