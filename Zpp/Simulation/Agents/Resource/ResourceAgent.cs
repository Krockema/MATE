using System;
using System.Diagnostics;
using Akka.Actor;
using AkkaSim;
using Master40.DB.Enums;
using Zpp.Common.ProviderDomain.Wrappers;
using Zpp.Simulation.Agents.JobDistributor.Skills;
using Zpp.Simulation.Agents.Resource.Skills;

namespace Zpp.Simulation.Agents.Resource
{
    partial class Resource : SimulationElement
    {
        // Temp for test
        Random r = new Random(1337);

        public Resource(IActorRef simulationContext, long time) : base(simulationContext, time)
        {
            Debug.WriteLine("Time: " + TimePeriod + " - " + Self.Path.Name + " is Ready");
        }
        public static Props Props(IActorRef simulationContext, long time)
        {
            return Akka.Actor.Props.Create(() => new Resource(simulationContext, time));
        }

        protected override void Do(object o)
        {
            switch (o)
            {
                case Work m: DoWork(m.GetOperation); break;
                case FinishWork f: WorkDone(f.GetOperation); break;
                default: new Exception("Message type could not be handled by SimulationElement"); break;
            }
        }

        private void DoWork(ProductionOrderOperation operation)
        {
            // TODO Use distribution from AkkaSIm
            var dur = operation.GetDuration().GetValue() + r.Next(-1, 2);
            var rawOperation = operation.GetValue();
            rawOperation.Start = (int)TimePeriod;
            rawOperation.End = rawOperation.Start + dur;
            rawOperation.ProducingState = ProducingState.Producing;

            _SimulationContext.Tell(message: WithDrawMaterialsFor.Create(operation, Context.Parent)
                                    ,sender: Self);

            Schedule(dur, FinishWork.Create(operation, Self));
            //_SimulationContext.Tell(s, null);
            Debug.WriteLine("Time: " + TimePeriod + " - " + Self.Path + " - Working on: " + operation.GetValue().Name);
        }

        private void WorkDone(ProductionOrderOperation operation)
        { 
            _SimulationContext.Tell(ProductionOrderFinished.Create(operation, Context.Parent), Self);
            Debug.WriteLine("Time: " + TimePeriod + " - " + Self.Path + " Finished: " + operation.GetValue().Name);
        }

        protected override void Finish()
        {
            base.Finish();
        }
    }
}