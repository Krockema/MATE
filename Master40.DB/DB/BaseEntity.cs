using System.ComponentModel.DataAnnotations;
using System.Linq;
using Master40.DB.Repository;
namespace Master40.DB
{
    public class BaseEntity : IBaseEntity
    {
        [Key]
        public int Id { get; set; }

        public BaseEntity() { }
        public BaseEntity(UnitOfWork unitOfWork)
        {
            if (unitOfWork.InMemory)
            {
                AddToContext(unitOfWork);
            }
        }
        public void AddToContext(UnitOfWork unitOfWork)
        {

            var item = this.GetType();
            var props = unitOfWork.GetType().GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name != "InMemory")
                {
                    var liste = unitOfWork.GetType().GetProperty(prop.Name).GetValue(unitOfWork, null);
                    var discriminator = liste.GetType().GetGenericArguments().Single();
                    if (discriminator.Name == item.Name)
                    {
                        liste.GetType().GetMethod("Add").Invoke(liste, new[] { this });
                    }
                }
            }

        }

    }

}
