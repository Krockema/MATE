using Master40.Agents.Agents.DataTransformation;
using Master40.Agents.Agents.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Master40.Agents.Agents
{
    public static class DataCollectionHelper
    {
        public static void WriteData(List<Dictionary<string, object>> childData)
        {
            // Write data to csv
            String csv = new String("");
            foreach (Dictionary<string, object> data in childData)
            {
                csv += String.Join(";", data.Keys) + Environment.NewLine;
                csv += String.Join(";", data.Values) + Environment.NewLine;
            }
            System.IO.File.WriteAllText(@"C:\source\out.csv", csv);
        }

        public static void WriteDataGrouped(List<Dictionary<string, object>> childData)
        {
            // Write data to csv
            string contCsv = new string("");
            string dispCsv = new string("");
            string prodCsv = new string("");
            string machCsv = new string("");
            string otherCsv = new string("");

            foreach (Dictionary<string, object> data in childData)
            {
                ref string csv = ref otherCsv;

                if ((Type)data["AgentType"] == typeof(ContractAgent))
                    csv = ref contCsv;
                else if ((Type)data["AgentType"] == typeof(DispoAgent))
                    csv = ref dispCsv;
                else if ((Type)data["AgentType"] == typeof(ProductionAgent))
                    csv = ref prodCsv;
                else if ((Type)data["AgentType"] == typeof(MachineAgent))
                    csv = ref machCsv;

                if (csv == "")
                    csv += String.Join(";", data.Keys) + Environment.NewLine;
                csv += String.Join(";", data.Values) + Environment.NewLine;
            }

            System.IO.File.WriteAllText(@"C:\source\agentdata\contract.csv", contCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\disp.csv", dispCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\production.csv", prodCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\machine.csv", machCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\other.csv", otherCsv);
        }

        public static void CollectPropsTree(object obj, ref Dictionary<string, object> props, String prefix = "")
        {
            Type type = obj.GetType();
            List<AgentPropertyBase> propTree = AgentPropertyManager.GetPropertiesByAgentName(type.Name);

            CollectPropsTreeRecursion(propTree, obj, ref props, prefix);
        }

        private static void CollectPropsTreeRecursion(List<AgentPropertyBase> propTree, object obj, ref Dictionary<string, object> props, String prefix = "")
        {
            Type type = obj.GetType();

            foreach (AgentPropertyBase agentProp in propTree)
            {
                PropertyInfo propInfo = type.GetProperty(agentProp.GetPropertyName(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                object propertyValue = propInfo.GetValue(obj);

                if(agentProp.GetType() == typeof(AgentProperty))
                    props.Add(prefix + propInfo.Name, propertyValue);
                else
                    CollectPropsTreeRecursion(((AgentPropertyNode)agentProp).GetProperties(), propertyValue, ref props, prefix + propInfo.Name + ".");
            }
        }
    }
}
