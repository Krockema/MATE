using System.ComponentModel.DataAnnotations;

namespace Master40.DB
{
    public class BaseEntity : IBaseEntity
    {
        [Key]
        public int Id { get; set; }
    }

    public interface IAggregateRoot {

    }
}
