using System;
using Mate.Production.Core.Environment.Abstractions;

namespace Mate.Production.Core.Environment.Options
{
    public class SaveToDB : Option<bool>
    {
        public SaveToDB(bool value)
        {
            _value = value;
        }

        public SaveToDB()
        {
            Action = (config, argument) => {
                config.AddOption(o: new SaveToDB(value: bool.Parse(value: argument)));
            };
        }
    }
}
