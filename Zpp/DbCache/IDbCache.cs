namespace Zpp
{
    /// <summary>
    /// A wrapper around the ProductionDomainContext (e.g. to find DB calls), that should be DB Cache in future.
    /// This class is only allowed to do transactions on the database.
    /// Convention: Prefix DatabaseBeingAffected
    /// </summary>
    public interface IDbCache
    {
        void DemandToProvidersRemoveAll();
    }
}