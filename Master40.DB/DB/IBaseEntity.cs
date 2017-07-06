namespace Master40.DB.DB
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        string SimulationIdent { get; set; }
        SimulationType SimulationType { get; set; }
    }
}