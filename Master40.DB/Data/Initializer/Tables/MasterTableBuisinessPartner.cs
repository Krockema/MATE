using Master40.DB.Data.Context;
using Master40.DB.DataModel;

namespace Master40.DB.Data.Initializer.Tables
{
    internal class MasterTableBusinessPartner
    {
        internal M_BusinessPartner DEBITOR_TOYS_R_US = new M_BusinessPartner() { Debitor = true, Kreditor = false, Name = "Toys'R'us toy department" };
        internal M_BusinessPartner KREDITOR_MATERIAL_WHOLSALE = new M_BusinessPartner() { Debitor = false, Kreditor = true, Name = "Material wholesale" };
        internal M_BusinessPartner[] Init(MasterDBContext context)
        {
            var businessPartner = new[]
            {
                DEBITOR_TOYS_R_US,
                KREDITOR_MATERIAL_WHOLSALE
            };

            context.BusinessPartners.AddRange(businessPartner);
            context.SaveChanges();
            return businessPartner;
        }
        
    }
}