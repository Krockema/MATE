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
            // TODO: via reflection: Store the caller of this constructor in map with created id for later debugging
        }
        
        public Id GetId()
        {
            return new Id(Id);
        }

        public override bool Equals(object obj)
        {
            BaseEntity other = (BaseEntity)obj;
            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

}
