using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Util.Internal;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;
using Microsoft.EntityFrameworkCore;

namespace Master40.Simulation.CLI
{
    public static class ArgumentConverter
    {
        public static bool IsArg(List<ICommand> validCommands, string argument, ref ICommand command)
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

        public static SimulationCore.Environment.Configuration ConfigurationConverter(ResultContext resultCtx, int id)
        {
            var validCommands =  Commands.GetAllValidCommands;
            var config = new SimulationCore.Environment.Configuration();
            ICommand command = validCommands.Single(x => x.ArgLong == "SimulationId");
            command.Action(config, id.ToString());

            var configurationItems = resultCtx.ConfigurationRelations
                                    .Include(x => x.ChildItem)
                                    .Where(x => x.Id == id).Select(x => x.ChildItem).ToList();

            if (id != 1)
            {
                configurationItems = AddDefaultItems(resultCtx, configurationItems);    
            }

            foreach (var item in configurationItems)
            {
                if (!IsArg(validCommands: validCommands, argument: "--" + item.Property, command: ref command))
                    throw new InvalidOperationException("No command found that is equal to:" + item.Property);
                //else
                command.Action(arg1: config, arg2: item.PropertyValue);
            }

            return config;
        }

        private static List<ConfigurationItem> AddDefaultItems(ResultContext resultCtx, List<ConfigurationItem> configurationSpecificItems)
        {
            var configurationItems = resultCtx.ConfigurationRelations
                .Include(x => x.ChildItem)
                .Where(x => x.Id == 1).Select(x => x.ChildItem).ToList();

            foreach (var specificItem in configurationSpecificItems)
            {
                var itemToRemove =
                    configurationItems.SingleOrDefault(x => x.Property == specificItem.Property);
                if (itemToRemove != null)
                {
                    configurationItems.Remove(itemToRemove);
                }
            }

            configurationItems.AddRange(configurationSpecificItems);
            return configurationItems;
        }
    }
}
