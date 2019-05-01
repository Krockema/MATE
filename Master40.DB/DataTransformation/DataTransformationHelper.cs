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
                string sourceTabName = mapping.From.Split(".")[0];
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
                    string destTabName = rule.To.Split(".")[0];
                    if (!srcRuleGroup.RuleGroups.ContainsKey(destTabName))
                        srcRuleGroup.RuleGroups[destTabName] = new DestinationRuleGroup();
                    srcRuleGroup.RuleGroups[destTabName].AddRule(rule);

                }
                SourceRuleGroups[srcGroup.Key] = srcRuleGroup;
            }
        }

        private IEnumerable<string> _getPrimaryKeys(DbContext context, Type objectType)
        {
            return context.Model.FindEntityType(objectType).FindPrimaryKey().Properties.Select(x => x.Name);
        }

        public bool TransformMasterToGp()
        {
            foreach (KeyValuePair<string, SourceRuleGroup> srcGroup in SourceRuleGroups)
            {
                // Locate source table
                Type sourceType = MasterContext.GetType();
                PropertyInfo sourceProp = sourceType.GetProperty(srcGroup.Key);
                // sourceTable and destTable are InternalDbSet which should not be used
                dynamic sourceTable = sourceProp.GetValue(MasterContext);

                foreach (object srcTuple in sourceTable)
                {
                    foreach(KeyValuePair<string, DestinationRuleGroup> destGroup in srcGroup.Value.RuleGroups)
                    {
                        // Dictionary<DestinationPropName, SourceData> 
                        Dictionary<string, object> tupleData = new Dictionary<string, object>();

                        // Read source data
                        foreach (Mapping rule in destGroup.Value.Rules)
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
                                PropertyInfo tupelProp = tupleType.GetProperty(rule.From.Split('.')[1]);
                                tupleData[rule.To.Split('.')[1]] = Conversion.DoConvert(rule.ConversionFunc, 
                                    rule.ConversionArgs, tupelProp.GetValue(srcTuple), false);
                            }
                        }

                        // Locate destination table
                        Type destType = GpContext.GetType();
                        PropertyInfo destProp = destType.GetProperty(destGroup.Key);
                        dynamic destTable = destProp.GetValue(GpContext);
                        Type destDataObjectType = destProp.PropertyType.GetTypeInfo().GenericTypeArguments[0];
                        object destDataObject = null;
                        
                        IEnumerable<string> primaryKeys = _getPrimaryKeys(GpContext, destDataObjectType);

                        // Search or create destination object based on primary keys
                        foreach(object destTuple in destTable)
                        {
                            bool equals = true;
                            // Compare primary key(s)
                            foreach(string keyName in primaryKeys)
                            {
                                if(tupleData[keyName] != destDataObjectType.GetProperty(keyName).GetValue(destTuple))
                                {
                                    equals = false;
                                    break;
                                }
                            }
                            if(equals)
                            {
                                destDataObject = destTuple;
                                break;
                            }
                        }
                        if (destDataObject == null)
                            destDataObject = Activator.CreateInstance(destDataObjectType);

                        // Copy data
                        foreach(KeyValuePair<string, object> data in tupleData)
                            destDataObjectType.GetProperty(data.Key).SetValue(destDataObject, data.Value);

                        GpContext.Add(destDataObject);
                    }
                }
            }
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
