using System;
using System.Collections.Generic;
using Akka.Hive.Definitions;
using Mate.Production.Core.Environment.Options;
using Mate.Production.Core.Helper;

namespace Mate.Production.Core.Environment
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

        public bool ReplaceOption(object o)
        {
            this.Remove(o.GetType());
            return AddOption(o);
        }

        public IHiveConfig GetContextConfiguration()
        {
            try
            {
                var config = HiveConfig.ConfigureSimulation(sequencial: false)
                                        .WithTimeSpanToTerminate(TimeSpan.FromMinutes(this.GetOption<SimulationEnd>().Value))
                                        .WithDebugging(akka: false // Debug Akka Core System
                                                       , hive: true) //  this.GetOption<DebugSystem>().Value)
                                        .WithInterruptInterval(this.GetOption<KpiTimeSpan>().Value)
                                        .WithStartTime(this.GetOption<SimulationStartTime>().Value)
                                        .Build();// set AkkaSim in Debug Mode
                return config;
            }
            catch (Exception)
            {
                throw new Exception(message: "Configuration Error.");
            }
        }
    }
}
