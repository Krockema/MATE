using System;

namespace Master40.DB.Data.WrappersForPrimitives
{
    public class DueTime : IntPrimitive<DueTime>
    {
        public const int INVALID_DUETIME = Int32.MinValue + INVALID_TOLERANCE;
        private const int INVALID_TOLERANCE = 100000;
        
        public DueTime(int? @int) : base(ToInt(@int))
        {
        }

        private static int ToInt(int? value)
        {
            if (value == null)
            {
                return INVALID_DUETIME;
            }
            else
            {
                return value.GetValueOrDefault();
            }
        }

        public DueTime(Duration duration):base(duration.GetValue())
        {
            
        }

        public DueTime()
        {
        }
        public bool IsInvalid()
        {
            return this.GetValue() < (INVALID_DUETIME + INVALID_TOLERANCE);
        }

    }
}