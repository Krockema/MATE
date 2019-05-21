using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using System.Linq;
using System.Reflection;
using Master40.DB.DataTransformation.Conversions;

namespace Master40.DB.DataTransformation
{
    public class DataTransformationHelper
    {
        private MasterDBContext MasterContext;
        private GPSzenarioContext GpContext;
        private Dictionary<string, SourceRuleGroup> SourceRuleGroups = new Dictionary<string, SourceRuleGroup>();

        public DataTransformationHelper(MasterDBContext masterContext, GPSzenarioContext gpContext)
        {
            this.MasterContext = masterContext;
            this.GpContext = gpContext;

            MasterContext.Database.EnsureCreated();
            GpContext.Database.EnsureCreated();

            GroupRules();
        }

        private void GroupRules()
        {
            Dictionary<string, DestinationRuleGroup> tmpRuleGroup = new Dictionary<string, DestinationRuleGroup>();
            
            // Group source tables
            foreach(Mapping mapping in MasterContext.Mappings)
            {
                string sourceTabName = mapping.GetFromTable();
                if (!tmpRuleGroup.ContainsKey(sourceTabName))
                    tmpRuleGroup[sourceTabName] = new DestinationRuleGroup();

                tmpRuleGroup[sourceTabName].AddRule(mapping);
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
                SourceRuleGroups[srcGroup.Key] = srcRuleGroup;
            }
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
            Dictionary<string, object> tupleData = new Dictionary<string, object>();

            // Read source data
            foreach (Mapping rule in rules)
            {
                if (rule.IsAgentData)
                {
                    // Agent to DB Rule
                    // TODO
                }
                else
                {
                    // DB to DB Rule
                    Type tupleType = srcTuple.GetType();
                    PropertyInfo tupelProp = tupleType.GetProperty(rule.GetFromColumn());
                    tupleData[rule.GetToColumn()] = Conversion.DoConvert(rule.ConversionFunc,
                        rule.ConversionArgs, tupelProp.GetValue(srcTuple), false);
                }
            }

            return tupleData;
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

        private void ProcessSourceRuleGroups(DbContext fromContext, DbContext toContext, Dictionary<string, SourceRuleGroup> sourceRuleGroups)
        {
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in SourceRuleGroups)
            {
                dynamic sourceTable = GetTableByName(fromContext, srcGroup.Key);

                foreach (object srcTuple in sourceTable)
                    ProcessDestinationRuleGroups(toContext, srcGroup.Value.RuleGroups, srcTuple);
            }
        }

        public bool TransformMasterToGp()
        {
            ProcessSourceRuleGroups(MasterContext, GpContext, SourceRuleGroups);
            GpContext.SaveChanges();
            return true;
        }

        public bool TransformGpToMaster()
        {
            foreach (Mapping rule in MasterContext.Mappings)
            {
                //TODO
            }
            return true;
        }
    }
}
