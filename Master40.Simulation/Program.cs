using Master40.DB.Data.Context;
using Master40.Simulation.CLI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Master40.SimulationCore.Environment.Abstractions;

namespace Master40.Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(value: "Welcome to AkkaSim Cli");
            

            var masterDb = ProductionDomainContext.GetContext(defaultCon: ConfigurationManager.AppSettings[index: 0]);
            var validCommands = new Commands();
            var command = validCommands.Single(predicate: x => x.ArgLong == "Help");
            var lastArg = 0;
            var config = new SimulationCore.Environment.Configuration();

            for (; lastArg < args.Length; lastArg++)
            {
                if (args[lastArg] == "-?" || args[lastArg] == "/?")
                {
                    command.Action(arg1: null, arg2: null);
                    return;
                }

                if (IsArg(validCommands: validCommands, argument: args[lastArg], command: ref command))
                {
                    if (command.HasProperty)
                    {
                        lastArg++;
                        command.Action(arg1: config, arg2: args[lastArg]);

                    }
                    else
                    {
                        command.Action(arg1: config, arg2: null);
                    }
                }
            }

            RunSimulationTask(masterDb: masterDb, config: config).Wait();
        }

        private static async Task RunSimulationTask(ProductionDomainContext masterDb
                                                    , SimulationCore.Environment.Configuration config)
        {
            foreach (var item in config)
            {
                Console.WriteLine(item.Key + " " + ((dynamic)item.Value).Value.ToString());
            }

            try
            {
                Console.WriteLine(value: "Starting AkkaSim.");
                var agentCore = new AgentCore(context: masterDb, messageHub: new ConsoleHub());
                await agentCore.RunAkkaSimulation(configuration: config);
                
            }
            catch (Exception)
            {
                Console.WriteLine(value: "Ooops. Something went wrong!");
                throw;
            }
        }

        private static bool IsArg(List<ICommand> validCommands, string argument, ref ICommand command)
        {
            if (null != (command = validCommands.SingleOrDefault(predicate: x => argument
                                                    .Equals(value: "-" + x.ArgShort, comparisonType: StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            if (null != (command = validCommands.SingleOrDefault(predicate: x => argument
                                                    .Equals(value: "--" + x.ArgLong, comparisonType: StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            return false;
        }
    }

}
