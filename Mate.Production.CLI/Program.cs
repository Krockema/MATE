using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Akka.Hive.Logging;
using Hangfire;
using Hangfire.Console;
using Mate.DataCore;
using NLog;

namespace Mate.Production.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(value: "Welcome to AkkaSim Cli");

            // has to be Installed here other wise it would attach a new log listener every time a simulation is called.
            LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_ACTORS, LogLevel.Info, LogLevel.Info);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AGENTS, LogLevel.Debug, LogLevel.Debug);
            //LogConfiguration.LogTo(TargetTypes.File, TargetNames.LOG_AKKA, LogLevel.Trace);
            //LogConfiguration.LogTo(TargetTypes.Console, TargetNames.LOG_AKKA, LogLevel.Warn);
            //LogConfiguration.LogTo(TargetTypes.Debugger, TargetNames.LOG_AKKA, LogLevel.Warn);

            var validCommands = Commands.GetAllValidCommands;
            var command = validCommands.Single(predicate: x => x.ArgLong == "Help");
            var lastArg = 0;
            var config = new Core.Environment.Configuration();

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
                RunSimulationTask(DataBaseConfiguration.MateDb, config: config).Wait();
                Console.WriteLine(value: "Simulation Run Finished.");
            }
        }


        private static async Task StartHangfire(bool silent)
        {
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseColouredConsoleLogProvider()
                .UseConsole()
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(ConfigurationManager.AppSettings["HangfireConnection"], HangfireConfiguration.StorageOptions.Default);
            
            Console.WriteLine("-------- Hangfire is Ready -------");


            if (!silent)
            {
                Console.WriteLine("Press any key to start processing.");
                Console.ReadKey();
            }
            
            await Task.Run(() => new BackgroundJobServer(new BackgroundJobServerOptions { WorkerCount = 1 }));
            Console.ReadLine();
        }

        private static async Task RunSimulationTask(string dbName
                                                    , Core.Environment.Configuration config)
        {
            foreach (var item in config)
            {
                Console.WriteLine(item.Key + " " + ((dynamic)item.Value).Value.ToString());
            }

            try
            {
                Console.WriteLine(value: "Starting AkkaSim.");
                var agentCore = new AgentCore(dbName, messageHub: new LoggingHub());
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
