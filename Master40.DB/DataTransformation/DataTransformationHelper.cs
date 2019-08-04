using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using System.Linq;
using System.Reflection;
using Master40.DB.DataTransformation.Conversions;
using Master40.DB.Data.Initializer;
using Master40.DB.GanttplanDB.Models;

namespace Master40.DB.DataTransformation
{
    public class DataTransformationHelper
    {
        private MasterDBContext MasterContext;
        private GPSzenarioContext GpContext;
        // Core data only needs to be transformed once
        private List<Mapping> CoreDataRules = new List<Mapping>();
        private Dictionary<string, SourceRuleGroup> CoreDataRuleGroups = new Dictionary<string, SourceRuleGroup>();
        // Agent data will be transformed periodically
        private List<Mapping> AgentDataRules = new List<Mapping>();
        private Dictionary<string, SourceRuleGroup> AgentDataRuleGroups = new Dictionary<string, SourceRuleGroup>();
        private Dictionary<string, SourceRuleGroup> AgentDataRuleGroupsReversed = new Dictionary<string, SourceRuleGroup>();

        public DataTransformationHelper(MasterDBContext masterContext, GPSzenarioContext gpContext)
        {
            this.MasterContext = masterContext;
            this.GpContext = gpContext;

            MasterContext.Database.EnsureCreated();
            SortTransformationRules();
            this.CoreDataRuleGroups = GroupRules(CoreDataRules);
            this.AgentDataRuleGroups = GroupRules(AgentDataRules);
            this.AgentDataRuleGroupsReversed = GroupRules(AgentDataRules, true);
        }

        private void SortTransformationRules()
        {
            foreach (Mapping mapping in MasterContext.Mappings)
            {
                if (mapping.IsAgentData)
                    AgentDataRules.Add(mapping);
                else
                    CoreDataRules.Add(mapping);
            }
        }

        private Dictionary<string, SourceRuleGroup> GroupRules(List<Mapping> rules, bool reversed = false)
        {
            Dictionary<string, SourceRuleGroup> RuleGroup = new Dictionary<string, SourceRuleGroup>();
            Dictionary<string, DestinationRuleGroup> tmpRuleGroup = new Dictionary<string, DestinationRuleGroup>();

            // Group source tables
            foreach (Mapping mapping in rules)
            {
                Mapping actualMapping;
                if (!reversed)
                    actualMapping = mapping;
                else
                    actualMapping = ReverseMapping(mapping);
                string sourceTabName = actualMapping.GetFromTable();
                if (!tmpRuleGroup.ContainsKey(sourceTabName))
                    tmpRuleGroup[sourceTabName] = new DestinationRuleGroup();
                tmpRuleGroup[sourceTabName].AddRule(actualMapping);
            }

            // Group destination tables
            foreach (KeyValuePair<string, DestinationRuleGroup> srcGroup in tmpRuleGroup)
            {
                SourceRuleGroup srcRuleGroup = new SourceRuleGroup();
                foreach (Mapping rule in srcGroup.Value.Rules)
                {
                    string destTabName = rule.GetToTable();
                    if (!srcRuleGroup.RuleGroups.ContainsKey(destTabName))
                        srcRuleGroup.RuleGroups[destTabName] = new DestinationRuleGroup();
                    srcRuleGroup.RuleGroups[destTabName].AddRule(rule);

                }
                RuleGroup[srcGroup.Key] = srcRuleGroup;
            }

            return RuleGroup;
        }

        private Mapping ReverseMapping(Mapping originalMapping)
        {
            return new Mapping
            {
                From = originalMapping.To,
                To = originalMapping.From,
                IsAgentData = originalMapping.IsAgentData,
                ConversionFunc = originalMapping.ConversionFunc,
                ConversionArgs = originalMapping.ConversionArgs
            };
        }

        public Boolean TransformCoreData()
        {
            GpContext.Database.EnsureDeleted();
            GPSzenarioInitializer.DbInitialize(this.GpContext);

            ProcessCoreSourceRuleGroups(MasterContext, GpContext, this.CoreDataRuleGroups);
            GpContext.SaveChanges();
            return true;
        }

        private void ProcessCoreSourceRuleGroups(DbContext fromContext, DbContext toContext, Dictionary<string, SourceRuleGroup> sourceRuleGroups)
        {
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in sourceRuleGroups)
            {
                dynamic sourceData = GetTableByName(fromContext, srcGroup.Key);

                foreach (object srcTuple in sourceData)
                    ProcessCoreDestinationRuleGroups(toContext, srcGroup.Value.RuleGroups, srcTuple);
            }
        }

