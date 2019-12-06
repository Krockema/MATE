using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class Duration : IntPrimitive<Duration>
    {
        public Duration(int? @int) : base(ToInt(@int))
        {
        }
        
        private static int ToInt(int? value)
        {
            if (value == null)
            {
                return DueTime.INVALID_DUETIME;
            }
            else
            {
                return value.GetValueOrDefault();
            }
        }

        public Duration()
        {
        }

        public DueTime ToDueTime()
        {
            return new DueTime(this.GetValue());
        }

    }
}