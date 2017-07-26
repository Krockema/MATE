using Master40.DB.Models;

namespace Master40.Agents.Agents.Model
{
    public class RequestItem
    {
        public Article Article { get; set; }
        public int Quantity { get; set; }
        public int DueTime { get; set; }
    }
}