        private void ProcessCoreDestinationRuleGroups(DbContext toContext, Dictionary<string, DestinationRuleGroup> destRuleGroups, object sourceTuple)
        {
            foreach (KeyValuePair<string, DestinationRuleGroup> destGroup in destRuleGroups)
            {
                // Dictionary<DestinationPropName, SourceData> 
                Dictionary<string, object> tupleData = ReadCoreSourceTupleData(destGroup.Value.Rules, sourceTuple);

                // Locate destination table
                dynamic destTable = GetTableByName(toContext, destGroup.Key);
                Type destDataObjectType = GetTupleTypeByTableName(toContext, destGroup.Key);

                // Acquire destination object
                object destDataObject = SearchOrCreateDestinationObject(toContext, destTable, tupleData, destDataObjectType);

                // Copy data
                foreach (KeyValuePair<string, object> data in tupleData)
                    destDataObjectType.GetProperty(data.Key).SetValue(destDataObject, data.Value);

                toContext.Add(destDataObject);
            }
        }

        private Dictionary<string, object> ReadCoreSourceTupleData(List<Mapping> rules, object srcTuple)
        {
            Dictionary<string, object> destTupleData = new Dictionary<string, object>();

            // Read source data
            foreach (Mapping rule in rules)
            {
                dynamic sourcePropData;
                if (rule.IsFromEmpty())
                {
                    sourcePropData = null;
                }
                else
                {
                    Type tupleType = srcTuple.GetType();
                    PropertyInfo tupelProp = tupleType.GetProperty(rule.GetFromColumn());
                    sourcePropData = tupelProp.GetValue(srcTuple);
                }
                destTupleData[rule.GetToColumn()] = Conversion.DoConvert(rule.ConversionFunc,
                    rule.ConversionArgs, sourcePropData, false);
            }

            return destTupleData;
        }

        public Boolean TransformAgentDataToGp(List<Dictionary<string, object>> agentData)
        {
            GpContext.Database.EnsureCreated();
            foreach(Productionorder po in GpContext.Productionorder)
            {
                GpContext.Productionorder.Remove(po);
                GpContext.SaveChanges();
            }
            foreach (ProductionorderOperationActivity pooa in GpContext.ProductionorderOperationActivity)
            {
                GpContext.ProductionorderOperationActivity.Remove(pooa);
                GpContext.SaveChanges();
            }

            ProcessAgentSourceRuleGroups(GpContext, this.AgentDataRuleGroups, agentData);
            GpContext.SaveChanges();
            return true;
        }

