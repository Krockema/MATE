using System;

namespace Master40.Agents.Agents.Model
{
    public class StockReservation
    {
        public int Quantity { get; set; }
        public bool IsPurchsed { get; set; }
        public bool IsInStock { get; set; }
        public int DueTime { get; set; }
    }
}