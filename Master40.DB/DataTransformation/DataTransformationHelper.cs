using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Master40.DB.DataTransformation;
using Master40.DB.Data.Context;
using Master40.DB.Models;
using Master40.DB.GanttplanDB.Models;

namespace Master40.DB.DataTransformer
{
    public class DataTransformationHelper
    {
        private MasterDBContext MasterContext;
        private GPSzenarioContext GpContext;
        private List<TransformationRule> Rules;

        public DataTransformationHelper(MasterDBContext masterContext, GPSzenarioContext gpContext)
        {
            this.MasterContext = masterContext;
            this.GpContext = gpContext;

            this.Rules = ReadRules();

            MasterContext.Database.EnsureCreated();
            GpContext.Database.EnsureCreated();
        }

        private List<TransformationRule> ReadRules()
        {
            List<TransformationRule> rules = new List<TransformationRule>();

            foreach(Mapping mapping in MasterContext.Mappings)
            {
                rules.Add(new TransformationRule(mapping.From, mapping.To));
            }

            return rules;
        }

        public bool TransformMasterToGp()
        {
            foreach(TransformationRule rule in Rules)
            {
                //TODO
            }
            return true;
        }

        public bool TransformGpToMaster()
        {
            foreach (TransformationRule rule in Rules)
            {
                //TODO
            }
            return true;
        }
    }
}
