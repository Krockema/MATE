using System;
using System.ComponentModel.DataAnnotations;


namespace Master40.Models.DB
{
    public class Unit
    {
        [Key]
        public int UnitID { get; set; }
        public String Name { get; set; }
    }
}
