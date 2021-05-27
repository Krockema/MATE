using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class CreateQualityData : Option<bool>
    {
        public static Type Type => typeof(CreateQualityData);
        public CreateQualityData(bool value)
        {
            _value = value;
        }

        public CreateQualityData()
        {
            Action = (config, argument) => {
                config.AddOption(o: new CreateQualityData(value: bool.Parse(value: argument)));
            };
        }
    }
}
