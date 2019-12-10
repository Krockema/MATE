using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Data.Helper.Types
{
    public class DbConnectionString : BaseWrapper<string>
    {
        public DbConnectionString(string dbConnectionString) : base(dbConnectionString)
        {

        }
    }
}
