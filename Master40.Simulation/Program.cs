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
            Console.WriteLine("Welcome to AkkaSim Cli");
            

            var masterDb = ProductionDomainContext.GetContext(ConfigurationManager.AppSettings[0]);
            var validCommands = new Commands();
            var command = validCommands.Single(x => x.ArgLong == "Help");
            var lastArg = 0;
            var config = new SimulationCore.Environment.Configuration();

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
                        command.Action(config, args[lastArg]);

                    }
                    else
                    {
                        command.Action(config, null);
                    }
                }
            }

            RunSimulationTask(masterDb, config).Wait();
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
                Console.WriteLine("Starting AkkaSim.");
                var agentCore = new AgentCore(masterDb, new ConsoleHub());
                await agentCore.RunAkkaSimulation(config);
                
            }
            catch (Exception)
            {
                Console.WriteLine("Ooops. Something went wrong!");
                throw;
            }
        }

        private static bool IsArg(List<ICommand> validCommands, string argument, ref ICommand command)
        {
            if (null != (command = validCommands.SingleOrDefault(x => argument
                                                    .Equals("-" + x.ArgShort, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            if (null != (command = validCommands.SingleOrDefault(x => argument
                                                    .Equals("--" + x.ArgLong, StringComparison.OrdinalIgnoreCase))))
            {
                return true;
            }
            return false;
        }
    }

}
