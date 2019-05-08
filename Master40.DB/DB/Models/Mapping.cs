using System;
using System.Collections.Generic;
using System.Text;

namespace Master40.DB.Models
{
    public class Mapping : BaseEntity
    {
        public string From { get; set; }
        public string To { get; set; }
        public bool IsAgentData { get; set; }
        public string ConversionFunc { get; set; }
        public string ConversionArgs { get; set; }

        public string GetFromTable()
        {
            return From.Split('.')[0];
        }

        public string GetFromColumn()
        {
            return From.Split('.')[1];
        }

        public string GetToTable()
        {
            return To.Split('.')[0];
        }

        public string GetToColumn()
        {
            return To.Split('.')[1];
        }
    }
}
