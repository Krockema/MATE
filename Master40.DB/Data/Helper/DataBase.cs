using Master40.DB.Data.Helper.Types;
using Microsoft.EntityFrameworkCore;

namespace Master40.DB.Data.Helper
{
    public class DataBase<T> where T : DbContext
    {
        public DataBase(string dataBaseName)
        {
            DataBaseName = new DataBaseName(dataBaseName);
        }
        public T DbContext { get; set; }
        public DbConnectionString ConnectionString { get; set; }
        public DataBaseName DataBaseName { get; set; }
    }
}
