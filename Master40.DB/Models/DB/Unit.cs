using System;
using System.ComponentModel.DataAnnotations;


namespace Master40.Models.DB
{
    public class Unit
    {
        [Key]
        public int UnitId { get; set; }
        public String Name { get; set; }
    }
}
