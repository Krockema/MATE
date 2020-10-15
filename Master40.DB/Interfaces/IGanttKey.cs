using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Master40.DB.Interfaces
{
    interface IGanttKey
    {
        string ProductionorderId { get; set; }
        string OperationId { get; set; }
        int ActivityId { get; set; }
        string GetKey { get; }
    }
}
