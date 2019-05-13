using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp
{
    public interface IProviderManager
    {
        IProvider GetProviderById(int id);

        void AddProvider(IProvider provider);

        List<IProvider> GetProviders();

        List<IProvider> getProvidersById(List<int> ids);
    }
}