using Master40.DB.DataModel;

namespace Master40.DB.Interfaces
{
    public interface IProvider 
    {
        int Id { get; set; }
        int? ProviderId { get; set; }
        T_Provider Provider { get; set; }
    }
}