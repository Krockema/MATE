using System;
using System.Collections.Generic;
using System.Text;
using Master40.DB.Data.Context;
using Master40.DB.Data.Helper.Types;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Helper
{
    public class DataBase<T> where T : DbContext
    {
        public DataBase(DataBaseName dataBaseName)
        {
            DataBaseName = dataBaseName;
        }
        public T DbContext { get; set; }
        public DbConnectionString ConnectionString { get; set; }
        public DataBaseName DataBaseName { get; set; }
    }
}
