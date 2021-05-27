using Mate.PiWebApi.Interfaces;

namespace Mate.DataCore.DataModel
{
    public class M_Attribute : BaseEntity, IPiWebAttribute
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public double Tolerance_Min { get; set; }
        public double Tolerance_Max { get; set; }
        public int ValueTypeId { get; set; }
        public int CharacteristicId { get; set; }
        public M_Characteristic Characteristic { get; set; }
        IPiWebCharacteristic IPiWebAttribute.Characteristic => this.Characteristic;
    }
}
