using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Configuration;
using System.Linq;
using Master40.DB.Enums;
using Master40.Simulation.CLI;
using Master40.Simulation.CLI.Arguments;

namespace Master40.Simulation
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            Console.WriteLine("Welcome to AkkaSim Cli");

            var masterDb = GetMasterDBContext();
            var resultDb = GetResultDBContext();
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
                    var agentCore = new AgentCore(masterDb, resultDb, new ConsoleHub());
                    var simConfig = agentCore.UpdateSettings(results.ConfigId, 550,0.0275, 800);
                    await agentCore.RunAkkaSimulation(simConfig);
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
