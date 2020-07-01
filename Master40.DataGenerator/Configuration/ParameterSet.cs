using Master40.DB;
using Master40.DB.Nominal.Model;
using System;
using System.Collections.Generic;

namespace Master40.DataGenerator.Configuration
{
    public class ParameterSet : Dictionary<Type, object>
    {
        public static ParameterSet Create(object[] args)
        {
            var s = new ParameterSet();
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

        public static object[] Defaults => 
            new object[]
            {
                Dbms.GetNewMasterDataBase(false, "Master40"),
                new Setup(ModelSize.Medium),
                new Operator(ModelSize.Small),
                new Resource(ModelSize.Medium),
            };
    }
}
