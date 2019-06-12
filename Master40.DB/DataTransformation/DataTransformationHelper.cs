using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using System.Linq;
using System.Reflection;
using Master40.DB.DataTransformation.Conversions;
using Master40.DB.Data.Initializer;

namespace Master40.DB.DataTransformation
{
    public class DataTransformationHelper
    {
        private MasterDBContext MasterContext;
        private GPSzenarioContext GpContext;
        // TODO: Later there will be no need to keep this data locally
        private List<Dictionary<string, object>> AgentData;
        // Core data only needs to be transformed once
        private List<Mapping> CoreDataRules = new List<Mapping>();
        private Dictionary<string, SourceRuleGroup> CoreDataRuleGroups = new Dictionary<string, SourceRuleGroup>();
        // Agent data will be transformed periodically
        private List<Mapping> AgentDataRules = new List<Mapping>();
        private Dictionary<string, SourceRuleGroup> AgentDataRuleGroups = new Dictionary<string, SourceRuleGroup>();
        private Dictionary<string, SourceRuleGroup> AgentDataRuleGroupsReversed = new Dictionary<string, SourceRuleGroup>();

        public DataTransformationHelper(MasterDBContext masterContext, GPSzenarioContext gpContext, List<Dictionary<string, object>> agentData)
        {
            this.MasterContext = masterContext;
            this.GpContext = gpContext;
            this.AgentData = agentData;

            MasterContext.Database.EnsureCreated();
            SortTransformationRules();
            this.CoreDataRuleGroups = GroupRules(CoreDataRules);
            this.AgentDataRuleGroups = GroupRules(AgentDataRules);
            this.AgentDataRuleGroupsReversed = GroupRules(AgentDataRules, true);
        }

        private void SortTransformationRules()
        {
            foreach(Mapping mapping in MasterContext.Mappings)
            {
                if (mapping.IsAgentData)
                    AgentDataRules.Add(mapping);
                else
                    CoreDataRules.Add(mapping);
            }
        }

        public Boolean TransformCoreData()
        {
            GpContext.Database.EnsureDeleted();
            GPSzenarioInitializer.DbInitialize(this.GpContext);

            ProcessSourceRuleGroups(MasterContext, GpContext, this.CoreDataRuleGroups);
            GpContext.SaveChanges();
            return true;
        }

        public Boolean TransformAgentDataToGp(List<Dictionary<string, object>> agentData)
        {
            ProcessSourceRuleGroups(MasterContext, GpContext, this.AgentDataRuleGroups);
            GpContext.SaveChanges();
            return true;
        }

        public List<Dictionary<string, object>> TransformAgentDataToMaster()
        {
            ProcessSourceRuleGroups(MasterContext, GpContext, this.AgentDataRuleGroupsReversed);
            GpContext.SaveChanges();
            return new List<Dictionary<string, object>>();
        }

        private Dictionary<string, SourceRuleGroup> GroupRules(List<Mapping> rules, bool reversed=false)
        {
            Dictionary<string, SourceRuleGroup> RuleGroup = new Dictionary<string, SourceRuleGroup>();
            Dictionary<string, DestinationRuleGroup> tmpRuleGroup = new Dictionary<string, DestinationRuleGroup>();
            
            // Group source tables
            foreach(Mapping mapping in rules)
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
            foreach(KeyValuePair<string, DestinationRuleGroup> srcGroup in tmpRuleGroup)
            {
                SourceRuleGroup srcRuleGroup = new SourceRuleGroup();
                foreach(Mapping rule in srcGroup.Value.Rules)
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

        private Dictionary<string, object> ReadSourceTupleData(List<Mapping> rules, object srcTuple)
        {
            Dictionary<string, object> destTupleData = new Dictionary<string, object>();

            // Read source data
            foreach (Mapping rule in rules)
            {
                if (rule.IsAgentData)
                {
                    // Agent to DB Rule
                    Dictionary<string, object> srcDict = (Dictionary<string, object>)srcTuple;
                    destTupleData[rule.GetToColumn()] = Conversion.DoConvert(rule.ConversionFunc,
                        rule.ConversionArgs, srcDict[rule.From], false);
                }
                else
                {
                    // DB to DB Rule
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
            }

            return destTupleData;
        }

        public object SearchOrCreateDestinationObject(DbContext toContext, dynamic destTable, 
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

        private void ProcessDestinationRuleGroups(DbContext toContext, Dictionary<string, DestinationRuleGroup> destRuleGroups, object sourceTuple)
        {
            foreach (KeyValuePair<string, DestinationRuleGroup> destGroup in destRuleGroups)
            {
                // Dictionary<DestinationPropName, SourceData> 
                Dictionary<string, object> tupleData = ReadSourceTupleData(destGroup.Value.Rules, sourceTuple);

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

        private void ProcessSourceRuleGroups(DbContext fromContext, DbContext toContext, Dictionary<string, SourceRuleGroup> sourceRuleGroups, bool reversed = false)
        {
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in sourceRuleGroups)
            {
                dynamic sourceData;
                if(srcGroup.Value.IsAgentRuleGroup() && !reversed)
                    sourceData = AgentData;
                else
                    sourceData = GetTableByName(fromContext, srcGroup.Key);

                foreach (object srcTuple in sourceData)
                    ProcessDestinationRuleGroups(toContext, srcGroup.Value.RuleGroups, srcTuple);
            }
        }

        public bool TransformMasterToGp()
        {
            GpContext.Database.EnsureDeleted();
            GPSzenarioInitializer.DbInitialize(this.GpContext);

            Dictionary<string, SourceRuleGroup> RuleGroup = GroupRules(MasterContext.Mappings.ToList());
            ProcessSourceRuleGroups(this.MasterContext, this.GpContext, RuleGroup);
            GpContext.SaveChanges();
            return true;
        }

        public bool TransformGpToMaster()
        {
            Dictionary<string, SourceRuleGroup> RuleGroup = GroupRules(MasterContext.Mappings.ToList(), true);
            // TODO: Actually transform data. In this direction only Rules with IsAgentData==true need to be applied
            return true;
        }
    }
}
