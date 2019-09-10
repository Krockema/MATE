using AkkaSim.Definitions;
using Master40.SimulationCore.Environment.Options;
using System;
using System.Collections.Generic;

namespace Master40.SimulationCore.Environment
{
    public class Configuration : Dictionary<Type, object>
    {
        public static Configuration Create(object[] args)
        {
            var s = new Configuration();
            foreach (var item in args)
            {
                s.AddOption(o: item);
            }
            return s;
        }
        public bool AddOption(object o)
        {
            return this.TryAdd(key: o.GetType(), value: o);
        }

        public T GetOption<T>()
        {
            this.TryGetValue(key: typeof(T), value: out object value);
            return (T)value;
        }

        public SimulationConfig GetContextConfiguration()
        {
            try
            {
                var config = new SimulationConfig(
                    debug: this.GetOption<DebugSystem>().Value
                    , interruptInterval: this.GetOption<KpiTimeSpan>().Value);
                return config;
            }
            catch (Exception)
            {
                throw new Exception(message: "Configuration Error.");
            }
        }
    }
}
