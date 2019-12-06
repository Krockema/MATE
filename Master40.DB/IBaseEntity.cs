namespace Master40.DB
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        /**
         * There comes a time, when an entity is finished e.g. ProductionOrder is finished producing
         * --> entity is not allowed to change anymore regarding time/amount
         * OR an initial StockExchangeDemand that simulates the initial stock is not allowed to change in time
         */
        bool IsReadOnly { get; set; }
    }
}