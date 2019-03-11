using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.DataTransformer
{
    class MasterToGanttplanTransformer
    {
        private DbContext coreDataIn;
        private Type transDataIn;
        private DbContext gpDataOut;

        public MasterToGanttplanTransformer(DbContext coreDataIn, Type transDataIn, DbContext gpDataOut)
        {
            this.coreDataIn = coreDataIn;
            this.transDataIn = transDataIn;
            this.gpDataOut = gpDataOut;

            coreDataIn.Database.EnsureCreated();
            gpDataOut.Database.EnsureCreated();
        }

        private Boolean _TransformCoreData()
        {
            // Do something
            return true;
        }

        private Boolean _TransformTransactionData()
        {
            // Do something
            return true;
        }

        public Boolean DoTransform()
        {
            return _TransformCoreData() && _TransformTransactionData();
        }
    }
}
