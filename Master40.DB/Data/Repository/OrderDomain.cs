using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.DB.Data.Repository
{
    public class OrderDomain : DbContext
    { 
        public OrderDomain(DbContextOptions<OrderDomain> options) : base(options) 
        {
            Orders = base.Set<Order>();
            OrderParts = base.Set<OrderPart>();
        }
        public IQueryable<Order> Orders { get; set; }
        public IQueryable<OrderPart> OrderParts { get; set; }


    }
}
