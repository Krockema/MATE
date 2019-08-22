using Master40.Agents.Agents.DataTransformation;
using Master40.Agents.Agents.Model;
using System;
using System.Collections;
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

        public static void WriteDataGrouped(List<Dictionary<string, object>> childData, string suffix)
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

            System.IO.File.WriteAllText(@"C:\source\agentdata\contract_" + suffix + ".csv", contCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\disp_" + suffix + ".csv", dispCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\production_" + suffix + ".csv", prodCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\machine_" + suffix + ".csv", machCsv);
            System.IO.File.WriteAllText(@"C:\source\agentdata\other_" + suffix + ".csv", otherCsv);
        }

        public static void CollectProps(object obj, ref Dictionary<string, object> props, String prefix = "")
        {
            Type type = obj.GetType();
            List<AgentPropertyBase> propTree = AgentPropertyManager.GetPropertiesByAgentName(type.Name);

            CollectPropsRecursion(propTree, obj, ref props, prefix);
        }

        public static void CollectPropsRecursion(List<AgentPropertyBase> propTree, object obj, ref Dictionary<string, object> props, String prefix = "")
        {
            Type type = obj.GetType();

            foreach (AgentPropertyBase agentProp in propTree)
            {
                PropertyInfo propInfo = type.GetProperty(agentProp.GetPropertyName(), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                object propertyValue = propInfo.GetValue(obj);

                if (propertyValue is IList)
                {
                    // Only save first list element for now
                    if(((IList)propertyValue).Count > 0)
                    {
                        if (agentProp.IsNode())
                            CollectPropsRecursion(((AgentPropertyNode)agentProp).GetProperties(), ((IList)propertyValue)[0], ref props, prefix + propInfo.Name + ".");
                        else
                            props.Add(prefix + propInfo.Name, ((IList)propertyValue)[0]);
                    }

                    //for (int i = 0; i < ((IList)propertyValue).Count; i++)
                    //{
                    //    if (agentProp.GetType() == typeof(AgentProperty))
                    //        props.Add(prefix + propInfo.Name + i, ((IList)propertyValue)[i]);
                    //    else
                    //        CollectPropsRecursion(((AgentPropertyNode)agentProp).GetProperties(), ((IList)propertyValue)[i], ref props, prefix + propInfo.Name + i + ".");
                    //}
                }
                else
                {
                    if (agentProp.IsNode())
                        CollectPropsRecursion(((AgentPropertyNode)agentProp).GetProperties(), propertyValue, ref props, prefix + propInfo.Name + ".");
                    else
                        props.Add(prefix + propInfo.Name, propertyValue);
                }
            }
        }

        public static List<Dictionary<string, object>> FilterAgentData(List<Dictionary<string, object>> data, Type filter)
        {
            List<Dictionary<string, object>> filteredData = new List<Dictionary<string, object>>();
            foreach (Dictionary<string, object> dict in data)
            {
                if ((Type)dict["AgentType"] == filter)
                {
                    filteredData.Add(dict);
                }
            }
            return filteredData;
        }
    }
}
