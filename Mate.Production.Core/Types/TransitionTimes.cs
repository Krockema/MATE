using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mate.Production.Core.Types
{
    public sealed class TransitionTimesDic
    {
        private static readonly Lazy<TransitionTimesDic> lazy =
            new Lazy<TransitionTimesDic>(() => new TransitionTimesDic());

        private Lazy<Dictionary<int, double>> transitionsDic =
        new Lazy<Dictionary<int, double>>(
            () => new Dictionary<int, double>());

        private Lazy<Dictionary<string, int>> keyValuePairs =
        new Lazy<Dictionary<string, int>>(
            () => new Dictionary<string, int>());

        public static TransitionTimesDic Instance { get { return lazy.Value; } }

        private TransitionTimesDic()
        {

        }

        public void AddKeyValue(string key, int value)
        {
            keyValuePairs.Value.TryAdd(key, value);
        }


        public void SetTransitionTime(string capability, double value)
        {
            int capabilityId = keyValuePairs.Value[capability];

            if(!transitionsDic.Value.TryAdd(capabilityId, value))
            {
                transitionsDic.Value[capabilityId] = value;
            }

        }

        internal double GetTransitionTime(int capability)
        {
            double value = 0.0;

            if(transitionsDic.Value.TryGetValue(capability, out double entry))
            {
                value = entry;
            }

            return value;
        }

    }
}
