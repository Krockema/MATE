using System.Collections.Generic;
using Master40.DB.DataModel;

namespace Zpp.WrappersForCollections
{
    public class BusinessPartners
    {
        private List<M_BusinessPartner> _businessPartners;

        public BusinessPartners(List<M_BusinessPartner> businessPartners)
        {
            _businessPartners = businessPartners;
        }

        public List<M_BusinessPartner> GetValue()
        {
            return _businessPartners;
        }
    }
}