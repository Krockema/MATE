using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Master40.DB.Data.Helper;
using Master40.DB.Data.WrappersForPrimitives;

namespace Master40.DB
{
    public class BaseEntity : IBaseEntity
    {
        // [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        protected BaseEntity()
        {
            Id = IdGeneratorHolder.GetIdGenerator().GetNewId();
        }
        
        public Id GetId()
        {
            return new Id(Id);
        }

    }

}
