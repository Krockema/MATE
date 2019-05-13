using System.Collections.Generic;
using Master40.DB.Interfaces;

namespace Zpp
{
    public class EfficientDemandToProvider
    {
        public Dictionary<int, IDemand> ToIDemandsAsDictionary(List<IDemand> demands)
        {
            Dictionary<int, IDemand> demandsAsDictionary = new Dictionary<int, IDemand>();
            foreach (IDemand demand in demands)
            {
                demandsAsDictionary.Add(demand.Id, demand);
            }

            return demandsAsDictionary;
        }

        public Dictionary<int, IProvider> ToIProvidersAsDictionary(List<IProvider> providers)
        {
            Dictionary<int, IProvider> providersAsDictionary = new Dictionary<int, IProvider>();
            foreach (IProvider provider in providers)
            {
                providersAsDictionary.Add(provider.Id, provider);
            }

            return providersAsDictionary;
        }
    }
}