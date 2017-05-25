using Master40.DB.Data.Context;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Repository
{
    class MrpDomainContext : MasterDBContext
    {
        public MrpDomainContext(DbContextOptions<MasterDBContext> options) : base(options)
        {

        }
    }
}
