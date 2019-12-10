using Master40.DB.Data.WrappersForPrimitives;
using Master40.DB.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace Master40.DB
{
    public class ResultBaseEntity : IBaseEntityDbGeneratedId, IId
    {
        [Key]
        public int Id { get; set; }

        protected ResultBaseEntity()
        {
            
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

        public override string ToString()
        {
            string fullName = GetType().FullName; 
            return $"{Id}: {fullName}";
        }
    }

}