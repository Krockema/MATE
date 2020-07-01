using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Master40.Models
    { 
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        [StringLength(maximumLength: 50)]
        public string MenuText { get; set; }
        [StringLength(maximumLength: 255)]
        public string LinkUrl { get; set; }
        public string Symbol { get; set; }
        public string Action { get; set; }
        public int? MenuOrder { get; set; }
        public int? ParentMenuItemId { get; set; }
        public virtual MenuItem Parent { get; set; }
        public virtual ICollection<MenuItem> Children { get; set; }
        public int MenuId { get; set; }
        public virtual Menu Menu { get; set; }
    }
}