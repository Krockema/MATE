namespace Master40.PiWebApi.Interfaces
{
    public interface IPiWebAttribute
    {
        string Name { get; }
        double Value { get; set; }
        double Tolerance_Min { get; set; }
        double Tolerance_Max { get; set; }
        int ValueTypeId { get; set; }
        int CharacteristicId { get; set; }
        IPiWebCharacteristic Characteristic { get; }
    }
}