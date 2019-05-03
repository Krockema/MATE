using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Configuration;
using System.Linq;
using Master40.DB.Enums;
using Master40.Simulation.CLI;

namespace Master40.Simulation
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Welcome to AkkaSim Cli");

            var success = true;
            var command = string.Empty;
            var lastArg = 0;
            var masterDb = GetMasterDBContext();
            var resultDb = GetResultDBContext();
            int configId = 1;
            var simType = SimulationType.Central;

            for (; lastArg < args.Length; lastArg++)
            {
                if (IsArg(args[lastArg], "h", "help") ||
                    args[lastArg] == "-?" ||
                    args[lastArg] == "/?")
                {
                    HelpCommand.PrintHelp();
                    return;
                }
                // Console.WriteLine(args[lastArg]);

                if (IsArg(args[lastArg], "c", "config"))
                {
                    lastArg++;
                    // Console.WriteLine(args[lastArg]);
                    configId = Int32.Parse(args[lastArg]);
                }

                if (IsArg(args[lastArg], "s", "simType"))
                {
                    lastArg++;
                    // Console.WriteLine(args[lastArg]);
                    if (IsArg("-" + args[lastArg], "Central", "central"))
                    {
                        Console.WriteLine("Central planned production is selected!");
                    }

                    if (IsArg("-" + args[lastArg], "Decentral", "decentral"))
                    {
                        simType = SimulationType.Decentral;
                        Console.WriteLine("Self organized production is selected!");
                    }
                } 
            }


            Console.WriteLine(simType.ToString());
            switch (simType)
            {
                case SimulationType.Central:
                    Console.WriteLine("Sorry, not implemented yet!");
                    break;
                case SimulationType.Decentral:
                    Console.WriteLine("Starting AkkaSim.");
                    var agentCore = new AgentCore(masterDb, resultDb, new ConsoleHub());
                    var simConfig = agentCore.UpdateSettings(configId, 550,0.0275, 800);
                    await agentCore.RunAkkaSimulation(simConfig);
                    break;
                default:
                    Console.WriteLine("Ooops. Something went wrong!");
                    break;
            }
        }

        private static bool IsArg(string candidate, string shortName, string longName)
        {
            return (shortName != null && candidate.Equals("-" + shortName, StringComparison.OrdinalIgnoreCase)) ||
                   (longName != null && candidate.Equals("--" + longName, StringComparison.OrdinalIgnoreCase));
        }


        private static ProductionDomainContext GetMasterDBContext()
        {
            var defaultConnectionString = ConfigurationManager.AppSettings[0];
            return new ProductionDomainContext(new DbContextOptionsBuilder<MasterDBContext>()
                .UseSqlServer(defaultConnectionString)
                .Options);
        }

        private static ResultContext GetResultDBContext()
        {

            var resultConnectionString = ConfigurationManager.AppSettings[1];
            return new ResultContext(new DbContextOptionsBuilder<ResultContext>()
                .UseSqlServer(resultConnectionString)
                .Options);
        }

    }

}
