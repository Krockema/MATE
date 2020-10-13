using System;
using System.Linq;
using Master40.DB.Data.Context;
using Master40.DB.ReportingModel;

namespace Master40.DataGenerator.Repository
{
    public class ConfigurationItemRepository
    {
        public static ConfigurationItem GetItemByProperty(String property, ResultContext ctx)
        {
            return ctx.ConfigurationItems.SingleOrDefault(x => x.Property == property);
        }
    }
}