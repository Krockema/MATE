using System.ComponentModel.DataAnnotations;

namespace Master40.DB.DB
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }

    public interface IAggregateRoot {

    }
}
