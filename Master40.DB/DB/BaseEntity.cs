using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

namespace Master40.DB.DB
{
    public class BaseEntity : IBaseEntity
    {
        public BaseEntity()
        {
            SimulationIdent = "0";
            SimulationType = SimulationType.None;
        }

        [Key]
        public int Id { get; set; }

        public SimulationType SimulationType { get; set; }
        public string SimulationIdent{ get; set; }
    }


    public enum SimulationType
    {
        None,
        Central,
        Decentral
    }

    public interface IAggregateRoot {

    }
}
