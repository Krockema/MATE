using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master40.DB.DataTransformation;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using System;
using System.Reflection;
using Master40.DB.GanttplanDB.Models;

namespace Master40.DB.DataTransformation
{
    public class DataTransformationHelper
    {
        private MasterDBContext MasterContext;
        private GPSzenarioContext GpContext;
        private Dictionary<string, TransformationRuleGroup> RuleGroupsDict = new Dictionary<string, TransformationRuleGroup>();

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
            // TODO: Evtl nach Source und Destination table sortieren?
            foreach(Mapping mapping in MasterContext.Mappings)
            {
                string sourceTabName = mapping.From.Split(".")[0];
                if (!RuleGroupsDict.ContainsKey(sourceTabName))
                    RuleGroupsDict[sourceTabName] = new TransformationRuleGroup();

                RuleGroupsDict[sourceTabName].AddRule(mapping);
            }
        }

        public bool TransformMasterToGp()
        {
            foreach(KeyValuePair<string, TransformationRuleGroup> group in RuleGroupsDict)
            {
                // Locate source table
                Type sourceType = MasterContext.GetType();
                PropertyInfo sourceProp = sourceType.GetProperty(group.Key.Split('.')[0]);
                dynamic sourceTable = sourceProp.GetValue(MasterContext);

                foreach (object tupel in sourceTable)
                {
                    foreach(Mapping rule in group.Value.Rules)
                    {
                        // Get source data
                        Type tupelType = tupel.GetType();
                        PropertyInfo tupelProp = tupelType.GetProperty(rule.From.Split('.')[1]);
                        object data = tupelProp.GetValue(tupel);

                        // Locate destination table
                        Type destinationType = GpContext.GetType();
                        PropertyInfo destinationProp = destinationType.GetProperty(rule.To.Split('.')[0]);
                        dynamic destinationTable = destinationProp.GetValue(GpContext);

                        // TODO: Funktioniert nicht, speichert ein Property und übernimmt dann ganzes Objekt -> doppelt gruppieren siehe GroupRules()
                        // TODO: Dynamisch machen
                        // TODO: Prüfen, ob Eintrag mit ID schon vorhanden
                        Material mat = new Material();
                        Type destTupelType = mat.GetType();
                        PropertyInfo destProp = destTupelType.GetProperty(rule.To.Split('.')[1]);
                        destProp.SetValue(mat, data.ToString());
                        //destinationTable.Add(mat);
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
