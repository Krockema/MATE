using System.ComponentModel.DataAnnotations;

namespace Master40.DB
{
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }
    }
}
