using System.ComponentModel.DataAnnotations;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB
{
    public class BaseEntity : IBaseEntity
    {
        [Key]
        public int Id { get; set; }

        public Id GetId()
        {
            return new Id(Id);
        }

    }

}
