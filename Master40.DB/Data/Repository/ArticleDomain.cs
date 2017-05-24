using Master40.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Master40.DB.Data.Repository
{
    public class ReadOnlyOrderDomain : DbContext
    { 
        public ReadOnlyOrderDomain(DbContextOptions<ReadOnlyOrderDomain> options) : base(options) 
        {
            Orders = base.Set<Order>();
            OrderParts = base.Set<OrderPart>();
        }
        public IQueryable<Order> Orders { get; }
        public IQueryable<OrderPart> OrderParts { get; }


    }
}
