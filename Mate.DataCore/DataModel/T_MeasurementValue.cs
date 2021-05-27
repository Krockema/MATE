namespace Mate.DataCore.DataModel
{
    public class T_MeasurementValue : BaseEntity
    {

        public int AttributeId { get; set; }

        public double Value { get; set; }
        public M_Attribute Attribute { get; set; }
    }
}