        private void ProcessAgentSourceRuleGroups(DbContext toContext, Dictionary<string, SourceRuleGroup> sourceRuleGroups, List<Dictionary<string, object>> agentData)
        {
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in sourceRuleGroups)
                foreach (Dictionary<string, object> srcDict in agentData)
                    ProcessAgentDestinationRuleGroups(toContext, srcGroup.Value.RuleGroups, srcDict);
        }

        private void ProcessAgentDestinationRuleGroups(DbContext toContext, Dictionary<string, DestinationRuleGroup> destRuleGroups, Dictionary<string, object> srcDict)
        {
            foreach (KeyValuePair<string, DestinationRuleGroup> destGroup in destRuleGroups)
            {
                // Dictionary<DestinationPropName, SourceData> 
                Dictionary<string, object> tupleData = ReadAgentSourceTupleData(destGroup.Value.Rules, srcDict);

                // Locate destination table
                dynamic destTable = GetTableByName(toContext, destGroup.Key);
                Type destDataObjectType = GetTupleTypeByTableName(toContext, destGroup.Key);

                // Acquire destination object
                object destDataObject = SearchOrCreateDestinationObject(toContext, destTable, tupleData, destDataObjectType);

                // Copy data
                foreach (KeyValuePair<string, object> data in tupleData)
                    destDataObjectType.GetProperty(data.Key).SetValue(destDataObject, data.Value);

                toContext.Add(destDataObject);
            }
        }

        private Dictionary<string, object> ReadAgentSourceTupleData(List<Mapping> rules, Dictionary<string, object> srcDict)
        {
            Dictionary<string, object> destTupleData = new Dictionary<string, object>();

            // Read source data
            foreach (Mapping rule in rules)
            {
                dynamic sourcePropData;
                if (rule.IsFromEmpty())
                {
                    sourcePropData = null;
                }
                else
                {
                    sourcePropData = srcDict[rule.From];
                }
                // Agent to DB Rule
                destTupleData[rule.GetToColumn()] = Conversion.DoConvert(rule.ConversionFunc,
                    rule.ConversionArgs, sourcePropData, false);
            }

            return destTupleData;
        }

        public List<List<Dictionary<string, object>>> TransformAgentDataToMaster()
        {
            List<List<Dictionary<string, object>>> agentData = ProcessAgentSourceRuleGroupsReversed(GpContext, this.AgentDataRuleGroupsReversed);
            return agentData;
        }

        private List<List<Dictionary<string, object>>> ProcessAgentSourceRuleGroupsReversed(DbContext fromContext, Dictionary<string, SourceRuleGroup> sourceRuleGroups)
        {
            List<List<Dictionary<string, object>>> allAgentData = new List<List<Dictionary<string, object>>>();
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in sourceRuleGroups)
            {
                dynamic sourceData = GetTableByName(fromContext, srcGroup.Key);
                List<Dictionary<string, object>> agentData = new List<Dictionary<string, object>>();

                foreach (object srcTuple in sourceData)
                    agentData.Add(ProcessAgentDestinationRuleGroupsReversed(srcGroup.Value.RuleGroups, srcTuple));

                allAgentData.Add(agentData);
            }
            return allAgentData;
        }

        private Dictionary<string, object> ProcessAgentDestinationRuleGroupsReversed(Dictionary<string, DestinationRuleGroup> destRuleGroups, object sourceTuple)
        {
            Dictionary<string, object> agentData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, DestinationRuleGroup> destGroup in destRuleGroups)
            {
                Dictionary<string, object> newAgentData = ReadAgentSourceTupleReversed(destGroup.Value.Rules, sourceTuple);
                agentData = agentData.Union(newAgentData).ToDictionary(k => k.Key, v => v.Value);
            }

            return agentData;
        }

        private Dictionary<string, object> ReadAgentSourceTupleReversed(List<Mapping> rules, object srcTuple)
        {
            Dictionary<string, object> destTupleData = new Dictionary<string, object>();

            // Read source data
            foreach (Mapping rule in rules)
            {
                Type tupleType = srcTuple.GetType();
                PropertyInfo tupelProp = tupleType.GetProperty(rule.GetFromColumn());
                dynamic sourcePropData = tupelProp.GetValue(srcTuple);
                try
                {
                    destTupleData[rule.To] = Conversion.DoConvert(rule.ConversionFunc,
                    rule.ConversionArgs, sourcePropData, true);
                }catch
                {
                    continue;
                }
            }

            return destTupleData;
        }

        private IEnumerable<string> GetPrimaryKeys(DbContext context, Type objectType)
        {
            return context.Model.FindEntityType(objectType).FindPrimaryKey().Properties.Select(x => x.Name);
        }

        private dynamic GetTableByName(DbContext context, string tableName)
        {
            Type sourceType = context.GetType();
            PropertyInfo sourceProp = sourceType.GetProperty(tableName);
            // Returns InternalDbSet which should not be used
            return sourceProp.GetValue(context);
        }

        private Type GetTupleTypeByTableName(DbContext context, string tableName)
        {
            Type destType = context.GetType();
            PropertyInfo destProp = destType.GetProperty(tableName);
            return destProp.PropertyType.GetTypeInfo().GenericTypeArguments[0];
        }

        private object SearchOrCreateDestinationObject(DbContext toContext, dynamic destTable, 
            Dictionary<string, object> srcTupleData, Type destinationObjectType)
        {
            object destDataObject = null;

            IEnumerable<string> primaryKeys = GetPrimaryKeys(toContext, destinationObjectType);

            // Search or create destination object based on primary keys
            foreach (object destTuple in destTable.Local)
            {
                bool equals = true;
                // Compare primary key(s)
                foreach (string keyName in primaryKeys)
                {
                    dynamic srcPkVal = srcTupleData[keyName];
                    dynamic destPkVal = destinationObjectType.GetProperty(keyName).GetValue(destTuple);

                    if (srcPkVal != destPkVal)
                    {
                        equals = false;
                        break;
                    }
                }
                if (equals)
                {
                    destDataObject = destTuple;
                    break;
                }
            }
            if (destDataObject == null)
                destDataObject = Activator.CreateInstance(destinationObjectType);

            return destDataObject;
        }
    }
}
