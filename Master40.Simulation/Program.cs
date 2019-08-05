using Master40.DB.Data.Context;
using Master40.DB.Enums;
using Master40.Simulation.CLI;
using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Master40.Simulation
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Welcome to AkkaSim Cli");

            var masterDb = ProductionDomainContext.GetContext(ConfigurationManager.AppSettings[0]);
            var resultDb = ResultContext.GetContext(ConfigurationManager.AppSettings[1]);
            var validCommands = Commands.GetCommands();
            var command = validCommands.Single(x => x.ArgLong == "Help");
            var lastArg = 0;
            var results = new ParseResult();

            for (; lastArg < args.Length; lastArg++)
            {
                if (args[lastArg] == "-?" || args[lastArg] == "/?")
                {
                    command.Action(null, null);
                    return;
                }

                if (IsArg(validCommands, args[lastArg], ref command))
                {
                    if (command.HasProperty)
                    {
                        lastArg++;
                        command.Action(results, args[lastArg]);
                        
                    } else
                    {
                        command.Action(results, null);
                    }
                }
            }

            switch (results.SimulationType)
            {
                case SimulationType.Central:
                    Console.WriteLine("Sorry, not implemented yet!");
                    break;
                case SimulationType.Decentral:
                    Console.WriteLine("Starting AkkaSim.");
                    var agentCore = new AgentCore(masterDb
                                                , new ConsoleHub());
                    
                    // ToDo Match Command and Update Settings.
                    await agentCore.RunAkkaSimulation(SimulationCore.Environment.Configuration.Create(new object[]
                                                {
                                                    new DBConnectionString(ConfigurationManager.AppSettings[1])
                                                    , new SimulationId(results.ConfigId)
                                                    , new SimulationNumber(1)
                                                    , new SimulationKind(SimulationType.Decentral) //SimulationType.Bucket 
                                                    , new OrderArrivalRate(0.0275)
                                                    , new OrderQuantity(550)
                                                    , new EstimatedThroughPut(800)
                                                    , new DebugAgents(false)
                                                    , new DebugSystem(false)
                                                    , new KpiTimeSpan(480)
                                                    , new Seed(1337)
                                                    , new SettlingStart(2880)
                                                    , new SimulationEnd(20160)
                                                    , new WorkTimeDeviation(0.2)
                                                    , new SaveToDB(false)
                                                }));
                    break;
                default:
                    Console.WriteLine("Ooops. Something went wrong!");
                    break;
            }
        }

        private static bool IsArg(List<ICommand> validCommands, string argument, ref ICommand command)
        {
            if (null != (command = validCommands.SingleOrDefault(x => argument.Equals("-" + x.ArgShort, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            if (null != (command = validCommands.SingleOrDefault(x => argument.Equals("--" + x.ArgLong, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            return false;
        }
    }

}
