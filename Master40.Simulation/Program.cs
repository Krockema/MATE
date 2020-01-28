using Hangfire;
using Master40.DB.Data.Context;
using Master40.Simulation.CLI;
using Master40.Simulation.CLI.Arguments;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Master40.Simulation.CLI.Arguments.External;
using Master40.Simulation.CLI.Arguments.Simulation;

namespace Master40.Simulation
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(value: "Welcome to AkkaSim Cli");
            

            var masterDb = ProductionDomainContext.GetContext(ConfigurationManager.AppSettings[index: 0]);
            var validCommands = Commands.GetAllValidCommands;
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

                if (ArgumentConverter.IsArg(validCommands: validCommands, argument: args[lastArg], command: ref command))
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

            if (config.TryGetValue(typeof(StartHangfire), out object startHangfire))
            {
                StartHangfire(((StartHangfire)startHangfire).Silent).Wait();

            } else {
                RunSimulationTask(masterDb: masterDb, config: config).Wait();
                Console.WriteLine(value: "Simulation Run Finished.");
            }
        }


        private static async Task StartHangfire(bool silent)
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(ConfigurationManager.AppSettings[index: 1], HangfireConfiguration.StorageOptions.Default);
            
            Console.WriteLine("-------- Hangfire is Ready -------");


            if (!silent)
            {
                Console.WriteLine("Press any key to start processing.");
                Console.ReadKey();
            }
            
            await  Task.Run(() => new BackgroundJobServer(new BackgroundJobServerOptions { WorkerCount = 1 }));
            Console.ReadLine();
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
    }

}
