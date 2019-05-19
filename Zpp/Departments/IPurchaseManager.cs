using Master40.DB.Interfaces;

namespace Zpp
{
    public interface IPurchaseManager
    {
        void createPurchaseOrderPart(IDemand demand);
    }
}